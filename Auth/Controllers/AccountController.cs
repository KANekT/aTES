using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Models;
using Auth.Repositories;
using Confluent.Kafka;
using Core;
using Core.EventModels;
using Core.Extensions;
using Core.Kafka;
using Core.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

[AllowAnonymous]
[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly AuthenticationOptions _authenticationOptions;
    private readonly KafkaDependentProducer<Null, string> _producerNull;
    private readonly KafkaDependentProducer<string, string> _producerString;
    private readonly ILogger<AccountController> _logger;
    private readonly IUserRepository _userRepository;

    public AccountController(
        AuthenticationOptions authenticationOptions,
        KafkaDependentProducer<Null, string> producerNull,
        ILogger<AccountController> logger,
        IUserRepository userRepository,
        KafkaDependentProducer<string, string> producerString
    )
    {
        _authenticationOptions = authenticationOptions;
        _logger = logger;
        _userRepository = userRepository;
        _producerNull = producerNull;
        _producerString = producerString;
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
            var json = new AccountCreatedEventModel
            {
                PublicId = user.Ulid,
                Role = user.Role
            }.Decode();

            _producerNull.Produce(
                Constants.KafkaTopic.AccountCreatedStream,
                new Message<Null, string> { Value = json },
                _deliveryReportHandlerNull
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

        _producerString.Produce(
            Constants.KafkaTopic.AccountRoleChange,
            new Message<string, string> { Key = identity.Name, Value = model.Role.ToString("G") },
            _deliveryReportHandlerString
        );

        return Ok();
    }

    private void _deliveryReportHandlerNull(DeliveryReport<Null, string> deliveryReport)
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

    private void _deliveryReportHandlerString(DeliveryReport<string, string> deliveryReport)
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
