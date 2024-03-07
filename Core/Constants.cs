namespace Core;

/// <summary>
/// Все константы проекта.
/// </summary>
public static class Constants
{
    public static class KafkaTopic
    {
        public const string RequestTime = "request_times";
        
        public const string AccountStreaming = "account_streaming";
        public const string AccountRoleChange = "account_role_change";
        public const string AccountBalanceChange = "account_balance_change";
        public const string TaskStreaming = "task_streaming";
        public const string TaskPropertiesMutation = "task_properties_mutation";
    }
    
    public static class KafkaEvent
    {
        public const string AccountCreated = "account.created";
        public const string AccountRoleChanged = "account.role_changed";
        public const string TaskCreated = "task.created";
        public const string TaskAssign = "task.assign";
        public const string TaskComplete = "task.complete";
        public const string AccountBalance = "account.balance_updated";
    }
}