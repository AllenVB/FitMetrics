namespace FitMetrics.Application.Common.Exceptions;

/// <summary>Aranan kayıt bulunamadığında (HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entity, object key)
        : base($"{entity} (#{key}) bulunamadı.") { }
}

/// <summary>İş kuralı çakışması, örn. e-posta zaten kayıtlı (HTTP 409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Geçersiz kimlik bilgisi / yetkisiz işlem (HTTP 401).</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>Bağımlı bir servis (örn. Claude API) yapılandırılmadığında (HTTP 503).</summary>
public class AiNotConfiguredException : Exception
{
    public AiNotConfiguredException()
        : base("Yapay zekâ özelliği yapılandırılmamış. Sunucuda ANTHROPIC_API_KEY tanımlanmalıdır.") { }
}

/// <summary>Dış servis çağrısı başarısız olduğunda (HTTP 502).</summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message) : base(message) { }
}
