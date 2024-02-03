using MassTransit;
using MediatR;
using Pepegov.Identity.PL.Endpoints.Account.Queries;

namespace Pepegov.Identity.PL.Endpoints.Account.Consumers;

public class GetAccountByIdConsumer : IConsumer<GetAccountByIdRequest>
{
    private readonly IMediator _mediator;
    
    public GetAccountByIdConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<GetAccountByIdRequest> context)
    {
        var result = await _mediator.Send(context.Message, context.CancellationToken);
        await context.RespondAsync(result);
    }
}