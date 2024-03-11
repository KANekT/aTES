using Accounting.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Accounting.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AccountingController : ControllerBase
{
    private readonly ILogger<AccountingController> _logger;
    private readonly ITransactionRepository _transactionRepository;

    public AccountingController(
        ILogger<AccountingController> logger,
        ITransactionRepository transactionRepository)
    {
        _logger = logger;
        _transactionRepository = transactionRepository;
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> GetMoneyForTop(
        CancellationToken cancellationToken
    )
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var tasks = await _transactionRepository.GetTopMoney(DateTime.UtcNow.Date, cancellationToken);
        return Ok(tasks);
    }
    
    private string? CheckUser()
    {
        var identity = HttpContext.User.Identity;
        return identity is not { IsAuthenticated: true } ? null : identity.Name;
    }
}
