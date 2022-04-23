using Microsoft.AspNetCore.Mvc;

namespace LegacyTransactionParser.Controllers;

[ApiController]
[Route("[controller]")]
public class LegacyTransactionController : ControllerBase
{
    private readonly ILogger<LegacyTransactionController> _logger;

    public LegacyTransactionController(ILogger<LegacyTransactionController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetTransactionFromRaw")]
    public IActionResult Get(string rawTransaction)
    {
        _logger.LogInformation("Start parsing raw transaction");
        var legacyTransactionParser = new LegacyTransactionParser(rawTransaction);
        return Ok(legacyTransactionParser.Parse());
    }
}