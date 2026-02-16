using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Nexus.DEB.Application.Common.Interfaces;
using Nexus.DEB.Infrastructure.Helpers;
using Nexus.DEB.Infrastructure.Services;

public class ChangeEventInterceptor : SaveChangesInterceptor
{
	private const string SessionContextEventId = "EventId";
	private const string SessionContextUserDetails = "UserDetails";
	protected readonly ILogger<ChangeEventInterceptor> _logger;
	protected readonly IHttpContextAccessor _httpContextAccessor;

	public ChangeEventInterceptor(ILogger<ChangeEventInterceptor> logger, IHttpContextAccessor httpContextAccessor)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
	}

	private async Task SetSessionContextAsync(DbContext context, string key, object value, CancellationToken cancellationToken)
	{
		_logger.LogDebug("***   Setting session context value (async): {Key} = {Value}   ***", key, value);

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
		_logger.LogDebug("*** Setting session context value: {Key} = {Value}   ***", key, value);

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
			var eventId = Guid.NewGuid();
			var userDetails = "";
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext != null)
			{
				if (httpContext.Items["CorrelationId"] != null)
				{
					eventId = Guid.Parse(httpContext.Items["CorrelationId"].ToString()!);
				}

				if(httpContext.User != null)
				{
					var user = new DebUser(httpContext.User);
					userDetails = user.UserDetails;
				}
			}
			await SetSessionContextAsync(eventData.Context, SessionContextEventId, eventId, cancellationToken);
			await SetSessionContextAsync(eventData.Context, SessionContextUserDetails, userDetails, cancellationToken);

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
			var userDetails = "";
			var httpContext = _httpContextAccessor.HttpContext;
			if (httpContext != null)
			{
				if (httpContext.Items["CorrelationId"] != null)
				{
					eventId = Guid.Parse(httpContext.Items["CorrelationId"].ToString()!);
				}

				if (httpContext.User != null)
				{
					var user = new DebUser(httpContext.User);
					userDetails = user.UserDetails;
				}
			}
			SetSessionContext(eventData.Context, SessionContextEventId, eventId);
			SetSessionContext(eventData.Context, SessionContextUserDetails, userDetails);
		}

		return result;
	}
}
