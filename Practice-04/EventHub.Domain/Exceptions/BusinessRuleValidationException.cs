namespace EventHub.Domain.Exceptions;
public sealed class BusinessRuleValidationException(string message) : Exception(message);
