using System.Threading;
using System.Threading.Tasks;

namespace CreatioChallengeBack.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Obtains a valid access token. Uses in-memory cache and refresh when needed.
        /// </summary>
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
