using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace StateBliss.Tests
{
    public class Tests
    {
        [Fact]
        public void Test_OnTransitioningHandlerCalled()
        {
            // Arrange
            var stateMachineManager = new StateMachineManager();
            stateMachineManager.Start();

            var targetHandlerClassMock = new Mock<Test1>();
            var state = SetupState(stateMachineManager, targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitioningHandler1(state, MyStates.Clicked), Times.Once);
            Assert.True(state.Current == MyStates.Clicked);
        }

        [Fact]
        public async Task Test_OnTransitionedHandlerCalled()
        {
            // Arrange
            var stateMachineManager = new StateMachineManager();
            stateMachineManager.Start();

            var targetHandlerClassMock = new Mock<Test1>();
            var state = SetupState(stateMachineManager, targetHandlerClassMock.Object);

            // Act
            state.ChangeTo(MyStates.Clicked);
            await stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            targetHandlerClassMock.Verify(a => a.OnTransitionedHandler1(MyStates.Clicked, state), Times.Once);
            //Assert.True(state.Current == MyStates.Clicked);
        }

        
        //        [Fact]
        public void Test2()
        {
            // Arrange
            var stateMachineManager = new StateMachineManager();
            stateMachineManager.Start();
            
            stateMachineManager.OnHandlerException += (sender, t) =>
            {
               
            };

            var targetHandlerClassMock = new Mock<Test1>();
            var state = SetupState(stateMachineManager, targetHandlerClassMock.Object);

            // Act
            var previousState = state.Current;
            
            state.ChangeTo(MyStates.NotClicked);
            
            

//            state.ChangeTo(MyStates.Clicked);
//
//            try
//            {
//                state.ChangeTo(MyStates.NotAllowedState);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.GetType().Name);
//            }
//            
//            try
//            {
//                state.ChangeTo(MyStates.Clicked);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.GetType().Name);
//            }
//
//            
//            
            // Assert
            //targetHandlerClassMock.Setup(a => a.)
            
            
            
            Assert.True(true);
        }

        
        private State<MyEntity, MyStates> SetupState(IStateMachineManager stateMachineManager, Test1 target)
        {
            var entity = new MyEntity();
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
//                        .OnTransitioning(target, a => a.OnTransitioningHandler1)
                        .OnTransitioned(target, a => a.OnTransitionedHandler1);

                    b.From(MyStates.Clicked).To(MyStates.NotClicked);
//                        .OnTransitioned(target, a => a.OnTransitionedHandler2)
//                        .OnTransitioning(target, a => a.OnTransitioningHandler2);

//                    b.OnEnter(MyStates.Clicked, target,a => a.OnEnterHandler1);
//                    b.OnExit(MyStates.NotClicked, target, a => a.OnExitHandler1);
                    
                    b.DisableSameStateTransitionFor(MyStates.Clicked);

                });

            stateMachineManager.Register(state);

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
    
    public class Test1
    {
        public virtual void OnExitHandler1(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual  void OnEnterHandler1(MyStates next, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual  void OnTransitioningHandler2(IState<MyStates> state, MyStates next)
        {
            throw new NotImplementedException();
        }

        public virtual  void OnTransitionedHandler2(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        public virtual  void OnTransitionedHandler1(MyStates previous, IState<MyStates> state)
        {
          //  throw new NotImplementedException();
        }

        public virtual void OnTransitioningHandler1(IState<MyStates> state, MyStates next)
        {
            throw new NotImplementedException();
        }
    }
}