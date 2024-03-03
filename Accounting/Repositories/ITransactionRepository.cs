using Accounting.Models;
using Core.Enums;

namespace Accounting.Repositories;

public interface ITransactionRepository
{
    public Task<TransactionDto?> Create(string publicId, RoleEnum role, CancellationToken cancellationToken);

}