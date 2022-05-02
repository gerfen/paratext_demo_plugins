using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;

namespace WebApiPlugin.Features.Verse
{
    public record GetCurrentVerseCommand() : IRequest<QueryResult<string>>;

    public class GetCurrentVerseCommandHandler : IRequestHandler<GetCurrentVerseCommand, QueryResult<string>>
    {
        private readonly IVerseRef _verseRef;
        private readonly ILogger<GetCurrentVerseCommandHandler> _logger;

        public GetCurrentVerseCommandHandler(IVerseRef verseRef, ILogger<GetCurrentVerseCommandHandler> logger)
        {
            _verseRef = verseRef;
            _logger = logger;
        }
        public Task<QueryResult<string>> Handle(GetCurrentVerseCommand request, CancellationToken cancellationToken)
        {
            var queryResult = new QueryResult<string>(string.Empty);
            try
            {
                var verseId = _verseRef.BBBCCCVVV.ToString();
                if (verseId.Length < 8)
                {
                    verseId = verseId.PadLeft(8, '0');
                }
                queryResult.Data = verseId;
               
            }
            catch (Exception ex)
            {
                queryResult.Success = false;
                queryResult.Message = ex.Message;
            }
            return Task.FromResult(queryResult);
        }
    }
}
