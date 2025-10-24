using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.Queries;

namespace Pepegov.Identity.PL.Endpoints.Application.Consumers;

public class UpdateApplicationDataCommandConsumer(IMediator mediator) : IConsumer<UpdateApplicationDataCommand>
{
    public async Task Consume(ConsumeContext<UpdateApplicationDataCommand> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}