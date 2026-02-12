using HotChocolate.CostAnalysis;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Execution;

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
				// This runs at request end
				if (_context.ContextData.TryGetValue("HotChocolate.CostAnalysis.CostMetricsKey", out var costObj) &&
					costObj is CostMetrics cost)
				{
					_logger.LogInformation("🔍 GraphQL Cost Metrics");
					_logger.LogInformation("   FieldCost: {FieldCost}", cost.FieldCost);
					_logger.LogInformation("   TypeCost:  {TypeCost}", cost.TypeCost);
				}
			}
		}
	}
}
