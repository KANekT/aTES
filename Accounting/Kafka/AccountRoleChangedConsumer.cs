using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;
using Proto.V1;

namespace Accounting.Kafka;

public class AccountRoleChangedConsumer : BaseConsumer<string, AccountRoleChangedProto>
{
    private readonly IUserRepository _userRepository;
    
    public AccountRoleChangedConsumer(IKafkaOptions options, IUserRepository userRepository) : base(options, Constants.KafkaTopic.AccountRoleChange)
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
        var user = await _userRepository.GetByPublicId(result.Message.Value.PublicId, cancellationToken) ??
                   await _userRepository.Create(result.Message.Value.PublicId, cancellationToken);

        var role = (RoleEnum)result.Message.Value.Role;
        user.Role = role;
        await _userRepository.Update(user, cancellationToken);
    }
}