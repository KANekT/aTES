namespace Core.Enums;

public enum TransactionTypeEnum: byte
{
    Init = 0,
    /// <summary>
    /// Зачисление
    /// </summary>
    Enrollment = 10,
    /// <summary>
    /// Снятие
    /// </summary>
    Withdrawal = 20,
    /// <summary>
    /// Выплата
    /// </summary>
    Reward = 30,
}