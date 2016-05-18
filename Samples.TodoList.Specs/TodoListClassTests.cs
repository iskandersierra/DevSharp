using System.Collections.Immutable;
using System.Runtime.Remoting.Channels;
using FluentValidation.Results;
using NUnit.Framework;

namespace Samples.TodoList.Specs
{
    [TestFixture]
    public class TodoListClassTests
    {
        [Test]
        public void OnEventCreatedFromNoState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var @event = new Events.Created(title);

            // When
            var newState = cls.OnEvent(@event);

            // Then
            var expectedState = new TodoListClass.State(title, ImmutableList<TodoListClass.TodoTask>.Empty, 1);

            Assert.That(newState, Is.EqualTo(expectedState));
        }

        [Test]
        public void OnEventCheckedFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var @event = new Events.Checked(2, true);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false)), 
                3);

            // When
            var newState = cls.OnEvent(@event, state);

            // Then
            var expectedState = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true)), 
                3);

            Assert.That(newState, Is.EqualTo(expectedState));
        }

        [Test]
        public void OnEventTaskAddedFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var @event = new Events.TaskAdded(3, "desc3");
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false)), 
                3);

            // When
            var newState = cls.OnEvent(@event, state);

            // Then
            var expectedState = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false))
                .Add(new TodoListClass.TodoTask(3, "desc3", false)), 
                4);

            Assert.That(newState, Is.EqualTo(expectedState));
        }

        [Test]
        public void OnEventTaskRemovedFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var @event = new Events.TaskRemoved(1);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false)), 
                3);

            // When
            var newState = cls.OnEvent(@event, state);

            // Then
            var expectedState = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(2, "desc2", false)), 
                3);

            Assert.That(newState, Is.EqualTo(expectedState));
        }

        [Test]
        public void OnEventTaskUpdatedFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var @event = new Events.TaskUpdated(1, "new desc1");
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false)), 
                3);

            // When
            var newState = cls.OnEvent(@event, state);

            // Then
            var expectedState = new TodoListClass.State(title,
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "new desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false)),
                3);

            Assert.That(newState, Is.EqualTo(expectedState));
        }

        [Test]
        public void WhenCommandCreateFromNoState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.Create(title);

            // When
            var events = cls.WhenCommand(command);

            // Then
            var expectedEvents = new object[] { new Events.Created(title), };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandAddTaskFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.AddTask("new desc3");
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", false)), 
                3);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.TaskAdded(3, "new desc3"), };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandUpdateTaskFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.UpdateTask(2, "new desc2");
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true)), 
                3);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.TaskUpdated(2, "new desc2"), };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandUpdateTaskFromStateNoExist()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.UpdateTask(3, "new desc3");
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true)), 
                3);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandRemoveTaskFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.RemoveTask(1);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true)), 
                3);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.TaskRemoved(1), };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandRemoveTaskFromStateNoExist()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.RemoveTask(3);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true)), 
                3);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandRemoveAllFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.RemoveAll();
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true)), 
                3);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.TaskRemoved(1), new Events.TaskRemoved(2), };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandRemoveAllDoneFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.RemoveAllDone();
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true))
                .Add(new TodoListClass.TodoTask(3, "desc3", false))
                .Add(new TodoListClass.TodoTask(4, "desc4", true)), 
                5);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.TaskRemoved(2), new Events.TaskRemoved(4), };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandCheckTrueFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.Check(1, true);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true))
                .Add(new TodoListClass.TodoTask(3, "desc3", false))
                .Add(new TodoListClass.TodoTask(4, "desc4", true)), 
                5);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.Checked(1, true) };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandCheckFalseFromState()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.Check(2, false);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true))
                .Add(new TodoListClass.TodoTask(3, "desc3", false))
                .Add(new TodoListClass.TodoTask(4, "desc4", true)), 
                5);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { new Events.Checked(2, false) };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandCheckTrueFromStateTrue()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.Check(2, true);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true))
                .Add(new TodoListClass.TodoTask(3, "desc3", false))
                .Add(new TodoListClass.TodoTask(4, "desc4", true)), 
                5);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void WhenCommandCheckFalseFromStateFalse()
        {
            // Given
            var cls = new TodoListClass();
            var title = "any title";
            var command = new Commands.Check(1, false);
            var state = new TodoListClass.State(title, 
                ImmutableList<TodoListClass.TodoTask>.Empty
                .Add(new TodoListClass.TodoTask(1, "desc1", false))
                .Add(new TodoListClass.TodoTask(2, "desc2", true))
                .Add(new TodoListClass.TodoTask(3, "desc3", false))
                .Add(new TodoListClass.TodoTask(4, "desc4", true)), 
                5);

            // When
            var events = cls.WhenCommand(command, state);

            // Then
            var expectedEvents = new object[] { };

            CollectionAssert.AreEqual(expectedEvents, events);
        }

        [Test]
        public void CreateValidations()
        {
            // Given
            var validator = new Commands.CreateValidator();

            // Then
            Assert.That(validator.Validate(new Commands.Create("any title")).IsValid, Is.True);
            Assert.That(validator.Validate(new Commands.Create(null)).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.Create("")).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.Create(new string('a', 3))).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.Create(new string('a', 41))).IsValid, Is.False);
        }

        [Test]
        public void AddTaskValidations()
        {
            // Given
            var validator = new Commands.AddTaskValidator();

            // Then
            Assert.That(validator.Validate(new Commands.AddTask("any description")).IsValid, Is.True);
            Assert.That(validator.Validate(new Commands.AddTask(null)).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.AddTask("")).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.AddTask(new string('a', 101))).IsValid, Is.False);
        }

        [Test]
        public void UpdateTaskValidations()
        {
            // Given
            var validator = new Commands.UpdateTaskValidator();

            // Then
            Assert.That(validator.Validate(new Commands.UpdateTask(6, "any description")).IsValid, Is.True);
            Assert.That(validator.Validate(new Commands.UpdateTask(0, "any description")).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.UpdateTask(-4, "any description")).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.UpdateTask(6, null)).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.UpdateTask(6, "")).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.UpdateTask(6, new string('a', 101))).IsValid, Is.False);
        }

        [Test]
        public void RemoveTaskValidations()
        {
            // Given
            var validator = new Commands.RemoveTaskValidator();

            // Then
            Assert.That(validator.Validate(new Commands.RemoveTask(6)).IsValid, Is.True);
            Assert.That(validator.Validate(new Commands.RemoveTask(0)).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.RemoveTask(-3)).IsValid, Is.False);
        }

        [Test]
        public void CheckValidations()
        {
            // Given
            var validator = new Commands.CheckValidator();

            // Then
            Assert.That(validator.Validate(new Commands.Check(6, true)).IsValid, Is.True);
            Assert.That(validator.Validate(new Commands.Check(6, false)).IsValid, Is.True);
            Assert.That(validator.Validate(new Commands.Check(0, true)).IsValid, Is.False);
            Assert.That(validator.Validate(new Commands.Check(-3, false)).IsValid, Is.False);
        }
    }
}
