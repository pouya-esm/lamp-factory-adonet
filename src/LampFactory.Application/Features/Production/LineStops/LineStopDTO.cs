using LampFactory.Application.Features.Production.LineStops;

namespace LampFactory.Application.Features.Production.LineStops
{
    // Write-side contract for the LineStop aggregate. The API binds this, not the
    // Domain entity, so the presentation layer never shapes aggregate state.
    public class LineStopDTO
    {
        public LineStop LineStop { get; set; } = new();
    }
}
