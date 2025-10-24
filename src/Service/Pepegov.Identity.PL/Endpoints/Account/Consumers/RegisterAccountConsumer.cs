using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Account.Queries;

namespace Pepegov.Identity.PL.Endpoints.Account.Consumers;

public class RegisterAccountConsumer(IMediator mediator) : IConsumer<RegisterAccountRequest>
{
    public async Task Consume(ConsumeContext<RegisterAccountRequest> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}