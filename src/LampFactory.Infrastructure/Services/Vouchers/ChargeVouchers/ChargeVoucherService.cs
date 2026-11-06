using FluentValidation;
using LampFactory.Application.Common.Results;
using LampFactory.Application.Features.Vouchers.ChargeVouchers;
using LampFactory.Application.Abstractions.Persistence.Vouchers.ChargeVouchers;

namespace LampFactory.Infrastructure.Services.Vouchers.ChargeVouchers
{
    public class ChargeVoucherService : IChargeVoucherService
    {
        private readonly IChargeVoucherRepository _chargeVoucherRepository;
        private readonly IValidator<ChargeVoucherProductDTO> _validator;

        public ChargeVoucherService(IChargeVoucherRepository chargeVoucherRepository, IValidator<ChargeVoucherProductDTO> validator)
        {
            _chargeVoucherRepository = chargeVoucherRepository;
            _validator = validator;
        }

        public async Task<Result<List<object>>> GetAllAsync()
        {

            var vouchers = await _chargeVoucherRepository.GetAllWithProductsAsync();
            var data = vouchers.Select(v => new { Voucher = v.Voucher, Products = v.Products }).ToList<object>();
            return Result<List<object>>.Success(data);
        }

        public async Task<Result<object>> GetByIdAsync(int id)
        {

            if (id <= 0)
                return Result<object>.BadRequest("Invalid ChargeVoucher ID.");

            var resultData = await _chargeVoucherRepository.GetByIdWithProductsAsync(id);
            if (resultData.Voucher == null)
                return Result<object>.NotFound("ChargeVoucher not found.");

            return Result<object>.Success(new { Voucher = resultData.Voucher, Products = resultData.Products });
        }

        public async Task<Result<object>> CreateAsync(ChargeVoucherProductDTO dto)
        {

            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return Result<object>.BadRequest("Validation failed", validationResult.Errors.Select(e => e.ErrorMessage).ToList());
            }

            var newVoucherId = await _chargeVoucherRepository.CreateWithProductsAsync(dto.Voucher, dto.Products);
            if (newVoucherId <= 0)
                return Result<object>.BadRequest("Failed to create ChargeVoucher.");

            var created = await _chargeVoucherRepository.GetByIdWithProductsAsync(newVoucherId);
            return Result<object>.Success(new { Voucher = created.Voucher, Products = created.Products });
        }

        public async Task<Result<ChargeVoucherProductDTO>> UpdateAsync(int id, ChargeVoucherProductDTO dto)
        {

            if (dto == null || dto.Voucher == null || id != dto.Voucher.ChargeVoucherID)
                return Result<ChargeVoucherProductDTO>.BadRequest("ID mismatch or invalid data.");

            var existing = await _chargeVoucherRepository.GetByIdWithProductsAsync(id);
            if (existing.Voucher == null)
                return Result<ChargeVoucherProductDTO>.NotFound("ChargeVoucher not found for update.");

            var success = await _chargeVoucherRepository.UpdateWithProductsAsync(dto.Voucher, dto.Products);
            if (!success)
                return Result<ChargeVoucherProductDTO>.BadRequest("Failed to update ChargeVoucher.");

            return Result<ChargeVoucherProductDTO>.Success(dto);
        }

        public async Task<Result> DeleteAsync(int id)
        {

            var existing = await _chargeVoucherRepository.GetByIdWithProductsAsync(id);
            if (existing.Voucher == null)
                return Result.NotFound("ChargeVoucher not found for deletion.");

            var success = await _chargeVoucherRepository.RemoveWithProductsAsync(id);
            if (!success)
                return Result.BadRequest("Failed to delete ChargeVoucher.");

            return Result.Success();
        }
    }
}