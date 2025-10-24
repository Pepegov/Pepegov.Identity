using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Account.Queries;

namespace Pepegov.Identity.PL.Endpoints.Account.Consumers;

public class GetAccountByIdConsumer(IMediator mediator) : IConsumer<GetAccountByIdRequest>
{
    public async Task Consume(ConsumeContext<GetAccountByIdRequest> context)
    {
        var result = await mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}