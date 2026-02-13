using System.Linq.Expressions;
using System.Net;
using AegisForge.Application.Dto;
using AegisForge.Application.Query.Session;
using AegisForge.Domain.Aggregate;
using AegisForge.Infrastructure.Extension;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pepegov.MicroserviceFramework.ApiResults;
using Pepegov.UnitOfWork;
using Pepegov.UnitOfWork.Entityes;
using Pepegov.UnitOfWork.EntityFramework;

namespace AegisForge.Application.Handler.Session;

public class SessionGetPagedRequestHandler(IUnitOfWorkManager unitOfWorkManager, IMapper mapper) : IRequestHandler<SessionGetPagedRequest, ApiResult<IPagedList<SessionViewModel>>>
{
    public async Task<ApiResult<IPagedList<SessionViewModel>>> Handle(SessionGetPagedRequest request, CancellationToken cancellationToken)
    {
        var sessionRepository = unitOfWorkManager.GetInstance<IUnitOfWorkEntityFrameworkInstance>().GetRepository<ApplicationSession>();

        Expression<Func<ApplicationSession, bool>> predicate = x => x.ApplicationUserId == request.UserId;
        if (request.SessionStatusType != null)
        {
            predicate.AndAlso(x => x.SessionStatusType == request.SessionStatusType);
        }
        
        var paged = await sessionRepository.GetPagedListAsync(
            predicate: predicate,
            orderBy: sessions => sessions.OrderByDescending(o => o.CreatedAt), 
            include: x => x
                .Include(i => i.UserConnectionInfo).ThenInclude(ti => ti.UserAgent)
                .Include(i => i.UserConnectionInfo).ThenInclude(ti => ti.GeoData)!,
            cancellationToken: cancellationToken);
        
        var result = mapper.Map<IPagedList<SessionViewModel>>(paged);
        return new ApiResult<IPagedList<SessionViewModel>>(result, HttpStatusCode.OK);
    }
}