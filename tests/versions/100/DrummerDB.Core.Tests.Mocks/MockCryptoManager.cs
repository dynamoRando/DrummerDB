using Drummersoft.DrummerDB.Core.Cryptography.Interface;
using System;

namespace Drummersoft.DrummerDB.Core.Tests.Mocks
{
    internal class MockCryptoManager : ICryptoManager
    {
        public byte[] GenerateHash(byte[] password, byte[] salt, int iterations, int length)
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateSalt(int length)
        {
            throw new NotImplementedException();
        }

        public int GetByteLength()
        {
            throw new NotImplementedException();
        }

        public int GetRandomNumber()
        {
            throw new NotImplementedException();
        }
    }
}
