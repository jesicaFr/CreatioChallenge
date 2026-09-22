using System.Threading;
using System.Threading.Tasks;

namespace CreatioChallengeBack.Services
{
    public interface ICreatioApiClient
    {
        Task<string> GetStringAsync(string relativeUrl, CancellationToken cancellationToken = default);
    }
}
