using HotChocolate.CostAnalysis;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution;
using System.Text.RegularExpressions;

namespace Nexus.DEB.Api.GraphQL._Helpers_
{
	public class GraphQLCostDiagnostics : ExecutionDiagnosticEventListener
	{
		private readonly ILogger<GraphQLCostDiagnostics> _logger;

		public GraphQLCostDiagnostics(ILogger<GraphQLCostDiagnostics> logger)
		{
			_logger = logger;
		}

		public override IDisposable ExecuteRequest(IRequestContext context)
		{
			return new RequestScope(context, _logger);
		}

		private class RequestScope : IDisposable
		{
			private readonly IRequestContext _context;
			private readonly ILogger _logger;

			public RequestScope(IRequestContext context, ILogger logger)
			{
				_context = context;
				_logger = logger;
			}

			public void Dispose()
			{
				var cost = _context.GetCostMetrics();
				string query = _context.Document.ToString();
				query = Regex.Replace(query, @"\s+", " ").Trim();
				_logger.LogDebug("🔍 GraphQL Cost Metrics");
				_logger.LogDebug("	 Query: {query}", query);
				_logger.LogDebug("   FieldCost: {FieldCost}", cost.FieldCost);
				_logger.LogDebug("   TypeCost:  {TypeCost}", cost.TypeCost);
			}
		}
	}
}
