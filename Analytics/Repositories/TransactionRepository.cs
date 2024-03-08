using Analytics.Models;
using Core;
using Core.Enums;

namespace Analytics.Repositories;

public class TransactionRepository : GenericRepository<TransactionDto>, ITransactionRepository
{
    public TransactionRepository(DapperContext context) : base(context)
    {

    }

    public async Task<decimal> GetTopMoney(DateTime date, CancellationToken cancellationToken)
    {
        var all = await GetAll(cancellationToken);
        var money = all.Where(x =>
            x.CreatedAt.Date == date &&
            x.Type is TransactionTypeEnum.Withdrawal or TransactionTypeEnum.Enrollment)
            .Sum(x => x.Money);

        return money > 0 ? money : 0;
    }

    public async Task<string[]> GetBankrupts(DateTime date, CancellationToken cancellationToken)
    {
        var all = await GetAll(cancellationToken);
        var transactions = all.Where(x => x.CreatedAt.Date == date).ToLookup(x => x.PoPugId);
        
        // выбираем попоугов у которых нет транзакции выплаты за день и считаем их баланс
        return (from transaction in transactions 
                where transaction.FirstOrDefault(t => t.Type == TransactionTypeEnum.Reward) == null 
                where transaction.Sum(x => x.Money) < 0 select transaction.Key)
            .ToArray();
    }
}