using LampFactory.Application.Abstractions.Persistence;

namespace LampFactory.Application.Abstractions.Persistence;

// Minimal repository contract shared by the ADO.NET repositories. Only the
// operations every aggregate supports live here; anything richer is declared
// on the feature-specific repository interface.
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<bool> RemoveAsync(int id);
    // Returns the new entity's ID (0 if the insert failed) so callers can
    // re-read the server-written audit columns instead of echoing client input.
    Task<int> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
}
