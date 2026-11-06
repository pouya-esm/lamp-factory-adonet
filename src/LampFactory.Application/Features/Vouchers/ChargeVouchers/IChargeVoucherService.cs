using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Vouchers.ChargeVouchers;

namespace LampFactory.Application.Features.Vouchers.ChargeVouchers;

public interface IChargeVoucherService
{
    Task<Result<List<object>>> GetAllAsync();
    Task<Result<object>> GetByIdAsync(int id);
    Task<Result<object>> CreateAsync(ChargeVoucherProductDTO dto);
    Task<Result<ChargeVoucherProductDTO>> UpdateAsync(int id, ChargeVoucherProductDTO dto);
    Task<Result> DeleteAsync(int id);
}