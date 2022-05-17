using System.Globalization;
using System.Text;
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

            var vout = GetVout(currentOffset);
            input.VOut = vout.vout;
            currentOffset = vout.offset;

            var ScriptSigSize = GetVarInt(currentOffset);
            input.ScriptSigSize = ScriptSigSize.varint;
            currentOffset = ScriptSigSize.offset;

            var scriptSig = GetScriptSig(currentOffset, Convert.ToInt32(input.ScriptSigSize * 2));
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
            var amount = GetAmount(currentOffset);

            output.Amount = amount.value;
            currentOffset = amount.offset;

            var scriptPubKeyLen = GetVarInt(currentOffset);

            output.ScriptPubKeyLen = scriptPubKeyLen.varint;
            currentOffset = scriptPubKeyLen.offset;

            var scriptPubKey = GetScriptPubKey(currentOffset, Convert.ToInt32(output.ScriptPubKeyLen * 2));
            output.ScriptPubKey = scriptPubKey.scriptPubKey;
            currentOffset = scriptPubKey.offset;

            transaction.Outputs.Add(output);
        }

        transaction.LockTime = GetLockTime(currentOffset);

        return transaction;
    }

    private string GetLockTime(int currentOffset)
    {
        return RawData.Substring(currentOffset, FieldSize.LOCKTIME);
    }

    private (int offset, string? scriptPubKey) GetScriptPubKey(int currentOffset, int scriptPubKeyLen)
    {
        var scriptPubKey = RawData.Substring(currentOffset, scriptPubKeyLen);

        return (currentOffset + scriptPubKeyLen, scriptPubKey);
    }

    private (int offset, ulong value) GetAmount(int currentOffset)
    {
        var valueString = StringRotation(new StringBuilder(RawData.Substring(currentOffset, FieldSize.VALUE)));
        var value = ulong.Parse(valueString.ToString(), NumberStyles.HexNumber);

        return (currentOffset + FieldSize.VALUE, value);
    }

    private (int offset, string? sequence) GetSequence(int currentOffset)
    {
        var sequence = RawData.Substring(currentOffset, FieldSize.SEQUENCE);

        return (currentOffset + FieldSize.SEQUENCE, sequence);
    }

    private (int offset, string? scriptSig) GetScriptSig(int currentOffset, int scriptSigSize)
    {
        var scriptSig = RawData.Substring(currentOffset, scriptSigSize);

        return (currentOffset + scriptSigSize, scriptSig);
    }

    private (int offset, uint version) GetVersion(int currentOffset)
    {
        var versionString = RawData.Substring(currentOffset, FieldSize.VERSION);
        var version = uint.Parse(versionString, NumberStyles.HexNumber);
        return (currentOffset + FieldSize.VERSION, version.ReverseBytes());
    }

    private (int offset, string? txid) GetTxid(int currentOffset)
    {
        var txidString = new StringBuilder(RawData.Substring(currentOffset, FieldSize.TXID));

        return (currentOffset + FieldSize.TXID, StringRotation(txidString).ToString());
    }

    private (int offset, uint vout) GetVout(int currentOffset)
    {
        var vout = Convert.ToUInt32(RawData.Substring(currentOffset, FieldSize.VOUT));

        return (currentOffset + FieldSize.VOUT, vout);
    }

    private static StringBuilder StringRotation(StringBuilder txidString)
    {
        var i = 1;
        var j = txidString.Length - 1;

        for (var k = 0; k < txidString.Length / 4; ++k)
        {
            (txidString[i], txidString[j]) = (txidString[j], txidString[i]);
            (txidString[i - 1], txidString[j - 1]) = (txidString[j - 1], txidString[i - 1]);
            i += 2;
            j -= 2;
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