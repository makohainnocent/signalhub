using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.Approvals.Abstractions;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Domain.Approvals.Requests;
using Domain.Core.Models;
using Api.Approvals.ApprovalsControllers;
using Microsoft.AspNetCore.Mvc;
namespace Api.Approvals.ApprovalsEnPointDefinations
{
    public class ApprovalsEndpoint : IEndpointDefinition
    {
        public void RegisterEndpoints(WebApplication app)
        {
            ApiVersionSet apiVersionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1))
                .ReportApiVersions()
                .Build();

            RouteGroupBuilder versionedGroup = app
                .MapGroup("/api/v{apiVersion:apiVersion}")
                .WithApiVersionSet(apiVersionSet);

            var approvals = versionedGroup.MapGroup("/approvals");

            approvals.MapPost("/", async (IApprovalsRepository repo, [FromBody] CreateApprovalRequest request, HttpContext httpContext) =>
            {
                return await ApprovalsControllers.ApprovalsControllers.CreateApprovalAsync(repo, request, httpContext);
            })
            .RequireAuthorization()
            .AddEndpointFilter<ValidationFilter<CreateApprovalRequest>>()
            .WithTags("Approvals");

            approvals.MapGet("/", async (IApprovalsRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null) =>
            {
                return await ApprovalsControllers.ApprovalsControllers.GetAllApprovalsAsync(repo, pageNumber, pageSize, search);
            })
            .RequireAuthorization()
            .WithTags("Approvals");

            approvals.MapGet("/{approvalId}", async (IApprovalsRepository repo, int approvalId) =>
            {
                return await ApprovalsControllers.ApprovalsControllers.GetApprovalByIdAsync(repo, approvalId);
            })
            .RequireAuthorization()
            .WithTags("Approvals");

            approvals.MapPut("/{approvalId}", async (IApprovalsRepository repo, int approvalId, [FromBody] Approval approval) =>
            {
                return await ApprovalsControllers.ApprovalsControllers.UpdateApprovalAsync(repo, approvalId, approval);
            })
            .RequireAuthorization()
            .WithTags("Approvals");

            approvals.MapDelete("/{approvalId}", async (IApprovalsRepository repo, int approvalId) =>
            {
                return await ApprovalsControllers.ApprovalsControllers.DeleteApprovalAsync(repo, approvalId);
            })
            .RequireAuthorization()
            .WithTags("Approvals");
        }
    }
}
