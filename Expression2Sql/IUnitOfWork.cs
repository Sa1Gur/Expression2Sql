using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework;

public interface IUnitOfWork
{
    int SaveChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
