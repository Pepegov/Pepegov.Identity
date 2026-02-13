using AegisForge.Application.Dto;
using AegisForge.Domain.Enum;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork.Entityes;

namespace AegisForge.Application.Query.Session;

public record SessionGetPagedRequest(Guid UserId, int PageIndex = 0, int PageSize = 20, SessionStatusType? SessionStatusType = null) : IRequest<ApiResult<IPagedList<SessionViewModel>>>;