using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.QualityControl;
using LampFactory.Application.Abstractions.Persistence.QualityControl;
using QualityControlEntity = LampFactory.Domain.QualityControl.QualityControl;

namespace LampFactory.Infrastructure.Services.QualityControl
{
    public class QualityControlService : IQualityControlService
    {
        private readonly IQualityControlRepository _qcRepository;
        private readonly IValidator<QualityControlDTO> _validator;

        public QualityControlService(IQualityControlRepository qcRepository, IValidator<QualityControlDTO> validator)
        {
            _qcRepository = qcRepository;
            _validator = validator;
        }

        public async Task<Result<List<QualityControlEntity>>> GetAllAsync()
        {

            var list = await _qcRepository.GetAllAsync();
            return Result<List<QualityControlEntity>>.Success(list);
        }

        public async Task<Result<QualityControlEntity>> GetByIdAsync(int id)
        {

            if (id <= 0)
                return Result<QualityControlEntity>.BadRequest("Invalid QC ID.");

            var qc = await _qcRepository.GetByIdAsync(id);
            if (qc == null)
                return Result<QualityControlEntity>.NotFound("Quality Control record not found.");

            return Result<QualityControlEntity>.Success(qc);
        }

        public async Task<Result<QualityControlEntity>> CreateAsync(QualityControlDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<QualityControlEntity>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newQcId = await _qcRepository.CreateAsync(dto.QualityControl);
            if (newQcId <= 0)
                return Result<QualityControlEntity>.BadRequest("Failed to create QC record.");

            var created = await _qcRepository.GetByIdAsync(newQcId);
            if (created == null)
                return Result<QualityControlEntity>.NotFound("QC record could not be loaded after creation.");

            return Result<QualityControlEntity>.Success(created);
        }

        public async Task<Result<QualityControlEntity>> UpdateAsync(int id, QualityControlDTO dto)
        {

            if (dto == null || dto.QualityControl == null || id != dto.QualityControl.QualityControlID)
                return Result<QualityControlEntity>.BadRequest("ID mismatch or invalid data.");

            var existing = await _qcRepository.GetByIdAsync(id);
            if (existing == null)
                return Result<QualityControlEntity>.NotFound("QC record not found for update.");

            var success = await _qcRepository.UpdateAsync(dto.QualityControl);
            if (!success)
                return Result<QualityControlEntity>.BadRequest("Failed to update QC record.");

            var updated = await _qcRepository.GetByIdAsync(id);
            if (updated == null)
                return Result<QualityControlEntity>.NotFound("QC record could not be loaded after update.");

            return Result<QualityControlEntity>.Success(updated);
        }

        public async Task<Result> DeleteAsync(int id)
        {

            var existing = await _qcRepository.GetByIdAsync(id);
            if (existing == null)
                return Result.NotFound("QC record not found for deletion.");

            var success = await _qcRepository.RemoveAsync(id);
            if (!success)
                return Result.BadRequest("Failed to delete QC record.");

            return Result.Success();
        }
    }
}