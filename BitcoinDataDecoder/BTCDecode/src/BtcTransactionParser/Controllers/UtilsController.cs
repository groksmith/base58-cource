using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransactionParser.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UtilsController : ControllerBase
{
    private readonly ILogger<UtilsController> _logger;

    public UtilsController(ILogger<UtilsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetLenghtAndHexRaw(string hex)
    {
        var retHexString = hex.GetRawHexSize() + " " + hex;
        return Ok(retHexString);
    }

    [HttpGet]
    public IActionResult ReverseHex(string hex)
    {
        var retHexString = hex.ReverseEndian();
        return Ok(retHexString);
    }
    
    [HttpGet]
    public IActionResult GetRawAmountFromDecimal(string amount)
    {
        var money = Money.Parse(amount);
        var sat = money.Satoshi;
        var hex = sat.ToString("x16");
        return Ok(hex.ReverseEndian());
    }

    [HttpPost]
    public IActionResult SignRawTransaction(string rawTransactionHash, string wif, NetSchema network)
    {
        var networkInfo = network.GetNetInfo();
        var key = Key.Parse(wif, networkInfo);

        var hash = uint256.Parse(rawTransactionHash);
        var hexSign = key.Sign(hash);
        var bytes = hexSign.ToDER();
        return Ok(Convert.ToHexString(bytes));
    }
}