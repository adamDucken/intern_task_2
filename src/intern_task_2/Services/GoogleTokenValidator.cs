using Google.Apis.Auth;

namespace intern_task_2.Services;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly IConfiguration _configuration;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<GoogleTokenPayload?> ValidateAsync(string idToken)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];

            var settings = new GoogleJsonWebSignature.ValidationSettings();

            if (!string.IsNullOrWhiteSpace(clientId))
                settings.Audience = new[] { clientId };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            return new GoogleTokenPayload
            {
                Email = payload.Email,
                Subject = payload.Subject
            };
        }
        catch
        {
            return null;
        }
    }
}
