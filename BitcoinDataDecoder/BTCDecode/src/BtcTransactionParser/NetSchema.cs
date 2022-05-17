using NBitcoin;

namespace BtcTransactionParser;

public enum NetSchema
{
    Main = 0,
    Test,
    Reg
}

public static class BtcUtils
{
    public static Network GetNetInfo(this NetSchema schema)
    {
        return schema switch
        {
            NetSchema.Main => Network.Main,
            NetSchema.Test => Network.TestNet,
            NetSchema.Reg => Network.RegTest,
            _ => Network.Main
        };
    }
}