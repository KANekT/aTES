using Accounting.Models;
using Core;
using Core.Enums;

namespace Accounting.Repositories;

public class TransactionRepository : GenericRepository<TransactionDto>, ITransactionRepository
{
    public TransactionRepository(DapperContext context) : base(context)
    {

    }

    public Task<TransactionDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}