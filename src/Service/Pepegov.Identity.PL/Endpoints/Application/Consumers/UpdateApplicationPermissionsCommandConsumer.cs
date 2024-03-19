using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class UpdateApplicationPermissionsCommandConsumer : IConsumer<UpdateApplicationPermissionsCommand>
{
    private readonly IMediator _mediator;

    public UpdateApplicationPermissionsCommandConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UpdateApplicationPermissionsCommand> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}