using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class UpdateApplicationDataCommandConsumer : IConsumer<UpdateApplicationDataCommand>
{
    private IMediator _mediator;

    public UpdateApplicationDataCommandConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UpdateApplicationDataCommand> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}