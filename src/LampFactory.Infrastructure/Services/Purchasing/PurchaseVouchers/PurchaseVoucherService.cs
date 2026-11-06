using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Purchasing.PurchaseVouchers;
using LampFactory.Application.Abstractions.Persistence.Purchasing.PurchaseVouchers;

namespace LampFactory.Infrastructure.Services.Purchasing.PurchaseVouchers
{
    public class PurchaseVoucherService : IPurchaseVoucherService
    {
        private readonly IPurchaseVoucherRepository _purchaseVoucherRepository;
        private readonly IValidator<PurchaseVoucherProductDTO> _validator;

        public PurchaseVoucherService(IPurchaseVoucherRepository purchaseVoucherRepository, IValidator<PurchaseVoucherProductDTO> validator)
        {
            _purchaseVoucherRepository = purchaseVoucherRepository;
            _validator = validator;
        }

        public async Task<Result<List<object>>> GetAllAsync()
        {

            var vouchers = await _purchaseVoucherRepository.GetAllWithProductsAsync();
            var data = vouchers.Select(v => new { Voucher = v.Voucher, Products = v.Products }).ToList<object>();
            return Result<List<object>>.Success(data);
        }

        public async Task<Result<object>> GetByIdAsync(int id)
        {

            if (id <= 0)
                return Result<object>.BadRequest("Invalid PurchaseVoucher ID.");

            var resultData = await _purchaseVoucherRepository.GetByIdWithProductsAsync(id);
            if (resultData.Voucher == null)
                return Result<object>.NotFound("PurchaseVoucher not found.");

            return Result<object>.Success(new { Voucher = resultData.Voucher, Products = resultData.Products });
        }

        public async Task<Result<object>> CreateAsync(PurchaseVoucherProductDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<object>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newVoucherId = await _purchaseVoucherRepository.CreateWithProductsAsync(dto.Voucher, dto.Products);
            if (newVoucherId <= 0)
                return Result<object>.BadRequest("Failed to create PurchaseVoucher.");

            var created = await _purchaseVoucherRepository.GetByIdWithProductsAsync(newVoucherId);
            return Result<object>.Success(new { Voucher = created.Voucher, Products = created.Products });
        }

        public async Task<Result<PurchaseVoucherProductDTO>> UpdateAsync(int id, PurchaseVoucherProductDTO dto)
        {

            if (dto == null || dto.Voucher == null || id != dto.Voucher.PurchaseVoucherID)
                return Result<PurchaseVoucherProductDTO>.BadRequest("ID mismatch or invalid data.");

            var existing = await _purchaseVoucherRepository.GetByIdWithProductsAsync(id);
            if (existing.Voucher == null)
                return Result<PurchaseVoucherProductDTO>.NotFound("PurchaseVoucher not found for update.");

            var success = await _purchaseVoucherRepository.UpdateWithProductsAsync(dto.Voucher, dto.Products);
            if (!success)
                return Result<PurchaseVoucherProductDTO>.BadRequest("Failed to update PurchaseVoucher.");

            return Result<PurchaseVoucherProductDTO>.Success(dto);
        }

        public async Task<Result> DeleteAsync(int id)
        {

            var existing = await _purchaseVoucherRepository.GetByIdWithProductsAsync(id);
            if (existing.Voucher == null)
                return Result.NotFound("PurchaseVoucher not found for deletion.");

            var success = await _purchaseVoucherRepository.RemoveWithProductsAsync(id);
            if (!success)
                return Result.BadRequest("Failed to delete PurchaseVoucher.");

            return Result.Success();
        }
    }
}