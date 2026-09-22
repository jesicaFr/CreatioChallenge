using System.Threading;
using System.Threading.Tasks;

namespace CreatioChallengeBack.Interfaces
{
    public interface IOAuthTokenService
    {
        /// <summary>
        /// Obtiene un access token válido para consumir Creatio. Implementación debe cachear el token en memoria.
        /// </summary>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
