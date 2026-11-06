using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.IdentityAccess.SysActions;
using LampFactory.Application.Abstractions.Persistence.IdentityAccess.SysActions;
using LampFactory.Domain.IdentityAccess;

namespace LampFactory.Infrastructure.Services.IdentityAccess.SysActions
{
    public class SysActionService : ISysActionService
    {
        private readonly ISysActionRepository _actionRepository;
        private readonly IValidator<SysActionDTO> _validator;

        public SysActionService(ISysActionRepository actionRepository, IValidator<SysActionDTO> validator)
        {
            _actionRepository = actionRepository;
            _validator = validator;
        }

        public async Task<Result<List<SysAction>>> GetAllActionsAsync()
        {

            var actions = await _actionRepository.GetAllAsync();
            return Result<List<SysAction>>.Success(actions);
        }

        public async Task<Result<SysAction>> GetActionByIdAsync(int id)
        {

            if (id <= 0) return Result<SysAction>.BadRequest("Invalid ID.");

            var action = await _actionRepository.GetByIdAsync(id);
            if (action == null) return Result<SysAction>.NotFound("Action not found.");

            return Result<SysAction>.Success(action);
        }

        public async Task<Result<SysAction>> CreateActionAsync(SysActionDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<SysAction>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newActionId = await _actionRepository.CreateAsync(dto.SysAction);
            if (newActionId <= 0) return Result<SysAction>.BadRequest("Failed to create action.");

            var created = await _actionRepository.GetByIdAsync(newActionId);
            if (created == null) return Result<SysAction>.NotFound("Action could not be loaded after creation.");

            return Result<SysAction>.Success(created);
        }

        public async Task<Result<SysAction>> UpdateActionAsync(int id, SysActionDTO dto)
        {

            if (dto == null || dto.SysAction == null || id != dto.SysAction.ActionID)
                return Result<SysAction>.BadRequest("ID mismatch.");

            var existing = await _actionRepository.GetByIdAsync(id);
            if (existing == null) return Result<SysAction>.NotFound("Action not found.");

            var success = await _actionRepository.UpdateAsync(dto.SysAction);
            if (!success) return Result<SysAction>.BadRequest("Failed to update action.");

            var updated = await _actionRepository.GetByIdAsync(id);
            if (updated == null) return Result<SysAction>.NotFound("Action could not be loaded after update.");

            return Result<SysAction>.Success(updated);
        }

        public async Task<Result> DeleteActionAsync(int id)
        {

            var existing = await _actionRepository.GetByIdAsync(id);
            if (existing == null) return Result.NotFound("Action not found.");

            var success = await _actionRepository.RemoveAsync(id);
            if (!success) return Result.BadRequest("Failed to delete action.");

            return Result.Success();
        }
    }
}