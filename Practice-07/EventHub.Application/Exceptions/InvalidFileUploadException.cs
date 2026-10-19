namespace EventHub.Application.Exceptions;

public sealed class InvalidFileUploadException(string message) : Exception(message);
