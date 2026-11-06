
namespace LampFactory.Application.Abstractions.Time;

// Indirection over the clock so use cases and tests agree on "now".
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
