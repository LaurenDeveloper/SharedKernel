using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain.Events;
using SharedKernel.Infrastructure.Events;
using SharedKernel.Integration.Tests.Shared;
using System.Threading.Tasks;
using SharedKernel.Domain.Tests.Users;
using SharedKernel.Infrastructure;
using Xunit;

namespace SharedKernel.Integration.Tests.Events.RabbitMq
{
    public class RabbitMqEventBusShould : InfrastructureTestCase
    {
        protected override string GetJsonFile()
        {
            return "Events/RabbitMq/appsettings.rabbitMq.json";
        }

        protected override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddSharedKernel()
                .AddDomainEvents(typeof(UserCreated))
                .AddRabbitMqEventBus(Configuration)
                .AddDomainEventsSubscribers(typeof(SetCountWhenUserCreatedSubscriber))
                .AddDomainEventSubscribers()
                .AddSingleton<PublishUserCreatedDomainEvent>();
        }

        [Fact]
        public async Task PublishDomainEventFromRabbitMq()
        {
            var user = await PublishUserCreatedDomainEventCase.PublishDomainEvent(GetRequiredService<IEventBus>());
            var singletonValueContainer = GetRequiredService<PublishUserCreatedDomainEvent>();
            Assert.Equal(user.Id, singletonValueContainer.UserId);
            Assert.True(singletonValueContainer.Total >= 2);
        }
    }
}