using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VincYonetim.Infrastructure
{
    /// <summary>
    /// Mobil API isteklerinde "X-Api-Key" başlığını doğrular.
    /// Anahtar koda veya appsettings.json'a yazılmaz; "Api:Key" ayarından
    /// (user-secrets veya Api__Key ortam değişkeni) okunur. Ayar yoksa tüm istekler reddedilir.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        public const string HeaderName = "X-Api-Key";
        private const int MinKeyLength = 32;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expected = configuration["Api:Key"];

            if (string.IsNullOrEmpty(expected) || expected.Length < MinKeyLength)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
                || !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(provided.ToString()),
                    Encoding.UTF8.GetBytes(expected)))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
