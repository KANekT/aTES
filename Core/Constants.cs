namespace Core;

/// <summary>
/// Все константы проекта.
/// </summary>
public static class Constants
{
    public static class KafkaTopic
    {
        public const string RequestTime = "request_times";
        
        public const string AccountCreatedStream = "account_created_stream";
        public const string AccountRoleChange = "account_role_change";
        public const string TaskCreatedStream = "task_created_stream";
        public const string TaskPropertiesMutation = "task_properties_mutation";
    }
}