using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.BillOfMaterials;
using LampFactory.Application.Abstractions.Persistence.Production.BillOfMaterials;

namespace LampFactory.Infrastructure.Services.Production.BillOfMaterials
{
    // Application workflow for bills of materials. Expected failures come back
    // as typed Result errors (validation, missing resource, conflicts);
    // unexpected persistence failures are left for the API exception handler.
    public class BOMService : IBOMService
    {
        private readonly IBOMRepository _bomRepository;
        private readonly IValidator<BOMDTO> _validator;

        public BOMService(IBOMRepository bomRepository, IValidator<BOMDTO> validator)
        {
            _bomRepository = bomRepository;
            _validator = validator;
        }

        public async Task<Result<IReadOnlyList<BOMWithComponentsDTO>>> GetAllBOMsAsync()
        {
            var boms = await _bomRepository.GetAllWithComponentsAsync();

            var data = boms.Select(b => MapToWithComponents(b.Bom, b.Components)).ToList();

            return Result<IReadOnlyList<BOMWithComponentsDTO>>.Success(data);
        }

        public async Task<Result<BOMWithComponentsDTO>> GetBOMByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Result<BOMWithComponentsDTO>.BadRequest("Invalid BOM ID.");
            }

            var (bom, components) = await _bomRepository.GetByIdWithComponentsAsync(id);
            if (bom == null)
            {
                return Result<BOMWithComponentsDTO>.NotFound("BOM not found.");
            }

            return Result<BOMWithComponentsDTO>.Success(MapToWithComponents(bom, components));
        }

        public async Task<Result<BOMWithComponentsDTO>> CreateBOMAsync(BOMDTO dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<BOMWithComponentsDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newBomId = await _bomRepository.CreateWithComponentsAsync(dto.BOM, dto.Components);
            if (newBomId <= 0)
            {
                return Result<BOMWithComponentsDTO>.BadRequest("Failed to create BOM.");
            }

            // Re-read the committed aggregate so the response carries the
            // database-written columns instead of the request payload.
            var (created, createdComponents) = await _bomRepository.GetByIdWithComponentsAsync(newBomId);
            if (created == null)
            {
                return Result<BOMWithComponentsDTO>.NotFound("BOM could not be loaded after creation.");
            }

            return Result<BOMWithComponentsDTO>.Success(MapToWithComponents(created, createdComponents));
        }

        public async Task<Result<BOMWithComponentsDTO>> UpdateBOMAsync(int id, BOMDTO dto)
        {
            // Route id is shared with the validator, which also rejects a DTO
            // whose BOM_ID disagrees with it ("BOM ID mismatch").
            var context = new ValidationContext<BOMDTO>(dto);
            context.RootContextData["RouteId"] = id;

            var validationResult = await _validator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                return Result<BOMWithComponentsDTO>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var (existingBOM, _) = await _bomRepository.GetByIdWithComponentsAsync(id);
            if (existingBOM == null)
            {
                return Result<BOMWithComponentsDTO>.NotFound("BOM not found for update.");
            }

            var success = await _bomRepository.UpdateWithComponentsAsync(dto.BOM, dto.Components);
            if (!success)
            {
                return Result<BOMWithComponentsDTO>.BadRequest("Failed to update BOM.");
            }

            // Return the persisted state (with ModifiedAt/ModifiedBy stamped by
            // the database) rather than the request DTO.
            var (updated, updatedComponents) = await _bomRepository.GetByIdWithComponentsAsync(id);
            if (updated == null)
            {
                return Result<BOMWithComponentsDTO>.NotFound("BOM could not be loaded after update.");
            }

            return Result<BOMWithComponentsDTO>.Success(MapToWithComponents(updated, updatedComponents));
        }

        public async Task<Result> DeleteBOMAsync(int id)
        {
            if (id <= 0)
            {
                return Result.BadRequest("Invalid BOM ID.");
            }

            var (existingBOM, _) = await _bomRepository.GetByIdWithComponentsAsync(id);
            if (existingBOM == null)
            {
                return Result.NotFound("BOM not found for deletion.");
            }

            var success = await _bomRepository.RemoveWithComponentsAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete BOM.");
            }

            return Result.Success();
        }

        private static BOMWithComponentsDTO MapToWithComponents(BOM bom, List<BOMComponent> components)
        {
            return new BOMWithComponentsDTO
            {
                BOM = new BOMResponseDTO
                {
                    BomId = bom.BOM_ID,
                    ProductID = bom.ProductID,
                    CreatedAt = bom.CreatedAt,
                    CreatedBy = bom.CreatedBy,
                    ModifiedAt = bom.ModifiedAt,
                    ModifiedBy = bom.ModifiedBy
                },
                Components = components
                    .Select(c => new BOMComponentResponseDTO
                    {
                        BomId = c.BOM_ID,
                        ProductID = c.ProductID,
                        ConsumptionAmount = c.ConsumptionAmount
                    })
                    .ToList()
            };
        }
    }
}
