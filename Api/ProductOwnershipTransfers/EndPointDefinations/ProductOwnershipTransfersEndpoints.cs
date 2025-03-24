// Api/ProductOwnershipTransfers/EndPointDefinations/ProductOwnershipTransfersEndpoints.cs
using Api.Common.Abstractions;
using Api.Common.Filters;
using Application.ProductOwnershipTransfers.Abstractions;
using Domain.ProductOwnershipTransfers.Requests;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning.Builder;
using Asp.Versioning;
using Api.ProductOwnershipTransfers.Controllers;

namespace Api.ProductOwnershipTransfers.EndPointDefinations
{
    public class ProductOwnershipTransfersEndpoints : IEndpointDefinition
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

            var transfers = versionedGroup.MapGroup("/transfers")
                .WithTags("Product Ownership Transfers");

            // Create a new transfer
            transfers.MapPost("/", async (IProductOwnershipTransfersRepository repo, [FromBody] TransferCreationRequest request, HttpContext httpContext) =>
            {
                return await ProductOwnershipTransfersControllers.CreateTransferAsync(repo, request, httpContext);
            })
            .RequireAuthorization();

            // Get all transfers (paginated)
            transfers.MapGet("/", async (IProductOwnershipTransfersRepository repo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int ? productId = null, [FromQuery] int ? premiseId = null,[FromQuery] string? search = null) =>
            {
                return await ProductOwnershipTransfersControllers.GetTransfersAsync(repo, pageNumber, pageSize,  productId, premiseId, search);
            });

            // Get a transfer by ID
            transfers.MapGet("/{transferId:int}", async (IProductOwnershipTransfersRepository repo, int transferId) =>
            {
                return await ProductOwnershipTransfersControllers.GetTransferByIdAsync(repo, transferId);
            });

            // Update a transfer
            transfers.MapPut("/", async (IProductOwnershipTransfersRepository repo, [FromBody] TransferUpdateRequest request) =>
            {
                return await ProductOwnershipTransfersControllers.UpdateTransferAsync(repo, request);
            })
            .RequireAuthorization();

            // Delete a transfer
            transfers.MapDelete("/{transferId:int}", async (IProductOwnershipTransfersRepository repo, int transferId) =>
            {
                return await ProductOwnershipTransfersControllers.DeleteTransferAsync(repo, transferId);
            })
            .RequireAuthorization();

            // Count all transfers
            transfers.MapGet("/count", async (IProductOwnershipTransfersRepository repo) =>
            {
                return await ProductOwnershipTransfersControllers.CountTransfersAsync(repo);
            });
        }
    }
}