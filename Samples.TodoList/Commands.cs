using DevSharp;
using DevSharp.Annotations;
using FluentValidation;

namespace Samples.TodoList
{
    public static class Commands
    {
        #region [ Commands ]

        [AggregateCommand(IsCreation = true)]
        public class Create
        {
            public Create(string title)
            {
                Title = title;
            }

            public string Title { get; }

            protected bool Equals(Create other)
            {
                return string.Equals(Title, other.Title);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Create) obj);
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

        [AggregateCommand]
        public class AddTask
        {
            public AddTask(string description)
            {
                Description = description;
            }

            public string Description { get; }

            protected bool Equals(AddTask other)
            {
                return string.Equals(Description, other.Description);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((AddTask) obj);
            }

            public override int GetHashCode()
            {
                return Description?.GetHashCode() ?? 0;
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateCommand]
        public class UpdateTask
        {
            public UpdateTask(int id, string description)
            {
                Id = id;
                Description = description;
            }

            public int Id { get; }
            public string Description { get; }

            protected bool Equals(UpdateTask other)
            {
                return Id == other.Id && string.Equals(Description, other.Description);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((UpdateTask) obj);
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

        [AggregateCommand]
        public class RemoveTask
        {
            public RemoveTask(int id)
            {
                Id = id;
            }

            public int Id { get; }

            protected bool Equals(RemoveTask other)
            {
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((RemoveTask) obj);
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

        [AggregateCommand]
        public class RemoveAll
        {
            protected bool Equals(RemoveAll other)
            {
                return true;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((RemoveAll) obj);
            }

            public override int GetHashCode()
            {
                return 42;
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateCommand]
        public class RemoveAllDone
        {
            protected bool Equals(RemoveAllDone other)
            {
                return true;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((RemoveAllDone) obj);
            }

            public override int GetHashCode()
            {
                return 37;
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        [AggregateCommand]
        public class Check
        {
            public Check(int id, bool isDone)
            {
                Id = id;
                IsDone = isDone;
            }

            public int Id { get; }
            public bool IsDone { get; }

            protected bool Equals(Check other)
            {
                return Id == other.Id && IsDone == other.IsDone;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Check) obj);
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

        [AggregateCommand]
        public class CheckAll
        {
            public CheckAll(bool isDone)
            {
                IsDone = isDone;
            }

            public bool IsDone { get; }

            protected bool Equals(CheckAll other)
            {
                return IsDone == other.IsDone;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((CheckAll) obj);
            }

            public override int GetHashCode()
            {
                return IsDone.GetHashCode();
            }

            public override string ToString()
            {
                return this.ToJsonInline();
            }
        }

        #endregion

        #region [ Validation ]

        public class CreateValidator : AbstractValidator<Create>
        {
            public CreateValidator()
            {
                RuleFor(c => c.Title).NotEmpty().Length(4, 40);
            }
        }

        public class AddTaskValidator : AbstractValidator<AddTask>
        {
            public AddTaskValidator()
            {
                RuleFor(c => c.Description).NotEmpty().Length(1, 100);
            }
        }

        public class UpdateTaskValidator : AbstractValidator<UpdateTask>
        {
            public UpdateTaskValidator()
            {
                RuleFor(c => c.Description).NotEmpty().Length(1, 100);
                RuleFor(c => c.Id).GreaterThan(0);
            }
        }

        public class RemoveTaskValidator : AbstractValidator<RemoveTask>
        {
            public RemoveTaskValidator()
            {
                RuleFor(c => c.Id).GreaterThan(0);
            }
        }

        public class CheckValidator : AbstractValidator<Check>
        {
            public CheckValidator()
            {
                RuleFor(c => c.Id).GreaterThan(0);
            }
        }

        #endregion
    }
}
