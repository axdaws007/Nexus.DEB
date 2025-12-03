using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class ChangeEventInterceptor : SaveChangesInterceptor
{
	private const string SessionContextKey = "EventId";

	private async Task SetSessionContextAsync(DbContext context, Guid eventId, CancellationToken cancellationToken)
	{
		var conn = context.Database.GetDbConnection();

		// Ensure connection is open
		if (conn.State != ConnectionState.Open)
		{
			await conn.OpenAsync(cancellationToken);
		}

		using var command = conn.CreateCommand();
		command.CommandText = "EXEC sys.sp_set_session_context @key, @value";

		var pKey = command.CreateParameter();
		pKey.ParameterName = "@key";
		pKey.DbType = DbType.String;
		pKey.Value = SessionContextKey;

		var pValue = command.CreateParameter();
		pValue.ParameterName = "@value";
		pValue.DbType = DbType.Guid;
		pValue.Value = eventId;

		command.Parameters.Add(pKey);
		command.Parameters.Add(pValue);

		await command.ExecuteNonQueryAsync(cancellationToken);
	}

	private void SetSessionContext(DbContext context, Guid eventId)
	{
		var conn = context.Database.GetDbConnection();

		if (conn.State != ConnectionState.Open)
		{
			conn.Open();
		}

		using var command = conn.CreateCommand();
		command.CommandText = "EXEC sys.sp_set_session_context @key, @value";

		var pKey = command.CreateParameter();
		pKey.ParameterName = "@key";
		pKey.DbType = DbType.String;
		pKey.Value = SessionContextKey;

		var pValue = command.CreateParameter();
		pValue.ParameterName = "@value";
		pValue.DbType = DbType.Guid;
		pValue.Value = eventId;

		command.Parameters.Add(pKey);
		command.Parameters.Add(pValue);

		command.ExecuteNonQuery();
	}

	// Async SaveChanges
	public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default)
	{
		if (eventData.Context != null)
		{
			var eventId = Guid.NewGuid();
			await SetSessionContextAsync(eventData.Context, eventId, cancellationToken);
		}

		return result;
	}

	// Sync SaveChanges
	public override InterceptionResult<int> SavingChanges(
		DbContextEventData eventData,
		InterceptionResult<int> result)
	{
		if (eventData.Context != null)
		{
			var eventId = Guid.NewGuid();
			SetSessionContext(eventData.Context, eventId);
		}

		return result;
	}
}
