using System;
using Akka;
using Akka.Actor;
using Akka.Routing;
using DevSharp.Domain;

namespace DevSharp.AkkaNet
{
    public class AggregateValidatorSupervisor : UntypedActor
    {
        private readonly IAggregateClass _aggregateClass;
        private int expectedValidatorsCount = 10;
        private IActorRef validators;

        public AggregateValidatorSupervisor(IAggregateClass aggregateClass)
        {
            _aggregateClass = aggregateClass;
        }

        protected override void PreStart()
        {
            base.PreStart();
            var props = Props.Create<AggregateValidatorActor>()
                .WithRouter(new SmallestMailboxPool(expectedValidatorsCount))
                .WithSupervisorStrategy(Akka.Actor.SupervisorStrategy.StoppingStrategy);

            validators = Context.ActorOf(props);
        }

        protected override void PostRestart(Exception reason)
        {
            base.PostRestart(reason);
            Context.Stop(validators);
            validators = null;
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                maxNrOfRetries: 10,
                withinTimeRange: TimeSpan.FromSeconds(30),
                decider: Decider.From(ex =>
                {
                    if (ex is NotSupportedException || ex is NotImplementedException) 
                        return Directive.Stop;
                    return Directive.Restart;
                }));
        }

        protected override void OnReceive(object message)
        {
            message.Match()
                .With<AggregateValidatorActor.ValidateCommand>(OnValidateCommand)
                .Default(Unhandled);
        }

        private void OnValidateCommand(AggregateValidatorActor.ValidateCommand message)
        {
            validators.Tell(message, Sender);
        }
    }
}