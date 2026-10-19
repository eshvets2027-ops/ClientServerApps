namespace EventHub.Application.DTOs;

public sealed record CoverImageUpload(Stream Content, string FileName, string ContentType, long Length);
