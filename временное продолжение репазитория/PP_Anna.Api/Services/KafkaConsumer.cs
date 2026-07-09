using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PP_Anna.Api.Services;

public class KafkaConsumer : BackgroundService
{
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly string _topic = "test-topic";
    private readonly int _maxRetries = 3;
    private readonly ConsumerConfig _config;

    public KafkaConsumer(ILogger<KafkaConsumer> logger)
    {
        _logger = logger;
        _config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "test-group",
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka Consumer запускается...");

        // Даём время Kafka подняться
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            IConsumer<Ignore, string>? consumer = null;
            try
            {
                consumer = new ConsumerBuilder<Ignore, string>(_config).Build();
                consumer.Subscribe(_topic);
                _logger.LogInformation("Подписан на топик {Topic}", _topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        if (consumeResult == null) continue;

                        var message = consumeResult.Message.Value;
                        _logger.LogInformation("Получено сообщение: {Message}", message);

                        bool processed = false;
                        for (int attempt = 1; attempt <= _maxRetries; attempt++)
                        {
                            try
                            {
                                _logger.LogInformation("Обработка (попытка {Attempt})", attempt);
                                processed = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Ошибка обработки (попытка {Attempt})", attempt);
                                if (attempt < _maxRetries)
                                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)), stoppingToken);
                            }
                        }

                        if (processed)
                        {
                            consumer.Commit(consumeResult);
                            _logger.LogInformation("Сообщение обработано, коммит выполнен");
                        }
                        else
                        {
                            _logger.LogWarning("Отправка в DLQ (имитация) для сообщения: {Msg}", message);
                            consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        _logger.LogWarning(ex, "Топик {Topic} недоступен, пересоздаю Consumer через 5 секунд...", _topic);
                        consumer?.Close();
                        consumer?.Dispose();
                        break; // выходим из внутреннего цикла для пересоздания
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Ошибка потребления");
                        await Task.Delay(1000, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Consumer остановлен");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Необработанная ошибка в цикле потребления");
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка в Consumer, перезапуск через 5 секунд");
                if (consumer != null)
                {
                    try { consumer.Close(); } catch { }
                    try { consumer.Dispose(); } catch { }
                }
                await Task.Delay(5000, stoppingToken);
            }
            finally
            {
                if (consumer != null)
                {
                    try { consumer.Close(); } catch { }
                    try { consumer.Dispose(); } catch { }
                }
            }
        }
    }
}