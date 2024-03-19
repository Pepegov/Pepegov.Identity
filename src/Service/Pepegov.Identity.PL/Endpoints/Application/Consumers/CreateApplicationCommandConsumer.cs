using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class CreateApplicationCommandConsumer : IConsumer<CreateApplicationCommand>
{
    private readonly IMediator _mediator;

    public CreateApplicationCommandConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<CreateApplicationCommand> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}