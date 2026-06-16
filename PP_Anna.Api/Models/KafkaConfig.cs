namespace PP_Anna.Api.Models;

public class KafkaConfig
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "test-group";
    public string Topic { get; set; } = "test-topic";
    public string DlqTopic { get; set; } = "test-topic-dlq";
}