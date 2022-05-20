using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransactionParser.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ScriptController : ControllerBase
{
    [HttpPost]
    public IActionResult GetP2SHLockScript(List<string> wifList, NetSchema network)
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
    public IActionResult GetP2SHUnlockScript(string rawTransaction, string wifTo, decimal amount,
        string redeemScriptHex, string wif, NetSchema network)
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

    [HttpPost]
    public IActionResult GetP2PKHLockScript(string publicKeyHex, NetSchema network)
    {
        var networkInfo = network.GetNetInfo();

        var publicKey = new PubKey(publicKeyHex);
        var scriptPubKey = publicKey.Hash.ScriptPubKey;

        var script = new
        {
            raw = new
            {
                ScriptPubKeySize = scriptPubKey.ToHex().GetRawHexSize(),
                ScriptPubKey = scriptPubKey.ToHex()
            }
        };
        return Ok(script);
    }

    [HttpPost]
    public IActionResult GetP2PKHUnlockScript(string rawTransaction, string wifOriginal, string wifTo, NetSchema network)
    {
        var networkInfo = network.GetNetInfo();
        var transactionResponse = Transaction.Parse(rawTransaction, networkInfo);

        var keyTo = Key.Parse(wifTo, networkInfo);
        var addressTo = keyTo.PubKey.GetAddress(ScriptPubKeyType.Legacy, networkInfo);

        var keyOriginal = Key.Parse(wifOriginal, networkInfo);

        var transaction = Transaction.Create(networkInfo);
        var coin = transactionResponse.Outputs.AsCoins().First();

        transaction.Inputs.Add(new TxIn {PrevOut = coin.Outpoint});
        transaction.Outputs.Add(coin.Amount - Money.Satoshis(1000), addressTo);
        
        transaction.Inputs[0].ScriptSig = addressTo.ScriptPubKey; 
        
        transaction.Sign(keyOriginal.GetWif(networkInfo), coin);
        return Ok(transaction.ToHex());
    }
}