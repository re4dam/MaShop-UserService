using UserService.DTOs.PublishDTO;

namespace UserService.Services;

public interface IMessageBusClient
{
    void PublishNewUser(UserPublishedDto userPublishedDto);
}