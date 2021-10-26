namespace Drummersoft.DrummerDB.Core.Cryptography.Interface
{
    internal interface ICryptoManager
    {
        public byte[] GenerateSalt(int length);
        public byte[] GenerateHash(byte[] password, byte[] salt, int iterations, int length);
        public int GetRandomNumber();
        public int GetByteLength();

    }
}
