using Microsoft.Extensions.Logging;

namespace PP_Anna.Api.Services;

public class KafkaProducerMock : IKafkaProducer
{
    private readonly ILogger<KafkaProducerMock> _logger;

    public KafkaProducerMock(ILogger<KafkaProducerMock> logger)
    {
        _logger = logger;
    }

    public Task ProduceAsync(string topic, string message)
    {
        _logger.LogInformation("[MOCK] Отправка в топик {Topic}: {Message}", topic, message);
        return Task.CompletedTask;
    }
}