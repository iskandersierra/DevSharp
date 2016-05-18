using System.Runtime.Remoting.Messaging;
using DevSharp;
using DevSharp.Annotations;

namespace Samples.TodoList
{
    public static class Events
    {
        [AggregateEvent(IsCreation = true)]
        public class Created
        {
            public Created(string title)
            {
                Title = title;
            }

            public string Title { get; }

            protected bool Equals(Created other)
            {
                return string.Equals(Title, other.Title);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Created) obj);
            }

            public override int GetHashCode()
            {
                return Title?.GetHashCode() ?? 0;
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateEvent]
        public class TaskAdded
        {
            public TaskAdded(int id, string description)
            {
                Id = id;
                Description = description;
            }

            public int Id { get; }
            public string Description { get; }

            protected bool Equals(TaskAdded other)
            {
                return Id == other.Id && string.Equals(Description, other.Description);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TaskAdded) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id*397) ^ (Description?.GetHashCode() ?? 0);
                }
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateEvent]
        public class TaskUpdated
        {
            public TaskUpdated(int id, string description)
            {
                Id = id;
                Description = description;
            }

            public int Id { get; }
            public string Description { get; }

            protected bool Equals(TaskUpdated other)
            {
                return Id == other.Id && string.Equals(Description, other.Description);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TaskUpdated) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id*397) ^ (Description?.GetHashCode() ?? 0);
                }
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateEvent]
        public class TaskRemoved
        {
            public TaskRemoved(int id)
            {
                Id = id;
            }

            public int Id { get; }

            protected bool Equals(TaskRemoved other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((TaskRemoved) obj);
            }

            public override int GetHashCode()
            {
                return Id;
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateEvent]
        public class Checked
        {
            public Checked(int id, bool isDone)
            {
                Id = id;
                IsDone = isDone;
            }

            public int Id { get; }
            public bool IsDone { get; }

            protected bool Equals(Checked other)
            {
                return Id == other.Id && IsDone == other.IsDone;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Checked) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id*397) ^ IsDone.GetHashCode();
                }
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }
    }
}