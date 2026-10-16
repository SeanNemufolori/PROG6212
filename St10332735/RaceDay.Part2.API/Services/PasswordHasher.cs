using System.Security.Cryptography;
using RaceDay.API.Models;

namespace RaceDay.API.Services;

public class PasswordHasher : IPasswordHasher
{
    private static readonly byte[] Key = System.Text.Encoding.UTF8.GetBytes("RaceDay-Password-Hashing-Key-32-Chars!");

    public string Hash(string password, out string salt)
    {
        var saltBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(16);
        salt = Convert.ToBase64String(saltBytes);
        using var hmac = new HMACSHA256(Key);
        var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string password, string salt, string hash)
    {
        using var hmac = new HMACSHA256(Key);
        var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(hashBytes) == hash;
    }
}