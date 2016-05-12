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
        }

        [AggregateEvent]
        public class TaskRemoved
        {
            public TaskRemoved(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        //[AggregateEvent]
        //public class AllRemoved
        //{
        //}

        //[AggregateEvent]
        //public class AllDoneRemoved
        //{
        //}

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
        }

        //[AggregateEvent]
        //public class AllChecked
        //{
        //    public AllChecked(bool isDone)
        //    {
        //        IsDone = isDone;
        //    }

        //    public bool IsDone { get; }
        //}
    }
}