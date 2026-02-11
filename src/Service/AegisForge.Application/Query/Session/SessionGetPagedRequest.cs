using AegisForge.Application.Dto;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork.Entityes;
using TelecomService.QRON.ID.Identity.Domain.Enum;

namespace AegisForge.Application.Query.Session;

public record SessionGetPagedRequest(Guid UserId, int PageIndex = 0, int PageSize = 20, SessionStatusType? SessionStatusType = null) : IRequest<ApiResult<IPagedList<SessionViewModel>>>;