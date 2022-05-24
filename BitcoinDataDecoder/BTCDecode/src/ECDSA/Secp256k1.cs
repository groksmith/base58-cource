using System.Numerics;
using System.Security.Cryptography;

namespace ECDSA
{
    public class Secp256k1
    {
        public static EllipticCurveParameters Params = new EllipticCurveParameters(
            Utility.ParseHex("FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE FFFFFC2F"), 0, 7);

        public static EcPoint Param_G =
            new EcPoint("02 79BE667E F9DCBBAC 55A06295 CE870B07 029BFCDB 2DCE28D9 59F2815B 16F81798", Params);

        public static BigInteger Param_n =
            Utility.ParseHex("FFFFFFFF FFFFFFFF FFFFFFFF FFFFFFFE BAAEDCE6 AF48A03B BFD25E8C D0364141");

        public static BigInteger Max256 = BuildMax256();

        private static BigInteger BuildMax256()
        {
            BigInteger ret = 2;
            for (int i = 0; i < 8; i++)
                ret *= ret;
            return ret;
        }

        private static RNGCryptoServiceProvider _rand = new();

        private static byte[] RandomBytes(int n)
        {
            var ret = new byte[n];
            _rand.GetBytes(ret);
            return ret;
        }

        public static BigInteger GenerateFieldCoordinate()
        {
            var max = Params.P / 2;
            var min = -max;
            while (true)
            {
                var ret = new BigInteger(RandomBytes(32));
                if (ret < min || ret > max)
                    continue;
                return ret.Mod(Params.P);
            }
        }

        public static BigInteger GenerateRandomNonce()
        {
            var max = Param_n / 2;
            var min = -max;
            while (true)
            {
                var ret = new BigInteger(RandomBytes(32));
                if (ret < min || ret > max)
                    continue;
                return ret.Mod(Param_n);
            }
        }

        public class Sig
        {
            public BigInteger R, S;

            public Sig(BigInteger r, BigInteger s)
            {
                R = r;
                S = s;
            }
        }

        public static BigInteger GeneratePrivateKey()
        {
            return GenerateRandomNonce();
        }

        private static BigInteger HashMessage(byte[] message)
        {
            return new BigInteger(message).Mod(Max256);
        }

        public static Sig? SignMessage(byte[] message, BigInteger privateKey, BigInteger? userNonce = null)
        {
            return SignDigest(HashMessage(message), privateKey, userNonce);
        }

        public static Sig? SignDigest(BigInteger digest, BigInteger privateKey, BigInteger? userNonce = null,
            bool lowS = true)
        {
            BigInteger k, r, s;
            var n = Param_n;
            for (var looped = false;; looped = true)
            {
                if (looped && userNonce != null)
                {
                    return null;
                }

                k = userNonce ?? GenerateRandomNonce();
                r = (Param_G * k).X % n;
                if (r == 0)
                {
                    continue;
                }

                s = ((privateKey * r + digest) * k.ExtendedEuclidean(n)).Mod(n);
                if (s > n / 2)
                {
                    s = n - s;
                }
                
                if (s == 0)
                {
                    continue;
                }

                break;
            }

            return new Sig(r, s);
        }

        public static bool VerifySignature(byte[] message, Sig signature, EcPoint publicKey)
        {
            return VerifySignature(HashMessage(message), signature, publicKey);
        }

        public static bool VerifySignature(BigInteger digest, Sig signature, EcPoint publicKey)
        {
            var n = Param_n;
            var r = signature.R;
            var s = signature.S;
            if (publicKey.IsInfinite || !publicKey.IsSolution() || !(publicKey * n).IsInfinite)
                return false;
            if (r < 1 || s < 1 || r >= n || s >= n)
                return false;
            var w = s.ExtendedEuclidean(n);
            var u1 = (digest * w).Mod(n);
            var u2 = (r * w).Mod(n);
            var x = Param_G * u1 + publicKey * u2;
            if (x.IsInfinite)
                return false;
            return r == x.X;
        }

        public static BigInteger RecoverPrivateKey(byte[] message1, Sig signature1, byte[] message2, Sig signature2)
        {
            var n = Param_n;
            var z1 = HashMessage(message1);
            var z2 = HashMessage(message2);
            var k = ((z1 - z2) * (signature1.S - signature2.S).ExtendedEuclidean(n)).Mod(n);
            return ((signature1.S * k - z1) * signature1.R.ExtendedEuclidean(n)).Mod(n);
        }
    }
}