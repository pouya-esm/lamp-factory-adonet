using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Sales;

namespace LampFactory.Application.Features.Sales;

public interface ISalesVoucherService
{
    Task<Result<List<object>>> GetAllAsync();
    Task<Result<object>> GetByIdAsync(int id);
    Task<Result<object>> CreateAsync(SalesVoucherProductDTO dto);
    Task<Result<SalesVoucherProductDTO>> UpdateAsync(int id, SalesVoucherProductDTO dto);
    Task<Result> DeleteAsync(int id);
}