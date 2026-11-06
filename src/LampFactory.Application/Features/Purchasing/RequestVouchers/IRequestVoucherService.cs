using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Purchasing.RequestVouchers;

namespace LampFactory.Application.Features.Purchasing.RequestVouchers;

public interface IRequestVoucherService
{
    Task<Result<List<object>>> GetAllRequestVouchersAsync();
    Task<Result<object>> GetRequestVoucherByIdAsync(int id);
    Task<Result<object>> CreateRequestVoucherAsync(RequestVoucherProductDTO dto);
    Task<Result<RequestVoucherProductDTO>> UpdateRequestVoucherAsync(int id, RequestVoucherProductDTO dto);
    Task<Result> DeleteRequestVoucherAsync(int id);
}