using FitMetrics.Domain.Entities;

namespace FitMetrics.Application.Common.Interfaces;

public record AuthToken(string Token, DateTime ExpiresAt);

/// <summary>JWT üretimi. Implementasyon Infrastructure katmanındadır.</summary>
public interface ITokenService
{
    AuthToken GenerateToken(User user);
}
