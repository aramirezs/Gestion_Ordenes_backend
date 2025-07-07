using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        IOrdenWriteRepository Ordenes { get; }
        IOrdenReadRepository OrdenesRead { get; } 
        Task CommitAsync();
        Task RollbackAsync();
    }
}
