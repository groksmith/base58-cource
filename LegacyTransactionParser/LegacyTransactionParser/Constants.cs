namespace LegacyTransactionParser;

public class FieldSize
{
    public const int LOCKTIME = 8;
    public const int VERSION = 8;
    public const int TXID = 64;
    public const int VOUT = 8;
    public const int SEQUENCE = 8;
    public const int VALUE = 16;
    
    public const int VARINT_PREFIX = 2;
    public const int VARINT_FD_SIZE = 4;
    public const int VARINT_FE_SIZE = 8;
    public const int VARINT_FF_SIZE = 16;
}