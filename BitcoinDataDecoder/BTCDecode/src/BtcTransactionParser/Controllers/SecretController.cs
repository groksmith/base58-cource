using Microsoft.AspNetCore.Mvc;
using NBitcoin;

namespace BtcTransactionParser.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SecretController : ControllerBase
{
    private readonly ILogger<SecretController> _logger;

    public SecretController(ILogger<SecretController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GenerateSecret(NetSchema network)
    {
        var privateKey = new Key();
        var wif = privateKey.GetBitcoinSecret(network.GetNetInfo());
        return Ok(wif.ToWif());
    }

    [HttpGet]
    public IActionResult GetPairFromSecret(string wif, NetSchema network)
    {
        var privateKey = Key.Parse(wif, network.GetNetInfo());
        var keyPair = new
        {
            PublicKeySize = privateKey.PubKey.ToHex().GetRawHexSize(),
            PublicKey = privateKey.PubKey.ToHex(),
            PrivateKeySize = privateKey.ToHex().GetRawHexSize(),
            PrivateKey = privateKey.ToHex()
        };
        return Ok(keyPair);
    }
    
    [HttpGet]
    public IActionResult GenerateMasterSecret(NetSchema network)
    {
        var networkInfo = network.GetNetInfo();
        var masterPrivateKey = new ExtKey();
        return Ok(masterPrivateKey.GetWif(networkInfo).ToWif());
    }

    [HttpGet]
    public IActionResult GetPairFromMasterPrivateKey(string wif, uint index, NetSchema network)
    {
        var networkInfo = network.GetNetInfo();
        var privateKey = ExtKey.Parse(wif, networkInfo);
        var derivedPrivateKey = privateKey.Derive(index).PrivateKey;
        var keyPair = new
        {
            PublicKeySize = derivedPrivateKey.PubKey.ToHex().GetRawHexSize(),
            PublicKey = derivedPrivateKey.PubKey.ToHex(),
            PrivateKeySize = derivedPrivateKey.ToHex().GetRawHexSize(),
            PrivateKey = derivedPrivateKey.ToHex(),
            Wif = derivedPrivateKey.GetWif(networkInfo).ToWif()
        };
        return Ok(keyPair);
    }
}