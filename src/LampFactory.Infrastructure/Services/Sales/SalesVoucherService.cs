using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Sales;
using LampFactory.Application.Abstractions.Persistence.Sales;

namespace LampFactory.Infrastructure.Services.Sales
{
    public class SalesVoucherService : ISalesVoucherService
    {
        private readonly ISalesVoucherRepository _salesVoucherRepository;
        private readonly IValidator<SalesVoucherProductDTO> _validator;

        public SalesVoucherService(ISalesVoucherRepository salesVoucherRepository, IValidator<SalesVoucherProductDTO> validator)
        {
            _salesVoucherRepository = salesVoucherRepository;
            _validator = validator;
        }

        public async Task<Result<List<object>>> GetAllAsync()
        {

            var vouchers = await _salesVoucherRepository.GetAllWithProductsAsync();
            var data = vouchers.Select(v => new { Voucher = v.Voucher, Products = v.Products }).ToList<object>();
            return Result<List<object>>.Success(data);
        }

        public async Task<Result<object>> GetByIdAsync(int id)
        {

            if (id <= 0)
                return Result<object>.BadRequest("Invalid SalesVoucher ID.");

            var resultData = await _salesVoucherRepository.GetByIdWithProductsAsync(id);
            if (resultData.Voucher == null)
                return Result<object>.NotFound("SalesVoucher not found.");

            return Result<object>.Success(new { Voucher = resultData.Voucher, Products = resultData.Products });
        }

        public async Task<Result<object>> CreateAsync(SalesVoucherProductDTO dto)
        {
            try
            {
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return Result<object>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
                }

                var newVoucherId = await _salesVoucherRepository.CreateWithProductsAsync(dto.Voucher, dto.Products);
                if (newVoucherId <= 0)
                    return Result<object>.BadRequest("Failed to create SalesVoucher.");

                var created = await _salesVoucherRepository.GetByIdWithProductsAsync(newVoucherId);
                return Result<object>.Success(new { Voucher = created.Voucher, Products = created.Products });
            }
            // Expected business-rule failure raised by the repository inside the
            // stock check; everything unexpected propagates to the API handler.
            catch (InsufficientStockException ise)
            {
                return Result<object>.Conflict(ise.Message);
            }
        }

        public async Task<Result<SalesVoucherProductDTO>> UpdateAsync(int id, SalesVoucherProductDTO dto)
        {
            try
            {
                if (dto == null || dto.Voucher == null || id != dto.Voucher.SalesVoucherID)
                    return Result<SalesVoucherProductDTO>.BadRequest("ID mismatch or invalid data.");

                var existing = await _salesVoucherRepository.GetByIdWithProductsAsync(id);
                if (existing.Voucher == null)
                    return Result<SalesVoucherProductDTO>.NotFound("SalesVoucher not found for update.");

                var success = await _salesVoucherRepository.UpdateWithProductsAsync(dto.Voucher, dto.Products);
                if (!success)
                    return Result<SalesVoucherProductDTO>.BadRequest("Failed to update SalesVoucher.");

                return Result<SalesVoucherProductDTO>.Success(dto);
            }
            catch (InsufficientStockException ise)
            {
                return Result<SalesVoucherProductDTO>.Conflict(ise.Message);
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {

            var existing = await _salesVoucherRepository.GetByIdWithProductsAsync(id);
            if (existing.Voucher == null)
                return Result.NotFound("SalesVoucher not found for deletion.");

            var success = await _salesVoucherRepository.RemoveWithProductsAsync(id);
            if (!success)
                return Result.BadRequest("Failed to delete SalesVoucher.");

            return Result.Success();
        }
    }
}