using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DevSharp.Annotations;
using DevSharp.Domain;
using FluentValidation;

namespace Samples.TodoList
{
    [AggregateRoot]
    public class TodoListClass
    {
        #region [ Event Handlers ]

        public State OnEvent(Events.Created e)
        {
            return new State(
                e.Title,
                ImmutableList<TodoTask>.Empty,
                1);
        }

        public State OnEvent(Events.AllChecked e, State s)
        {
            return new State(
                s.Title,
                s.Tasks.Select(t => t.IsDone == e.IsDone ? t : new TodoTask(t.Id, t.Description, e.IsDone))
                    .ToImmutableList(),
                s.NextId);
        }

        public State OnEvent(Events.AllDoneRemoved e, State s)
        {
            return new State(
                s.Title,
                s.Tasks.Where(t => !t.IsDone).ToImmutableList(),
                s.NextId);
        }

        public State OnEvent(Events.AllRemoved e, State s)
        {
            return new State(
                s.Title,
                ImmutableList<TodoTask>.Empty,
                s.NextId); // It could be 1 again but this is not a good practice
        }

        public State OnEvent(Events.Checked e, State s)
        {
            return new State(
                s.Title,
                s.Tasks.Select(
                    t => t.Id != e.Id || t.IsDone == e.IsDone ? t : new TodoTask(t.Id, t.Description, e.IsDone))
                    .ToImmutableList(),
                s.NextId);
        }

        public State OnEvent(Events.TaskAdded e, State s)
        {
            return new State(
                s.Title,
                s.Tasks.Add(new TodoTask(e.Id, e.Description, false)),
                s.NextId + 1);
        }

        public State OnEvent(Events.TaskRemoved e, State s)
        {
            return new State(
                s.Title,
                s.Tasks.Where(t => t.Id != e.Id).ToImmutableList(),
                s.NextId);
        }

        public State OnEvent(Events.TaskUpdated e, State s)
        {
            return new State(
                s.Title,
                s.Tasks.Select(
                    t =>
                        t.Id != e.Id || t.Description == e.Description ? t : new TodoTask(t.Id, e.Description, t.IsDone))
                    .ToImmutableList(),
                s.NextId);
        }

        #endregion

        #region [ Command Handlers ]

        public IEnumerable<object> WhenCommand(Commands.Create c)
        {
            yield return new Events.Created(c.Title);
        }

        public IEnumerable<object> WhenCommand(Commands.CheckAll c, State s)
        {
            if (s.Tasks.All(t => t.IsDone == c.IsDone))
                yield return new NothingHappened();
            else
                yield return new Events.AllChecked(c.IsDone);
        }

        public IEnumerable<object> WhenCommand(Commands.RemoveAllDone c, State s)
        {
            if (s.Tasks.All(t => !t.IsDone))
                yield return new NothingHappened();
            else
                yield return new Events.AllDoneRemoved();
        }

        public IEnumerable<object> WhenCommand(Commands.RemoveAll c, State s)
        {
            if (!s.Tasks.Any())
                yield return new NothingHappened();
            else
                yield return new Events.AllRemoved();
        }

        public IEnumerable<object> WhenCommand(Commands.Check c, State s)
        {
            if (s.Tasks.All(t => t.Id != c.Id || t.IsDone == c.IsDone))
                yield return new NothingHappened();
            else
                yield return new Events.Checked(c.Id, c.IsDone);
        }

        public IEnumerable<object> WhenCommand(Commands.AddTask c, State s)
        {
            yield return new Events.TaskAdded(s.NextId, c.Description);
        }

        public IEnumerable<object> WhenCommand(Commands.RemoveTask c, State s)
        {
            if (s.Tasks.All(t => t.Id != c.Id))
                yield return new NothingHappened();
            else
                yield return new Events.TaskRemoved(c.Id);
        }

        public IEnumerable<object> WhenCommand(Commands.UpdateTask c, State s)
        {
            if (s.Tasks.All(t => t.Id != c.Id || t.Description == c.Description))
                yield return new NothingHappened();
            else
                yield return new Events.TaskUpdated(c.Id, c.Description);
        }

        #endregion

        #region [ States ]

        public class State
        {
            public State(string title, ImmutableList<TodoTask> tasks, int nextId)
            {
                Title = title;
                Tasks = tasks;
                NextId = nextId;
            }

            public string Title { get; }
            public ImmutableList<TodoTask> Tasks { get; }
            public int NextId { get; }
        }

        public class TodoTask
        {
            public TodoTask(int id, string description, bool isDone)
            {
                Id = id;
                Description = description;
                IsDone = isDone;
            }

            public int Id { get; }
            public string Description { get; }
            public bool IsDone { get; }
        }

        #endregion

        #region [ Preconditions ]

        public class RemoveTaskPrecondition : AbstractValidator<CommandPrecondition<Commands.RemoveTask, State>>
        {
            public RemoveTaskPrecondition()
            {
                // This is for example purposes. A command must be idempotent as much as possible, so if the task is already removed it should not be a precondition error
                // RuleFor(c => c.Command.Id).Must((e, id) => e.State.Tasks.Any(t => t.Id == id));
            }
        }

        #endregion
    }
}
