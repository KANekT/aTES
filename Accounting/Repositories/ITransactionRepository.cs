using Accounting.Models;
using Core;
using Core.Enums;

namespace Accounting.Repositories;

public interface ITransactionRepository: IGenericRepository<TransactionDto>
{
    public Task<TransactionDto?> Create(string publicId, TransactionTypeEnum type, decimal money, CancellationToken cancellationToken);

    public Task<decimal> GetTopMoney(DateTime utcNowDate, CancellationToken cancellationToken);
}