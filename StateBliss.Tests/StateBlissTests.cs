﻿using System;
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
            _stateMachineManager.Stop();
        }

        [Fact]
        public async Task Test_OnTransitioningHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnTransitioningHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(OnTransitioningHandler1_TimesCalled == 1);
            Assert.True(state.Current == MyStates.Clicked);
        }

        [Fact]
        public async Task Test_OnTransitionedHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnTransitionedHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(OnTransitionedHandler1_TimesCalled == 1);
            Assert.True(state.Current == MyStates.Clicked);
        }

        [Fact]
        public async Task Test_OnEnterHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnEnterHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(OnEnterHandler1_TimesCalled == 2);
            Assert.True(state.Current == MyStates.Clicked);
        }
        
        [Fact]
        public async Task Test_OnExitHandlerCalled()
        {
            // Arrange
            var state = SetupState();
            OnExitHandler1_TimesCalled = 0;

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(OnExitHandler1_TimesCalled == 1);
            Assert.True(state.Current == MyStates.Clicked);
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

            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(OnEnterHandler1_TimesCalled == 0);
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
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(OnEnterHandler1_TimesCalled == 2);
            Assert.True(OnExitHandler1_TimesCalled == 1);
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
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(state.Current == MyStates.NotClicked);
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
            await _stateMachineManager.WaitAllHandlersProcessed();

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
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitioningHandler1(state, MyStates.Clicked), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
            
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
            await _stateMachineManager.WaitAllHandlersProcessed();
            _stateMachineManager.Stop();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitionedHandler1(MyStates.NotClicked, state), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
        }

        [Fact]
        public async Task Test_WithTarget_OnEnterHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnEnterHandler1(MyStates.Clicked, state), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
        }
        
        [Fact]
        public async Task Test_WithTarget_OnExitHandlerCalled()
        {
            // Arrange
            var targetHandlerClassMock = new Mock<TestHandlers>();
            var state = SetupStateWithTarget(targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnExitHandler1(MyStates.NotClicked, state), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
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

            await _stateMachineManager.WaitAllHandlersProcessed();

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
            await _stateMachineManager.WaitAllHandlersProcessed();

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
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(state.Current == MyStates.NotClicked);
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
            await _stateMachineManager.WaitAllHandlersProcessed();

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
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changing(target, a => a.OnTransitioningHandler1)
                        .Changed(target, a => a.OnTransitionedHandler1);

                    b.From(MyStates.Clicked).To(MyStates.NotClicked)
                        .Changed(target, a => a.OnTransitionedHandler2)
                        .Changing(target, a => a.OnTransitioningHandler2);

                    b.OnEnter(MyStates.Clicked, target,a => a.OnEnterHandler1);
                    b.OnExit(MyStates.NotClicked, target, a => a.OnExitHandler1);

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
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .Changing(OnTransitioningHandler1)
                        .Changed(OnTransitionedHandler1);
                    
                    b.OnEnter(MyStates.Clicked, OnEnterHandler1);
                    b.OnEnter(MyStates.NotClicked, OnEnterHandler1);
                    b.OnExit(MyStates.NotClicked, OnExitHandler1);

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