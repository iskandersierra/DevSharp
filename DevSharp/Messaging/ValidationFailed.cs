using DevSharp.Annotations;
using FluentValidation.Results;

namespace DevSharp.Messaging
{
    [AggregateEvent(IsPure = true)]
    public class ValidationFailed
    {
        public ValidationFailed(ValidationResult validationResult)
        {
            ValidationResult = validationResult;
        }

        public ValidationResult ValidationResult { get; }
    }
}