using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Accounting.Kafka;

public class AccountAddedConsumer : BaseConsumer<Null, AccountAddedProto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AccountAddedConsumer(IKafkaOptions options, IUserRepository userRepository, ITransactionRepository transactionRepository)
        : base(options, Constants.KafkaTopic.AccountPropertiesMutation)
    {
        _userRepository = userRepository;
        _transactionRepository = transactionRepository;
    }

    protected override async Task Consume(ConsumeResult<Null, AccountAddedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<Null, AccountAddedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }

    private async Task RequestToDb(ConsumeResult<Null, AccountAddedProto> result, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                   await _userRepository.Create(result.Message.Value.PublicId, cancellationToken);

        await _transactionRepository.Create(user.Ulid, TransactionTypeEnum.Init, 0, cancellationToken);
    }
}