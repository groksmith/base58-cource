using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransactionParser.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class TransactionController : ControllerBase
{
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(ILogger<TransactionController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult DecodeLegacyRawTransaction(string rawTransaction)
    {
        _logger.LogInformation("Start parsing raw transaction");
        var legacyTransactionParser = new LegacyTransactionParser(rawTransaction);
        return Ok(legacyTransactionParser.Parse());
    }
    
    [HttpGet]
    public IActionResult DecodeRawTransaction(string rawTransaction, NetSchema network)
    {
        var transaction = Transaction.Parse(rawTransaction, network.GetNetInfo());
        return Ok(transaction.ToString());
    }

}