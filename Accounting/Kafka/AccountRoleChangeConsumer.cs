using Accounting.Repositories;
using Confluent.Kafka;
using Core;
using Core.Enums;
using Core.Kafka;
using Core.Options;

namespace Accounting.Kafka;

public class AccountRoleChangeConsumer : BaseConsumer<string, string>
{
    private readonly IUserRepository _userRepository;
    
    public AccountRoleChangeConsumer(IKafkaOptions options, IUserRepository userRepository) : base(options, Constants.KafkaTopic.AccountRoleChange)
    {
        _userRepository = userRepository;
    }

    protected override async Task Consume(ConsumeResult<string, string> result, CancellationToken cancellationToken)
    {
        var role = Enum.Parse<RoleEnum>(result.Message.Value);
        await _userRepository.RoleChange(result.Message.Key, role, cancellationToken);
    }
}