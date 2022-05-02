using MediatR;
using WebApiPlugin.Features;

namespace WebApiPlugin.Common.Features.Verse
{
    public record GetCurrentVerseCommand() : IRequest<QueryResult<string>>;
}
