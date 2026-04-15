using UserService.DTOs.PublishDTO;

namespace UserService.AsyncDataServices;

public interface IMessageBusClient
{
    Task PublishNewUser(UserPublishedDto userPublishedDto);
}