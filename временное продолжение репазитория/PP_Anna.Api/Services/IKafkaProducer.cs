namespace PP_Anna.Api.Services;

public interface IKafkaProducer
{
    Task ProduceAsync(string topic, string message);
}