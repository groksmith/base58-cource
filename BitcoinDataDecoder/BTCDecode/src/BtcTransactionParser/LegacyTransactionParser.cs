using System.Diagnostics;
using System.Globalization;
using BtcTransactionParser.Legacy;

namespace BtcTransactionParser;

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

            var txid = GetTxid(currentOffset);
            input.Txid = txid.txid;
            currentOffset = txid.offset;

            var vOut = GetOutCount(currentOffset);
            input.VOut = vOut.outCount;
            currentOffset = vOut.offset;

            var scriptSigSize = GetVarInt(currentOffset);
            input.ScriptSigSize = scriptSigSize.varint;
            currentOffset = scriptSigSize.offset;
            
            var scriptSig = GetScriptSig(currentOffset, input.ScriptSigSize);
            input.ScriptSig = scriptSig.scriptSig;
            currentOffset = scriptSig.offset;

            var sequence = GetSequence(currentOffset);
            input.Sequence = sequence.sequence;
            currentOffset = sequence.offset;

            transaction.Inputs.Add(input);
        }
        
        var outputCount = GetVarInt(currentOffset);
        transaction.OutputCount = outputCount.varint;
        currentOffset = outputCount.offset;

        transaction.Outputs = new List<Output>();
        for (var i = 0; i < transaction.OutputCount; i++)
        {
            var output = new Output();
            var amount  = GetAmount(currentOffset);
            output.Amount = amount.value;
            currentOffset = amount.offset;
            
            var scriptPubKeySize = GetVarInt(currentOffset);
            output.ScriptPubKeyLen = scriptPubKeySize.varint;
            currentOffset = scriptPubKeySize.offset;
            
            var scriptPubKey = GetScriptPubKey(currentOffset, output.ScriptPubKeyLen);
            output.ScriptPubKey = scriptPubKey.scriptPubKey;
            currentOffset = scriptPubKey.offset;

            transaction.Outputs.Add(output);
        }
        
        var locktime = GetLocktime(currentOffset);
        transaction.LockTime = locktime.locktime;
        currentOffset = locktime.offset;

        Debug.Assert(currentOffset == RawData.Length);
        return transaction;
    }

    private (int offset, uint version) GetVersion(int currentOffset)
    {
        var versionString = RawData.Substring(currentOffset, FieldSize.VERSION);
        var version = uint.Parse(versionString, NumberStyles.HexNumber);
        
        return (currentOffset + FieldSize.VERSION, version.ReverseBytes());
    }

    private (int offset, uint outCount) GetOutCount(int currentOffset)
    {
        var outCountString = RawData.Substring(currentOffset, FieldSize.VOUT);
        var outCount = uint.Parse(outCountString, NumberStyles.HexNumber);
        
        return (currentOffset + FieldSize.VOUT, outCount.ReverseBytes());
    }

    private (int offset, uint sequence) GetSequence(int currentOffset)
    {
        var sequenceString = RawData.Substring(currentOffset, FieldSize.SEQUENCE);
        var sequence = uint.Parse(sequenceString, NumberStyles.HexNumber);
        
        return (currentOffset + FieldSize.SEQUENCE, sequence.ReverseBytes());
    }

    private (int offset, ulong value) GetAmount(int currentOffset)
    {
        var valueString = RawData.Substring(currentOffset, FieldSize.VALUE);
        var value = ulong.Parse(valueString, NumberStyles.HexNumber);
        
        return (currentOffset + FieldSize.VALUE, value.ReverseBytes());
    }

    private (int offset, string? txid) GetTxid(int currentOffset)
    {
        var txidRawString = RawData.Substring(currentOffset, FieldSize.TXID);
        var txidBytesBigEndian = txidRawString.ReverseEndian();
        return (currentOffset + FieldSize.TXID, txidBytesBigEndian);
    }

    private (int offset, string? scriptSig) GetScriptSig(int currentOffset, uint scriptSigSize)
    {
        var scriptSigSizeInt = (int)scriptSigSize * 2;
        var scriptSigRawString = RawData.Substring(currentOffset, scriptSigSizeInt);
        return (currentOffset + scriptSigSizeInt, scriptSigRawString);
    }

    private (int offset, string? scriptPubKey) GetScriptPubKey(int currentOffset, uint scriptPubKeySize)
    {
        var scriptPubKeySizeInt = (int)scriptPubKeySize * 2;
        var scriptPubKeyRawString = RawData.Substring(currentOffset, scriptPubKeySizeInt);
        return (currentOffset + scriptPubKeySizeInt, scriptPubKeyRawString);
    }
    
    private (int offset, uint locktime) GetLocktime(int currentOffset)
    {
        var locktimeString = RawData.Substring(currentOffset, FieldSize.LOCKTIME);
        var locktime = uint.Parse(locktimeString, NumberStyles.HexNumber);
        
        return (currentOffset + FieldSize.LOCKTIME, locktime.ReverseBytes());
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