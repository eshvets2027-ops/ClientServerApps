using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct) =>
        await db.Categories.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<Category> AddAsync(Category category, CancellationToken ct)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
        return category;
    }
}
