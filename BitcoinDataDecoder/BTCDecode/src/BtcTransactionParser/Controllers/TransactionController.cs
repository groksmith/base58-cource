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

    [HttpPost]
    public IActionResult GetP2ShLockScript(List<string> wifList, NetSchema network)
    {
        var networkInfo = network.GetNetInfo();
        
        var pubKeys = wifList
            .Where(item => item != string.Empty)
            .Select(item => Key.Parse(item, networkInfo))
            .Select(privateKey => privateKey.PubKey)
            .ToList();

        var paymentScript = PayToMultiSigTemplate
            .Instance
            .GenerateScriptPubKey(pubKeys.Count, pubKeys.ToArray()).PaymentScript;

        var script = new
        {
            asm = paymentScript.ToString(), 
            raw = new
            {
                ScriptPubKeySize = paymentScript.ToHex().GetRawHexSize(),
                ScriptPubKey = paymentScript.ToHex()
            }
            
        };
        return Ok(script);
    }

    [HttpPost]
    public IActionResult GetP2ShUnlockScript(string rawTransaction, string wifTo, decimal amount, string redeemScriptHex, string wif, NetSchema network)
    {
        var networkInfo = network.GetNetInfo();
        var transactionResponse = Transaction.Parse(rawTransaction, networkInfo);

        var keyTo = Key.Parse(wifTo, networkInfo);
        var addressTo = keyTo.PubKey.GetAddress(ScriptPubKeyType.Legacy, networkInfo);

        var redeemScript = Script.FromHex(redeemScriptHex);
        var scriptCoin = transactionResponse.Outputs
            .AsCoins()
            .First()
            .ToScriptCoin(redeemScript);

        var builder = networkInfo.CreateTransactionBuilder();
        var unsigned = 
            builder
                .AddCoins(scriptCoin)
                .SetChange(addressTo)
                .Send(addressTo, Money.Coins(amount))
                .SendFees(Money.Coins(0.00001m))
                .BuildTransaction(false);

        var key = Key.Parse(wif, networkInfo);
        var oneSigned =
            builder
                .AddCoins(scriptCoin)
                .AddKeys(key)
                .SignTransaction(unsigned);
        
        return Ok(oneSigned.ToHex());
    }
}