using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.Operators;
using LampFactory.Application.Abstractions.Persistence.Production.Operators;
using LampFactory.Domain.Operators;

namespace LampFactory.Infrastructure.Services.Production.Operators
{
    public class OperatorService : IOperatorService
    {
        private readonly IOperatorRepository _operatorRepository;
        private readonly IValidator<OperatorDTO> _validator;

        public OperatorService(IOperatorRepository operatorRepository, IValidator<OperatorDTO> validator)
        {
            _operatorRepository = operatorRepository;
            _validator = validator;
        }

        public async Task<Result<List<Operator>>> GetAllOperatorsAsync()
        {

            var operators = await _operatorRepository.GetAllAsync();
            return Result<List<Operator>>.Success(operators);
        }

        public async Task<Result<Operator>> GetOperatorByIdAsync(int id)
        {

            if (id <= 0)
            {
                return Result<Operator>.BadRequest("Invalid Operator ID.");
            }

            var operatorEntity = await _operatorRepository.GetByIdAsync(id);
            if (operatorEntity == null)
            {
                return Result<Operator>.NotFound("Operator not found.");
            }

            return Result<Operator>.Success(operatorEntity);
        }

        public async Task<Result<Operator>> CreateOperatorAsync(OperatorDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<Operator>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newOperatorId = await _operatorRepository.CreateAsync(dto.Operator);
            if (newOperatorId <= 0)
            {
                return Result<Operator>.BadRequest("Failed to create operator.");
            }

            var created = await _operatorRepository.GetByIdAsync(newOperatorId);
            if (created == null)
            {
                return Result<Operator>.NotFound("Operator could not be loaded after creation.");
            }

            return Result<Operator>.Success(created);
        }

        public async Task<Result<Operator>> UpdateOperatorAsync(int id, OperatorDTO dto)
        {

            if (dto == null || dto.Operator == null || id != dto.Operator.OperatorID)
            {
                return Result<Operator>.BadRequest("Operator ID mismatch or invalid data.");
            }

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<Operator>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var existingOperator = await _operatorRepository.GetByIdAsync(id);
            if (existingOperator == null)
            {
                return Result<Operator>.NotFound("Operator not found for update.");
            }

            var success = await _operatorRepository.UpdateAsync(dto.Operator);
            if (!success)
            {
                return Result<Operator>.BadRequest("Failed to update operator.");
            }

            var updated = await _operatorRepository.GetByIdAsync(id);
            if (updated == null)
            {
                return Result<Operator>.NotFound("Operator could not be loaded after update.");
            }

            return Result<Operator>.Success(updated);
        }

        public async Task<Result> DeleteOperatorAsync(int id)
        {

            var existingOperator = await _operatorRepository.GetByIdAsync(id);
            if (existingOperator == null)
            {
                return Result.NotFound("Operator not found for deletion.");
            }

            var success = await _operatorRepository.RemoveAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete operator.");
            }

            return Result.Success();
        }
    }
}