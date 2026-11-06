using LampFactory.Application.Features.Production.Operators;

namespace LampFactory.Application.Features.Production.Operators
{
    // Write-side contract for the Operator aggregate. The API binds this, not the
    // Domain entity, so the presentation layer never shapes aggregate state.
    public class OperatorDTO
    {
        public Operator Operator { get; set; } = new();
    }
}
