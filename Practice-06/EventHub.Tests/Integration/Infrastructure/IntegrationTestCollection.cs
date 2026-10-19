namespace EventHub.Tests.Integration.Infrastructure;

public static class IntegrationTestCollection
{
    public const string Name = "Integration tests";
}

[CollectionDefinition(IntegrationTestCollection.Name, DisableParallelization = true)]
public sealed class IntegrationTestCollectionDefinition : ICollectionFixture<EventHubWebApplicationFactory>;
