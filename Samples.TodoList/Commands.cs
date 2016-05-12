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
        }

        [AggregateCommand]
        public class AddTask
        {
            public AddTask(string description)
            {
                Description = description;
            }

            public string Description { get; }
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
        }

        [AggregateCommand]
        public class RemoveTask
        {
            public RemoveTask(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        [AggregateCommand]
        public class RemoveAll
        {
        }

        [AggregateCommand]
        public class RemoveAllDone
        {
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
        }

        [AggregateCommand]
        public class CheckAll
        {
            public CheckAll(bool isDone)
            {
                IsDone = isDone;
            }

            public bool IsDone { get; }
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
