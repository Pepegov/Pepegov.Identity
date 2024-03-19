using MediatR;
using Pepegov.Identity.PL.Endpoints.Application.ViewModel;
using Pepegov.MicroserviceFramework.ApiResults;

namespace Pepegov.Identity.PL.Endpoints.Application.Queries;

public class UpdateApplicationDataCommand : IRequest<ApiResult>
{
    public string TargetClientId { get; set; } = null!;
    public ApplicationDataUpdateModel UpdateModel { get; set; } = null!;

    public UpdateApplicationDataCommand(string targetClientId, ApplicationDataUpdateModel updateModel)
    {
        TargetClientId = targetClientId;
        UpdateModel = updateModel;
    }
}