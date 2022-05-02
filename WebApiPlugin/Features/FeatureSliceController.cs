using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MediatR;
using Microsoft.Extensions.Logging;

namespace WebApiPlugin.Features
{
    public abstract class FeatureSliceController : ApiController
    {
        protected readonly IMediator Mediator;
        protected readonly ILogger Logger;

        protected FeatureSliceController(IMediator mediator, ILogger logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        protected async Task<QueryResult<TData>> ExecuteCommandAsync<TResponse, TData>(IRequest<TResponse> request, CancellationToken cancellationToken) where TResponse: QueryResult<TData>
        {
            try
            {
                return await Mediator.Send(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected error occurred while executing a command.");
                return new QueryResult<TData>(default(TData), false, ex.Message);
            }
        }

    }
}
