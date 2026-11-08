using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace RSA_lab2;

public static class RsaEngine
{
    private const int ModulusDigits = 49;
    private const int MillerRabinRounds = 20;

    public static void GenerateKeys(
        out BigInteger publicKeyS,
        out BigInteger privateKeyE,
        out BigInteger modulusN)
    {
        while (true)
        {
            BigInteger p = GeneratePrime(24);
            BigInteger q = GeneratePrime(25);

            if (p == q)
            {
                continue;
            }

            BigInteger n = p * q;
            if (GetDecimalDigitCount(n) != ModulusDigits)
            {
                continue;
            }

            BigInteger phi = (p - 1) * (q - 1);
            BigInteger s = 65537;

            while (s < phi && BigInteger.GreatestCommonDivisor(s, phi) != 1)
            {
                s += 2;
            }

            if (s >= phi || BigInteger.GreatestCommonDivisor(s, phi) != 1)
            {
                continue;
            }

            publicKeyS = s;
            privateKeyE = ModularInverse(s, phi);
            modulusN = n;
            return;
        }
    }

    public static List<BigInteger> Encrypt(string plainText, BigInteger s, BigInteger N)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        ValidateModulus(N);
        ValidateExponent(s);

        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBlocks = new List<BigInteger>(bytes.Length);

        foreach (byte value in bytes)
        {
            BigInteger messageBlock = value;
            if (messageBlock >= N)
            {
                throw new ArgumentException("Каждый блок сообщения должен быть меньше N.", nameof(N));
            }

            encryptedBlocks.Add(BigInteger.ModPow(messageBlock, s, N));
        }

        return encryptedBlocks;
    }

    public static string Decrypt(List<BigInteger> encryptedBlocks, BigInteger e, BigInteger N)
    {
        ArgumentNullException.ThrowIfNull(encryptedBlocks);
        ValidateModulus(N);
        ValidateExponent(e);

        var result = new List<byte>(encryptedBlocks.Count);
        foreach (BigInteger encryptedBlock in encryptedBlocks)
        {
            if (encryptedBlock < 0 || encryptedBlock >= N)
            {
                throw new ArgumentException("Зашифрованный блок должен находиться в диапазоне от 0 до N - 1.", nameof(encryptedBlocks));
            }

            BigInteger messageBlock = BigInteger.ModPow(encryptedBlock, e, N);
            if (messageBlock > byte.MaxValue)
            {
                throw new FormatException("Расшифрованный блок не является корректным байтом UTF-8.");
            }

            result.Add((byte)messageBlock);
        }

        return Encoding.UTF8.GetString(result.ToArray());
    }

    private static BigInteger GeneratePrime(int decimalDigits)
    {
        BigInteger lowerBound = BigInteger.Pow(10, decimalDigits - 1);
        BigInteger upperBound = BigInteger.Pow(10, decimalDigits) - 1;
        byte[] randomBytes = new byte[decimalDigits * 4];

        while (true)
        {
            RandomNumberGenerator.Fill(randomBytes);
            BigInteger candidate = lowerBound + BigInteger.Abs(new BigInteger(randomBytes)) % (upperBound - lowerBound + 1);

            if (candidate.IsEven)
            {
                candidate++;
            }

            if (candidate <= upperBound && IsProbablePrime(candidate))
            {
                return candidate;
            }
        }
    }

    private static bool IsProbablePrime(BigInteger value)
    {
        if (value < 2)
        {
            return false;
        }

        int[] smallPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
        foreach (int prime in smallPrimes)
        {
            if (value == prime)
            {
                return true;
            }

            if (value % prime == 0)
            {
                return false;
            }
        }

        BigInteger d = value - 1;
        int powerOfTwo = 0;
        while (d.IsEven)
        {
            d /= 2;
            powerOfTwo++;
        }

        byte[] randomBytes = new byte[value.ToByteArray().Length];
        for (int round = 0; round < MillerRabinRounds; round++)
        {
            RandomNumberGenerator.Fill(randomBytes);
            BigInteger witness = 2 + BigInteger.Abs(new BigInteger(randomBytes)) % (value - 3);
            BigInteger current = BigInteger.ModPow(witness, d, value);

            if (current == 1 || current == value - 1)
            {
                continue;
            }

            bool passedRound = false;
            for (int r = 1; r < powerOfTwo; r++)
            {
                current = BigInteger.ModPow(current, 2, value);
                if (current == value - 1)
                {
                    passedRound = true;
                    break;
                }
            }

            if (!passedRound)
            {
                return false;
            }
        }

        return true;
    }

    private static BigInteger ModularInverse(BigInteger value, BigInteger modulus)
    {
        BigInteger oldRemainder = value;
        BigInteger remainder = modulus;
        BigInteger oldCoefficient = 1;
        BigInteger coefficient = 0;

        while (remainder != 0)
        {
            BigInteger quotient = oldRemainder / remainder;
            (oldRemainder, remainder) = (remainder, oldRemainder - quotient * remainder);
            (oldCoefficient, coefficient) = (coefficient, oldCoefficient - quotient * coefficient);
        }

        if (oldRemainder != 1)
        {
            throw new InvalidOperationException("Обратный элемент по модулю не существует.");
        }

        return (oldCoefficient % modulus + modulus) % modulus;
    }

    private static int GetDecimalDigitCount(BigInteger value)
    {
        return value.ToString().Length;
    }

    private static void ValidateModulus(BigInteger modulus)
    {
        if (modulus <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(modulus), "Модуль N должен быть больше единицы.");
        }
    }

    private static void ValidateExponent(BigInteger exponent)
    {
        if (exponent <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(exponent), "Показатель степени должен быть положительным.");
        }
    }
}
