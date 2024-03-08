using Analytics.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly ITaskRepository _taskRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AnalyticsController(
        ILogger<AnalyticsController> logger,
        ITaskRepository taskRepository,
        ITransactionRepository transactionRepository)
    {
        _logger = logger;
        _taskRepository = taskRepository;
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

    
    [HttpGet("[action]")]
    public async Task<IActionResult> GetBankrupts(
        CancellationToken cancellationToken
    )
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var users = await _transactionRepository.GetBankrupts(DateTime.UtcNow.Date, cancellationToken);
        return Ok(users.Length);
    }

    
    [HttpGet("[action]")]
    public async Task<IActionResult> MostExpensiveTask(
        CancellationToken cancellationToken
    )
    {
        var userName = CheckUser();
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized();
        }

        var tasks = await _taskRepository.MostExpensiveTask(DateTime.UtcNow.Date, DateTime.UtcNow.Date, cancellationToken);
        return Ok(tasks);
    }

    private string? CheckUser()
    {
        var identity = HttpContext.User.Identity;
        return identity is not { IsAuthenticated: true } ? null : identity.Name;
    }
}
