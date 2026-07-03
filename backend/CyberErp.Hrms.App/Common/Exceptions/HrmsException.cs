using System.Runtime.Serialization;

namespace CyberErp.Hrms.App.Common.Exceptions
{
    /// <summary>
    /// Base exception for all CyberErp.Hrms application exceptions
    /// </summary>
    [Serializable]
    public class HrmsException : Exception
    {
        public string ErrorCode { get; }

        public HrmsException()
            : base()
        {
            ErrorCode = "BB000";
        }

        public HrmsException(string message)
            : base(message)
        {
            ErrorCode = "BB000";
        }

        public HrmsException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public HrmsException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = "BB000";
        }

        public HrmsException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        protected HrmsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString(nameof(ErrorCode)) ?? "BB000";
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), ErrorCode);
        }
    }

    /// <summary>
    /// Exception thrown when a resource is not found
    /// </summary>
    [Serializable]
    public class NotFoundException : HrmsException
    {
        public string ResourceId { get; }
        public string ResourceType { get; }

        public NotFoundException(string resourceType, string resourceId)
            : base($"Resource of type '{resourceType}' with id '{resourceId}' was not found.", "BB001")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        public NotFoundException(string resourceType, string resourceId, string message)
            : base(message, "BB001")
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }

        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ResourceId = info.GetString(nameof(ResourceId)) ?? string.Empty;
            ResourceType = info.GetString(nameof(ResourceType)) ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ResourceId), ResourceId);
            info.AddValue(nameof(ResourceType), ResourceType);
        }
    }

    /// <summary>
    /// Exception thrown when a duplicate resource is found
    /// </summary>
    [Serializable]
    public class DuplicateException : HrmsException
    {
        public string ResourceType { get; }
        public string DuplicateField { get; }
        public string FieldValue { get; }

        public DuplicateException(string resourceType, string duplicateField, string fieldValue)
            : base($"Duplicate {resourceType} found with {duplicateField}: '{fieldValue}'", "BB002")
        {
            ResourceType = resourceType;
            DuplicateField = duplicateField;
            FieldValue = fieldValue;
        }

        protected DuplicateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ResourceType = info.GetString(nameof(ResourceType)) ?? string.Empty;
            DuplicateField = info.GetString(nameof(DuplicateField)) ?? string.Empty;
            FieldValue = info.GetString(nameof(FieldValue)) ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ResourceType), ResourceType);
            info.AddValue(nameof(DuplicateField), DuplicateField);
            info.AddValue(nameof(FieldValue), FieldValue);
        }
    }

    /// <summary>
    /// Exception thrown when validation fails
    /// </summary>
    [Serializable]
    public class ValidationException : HrmsException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors)
            : base("Validation failed for one or more fields.", "BB003")
        {
            Errors = errors;
        }

        public ValidationException(string field, string message)
            : base(message, "BB003")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            };
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Errors = info.GetValue(nameof(Errors), typeof(IDictionary<string, string[]>)) as IDictionary<string, string[]>
                     ?? new Dictionary<string, string[]>();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Errors), Errors);
        }
    }

    /// <summary>
    /// Exception thrown when an authorization check fails
    /// </summary>
    [Serializable]
    public class UnauthorizedException : HrmsException
    {
        public string UserId { get; }
        public string RequiredPermission { get; }

        public UnauthorizedException(string userId, string requiredPermission)
            : base($"User '{userId}' is not authorized to perform this action. Required permission: '{requiredPermission}'", "BB004")
        {
            UserId = userId;
            RequiredPermission = requiredPermission;
        }

        public UnauthorizedException(string message)
            : base(message, "BB004")
        {
            UserId = string.Empty;
            RequiredPermission = string.Empty;
        }

        protected UnauthorizedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            UserId = info.GetString(nameof(UserId)) ?? string.Empty;
            RequiredPermission = info.GetString(nameof(RequiredPermission)) ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(UserId), UserId);
            info.AddValue(nameof(RequiredPermission), RequiredPermission);
        }
    }

    /// <summary>
    /// Exception thrown when a database operation fails
    /// </summary>
    [Serializable]
    public class DatabaseException : HrmsException
    {
        public string Operation { get; }
        public string EntityType { get; }

        public DatabaseException(string operation, string entityType, Exception innerException)
            : base($"Database {operation} failed for entity '{entityType}': {innerException.Message}", "BB005", innerException)
        {
            Operation = operation;
            EntityType = entityType;
        }

        public DatabaseException(string message, Exception innerException)
            : base(message, "BB005", innerException)
        {
            Operation = string.Empty;
            EntityType = string.Empty;
        }

        protected DatabaseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Operation = info.GetString(nameof(Operation)) ?? string.Empty;
            EntityType = info.GetString(nameof(EntityType)) ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Operation), Operation);
            info.AddValue(nameof(EntityType), EntityType);
        }
    }

    /// <summary>
    /// Exception thrown when a circuit breaker is open
    /// </summary>
    [Serializable]
    public class CircuitBreakerOpenException : HrmsException
    {
        public DateTimeOffset OpenUntil { get; }
        public string CircuitName { get; }

        public CircuitBreakerOpenException(string circuitName, DateTimeOffset openUntil)
            : base($"Circuit breaker '{circuitName}' is open until {openUntil:yyyy-MM-dd HH:mm:ss}. Please try again later.", "BB006")
        {
            CircuitName = circuitName;
            OpenUntil = openUntil;
        }

        protected CircuitBreakerOpenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            OpenUntil = info.GetDateTime(nameof(OpenUntil));
            CircuitName = info.GetString(nameof(CircuitName)) ?? string.Empty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(OpenUntil), OpenUntil);
            info.AddValue(nameof(CircuitName), CircuitName);
        }
    }
}

