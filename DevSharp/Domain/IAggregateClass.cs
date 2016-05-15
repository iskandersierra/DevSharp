using System.Collections.Generic;
using FluentValidation.Results;

namespace DevSharp.Domain
{
    public interface IAggregateClass
    {
        ValidationResult ValidateCommand(object command);
        IEnumerable<object> ExecuteCommand(object currentState, object command);
        object ApplyEvent(object currentState, object @event);
    }
}
