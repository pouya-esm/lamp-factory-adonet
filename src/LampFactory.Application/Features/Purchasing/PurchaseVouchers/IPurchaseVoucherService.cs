using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Purchasing.PurchaseVouchers;

namespace LampFactory.Application.Features.Purchasing.PurchaseVouchers;

public interface IPurchaseVoucherService
{
    Task<Result<List<object>>> GetAllAsync();
    Task<Result<object>> GetByIdAsync(int id);
    Task<Result<object>> CreateAsync(PurchaseVoucherProductDTO dto);
    Task<Result<PurchaseVoucherProductDTO>> UpdateAsync(int id, PurchaseVoucherProductDTO dto);
    Task<Result> DeleteAsync(int id);
}