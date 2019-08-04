using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace StateBliss.Tests
{
    public class StateBlissTests : IDisposable
    {
        private IStateMachineManager _stateMachineManager;
        private int OnExitHandler1_TimesCalled;
        private int OnEnterHandler1_TimesCalled;
        private int OnTransitionedHandler1_TimesCalled;
        private int OnTransitioningHandler1_TimesCalled;
        private bool OnTransitioningHandler1_ThrowsException;
        
        public void Dispose()
        {
            _stateMachineManager?.Stop();
            StateMachineManager.Default.Stop();
        }

        [Fact]
        public async Task Test_DefaultStateMachineManager()
        {
            // Arrange
            StateMachineManager.Default.Start();

            var entity = new MyEntity
            {
                Status = MyStates.NotClicked
            };
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Uid, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changed(OnTransitionedHandler1);
                });

            // Act
            state.ChangeTo(MyStates.Clicked);
            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run

            // Assert
            Assert.Equal(1, OnTransitionedHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        [Fact]
        public async Task Test_StateCtorWithoutEntityParam()
        {
            // Arrange
            StateMachineManager.Default.Start();
            var initialState = MyStates.NotClicked;
            
            var state = new State<MyStates>(initialState)
                .Define(b =>
                {
                    var triggerToClicked = "ToClicked";
                    
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changed(OnTransitionedHandler1);
                    
                });

            // Act
            state.ChangeTo(MyStates.Clicked);
            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run

            // Assert
            Assert.Equal(1, OnTransitionedHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        [Fact]
        public async Task Test_TriggeredBy()
        {
            // Arrange
            StateMachineManager.Default.Start();
            var initialState = MyStates.NotClicked;
            var triggerNotClickedToClicked = "NotClickedToClicked";
            
            var state = new State<MyStates>(initialState)
                .Define(b =>
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .TriggeredBy(triggerNotClickedToClicked)
                        .Changed(OnTransitionedHandler1);
                });

            // Act
            StateMachineManager.Trigger(triggerNotClickedToClicked);
            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run

            // Assert
            Assert.Equal(1, OnTransitionedHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        [Fact]
        public async Task Test_TriggerTo()
        {
            // Arrange
            StateMachineManager.Default.Start();
            var initialState = MyStates.NotClicked;
            var triggerToClicked = "ToClicked";
            
            var state = new State<MyStates>(initialState)
                .Define(b =>
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changed(OnTransitionedHandler1);

                    b.TriggerTo(MyStates.Clicked, triggerToClicked);

                });

            // Act
            StateMachineManager.Trigger(triggerToClicked);
            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run

            // Assert
            Assert.Equal(1, OnTransitionedHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        
        [Fact]
        public async Task Test_AsStateExtension()
        {
            // Arrange
            var initialState = MyStates.NotClicked;
            
            var state = initialState.AsState(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changed(OnTransitionedHandler1);
                });

            // Act
            state.ChangeTo(MyStates.Clicked);
            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run

            // Assert
            Assert.Equal(1, OnTransitionedHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        [Fact]
        public async Task Test_OnTransitioningHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnTransitioningHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(1, OnTransitioningHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }

        [Fact]
        public async Task Test_OnTransitionedHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnTransitionedHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(1, OnTransitionedHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }

        [Fact]
        public async Task Test_OnEnterHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnEnterHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(1, OnEnterHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        [Fact]
        public async Task Test_OnExitHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnExitHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(1, OnExitHandler1_TimesCalled);
            Assert.Equal(MyStates.Clicked, state.Current);
        }

        [Fact]
        public async Task Test_DisabledSameStateTransition()
        {
            // Arrange
            var state = SetupState(MyStates.Clicked);
            OnEnterHandler1_TimesCalled = 0;

            // Act
            Assert.Throws<SameStateTransitionDisabledException>(() =>
            {
                state.ChangeTo(MyStates.Clicked);
            });

            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(0, OnEnterHandler1_TimesCalled);
        }

        [Fact]
        public async Task Test_SameStateTransitionAllowed()
        {
            // Arrange
            var state = SetupState(MyStates.NotClicked);
            OnEnterHandler1_TimesCalled = 0;
            OnExitHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.NotClicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(OnEnterHandler1_TimesCalled, 1);
            Assert.Equal(OnExitHandler1_TimesCalled, 1);
        }

        [Fact]
        public async Task Test_OnTransitioningHandler_Exception_DoesNotChangeState()
        {
            // Arrange
            var state = SetupState(MyStates.NotClicked);
            OnTransitioningHandler1_ThrowsException = true;
            
            // Act
            Assert.Throws<Exception>(() =>
            {
                state.ChangeTo(MyStates.Clicked);                
            });
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(MyStates.NotClicked, state.Current);
        }

        [Fact]
        public async Task Test_OnHandlerExceptionEvent()
        {
            // Arrange
            var state = SetupState(MyStates.NotClicked);
            var handlerEventExceptionCalled = false;
            _stateMachineManager.OnHandlerException += (sender, t) =>
            {
                handlerEventExceptionCalled = true;
            };
            OnTransitioningHandler1_ThrowsException = true;

            // Act
            Assert.Throws<Exception>(() =>
            {
                state.ChangeTo(MyStates.Clicked);
            });
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.True(handlerEventExceptionCalled);
        }


//** _WithTarget

      [Fact]
        public async Task Test__WithTarget_OnTransitioningHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitioningHandler1(state, MyStates.Clicked), Times.Once);
            Assert.Equal( MyStates.Clicked, state.Current);
            
            _stateMachineManager.Stop();
        }

        [Fact]
        public async Task Test_WithTarget_OnTransitionedHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();
            _stateMachineManager.Stop();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitionedHandler1(MyStates.NotClicked, state), Times.Once);
            Assert.Equal(MyStates.Clicked, state.Current);
        }

        [Fact]
        public async Task Test_WithTarget_OnEnterHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.Clicked, state), Times.Once);
            Assert.Equal(MyStates.Clicked, state.Current);
        }
        
        [Fact]
        public async Task Test_WithTarget_OnExitHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnExitHandler1(MyStates.NotClicked, state), Times.Once);
            Assert.Equal(MyStates.Clicked, state.Current);
        }

        [Fact]
        public async Task Test_WithTarget_DisabledSameStateTransition()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object, MyStates.Clicked);

            // Act
            Assert.Throws<SameStateTransitionDisabledException>(() =>
            {
                state.ChangeTo(MyStates.Clicked);
            });

            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.Clicked, state), Times.Never);
        }

        [Fact]
        public async Task Test_WithTarget_SameStateTransitionAllowed()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object, MyStates.NotClicked);

            // Act
            state.ChangeTo(MyStates.NotClicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.NotClicked, state), Times.Once);
        }

        [Fact]
        public async Task Test_WithTarget_OnTransitioningHandler_Exception_DoesNotChangeState()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object, MyStates.NotClicked);
            targetHandlerClassMock.Setup(a => a.OnTransitioningHandler1(state,MyStates.Clicked))
                .Throws<Exception>();

            // Act
            Assert.Throws<Exception>(() =>
            {
                state.ChangeTo(MyStates.Clicked);                
            });
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.Equal(MyStates.NotClicked, state.Current);
        }

        [Fact]
        public async Task Test_WithTarget_OnHandlerExceptionEvent()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object, MyStates.NotClicked);
            var handlerEventExceptionCalled = false;
            _stateMachineManager.OnHandlerException += (sender, t) =>
            {
                handlerEventExceptionCalled = true;
            };

            targetHandlerClassMock.Setup(a => a.OnTransitionedHandler1(MyStates.NotClicked, state))
                .Throws<Exception>();

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessedAsync();

            // Assert
            Assert.True(handlerEventExceptionCalled);
        }


        private State<MyEntity, MyStates> SetupStateWithTarget(TestHandlers target, MyStates stateEnum = MyStates.NotClicked)
        {
            _stateMachineManager = new StateMachineManager();
            _stateMachineManager.Start();

            var entity = new MyEntity
            {
                Status = stateEnum
            };
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Uid, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changing(target, a => a.OnTransitioningHandler1)
                        .Changed(target, a => a.OnTransitionedHandler1);

                    b.From(MyStates.Clicked).To(MyStates.NotClicked)
                        .Changed(target, a => a.OnTransitionedHandler2)
                        .Changing(target, a => a.OnTransitioningHandler2);

                    b.OnEntered(MyStates.Clicked, target,a => a.OnEnterHandler1);
                    b.OnEntered(MyStates.NotClicked, target, a => a.OnEnterHandler1);
                    b.OnExited(MyStates.NotClicked, target, a => a.OnExitHandler1);

                    b.DisableSameStateTransitionFor(MyStates.Clicked);
                });

            _stateMachineManager.Register(state);

            return state;
        }
        
        private State<MyEntity, MyStates> SetupState(MyStates stateEnum = MyStates.NotClicked)
        {
            _stateMachineManager = new StateMachineManager();
            _stateMachineManager.Start();

            var entity = new MyEntity
            {
                Status = stateEnum
            };
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Uid, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changing(OnTransitioningHandler1)
                        .Changed(OnTransitionedHandler1);
                    
                    b.OnEntered(MyStates.Clicked, OnEnterHandler1);
                    b.OnEntered(MyStates.NotClicked, OnEnterHandler1);
                    b.OnExited(MyStates.NotClicked, OnExitHandler1);

                    b.DisableSameStateTransitionFor(MyStates.Clicked);
                });

            _stateMachineManager.Register(state);

            return state;
        }

        private void OnExitHandler1(MyStates previous, IState<MyStates> state)
        {
            OnExitHandler1_TimesCalled++;
        }

        private void OnEnterHandler1(MyStates next, IState<MyStates> state)
        {
            OnEnterHandler1_TimesCalled++;
        }

        private void OnTransitionedHandler1(MyStates previous, IState<MyStates> state)
        {
            OnTransitionedHandler1_TimesCalled++;
        }

        private void OnTransitioningHandler1(IState<MyStates> state, MyStates next)
        {
            OnTransitioningHandler1_TimesCalled++;
            if (OnTransitioningHandler1_ThrowsException)
            {
                throw new Exception();
            }
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
        public Guid Uid { get; set; }
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