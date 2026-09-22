using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CreatioChallengeBack.Dtos;

namespace CreatioChallengeBack.Interfaces
{
    public interface ICreatioClient
    {
        // Example operation: obtener contactos desde Creatio
        Task<IEnumerable<CreatioContactDto>> GetContactsAsync(CancellationToken cancellationToken = default);

        // Obtener cuentas con búsqueda y paginación server-side.
        Task<CreatioChallengeBack.Dtos.PagedResult<CreatioChallengeBack.Dtos.AccountListItemDto>> GetAccountsAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

        // Crear una nueva cuenta en Creatio a partir del payload standard.
        Task<object> CreateAccountAsync(CreatioChallengeBack.Dtos.CreateAccountRequestDto request, CancellationToken cancellationToken = default);

        // Aquí pueden agregarse más métodos necesarios por el skill (consultas OData, operaciones específicas)
    }
}
