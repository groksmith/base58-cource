using System.Text.Json.Serialization;

namespace BtcTransactionParser.Legacy;

public class Input
{
    [JsonPropertyName("txid")]
    public string? Txid { get; set; }

    [JsonPropertyName("vout")]
    public uint VOut { get; set; }

    [JsonPropertyName("script_sig_size")]
    public uint ScriptSigSize { get; set; }
    
    [JsonPropertyName("script_sig")]
    public string? ScriptSig { get; set; }

    [JsonPropertyName("sequence")]
    public uint Sequence { get; set; }
}
