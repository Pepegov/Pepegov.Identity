using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class CreateApplicationCommandConsumer(IMediator mediator) : IConsumer<CreateApplicationCommand>
{
    public async Task Consume(ConsumeContext<CreateApplicationCommand> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}