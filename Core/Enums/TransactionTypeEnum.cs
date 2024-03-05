namespace Core.Enums;

public enum TransactionTypeEnum: byte
{
    Init = 0,
    /// <summary>
    /// Зачисление на счет компании
    /// </summary>
    Enrollment = 10,
    /// <summary>
    /// Снятие со счета компании
    /// </summary>
    Withdrawal = 20,
    /// <summary>
    /// Выплата со счета компании
    /// </summary>
    Reward = 30,
}