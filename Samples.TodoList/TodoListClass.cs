using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DevSharp;
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
            return s.Tasks
                .Where(t => t.IsDone != c.IsDone)
                .Select(t => new Events.Checked(t.Id, c.IsDone));
            //if (s.Tasks.Any(t => t.IsDone != c.IsDone))
            //    yield return new Events.AllChecked(c.IsDone);
        }

        public IEnumerable<object> WhenCommand(Commands.RemoveAllDone c, State s)
        {
            return s.Tasks
                .Where(t => t.IsDone)
                .Select(t => new Events.TaskRemoved(t.Id));
            //if (s.Tasks.Any(t => t.IsDone))
            //    yield return new Events.AllDoneRemoved();
        }

        public IEnumerable<object> WhenCommand(Commands.RemoveAll c, State s)
        {
            return s.Tasks
                .Select(t => new Events.TaskRemoved(t.Id));
            //if (s.Tasks.Any())
            //    yield return new Events.AllRemoved();
        }

        public IEnumerable<object> WhenCommand(Commands.Check c, State s)
        {
            if (s.Tasks.Any(t => t.Id == c.Id && t.IsDone != c.IsDone))
                yield return new Events.Checked(c.Id, c.IsDone);
        }

        public IEnumerable<object> WhenCommand(Commands.AddTask c, State s)
        {
            yield return new Events.TaskAdded(s.NextId, c.Description);
        }

        public IEnumerable<object> WhenCommand(Commands.RemoveTask c, State s)
        {
            if (s.Tasks.Any(t => t.Id == c.Id))
                yield return new Events.TaskRemoved(c.Id);
        }

        public IEnumerable<object> WhenCommand(Commands.UpdateTask c, State s)
        {
            if (s.Tasks.Any(t => t.Id == c.Id && t.Description != c.Description))
                yield return new Events.TaskUpdated(c.Id, c.Description);
        }

        #endregion

        #region [ States ]

        public class State
        {
            public State(string title, ImmutableList<TodoTask> tasks, int nextId)
            {
                if (tasks == null) throw new ArgumentNullException(nameof(tasks));

                Title = title;
                Tasks = tasks;
                NextId = nextId;
            }

            public string Title { get; }
            public ImmutableList<TodoTask> Tasks { get; }
            public int NextId { get; }

            protected bool Equals(State other)
            {
                return string.Equals(Title, other.Title) && Tasks.SequenceEqual(other.Tasks) && NextId == other.NextId;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((State) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Title?.GetHashCode() ?? 0;
                    hashCode = (hashCode*397) ^ NextId;
                    return hashCode;
                }
            }

            public override string ToString()
            {
                return this.ToJsonText();
            }
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

            protected bool Equals(TodoTask other)
            {
                return Id == other.Id && string.Equals(Description, other.Description) && IsDone == other.IsDone;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((TodoTask) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Id;
                    hashCode = (hashCode*397) ^ (Description?.GetHashCode() ?? 0);
                    hashCode = (hashCode*397) ^ IsDone.GetHashCode();
                    return hashCode;
                }
            }

            public override string ToString()
            {
                return this.ToJsonText();
            }
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
