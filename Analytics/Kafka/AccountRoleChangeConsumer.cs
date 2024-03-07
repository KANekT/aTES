using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Accounting.Kafka;

public class AccountRoleChangeConsumer : BaseConsumer<string, AccountRoleChangedProto>
{
    private readonly IUserRepository _userRepository;
    
    public AccountRoleChangeConsumer(IKafkaOptions options, IUserRepository userRepository) : base(options, Constants.KafkaTopic.AccountRoleChange)
    {
        _userRepository = userRepository;
    }

    protected override async Task Consume(ConsumeResult<string, AccountRoleChangedProto> result, CancellationToken cancellationToken)
    {
        await RequestToDb(result, cancellationToken);
    }

    protected override async Task ConsumeBatch(IEnumerable<ConsumeResult<string, AccountRoleChangedProto>> results, CancellationToken cancellationToken)
    {
        foreach (var result in results)
        {
            await RequestToDb(result, cancellationToken);
        }
    }
    
    private async Task RequestToDb(ConsumeResult<string, AccountRoleChangedProto> result, CancellationToken cancellationToken)
    {
        if (result.Message.Value.Base.EventName == Constants.KafkaEvent.AccountRoleChanged)
        {
            var role = (RoleEnum)result.Message.Value.Role;
            await _userRepository.RoleChange(result.Message.Value.PublicId, role, cancellationToken);
        }
    }
}