using System.Collections.Generic;
using DevFSharp;

namespace DevSharp.Domain
{
    public interface IAggregateClass
    {
        IEnumerable<Validations.ValidationResult> ValidateCommand(object command);
        IEnumerable<object> ExecuteCommand(object currentState, object command);
        object ApplyEvent(object currentState, object @event);
    }
}
