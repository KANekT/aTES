using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.EventModels;
using Core.Extensions;
using Core.Kafka;
using Core.Options;

namespace Accounting.Kafka;

public class AccountCreateConsumer : BaseConsumer<Null, string>
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AccountCreateConsumer(IKafkaOptions options, IUserRepository userRepository) : base(options, Constants.KafkaTopic.AccountCreatedStream)
    {
        _userRepository = userRepository;
    }

    protected override async Task Consume(ConsumeResult<Null, string> result, CancellationToken cancellationToken)
    {
        var user = result.Message.Value.Encode<AccountCreatedEventModel>();
        await _userRepository.Create(user.PublicId, user.Role, cancellationToken);
    }
}