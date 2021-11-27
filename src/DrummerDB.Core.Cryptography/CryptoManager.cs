using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using System;
using System.Security.Cryptography;

namespace Drummersoft.DrummerDB.Core.Cryptography
{
    //https://www.mking.net/blog/password-security-best-practices-with-examples-in-csharp
    internal class CryptoManager : ICryptoManager
    {
        #region Private Fields
        #endregion

        #region Public Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public byte[] GenerateSalt(int length)
        {
            var bytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        public byte[] GenerateHash(byte[] password, byte[] salt, int iterations, int length)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return deriveBytes.GetBytes(length);
            }
        }

        public int GetRandomNumber()
        {
            const int maxValue = 5000;
            return new Random().Next(maxValue);
        }

        public int GetByteLength()
        {
            return 10;
        }

        public static string GenerateTokenString()
        {
            // http://www.programmerguide.net/2015/02/generating-unique-token-in-c-generating.html
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public static byte[] GenerateToken()
        {
            // http://www.programmerguide.net/2015/02/generating-unique-token-in-c-generating.html
            return Convert.FromBase64String(GenerateTokenString());
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
