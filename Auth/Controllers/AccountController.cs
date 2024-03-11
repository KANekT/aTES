using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Models;
using Auth.Repositories;
using Confluent.Kafka;
using Core;
using Core.Extensions;
using Core.Kafka;
using Core.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proto;
using Proto.V1;

namespace Auth.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly ILogger<AccountController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IKafkaDependentProducer<Null, AccountCreatedProto> _producerAccountCreated;
    private readonly IKafkaDependentProducer<string, AccountRoleChangedProto> _producerAccountRoleChanged;

    public AccountController(
        AuthenticationOptions authenticationOptions,
        ILogger<AccountController> logger,
        IUserRepository userRepository,
        IKafkaDependentProducer<Null, AccountCreatedProto> producerAccountCreated,
        IKafkaDependentProducer<string, AccountRoleChangedProto> producerAccountRoleChanged
    )
    {
        _authenticationOptions = authenticationOptions;
        _logger = logger;
        _userRepository = userRepository;
        _producerAccountCreated = producerAccountCreated;
        _producerAccountRoleChanged = producerAccountRoleChanged;
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInFormModel model,
        CancellationToken cancellationToken
    )
    {
        var user = await _userRepository.GetUser(model.Login, cancellationToken);
        if (user == null)
            return BadRequest("Error in login or password");

        var claims = new Claim[] { new(ClaimTypes.Role, user.Role.ToString("G")) };
        var token = _authenticationOptions.GenerateToken(user.Ulid, claims);
        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }

    [HttpPost("[action]")]
    public async Task SignUp([FromBody] SignUpFormModel model, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Create(model, cancellationToken);

        if (user != null)
        {
            var value = new AccountCreatedProto
            {
                Base = new BaseProto
                {
                    EventId = Guid.NewGuid().ToString("N"),
                    EventName = Constants.KafkaEvent.AccountCreated,
                    EventTime = DateTime.UtcNow.ToString("u"),
                    EventVersion = "1"
                },
                PublicId = user.Ulid,
                Role = (int)user.Role
            };

            _producerAccountCreated.Produce(
                Constants.KafkaTopic.AccountStreaming,
                new Message<Null, AccountCreatedProto> { Value = value },
                _deliveryReportHandlerAccountCreated
            );
            //await _producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });
        }
    }

    [Authorize]
    [HttpPost("[action]")]
    public async Task<IActionResult> UpdateRole(
        [FromBody] UpdateRoleFormModel model,
        CancellationToken cancellationToken
    )
    {
        var identity = HttpContext.User.Identity;
        if (identity is not { IsAuthenticated: true } || string.IsNullOrEmpty(identity.Name))
        {
            return Unauthorized();
        }

        await _userRepository.UpdateRole(identity.Name, model.Role, cancellationToken);

        var value = new AccountRoleChangedProto
        {
            Base = new BaseProto
            {
                EventId = Guid.NewGuid().ToString("N"),
                EventName = Constants.KafkaEvent.AccountRoleChanged,
                EventTime = DateTime.UtcNow.ToString("u"),
                EventVersion = "1"
            },
            PublicId = identity.Name,
            Role = (int)model.Role
        };
        
        _producerAccountRoleChanged.Produce(
            Constants.KafkaTopic.AccountRoleChange,
            new Message<string, AccountRoleChangedProto> { Key = identity.Name, Value = value },
            _deliveryReportHandlerAccountRoleChanged
        );

        return Ok();
    }

    private void _deliveryReportHandlerAccountCreated(DeliveryReport<Null, AccountCreatedProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // It is common to write application logs to Kafka (note: this project does not provide
            // an example logger implementation that does this). Such an implementation should
            // ideally fall back to logging messages locally in the case of delivery problems.
            _logger.Log(
                LogLevel.Warning,
                $"Message delivery failed: {deliveryReport.Message.Value}"
            );
        }
    }

    private void _deliveryReportHandlerAccountRoleChanged(DeliveryReport<string, AccountRoleChangedProto> deliveryReport)
    {
        if (deliveryReport.Status == PersistenceStatus.NotPersisted)
        {
            // It is common to write application logs to Kafka (note: this project does not provide
            // an example logger implementation that does this). Such an implementation should
            // ideally fall back to logging messages locally in the case of delivery problems.
            _logger.Log(
                LogLevel.Warning,
                $"Message delivery failed: {deliveryReport.Message.Value}"
            );
        }
    }
}
