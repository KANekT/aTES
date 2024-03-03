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
            CreatedAt = DateTime.UtcNow,
            EditedAt = DateTime.UtcNow,
            Type = type,
            PoPugId = publicId,
            Money = money,
        };
        
        var transactionAdd = await Add(transactionDto, cancellationToken);
        return transactionAdd ? transactionDto : null;
    }
}