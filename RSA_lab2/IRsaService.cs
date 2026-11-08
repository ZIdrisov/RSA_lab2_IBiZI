using System.Numerics;

namespace RSA_lab2;

public interface IRsaService
{
    RsaKeyPair GenerateKeys();

    IReadOnlyList<BigInteger> Encrypt(string plainText, RsaKeyPair keyPair);

    string Decrypt(IEnumerable<BigInteger> encryptedBlocks, RsaKeyPair keyPair);
}
