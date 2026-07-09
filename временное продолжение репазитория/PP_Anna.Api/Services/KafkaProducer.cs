using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace PP_Anna.Api.Services;

public class KafkaProducer : IKafkaProducer
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task ProduceAsync(string topic, string message)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            _logger.LogInformation("Сообщение отправлено в топик {Topic}, оффсет {Offset}, партиция {Partition}",
                topic, result.Offset, result.Partition);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Ошибка отправки в Kafka");
        }
    }
}