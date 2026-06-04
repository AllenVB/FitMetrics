namespace FitMetrics.Application.Common.Interfaces;

/// <summary>Parola hash'leme/doğrulama. Implementasyon (BCrypt) Infrastructure'dadır.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
