using Microsoft.Extensions.Hosting;

namespace PP_Anna.Api.Services;

public interface IKafkaConsumer : IHostedService
{
    //реализация в фоновом сервисе
}