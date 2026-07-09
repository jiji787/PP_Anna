using PP_Anna.Api.Services;
using PP_Anna.Api.Models;
using StackExchange.Redis;
using System.IO;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.Configure<KafkaConfig>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();
builder.Services.AddHostedService<KafkaConsumer>();

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

// Регистрация Vault (если доступен)
try
{
    // Проверим, доступен ли Vault, выполнив простой запрос
    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync("http://localhost:8200/v1/sys/health");
    if (response.IsSuccessStatusCode)
    {
        builder.Services.AddSingleton<ISecretService, VaultSecretService>();
        Console.WriteLine("✅ Vault подключен, используется реальный VaultSecretService.");
    }
    else
    {
        Console.WriteLine("Vault недоступен, используется MockSecretService.");
        builder.Services.AddSingleton<ISecretService, MockSecretService>();
    }
}
catch
{
    Console.WriteLine("Vault недоступен, используется MockSecretService.");
    builder.Services.AddSingleton<ISecretService, MockSecretService>();
}
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