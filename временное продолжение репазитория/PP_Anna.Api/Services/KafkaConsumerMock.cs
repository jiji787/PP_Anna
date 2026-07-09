using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PP_Anna.Api.Services;

public class KafkaConsumerMock : BackgroundService
{
    private readonly ILogger<KafkaConsumerMock> _logger;
    private readonly int _maxRetries = 3;

    public KafkaConsumerMock(ILogger<KafkaConsumerMock> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[MOCK] Consumer запущен, имитирует чтение сообщений каждые 10 секунд");

        var counter = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000, stoppingToken);
            counter++;
            var fakeMessage = $"{{\"id\":{counter}, \"text\":\"Тестовое сообщение {counter}\"}}";

            // Имитация обработки с ретраями
            var processed = false;
            for (int attempt = 1; attempt <= _maxRetries; attempt++)
            {
                try
                {
                    // Имитация возможной ошибки (для демонстрации ретраев)
                    if (counter % 5 == 0) // каждое 5-е сообщение падает
                        throw new Exception("Симуляция ошибки");

                    _logger.LogInformation("[MOCK] Обработка сообщения: {Msg} (попытка {Attempt})", fakeMessage, attempt);
                    processed = true;
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[MOCK] Ошибка обработки (попытка {Attempt})", attempt);
                    if (attempt < _maxRetries)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                        await Task.Delay(delay, stoppingToken);
                    }
                }
            }

            if (processed)
            {
                _logger.LogInformation("[MOCK] Сообщение обработано успешно.");
            }
            else
            {
                _logger.LogWarning("[MOCK] Отправка в DLQ (имитация): {Msg}", fakeMessage);
            }
        }
    }
}