using System.Globalization;
using System.Text;
using LegacyTransactionParser.Legacy;

namespace LegacyTransactionParser;

public class LegacyTransactionParser
{
    private string RawData { get; }

    public LegacyTransactionParser(string rawData)
    {
        RawData = rawData;
    }

    public Transaction Parse()
    {
        var currentOffset = 0;
        var transaction = new Transaction();

        var version = GetVersion(currentOffset);
        transaction.Version = version.version;
        currentOffset = version.offset;
        
        var inputCount = GetVarInt(currentOffset);
        transaction.InputCount = inputCount.varint;
        currentOffset = inputCount.offset;

        transaction.Inputs = new List<Input>();
        for (var i = 0; i < transaction.InputCount; i++)
        {
            var input = new Input();

            var txid = GetTXID(currentOffset);
            input.Txid = txid.txid;
            currentOffset = txid.offset;
            transaction.Inputs.Add(input);
        }
        
        return transaction;
    }

    private (int offset, uint version) GetVersion(int currentOffset)
    {
        var versionString = RawData.Substring(currentOffset, FieldSize.VERSION);
        var version = uint.Parse(versionString, NumberStyles.HexNumber);
        return (currentOffset + FieldSize.VERSION, version.ReverseBytes());
    }

    private (int offset, string? txid) GetTXID(int currentOffset)
    {
        var txidString = new StringBuilder(RawData.Substring(currentOffset, FieldSize.TXID));

        return (currentOffset + FieldSize.TXID, StringRotation(txidString).ToString());
    }

    private StringBuilder StringRotation(StringBuilder txidString)
    {
        var i = 1;
        var j = txidString.Length-1; 
        
        for (var k = 0;k < txidString.Length/4 ;++k )
        {
            (txidString[i], txidString[j]) = (txidString[j], txidString[i]);
            (txidString[i-1], txidString[j-1]) = (txidString[j-1], txidString[i-1]);
            i+=2;
            j-=2;
        }

        return txidString;
    }

    private (int offset, uint varint) GetVarInt(int currentOffset)
    {
        string value;
        var prefix = RawData.Substring(currentOffset, FieldSize.VARINT_PREFIX);
        currentOffset += FieldSize.VARINT_PREFIX;

        switch (prefix.ToLower())
        {
            case "fd":
                value = RawData.Substring(currentOffset, FieldSize.VARINT_FD_SIZE);
                currentOffset += FieldSize.VARINT_FD_SIZE;
                break;
            case "fe":
                value = RawData.Substring(currentOffset, FieldSize.VARINT_FE_SIZE);
                currentOffset += FieldSize.VARINT_FE_SIZE;
                break;
            case "ff":
                value = RawData.Substring(currentOffset, FieldSize.VARINT_FF_SIZE);
                currentOffset += FieldSize.VARINT_FF_SIZE;
                break;
            default:
                value = prefix;
                break;
        }
        
        var version = uint.Parse(value, NumberStyles.HexNumber);
        return (currentOffset, version);
    }
}