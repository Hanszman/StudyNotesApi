using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using StudyNotesApi.Application.Common.Exceptions;
using StudyNotesApi.Application.Interfaces.Security;

namespace StudyNotesApi.Api.Security.PasswordHasher;

public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemorySizeKb = 65536;
    private const int DegreeOfParallelism = 4;

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("Password cannot be empty.");
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = HashPassword(password, salt);

        return string.Join(
            '$',
            "argon2id",
            $"m={MemorySizeKb},t={Iterations},p={DegreeOfParallelism}",
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("Password cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ValidationException("Password hash cannot be empty.");
        }

        var segments = passwordHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 4 || !string.Equals(segments[0], "argon2id", StringComparison.Ordinal))
        {
            return false;
        }

        if (!TryParseParameters(segments[1], out var memorySizeKb, out var iterations, out var degreeOfParallelism))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(segments[2]);
            var expectedHash = Convert.FromBase64String(segments[3]);
            var actualHash = HashPassword(password, salt, expectedHash.Length, memorySizeKb, iterations, degreeOfParallelism);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] HashPassword(
        string password,
        byte[] salt,
        int hashSize = HashSize,
        int memorySizeKb = MemorySizeKb,
        int iterations = Iterations,
        int degreeOfParallelism = DegreeOfParallelism)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memorySizeKb,
            Iterations = iterations,
            DegreeOfParallelism = degreeOfParallelism
        };

        return argon2.GetBytes(hashSize);
    }

    private static bool TryParseParameters(string parameters, out int memorySizeKb, out int iterations, out int degreeOfParallelism)
    {
        memorySizeKb = 0;
        iterations = 0;
        degreeOfParallelism = 0;

        var pairs = parameters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (pairs.Length != 3)
        {
            return false;
        }

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var value))
            {
                return false;
            }

            switch (parts[0])
            {
                case "m":
                    memorySizeKb = value;
                    break;
                case "t":
                    iterations = value;
                    break;
                case "p":
                    degreeOfParallelism = value;
                    break;
                default:
                    return false;
            }
        }

        return memorySizeKb > 0 && iterations > 0 && degreeOfParallelism > 0;
    }
}
