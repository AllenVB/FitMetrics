using FitMetrics.Application.Common.Interfaces;
using BCryptNet = BCrypt.Net.BCrypt;

namespace FitMetrics.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCryptNet.HashPassword(password);

    public bool Verify(string password, string hash) => BCryptNet.Verify(password, hash);
}
