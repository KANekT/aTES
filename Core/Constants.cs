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
        public const string BillingStreaming = "billing_streaming";
    }
    
    public static class KafkaEvent
    {
        public const string AccountCreated = "account.created";
        public const string AccountRoleChanged = "account.role_changed";
        public const string TaskCreated = "task.created";
        public const string TaskAdded = "task.added";
        public const string TaskAssigned = "task.assigned";
        public const string TaskCompleted = "task.completed";
        public const string TransactionCreated = "transaction.created";
        public const string TaskPriceSet = "task.price_set";
    }
}