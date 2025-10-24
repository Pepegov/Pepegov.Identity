using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class UpdateApplicationPermissionsCommandConsumer(IMediator mediator) : IConsumer<UpdateApplicationPermissionsCommand>
{
    public async Task Consume(ConsumeContext<UpdateApplicationPermissionsCommand> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}