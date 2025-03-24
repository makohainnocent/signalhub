using Domain.ProductOwnershipTransfers.Models;
using Domain.ProductOwnershipTransfers.Requests;
using Application.ProductOwnershipTransfers.Abstractions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Security.Claims;
using DataAccess.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.ProductOwnershipTransfers.Controllers
{
    public static class ProductOwnershipTransfersControllers
    {
        // Create a new product ownership transfer
        public static async Task<IResult> CreateTransferAsync(
            IProductOwnershipTransfersRepository repo,
            [FromBody] TransferCreationRequest request,
            HttpContext httpContext)
        {
            try
            {
                // Ensure the user is authenticated
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Results.Json(new { message = "You must be logged in to perform this action" }, statusCode: StatusCodes.Status401Unauthorized);
                }

                Log.Information("Attempting to create transfer for product: {ProductName}", request.ProductName);

                // Create the transfer
                var createdTransfer = await repo.CreateTransferAsync(request);

                Log.Information("Transfer created successfully with ID: {TransferId}", createdTransfer.TransferId);
                return Results.Created($"/transfers/{createdTransfer.TransferId}", createdTransfer);
            }
            catch (ItemAlreadyExistsException ex)
            {
                Log.Warning(ex, "Transfer creation failed - transfer already exists.");
                return Results.Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating the transfer.");
                return Results.Problem(ex.Message);
            }
        }

        // Get all product ownership transfers with pagination
        public static async Task<IResult> GetTransfersAsync(
            IProductOwnershipTransfersRepository repo,
            int pageNumber = 1,
            int pageSize = 10,
            int? productId=null,
            int? premiseId=null,
            string? search = null)
        {
            try
            {
                Log.Information("Attempting to retrieve transfers with pagination: Page {PageNumber}, PageSize {PageSize}.", pageNumber, pageSize);

                // Retrieve paginated transfers
                var pagedResult = await repo.GetTransfersAsync(pageNumber, pageSize, productId, premiseId, search);

                if (pagedResult == null || !pagedResult.Items.Any())
                {
                    return Results.NotFound(new { message = "No transfers found." });
                }

                Log.Information("Successfully retrieved {TransferCount} transfers out of {TotalCount}.", pagedResult.Items.Count(), pagedResult.TotalCount);
                return Results.Ok(pagedResult);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving transfers.");
                return Results.Problem(ex.Message);
            }
        }

        // Get a product ownership transfer by ID
        public static async Task<IResult> GetTransferByIdAsync(
            IProductOwnershipTransfersRepository repo,
            int transferId)
        {
            try
            {
                Log.Information("Attempting to retrieve transfer with ID: {TransferId}", transferId);

                // Retrieve the transfer by ID
                var transfer = await repo.GetTransferByIdAsync(transferId);

                if (transfer == null)
                {
                    Log.Warning("Transfer with ID: {TransferId} not found.", transferId);
                    return Results.NotFound(new { message = "Transfer not found." });
                }

                Log.Information("Successfully retrieved transfer with ID: {TransferId}", transferId);
                return Results.Ok(transfer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving the transfer.");
                return Results.Problem(ex.Message);
            }
        }

        // Update a product ownership transfer
        public static async Task<IResult> UpdateTransferAsync(
            IProductOwnershipTransfersRepository repo,
            [FromBody] TransferUpdateRequest request)
        {
            try
            {
                Log.Information("Attempting to update transfer with ID: {TransferId}", request.TransferId);

                // Update the transfer
                var updatedTransfer = await repo.UpdateTransferAsync(request);

                Log.Information("Transfer updated successfully with ID: {TransferId}", updatedTransfer.TransferId);
                return Results.Ok(updatedTransfer);
            }
            catch (ItemDoesNotExistException ex)
            {
                Log.Warning(ex, "Transfer update failed - transfer does not exist.");
                return Results.NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while updating the transfer.");
                return Results.Problem(ex.Message);
            }
        }

        // Delete a product ownership transfer
        public static async Task<IResult> DeleteTransferAsync(
            IProductOwnershipTransfersRepository repo,
            int transferId)
        {
            try
            {
                Log.Information("Attempting to delete transfer with ID: {TransferId}", transferId);

                // Delete the transfer
                var deleted = await repo.DeleteTransferAsync(transferId);

                if (deleted)
                {
                    Log.Information("Transfer deleted successfully with ID: {TransferId}", transferId);
                    return Results.Ok(new { message = "Transfer deleted successfully." });
                }
                else
                {
                    Log.Warning("Transfer deletion failed - transfer does not exist.");
                    return Results.NotFound(new { message = "Transfer not found." });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting the transfer.");
                return Results.Problem(ex.Message);
            }
        }

        // Count all product ownership transfers
        public static async Task<IResult> CountTransfersAsync(
            IProductOwnershipTransfersRepository repo)
        {
            try
            {
                Log.Information("Attempting to count all transfers.");

                // Count the transfers
                var count = await repo.CountTransfersAsync();

                Log.Information("Successfully counted {TransferCount} transfers.", count);
                return Results.Ok(new { count });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while counting transfers.");
                return Results.Problem(ex.Message);
            }
        }
    }
}