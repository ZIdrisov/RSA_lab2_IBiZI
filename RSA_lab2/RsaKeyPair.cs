using System.Numerics;

namespace RSA_lab2;

public sealed record RsaKeyPair(
    BigInteger PublicKeyS,
    BigInteger PrivateKeyE,
    BigInteger ModulusN);
