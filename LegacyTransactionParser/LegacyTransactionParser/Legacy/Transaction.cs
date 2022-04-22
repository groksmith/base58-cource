using System.Text.Json.Serialization;

namespace LegacyTransactionParser.Legacy;

public class Transaction
{
    [JsonPropertyName("version")]
    public uint Version { get; set; }

    [JsonPropertyName("input_count")]
    public uint InputCount { get; set; }

    [JsonPropertyName("inputs")]
    public List<Input> Inputs { get; set; }

    [JsonPropertyName("output_count")]
    public uint OutputCount { get; set; }

    [JsonPropertyName("outputs")]
    public List<Output> Outputs { get; set; }

    [JsonPropertyName("locktime")]
    public string LockTime { get; set; }
}


