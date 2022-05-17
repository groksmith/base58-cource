using System.Text.Json.Serialization;

namespace BtcTransactionParser.Legacy;

public class Output
{
    [JsonPropertyName("amount")]
    public ulong Amount { get; set; }
    
    [JsonPropertyName("script_pub_key_len")]
    public uint ScriptPubKeyLen { get; set; }
    
    [JsonPropertyName("script_pub_key")]
    public string? ScriptPubKey { get; set; }
}