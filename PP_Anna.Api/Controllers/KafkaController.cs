using Microsoft.AspNetCore.Mvc;
using PP_Anna.Api.Services;

namespace PP_Anna.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KafkaController : ControllerBase
{
    private readonly IKafkaProducer _producer;

    public KafkaController(IKafkaProducer producer)
    {
        _producer = producer;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] object message)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        await _producer.ProduceAsync("test-topic", json);
        return Ok(new { status = "sent (mock)", message = json });
    }
}