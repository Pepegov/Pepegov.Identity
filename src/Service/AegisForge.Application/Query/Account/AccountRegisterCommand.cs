using AegisForge.Application.Dto;
using MediatR;
using Pepegov.MicroserviceFramework.ApiResults;

namespace AegisForge.Application.Query.Account;

/// <summary>
/// Request: Register new account
/// </summary>
public record AccountRegisterCommand(RegisterViewModel Model) : IRequest<ApiResult>;