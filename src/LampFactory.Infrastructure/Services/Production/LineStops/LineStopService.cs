using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Production.LineStops;
using LampFactory.Application.Abstractions.Persistence.Production.LineStops;
using LampFactory.Domain.Production;

namespace LampFactory.Infrastructure.Services.Production.LineStops
{
    public class LineStopService : ILineStopService
    {
        private readonly ILineStopRepository _lineStopRepository;
        private readonly IValidator<LineStopDTO> _validator;

        public LineStopService(ILineStopRepository lineStopRepository, IValidator<LineStopDTO> validator)
        {
            _lineStopRepository = lineStopRepository;
            _validator = validator;
        }

        public async Task<Result<List<LineStop>>> GetAllAsync()
        {

            var stops = await _lineStopRepository.GetAllAsync();
            return Result<List<LineStop>>.Success(stops);
        }

        public async Task<Result<LineStop>> GetByIdAsync(int id)
        {

            if (id <= 0)
                return Result<LineStop>.BadRequest("Invalid LineStop ID.");

            var stop = await _lineStopRepository.GetByIdAsync(id);
            if (stop == null)
                return Result<LineStop>.NotFound("LineStop not found.");

            return Result<LineStop>.Success(stop);
        }

        public async Task<Result<LineStop>> CreateAsync(LineStopDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<LineStop>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newLineStopId = await _lineStopRepository.CreateAsync(dto.LineStop);
            if (newLineStopId <= 0)
                return Result<LineStop>.BadRequest("Failed to record LineStop.");

            var created = await _lineStopRepository.GetByIdAsync(newLineStopId);
            if (created == null)
                return Result<LineStop>.NotFound("LineStop could not be loaded after creation.");

            return Result<LineStop>.Success(created);
        }

        public async Task<Result<LineStop>> UpdateAsync(int id, LineStopDTO dto)
        {

            if (dto == null || dto.LineStop == null || id != dto.LineStop.LineStopID)
                return Result<LineStop>.BadRequest("ID mismatch or invalid data.");

            var existing = await _lineStopRepository.GetByIdAsync(id);
            if (existing == null)
                return Result<LineStop>.NotFound("LineStop record not found for update.");

            var success = await _lineStopRepository.UpdateAsync(dto.LineStop);
            if (!success)
                return Result<LineStop>.BadRequest("Failed to update LineStop.");

            var updated = await _lineStopRepository.GetByIdAsync(id);
            if (updated == null)
                return Result<LineStop>.NotFound("LineStop could not be loaded after update.");

            return Result<LineStop>.Success(updated);
        }

        public async Task<Result> DeleteAsync(int id)
        {

            var existing = await _lineStopRepository.GetByIdAsync(id);
            if (existing == null)
                return Result.NotFound("LineStop record not found for deletion.");

            var success = await _lineStopRepository.RemoveAsync(id);
            if (!success)
                return Result.BadRequest("Failed to delete LineStop.");

            return Result.Success();
        }
    }
}