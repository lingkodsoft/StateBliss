using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace StateBliss.Tests
{
    public class StateBlissTests : IDisposable
    {
        private readonly IStateMachineManager _stateMachineManager;

        public StateBlissTests()
        {
            _stateMachineManager = new StateMachineManager();
            _stateMachineManager.Start();
        }

        public void Dispose()
        {
            _stateMachineManager.Stop();
        }

        [Fact]
        public async Task Test_OnTransitioningHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitioningHandler1(state, MyStates.Clicked), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
            
            _stateMachineManager.Stop();
        }

        [Fact]
        public async Task Test_OnTransitionedHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();
            _stateMachineManager.Stop();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitionedHandler1(MyStates.NotClicked, state), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
        }

        [Fact]
        public async Task Test_OnEnterHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.Clicked, state), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
        }
        
        [Fact]
        public async Task Test_OnExitHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnExitHandler1(MyStates.NotClicked, state), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
        }

        [Fact]
        public async Task Test_DisabledSameStateTransition()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object, MyStates.Clicked);

            // Act
            Assert.Throws<SameStateTransitionDisabledException>(() =>
            {
                state.ChangeTo(MyStates.Clicked);
            });

            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.Clicked, state), Times.Never);
        }

        [Fact]
        public async Task Test_SameStateTransitionAllowed()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object, MyStates.NotClicked);

            // Act
            state.ChangeTo(MyStates.NotClicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.NotClicked, state), Times.Once);
        }

        [Fact]
        public async Task Test_OnTransitioningHandler_Exception_DoesNotChangeState()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object, MyStates.NotClicked);
            targetHandlerClassMock.Setup(a => a.OnTransitioningHandler1(state,MyStates.Clicked))
                .Throws<Exception>();

            // Act
            Assert.Throws<Exception>(() =>
            {
                state.ChangeTo(MyStates.Clicked);                
            });
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(state.Current == MyStates.NotClicked);
        }

        [Fact]
        public async Task Test_OnHandlerExceptionEvent()
        {
            // Arrange
            var handlerEventExceptionCalled = false;
            _stateMachineManager.OnHandlerException += (sender, t) =>
            {
                handlerEventExceptionCalled = true;
            };

            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupState(targetHandlerClassMock.Object, MyStates.NotClicked);
            targetHandlerClassMock.Setup(a => a.OnTransitionedHandler1(MyStates.NotClicked, state))
                .Throws<Exception>();

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(handlerEventExceptionCalled);
        }

        private State<MyEntity, MyStates> SetupState(TestHandlers target, MyStates stateEnum = MyStates.NotClicked)
        {
            var entity = new MyEntity
            {
                Status = stateEnum
            };
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .OnTransitioning(target, a => a.OnTransitioningHandler1)
                        .OnTransitioned(target, a => a.OnTransitionedHandler1);

                    b.From(MyStates.Clicked).To(MyStates.NotClicked)
                        .OnTransitioned(target, a => a.OnTransitionedHandler2)
                        .OnTransitioning(target, a => a.OnTransitioningHandler2);

                    b.OnEnter(MyStates.Clicked, target,a => a.OnEnterHandler1);
                    b.OnExit(MyStates.NotClicked, target, a => a.OnExitHandler1);

                    b.DisableSameStateTransitionFor(MyStates.Clicked);
                });

            _stateMachineManager.Register(state);

            return state;
        }
    }

    public enum MyStates
    {
        NotClicked,
        Clicked,
        NotAllowedState
    }

    public class MyEntity
    {
        public int Id;
        public MyStates Status { get; set; }
    }

    public class TestHandlers
    {
        public virtual void OnExitHandler1(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual void OnEnterHandler1(MyStates next, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual void OnTransitioningHandler2(IState<MyStates> state, MyStates next)
        {
            throw new NotImplementedException();
        }

        public virtual void OnTransitionedHandler2(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual void OnTransitionedHandler1(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual void OnTransitioningHandler1(IState<MyStates> state, MyStates next)
        {
            throw new NotImplementedException();
        }
    }
}