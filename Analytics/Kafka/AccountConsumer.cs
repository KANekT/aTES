using Analytics.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Analytics.Kafka;

public class AccountCreateConsumer : BaseConsumer<Null, AccountCreatedProto>
{
    private readonly IUserRepository _userRepository;
    
    public AccountCreateConsumer(IKafkaOptions options, IUserRepository userRepository)
        : base(options, Constants.KafkaTopic.AccountStreaming)
    {
        _userRepository = userRepository;
    }

    protected override async Task Consume(ConsumeResult<Null, AccountCreatedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<Null, AccountCreatedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }
    
    private async Task RequestToDb(ConsumeResult<Null, AccountCreatedProto> result, CancellationToken cancellationToken)
    {
        await _userRepository.Create(result.Message.Value.PublicId, (RoleEnum)result.Message.Value.Role, cancellationToken);
    }
}