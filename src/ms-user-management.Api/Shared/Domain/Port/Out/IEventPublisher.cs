namespace ms_user_management.Api.Shared.Domain.Port.Out;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T @event);
}