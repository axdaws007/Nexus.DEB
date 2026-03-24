using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
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
		command.SetSessionContextCommand(key, value);
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
		command.SetSessionContextCommand(key, value);
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

				if (httpContext.User != null)
				{
					var user = new DebUser(httpContext.User);
					userDetails = user.UserDetails;
				}

				if (httpContext.Items.TryGetValue("MovedSectionId", out var movedSectionIdObj)
					&& movedSectionIdObj is Guid movedSectionId)
				{
					await SetSessionContextAsync(eventData.Context, "MovedSectionId", movedSectionId, cancellationToken);
				}

				if (httpContext.Items.TryGetValue("MovedRequirementId", out var movedReqIdObj)
					&& movedReqIdObj is Guid movedRequirementId)
				{
					await SetSessionContextAsync(eventData.Context, "MovedRequirementId", movedRequirementId, cancellationToken);
				}

				if (httpContext.Items.TryGetValue("SuppressOrdinalAudit", out var suppressObj)
					&& suppressObj is bool suppress && suppress)
				{
					await SetSessionContextAsync(eventData.Context, "SuppressOrdinalAudit", "true", cancellationToken);
				}

				if (httpContext.Items.TryGetValue("MovedRequirementOldSectionId", out var oldSectionObj)
					&& oldSectionObj is Guid oldSectionId)
				{
					await SetSessionContextAsync(eventData.Context, "MovedRequirementOldSectionId", oldSectionId, cancellationToken);
				}

				if (httpContext.Items.TryGetValue("MovedRequirementOldOrdinal", out var oldOrdinalObj)
					&& oldOrdinalObj is int oldOrdinal)
				{
					await SetSessionContextAsync(eventData.Context, "MovedRequirementOldOrdinal", oldOrdinal.ToString(), cancellationToken);
				}

				if (httpContext.Items.TryGetValue("IgnoreAudit", out var ignoreAudit) && (bool)ignoreAudit)
				{
					await SetSessionContextAsync(eventData.Context, "IgnoreAudit", ignoreAudit, cancellationToken);
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

                if (httpContext.Items.TryGetValue("MovedSectionId", out var movedSectionIdObj)
                    && movedSectionIdObj is Guid movedSectionId)
                {
                    SetSessionContext(eventData.Context, "MovedSectionId", movedSectionId);
                }

                if (httpContext.Items.TryGetValue("MovedRequirementId", out var movedReqIdObj)
                    && movedReqIdObj is Guid movedRequirementId)
                {
                    SetSessionContext(eventData.Context, "MovedRequirementId", movedRequirementId);
                }

                if (httpContext.Items.TryGetValue("SuppressOrdinalAudit", out var suppressObj)
                    && suppressObj is bool suppress && suppress)
                {
                    SetSessionContext(eventData.Context, "SuppressOrdinalAudit", "true");
                }

                if (httpContext.Items.TryGetValue("MovedRequirementOldSectionId", out var oldSectionObj)
                    && oldSectionObj is Guid oldSectionId)
                {
                    SetSessionContext(eventData.Context, "MovedRequirementOldSectionId", oldSectionId);
                }

                if (httpContext.Items.TryGetValue("MovedRequirementOldOrdinal", out var oldOrdinalObj)
                    && oldOrdinalObj is int oldOrdinal)
                {
                    SetSessionContext(eventData.Context, "MovedRequirementOldOrdinal", oldOrdinal.ToString());
				}

				if (httpContext.Items.TryGetValue("IgnoreAudit", out var ignoreAudit) && (bool)ignoreAudit)
				{
					SetSessionContext(eventData.Context, "IgnoreAudit", ignoreAudit);
				}
			}

			SetSessionContext(eventData.Context, SessionContextEventId, eventId);
			SetSessionContext(eventData.Context, SessionContextUserDetails, userDetails);
		}

		return result;
	}
}

internal static class DbCommandExtensions
{
	public static void SetSessionContextCommand(this DbCommand command, string key, object value)
	{
		command.CommandText = "EXEC sys.sp_set_session_context @key, @value";

		var pKey = command.CreateParameter();
		pKey.ParameterName = "@key";
		pKey.DbType = DbType.String;
		pKey.Value = key;

		var pValue = command.CreateParameter();
		pValue.ParameterName = "@value";
		if (value is Guid)
			pValue.DbType = DbType.Guid;
		else if(value is bool)
			pValue.DbType = DbType.Boolean;
		else
			pValue.DbType = DbType.String;
		pValue.Value = value;

		command.Parameters.Add(pKey);
		command.Parameters.Add(pValue);
	}
}
