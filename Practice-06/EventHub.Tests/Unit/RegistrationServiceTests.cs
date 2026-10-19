using EventHub.Application.DTOs;
using EventHub.Application.Services;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Moq;

namespace EventHub.Tests.Unit;

public sealed class RegistrationServiceTests
{
    [Fact]
    public async Task CreateAsync_WithFreeSeat_NormalizesEmailAndSavesRegistration()
    {
        var eventRepository = new Mock<IEventRepository>();
        eventRepository.Setup(x => x.GetByIdAsync(7)).ReturnsAsync(new Event { Id = 7, Capacity = 20 });
        var registrationRepository = new Mock<IRegistrationRepository>();
        registrationRepository.Setup(x => x.ExistsAsync(7, "student@example.com")).ReturnsAsync(false);
        registrationRepository.Setup(x => x.CountActiveByEventIdAsync(7)).ReturnsAsync(3);
        Registration? saved = null;
        registrationRepository.Setup(x => x.AddAsync(It.IsAny<Registration>()))
            .Callback<Registration>(x => saved = x).Returns(Task.CompletedTask);
        var service = new RegistrationService(eventRepository.Object, registrationRepository.Object);

        var result = await service.CreateAsync(7, new CreateRegistrationRequest
        {
            AttendeeName = " Oksana ",
            AttendeeEmail = " STUDENT@EXAMPLE.COM "
        });

        Assert.Equal("Oksana", result.AttendeeName);
        Assert.Equal("student@example.com", result.AttendeeEmail);
        Assert.Equal("pending", result.Status);
        Assert.NotNull(saved);
        Assert.Equal(7, saved.EventId);
        registrationRepository.Verify(x => x.AddAsync(It.IsAny<Registration>()), Times.Once);
    }
}
