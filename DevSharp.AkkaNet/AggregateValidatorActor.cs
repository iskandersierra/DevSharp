using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Akka;
using Akka.Actor;
using DevSharp.Domain;
using FluentValidation.Results;

namespace DevSharp.AkkaNet
{
    public class AggregateValidatorActor : UntypedActor
    {
        private readonly IAggregateClass _aggregateClass;

        public AggregateValidatorActor(IAggregateClass aggregateClass)
        {
            _aggregateClass = aggregateClass;
        }

        #region [ States ]

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<ValidateCommand>(OnValidateCommand)
                .Default(Unhandled);
        }

        #endregion

        #region [ Handlers ]

        private void OnValidateCommand(ValidateCommand message)
        {
            var result = _aggregateClass.ValidateCommand(message);
            Sender.Tell(new ValidatedCommand(result));
        }

        #endregion

        #region [ Messages ]

        public class ValidateCommand
        {
            private readonly ReadOnlyDictionary<string, object> EmptyProperties =
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

            protected ValidateCommand(
                object command,
                IReadOnlyDictionary<string, object> properties)
            {
                if (command == null) throw new ArgumentNullException(nameof(command));
                Command = command;
                Properties = properties ?? EmptyProperties;
            }

            public object Command { get; }
            public IReadOnlyDictionary<string, object> Properties { get; }
        }

        public class ValidatedCommand
        {
            public ValidatedCommand(ValidationResult result)
            {
                if (result == null) throw new ArgumentNullException(nameof(result));
                this.Result = result;
            }

            public ValidationResult Result { get; }
        }

        #endregion
    }
}