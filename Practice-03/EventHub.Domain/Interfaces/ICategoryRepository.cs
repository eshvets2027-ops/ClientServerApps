using EventHub.Domain.Entities;

namespace EventHub.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken);
}
