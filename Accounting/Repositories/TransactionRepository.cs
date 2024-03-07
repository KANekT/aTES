using Accounting.Models;
using Core;
using Core.Enums;

namespace Accounting.Repositories;

public class TransactionRepository : GenericRepository<TransactionDto>, ITransactionRepository
{
    public TransactionRepository(DapperContext context) : base(context)
    {

    }

    public async Task<TransactionDto?> Create(string publicId, TransactionTypeEnum type, decimal money, CancellationToken cancellationToken)
    {
        var transactionDto = new TransactionDto
        {
            Ulid = Ulid.NewUlid().ToString(),
            CreatedAt = DateTime.UtcNow,
            Type = type,
            PoPugId = publicId,
            Money = money,
        };
        
        var transactionAdd = await Add(transactionDto, cancellationToken);
        return transactionAdd ? transactionDto : null;
    }

    public async Task<decimal> GetTopMoney(DateTime utcNowDate, CancellationToken cancellationToken)
    {
        var all = await GetAll(cancellationToken);
        var money = all.Where(x =>
            x.CreatedAt.Date == utcNowDate &&
            x.Type is TransactionTypeEnum.Withdrawal or TransactionTypeEnum.Enrollment)
            .Sum(x => x.Money);

        return money > 0 ? money : 0;
    }
}