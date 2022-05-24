using System.Globalization;
using System.Numerics;
using System.Text;
using ECDSA;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransactionParser.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class UtilsController : ControllerBase
{
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

    [HttpGet]
    public IActionResult Trim(string raw)
    {
        var values = new StringBuilder();
        raw = raw.Replace(':', ' ');

        var components = raw.Split(' ');

        foreach (var item in components)
        {
            if (item.OnlyHexInString() && !string.IsNullOrWhiteSpace(item))
            {
                values.Append(item);
            }
        }

        return Ok(values.Trim());
    }

    [HttpPost]
    public IActionResult SignRawTransaction(string rawTransactionHashDecimal, string privateKeyDecimal)
    {
        var privateKey = BigInteger.Parse(privateKeyDecimal);
        var nonce = BigInteger.Parse("123456789");

        var bytes = rawTransactionHashDecimal.ComputeSha256Hash();
        var signature = Secp256k1.SignMessage(bytes, privateKey, nonce);
        
        var r = signature?.R.ToString("x");
        var s = signature?.S.ToString("x");

        return Ok(r!.GetCreateDerEncodSignature(s!));
    }
}