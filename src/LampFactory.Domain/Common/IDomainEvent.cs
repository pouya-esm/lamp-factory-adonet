namespace LampFactory.Domain.Common;

// Marker for a domain event. The project currently has no event producers, so
// this stays as an empty contract — it exists so that future aggregate roots
// can raise events without a structural change.
public interface IDomainEvent
{
}
