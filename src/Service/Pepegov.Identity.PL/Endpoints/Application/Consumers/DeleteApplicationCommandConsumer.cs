using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class DeleteApplicationCommandConsumer : IConsumer<DeleteApplicationCommand>
{
    private readonly IMediator _mediator;

    public DeleteApplicationCommandConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public  async Task Consume(ConsumeContext<DeleteApplicationCommand> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}