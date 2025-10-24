using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class DeleteApplicationCommandConsumer(IMediator mediator) : IConsumer<DeleteApplicationCommand>
{
    public  async Task Consume(ConsumeContext<DeleteApplicationCommand> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}