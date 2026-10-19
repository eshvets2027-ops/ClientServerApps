using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface ICoverImageStorage
{
    Task<string> SaveAsync(Stream content, string extension);
    Task<CoverImageFile?> OpenReadAsync(string storagePath);
    Task DeleteAsync(string storagePath);
}
