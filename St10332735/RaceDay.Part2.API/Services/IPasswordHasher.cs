namespace RaceDay.API.Services;

public interface IPasswordHasher
{
    string Hash(string password, out string salt);
    bool Verify(string password, string salt, string hash);
}