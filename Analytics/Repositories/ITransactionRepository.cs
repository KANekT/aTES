using Analytics.Models;
using Core;
using Core.Enums;

namespace Analytics.Repositories;

public interface ITransactionRepository: IGenericRepository<TransactionDto>
{
    public Task<decimal> GetTopMoney(DateTime date, CancellationToken cancellationToken);
    Task<string[]> GetBankrupts(DateTime date, CancellationToken cancellationToken);
}