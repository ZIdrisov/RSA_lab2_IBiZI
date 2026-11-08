using System.Numerics;

namespace RSA_lab2;

public sealed class RsaService : IRsaService
{
    public RsaKeyPair GenerateKeys()
    {
        RsaEngine.GenerateKeys(out BigInteger publicKeyS, out BigInteger privateKeyE, out BigInteger modulusN);
        return new RsaKeyPair(publicKeyS, privateKeyE, modulusN);
    }

    public IReadOnlyList<BigInteger> Encrypt(string plainText, RsaKeyPair keyPair)
    {
        ArgumentNullException.ThrowIfNull(keyPair);
        return RsaEngine.Encrypt(plainText, keyPair.PublicKeyS, keyPair.ModulusN);
    }

    public string Decrypt(IEnumerable<BigInteger> encryptedBlocks, RsaKeyPair keyPair)
    {
        ArgumentNullException.ThrowIfNull(encryptedBlocks);
        ArgumentNullException.ThrowIfNull(keyPair);
        return RsaEngine.Decrypt(encryptedBlocks.ToList(), keyPair.PrivateKeyE, keyPair.ModulusN);
    }
}
