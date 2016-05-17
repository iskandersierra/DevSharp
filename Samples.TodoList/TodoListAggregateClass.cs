using System;
using System.Collections.Generic;
using System.Linq;
using DevSharp.Domain;
using FluentValidation;
using FluentValidation.Results;

namespace Samples.TodoList
{
    /// <summary>
    /// This class will be generated on the fly or Reflection will be used on early development stages
    /// </summary>
    public class TodoListAggregateClass : IAggregateClass
    {
        public ValidationResult ValidateCommand(object command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandType = command.GetType();

            Type[] types;
            if (!ValidatorsMap.TryGetValue(commandType, out types))
                throw new ArgumentOutOfRangeException(nameof(command), $"Unknown command type {commandType.FullName}");

            var validators = types
                    .Select(Activator.CreateInstance)
                    .Cast<IValidator>()
                    .ToArray();

            var failures = validators.SelectMany(v => v.Validate(command).Errors);

            var result = new ValidationResult(failures);

            return result;
        }

        public IEnumerable<object> ExecuteCommand(object currentState, object command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var commandType = command.GetType();

            Func<object, object, IEnumerable<object>> func;
            if (!WhenCommandMap.TryGetValue(commandType, out func))
                throw new ArgumentOutOfRangeException(nameof(command), $"Unknown command type {commandType.FullName}");

            var result = func(command, currentState);

            return result;
        }

        public object ApplyEvent(object currentState, object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var eventType = @event.GetType();

            Func<object, object, object> func;
            if (!OnEventMap.TryGetValue(eventType, out func))
                throw new ArgumentOutOfRangeException(nameof(@event), $"Unknown command type {eventType.FullName}");

            var result = func(@event, currentState);

            return result;
        }

        #region [ Types ]

        private static readonly Type CreateType        = typeof (Commands.Create);
        private static readonly Type CheckAllType      = typeof (Commands.CheckAll);
        private static readonly Type RemoveAllDoneType = typeof (Commands.RemoveAllDone);
        private static readonly Type RemoveAllType     = typeof (Commands.RemoveAll);
        private static readonly Type CheckType         = typeof (Commands.Check);
        private static readonly Type AddTaskType       = typeof (Commands.AddTask);
        private static readonly Type RemoveTaskType    = typeof (Commands.RemoveTask);
        private static readonly Type UpdateTaskType    = typeof (Commands.UpdateTask);
        private static readonly Type CreatedType       = typeof (Events.Created);
        private static readonly Type CheckedType       = typeof (Events.Checked);
        private static readonly Type TaskAddedType     = typeof (Events.TaskAdded);
        private static readonly Type TaskRemovedType   = typeof (Events.TaskRemoved);
        private static readonly Type TaskUpdatedType   = typeof (Events.TaskUpdated);
        #endregion [ Types ]
        private readonly TodoListClass instance;

        private readonly Dictionary<Type, Func<object, object, IEnumerable<object>>> WhenCommandMap;
        private readonly Dictionary<Type, Func<object, object, object>> OnEventMap;
        private readonly Dictionary<Type, Type[]> ValidatorsMap;

        public TodoListAggregateClass()
        {
            instance = new TodoListClass();
            WhenCommandMap = new Dictionary<Type, Func<object, object, IEnumerable<object>>>
            {
                { CreateType,        (command, state) => instance.WhenCommand((Commands.Create)command) },
                { CheckAllType,      (command, state) => instance.WhenCommand((Commands.CheckAll)command,      (TodoListClass.State)state) },
                { RemoveAllDoneType, (command, state) => instance.WhenCommand((Commands.RemoveAllDone)command, (TodoListClass.State)state) },
                { RemoveAllType,     (command, state) => instance.WhenCommand((Commands.RemoveAll)command,     (TodoListClass.State)state) },
                { CheckType,         (command, state) => instance.WhenCommand((Commands.Check)command,         (TodoListClass.State)state) },
                { AddTaskType,       (command, state) => instance.WhenCommand((Commands.AddTask)command,       (TodoListClass.State)state) },
                { RemoveTaskType,    (command, state) => instance.WhenCommand((Commands.RemoveTask)command,    (TodoListClass.State)state) },
                { UpdateTaskType,    (command, state) => instance.WhenCommand((Commands.UpdateTask)command,    (TodoListClass.State)state) },
            };

            OnEventMap = new Dictionary<Type, Func<object, object, object>>
            {
                { CreatedType,     (@event, state) => instance.OnEvent((Events.Created)@event) },
                { CheckedType,     (@event, state) => instance.OnEvent((Events.Checked)@event,     (TodoListClass.State)state) },
                { TaskAddedType,   (@event, state) => instance.OnEvent((Events.TaskAdded)@event,   (TodoListClass.State)state) },
                { TaskRemovedType, (@event, state) => instance.OnEvent((Events.TaskRemoved)@event, (TodoListClass.State)state) },
                { TaskUpdatedType, (@event, state) => instance.OnEvent((Events.TaskUpdated)@event, (TodoListClass.State)state) },
            };

            ValidatorsMap = new Dictionary<Type, Type[]>
            {
                { CreateType, new []{ typeof(Commands.CreateValidator)} },
                { AddTaskType, new []{ typeof(Commands.AddTaskValidator) } },
                { UpdateTaskType, new []{ typeof(Commands.UpdateTaskValidator) } },
                { RemoveTaskType, new []{ typeof(Commands.RemoveTaskValidator) } },
                { CheckType, new []{ typeof(Commands.CheckValidator) } },
                { RemoveAllType, new Type[]{ } },
                { RemoveAllDoneType, new Type[]{ } },
                { CheckAllType, new Type[]{ } },
            };
        }
    }
}
