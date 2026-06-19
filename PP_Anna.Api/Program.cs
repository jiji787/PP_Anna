using PP_Anna.Api.Services;
using PP_Anna.Api.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducer, KafkaProducerMock>();
builder.Services.AddHostedService<KafkaConsumerMock>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();


// Регистрация Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    try
    {
        var redis = ConnectionMultiplexer.Connect(redisConnection);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
        builder.Services.AddSingleton<ICacheService, RedisCacheService>();
        Console.WriteLine("Redis подключен, используется реальный кэш.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Redis недоступен: {ex.Message}. Используется MockCacheService.");
        builder.Services.AddSingleton<ICacheService, MockCacheService>();
    }
}
else
{
    Console.WriteLine("Redis не настроен, используется MockCacheService.");
    builder.Services.AddSingleton<ICacheService, MockCacheService>();
}

// Регистрация сервиса секретов (пока мок)
builder.Services.AddSingleton<ISecretService, MockSecretService>();