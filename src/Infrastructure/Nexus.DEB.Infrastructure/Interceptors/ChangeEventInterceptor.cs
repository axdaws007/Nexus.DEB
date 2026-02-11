using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Services;

public class ChangeEventInterceptor : SaveChangesInterceptor
{
	private const string SessionContextEventId = "EventId";
	private const string SessionContextUserDetails = "UserDetails";
	protected readonly ILogger<ChangeEventInterceptor> logger;

	public ChangeEventInterceptor(ILogger<ChangeEventInterceptor> _logger)
	{
		logger = _logger;
	}

	private async Task SetSessionContextAsync(DbContext context, string key, object value, CancellationToken cancellationToken)
	{
		logger.LogDebug("Setting session context value: {Key} = {Value}", key, value);

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
		pKey.Value = key;

		var pValue = command.CreateParameter();
		pValue.ParameterName = "@value";
		if(value is Guid)
			pValue.DbType = DbType.Guid;
		else
			pValue.DbType = DbType.String;
		pValue.Value = value;

		command.Parameters.Add(pKey);
		command.Parameters.Add(pValue);

		await command.ExecuteNonQueryAsync(cancellationToken);
	}

	private void SetSessionContext(DbContext context, string key, object value)
	{
		logger.LogDebug("Setting session context value: {Key} = {Value}", key, value);

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
		pKey.Value = key;

		var pValue = command.CreateParameter();
		pValue.ParameterName = "@value";
		pValue.DbType = DbType.Guid;
		pValue.Value = value;

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
			var _context = (IDebContext)eventData.Context!;
			await SetSessionContextAsync(eventData.Context, SessionContextEventId, _context.EventId, cancellationToken);
			await SetSessionContextAsync(eventData.Context, SessionContextUserDetails, _context.UserDetails, cancellationToken);
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
			var _context = (IDebContext)eventData.Context!;
			SetSessionContext(eventData.Context, SessionContextEventId, _context.EventId);
			SetSessionContext(eventData.Context, SessionContextUserDetails, _context.UserDetails);
		}

		return result;
	}
}
