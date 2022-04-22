namespace LegacyTransactionParser;

public static class Utils
{
    public static uint ReverseBytes(this uint value)
    {
        return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
               (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
    }

    public static ulong ReverseBytes(this ulong value)
    {
        return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
               (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
               (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
               (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
    }

    public static string ReverseEndian(this string value)
    {
        if (value.Length % 2 != 0)
        {
            throw new ArgumentException($"Not valid parameter, wrong padding of endian!");
        }

        var array = value.ToCharArray();
        var offsetForward = 0;
        var offsetBackward = array.Length - 2;

        for (var i = 0; i < array.Length / 2 - 1; i += 2)
        {
            SwapCharsBytes(ref array[offsetForward], ref array[offsetBackward]);
            SwapCharsBytes(ref array[offsetForward + 1], ref array[offsetBackward + 1]);
            offsetForward =+ 2;
            offsetBackward -= 2;
        }

        return new string(array);
    }
    
    private static void SwapCharsBytes(ref char a, ref char b)
    {
        (a, b) = (b, a);
    }
}