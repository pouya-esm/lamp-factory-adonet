using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.QualityControl;
using QualityControlEntity = LampFactory.Domain.QualityControl.QualityControl;

namespace LampFactory.Application.Features.QualityControl;

public interface IQualityControlService
{
    Task<Result<List<QualityControlEntity>>> GetAllAsync();
    Task<Result<QualityControlEntity>> GetByIdAsync(int id);
    Task<Result<QualityControlEntity>> CreateAsync(QualityControlDTO dto);
    Task<Result<QualityControlEntity>> UpdateAsync(int id, QualityControlDTO dto);
    Task<Result> DeleteAsync(int id);
}