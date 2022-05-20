using System.Text;

namespace BtcTransactionParser;

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

    public static string ReverseEndian(this string hexValue)
    {
        var hexString = new StringBuilder(hexValue);
        var i = 1;
        var j = hexString.Length - 1;

        for (var k = 0; k < hexString.Length / 4; ++k)
        {
            (hexString[i], hexString[j]) = (hexString[j], hexString[i]);
            (hexString[i - 1], hexString[j - 1]) = (hexString[j - 1], hexString[i - 1]);
            i += 2;
            j -= 2;
        }

        return hexString.ToString();
    }
    public static string GetRawHexSize(this string hex)
    {
        return (hex.Length / 2).ToString("X");
    }
    
    public static bool OnlyHexInString(this string text)
    {
        return text.All(current => char.IsDigit(current) || current is >= 'a' and <= 'f');
    }

    public static string Trim(this StringBuilder text)
    {
        return string.Concat(text
            .ToString()
            .Where(c => !char.IsWhiteSpace(c)));
    }
}