using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Purchasing.RequestVouchers;
using LampFactory.Application.Abstractions.Persistence.Purchasing.RequestVouchers;

namespace LampFactory.Infrastructure.Services.Purchasing.RequestVouchers
{
    public class RequestVoucherService : IRequestVoucherService
    {
        private readonly IRequestVoucherRepository _requestVoucherRepository;
        private readonly IValidator<RequestVoucherProductDTO> _validator;

        public RequestVoucherService(IRequestVoucherRepository requestVoucherRepository, IValidator<RequestVoucherProductDTO> validator)
        {
            _requestVoucherRepository = requestVoucherRepository;
            _validator = validator;
        }

        public async Task<Result<List<object>>> GetAllRequestVouchersAsync()
        {

            var vouchers = await _requestVoucherRepository.GetAllWithProductsAsync();

            var data = vouchers.Select(v => new
            {
                Voucher = v.Voucher,
                Products = v.Products
            }).ToList<object>();

            return Result<List<object>>.Success(data);
        }

        public async Task<Result<object>> GetRequestVoucherByIdAsync(int id)
        {

            if (id <= 0)
            {
                return Result<object>.BadRequest("Invalid Request Voucher ID.");
            }

            var (voucher, products) = await _requestVoucherRepository.GetByIdWithProductsAsync(id);
            if (voucher == null)
            {
                return Result<object>.NotFound("Request Voucher not found.");
            }

            return Result<object>.Success(new { Voucher = voucher, Products = products });
        }

        public async Task<Result<object>> CreateRequestVoucherAsync(RequestVoucherProductDTO dto)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return Result<object>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var newVoucherId = await _requestVoucherRepository.CreateWithProductsAsync(dto.Voucher, dto.Products);
                if (newVoucherId <= 0)
                {
                    return Result<object>.BadRequest("Failed to create Request Voucher.");
                }

                var (voucher, products) = await _requestVoucherRepository.GetByIdWithProductsAsync(newVoucherId);
                return Result<object>.Success(new { Voucher = voucher, Products = products });
            }
            // Expected business-rule failure raised by the repository inside the
            // stock check; everything unexpected propagates to the API handler.
            catch (InsufficientStockException ise)
            {
                return Result<object>.Conflict(ise.Message);
            }
        }

        public async Task<Result<RequestVoucherProductDTO>> UpdateRequestVoucherAsync(int id, RequestVoucherProductDTO dto)
        {
            try
            {
                if (dto == null || dto.Voucher == null || id != dto.Voucher.RequestVoucherID)
                {
                    return Result<RequestVoucherProductDTO>.BadRequest("Voucher ID mismatch or invalid data.");
                }

                var (existingVoucher, _) = await _requestVoucherRepository.GetByIdWithProductsAsync(id);
                if (existingVoucher == null)
                {
                    return Result<RequestVoucherProductDTO>.NotFound("Request Voucher not found for update.");
                }

                var success = await _requestVoucherRepository.UpdateWithProductsAsync(dto.Voucher, dto.Products);
                if (!success)
                {
                    return Result<RequestVoucherProductDTO>.BadRequest("Failed to update Request Voucher.");
                }

                return Result<RequestVoucherProductDTO>.Success(dto);
            }
            catch (InsufficientStockException ise)
            {
                return Result<RequestVoucherProductDTO>.Conflict(ise.Message);
            }
        }

        public async Task<Result> DeleteRequestVoucherAsync(int id)
        {

            var (existingVoucher, _) = await _requestVoucherRepository.GetByIdWithProductsAsync(id);
            if (existingVoucher == null)
            {
                return Result.NotFound("Request Voucher not found for deletion.");
            }

            var success = await _requestVoucherRepository.RemoveWithProductsAsync(id);
            if (!success)
            {
                return Result.BadRequest("Failed to delete Request Voucher.");
            }

            return Result.Success();
        }
    }
}