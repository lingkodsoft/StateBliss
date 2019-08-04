﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace StateBliss.Tests
{
    public class MultiThreadingTests : IDisposable
    {
        private IStateMachineManager _stateMachineManager;
        private volatile int CallTimes_OnChangingHandlerForClicked;
        private volatile int CallTimes_OnChangingHandlerForNotClicked;
        private volatile int CallTimes_OnChangedHandlerForClicked;
        private volatile int CallTimes_OnChangedHandlerForNotClicked;
        private volatile int CallTimes_OnEditHandlerForNotClicked;
        private volatile int CallTimes_Clicked;
        private volatile int CallTimes_NotClicked;
        
        public void Dispose()
        {
            _stateMachineManager?.Stop();
        }

        
        [Fact]
        public async Task Test_MultiThreadCalls_DoesNotThrowExceptions()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var context = new GuardContext<MyStates>();

            var stateFactory = StateFactoryTestHelper.SetupStateFactory(guid, () => 

                new State<MyStates>(guid, MyStates.NotClicked)
                    .Define(b =>
                    {
                        b.From(MyStates.NotClicked).To(MyStates.Clicked);
//                        b.OnEdited(MyStates.NotClicked, OnEditHandler1);
                        
                        b.OnEditing(MyStates.NotClicked, Guards.From(context, 
                            OnEditingHandler1, OnEditingHandler2));
                    })
            );
            StateMachineManager.Default.SetStateFactory(stateFactory);

            var startTime = DateTime.Now;
            
            // Act
            
            for (int i = 0; i < 10; i++)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    while (DateTime.Now.Subtract(startTime).TotalSeconds < 10)
                    {
                        Interlocked.Increment(ref CallTimes_NotClicked);
                        StateMachineManager.ChangeState(MyStates.NotClicked, guid);

//                        
//                        if (StateMachineManager.GetState<MyStates>(guid).Current == MyStates.Clicked)
//                        {
//                            Interlocked.Increment(ref CallTimes_NotClicked);
//                            StateMachineManager.ChangeState(MyStates.NotClicked, guid);
//                        }
//                        else
//                        {
//                            Interlocked.Increment(ref CallTimes_Clicked);
//                            StateMachineManager.ChangeState(MyStates.Clicked, guid);
//                        }
                    }
                });
            }
            
            //Thread.Sleep(6000);
            var spin = new SpinWait();
            for (int i = 0; i < 10000; i++)
            {
                spin.SpinOnce();
            }
            StateMachineManager.Default.WaitAllHandlersProcessed(); //this is only needed for unit test to ensure that all handlers are run
            
            // Assert

            Assert.True(Math.Abs(CallTimes_OnEditHandlerForNotClicked - CallTimes_NotClicked) < 5);
//            Assert.Equal(CallTimes_OnChangingHandlerForNotClicked, CallTimes_NotClicked);
//            Assert.Equal(CallTimes_OnChangedHandlerForClicked, CallTimes_Clicked);
//            Assert.Equal(CallTimes_OnChangedHandlerForNotClicked, CallTimes_NotClicked);
            
        }

        private void OnEditingHandler1(GuardContext<MyStates> context)
        {
            Interlocked.Increment(ref CallTimes_OnEditHandlerForNotClicked);
            context.Continue = true;
        }
        
        private void OnEditingHandler2(GuardContext<MyStates> context)
        {
            context.Continue = true;
        }

        private void OnEditHandler1(MyStates next, IState<MyStates> state)
        {
            Interlocked.Increment(ref CallTimes_OnEditHandlerForNotClicked);
        }

        [Fact]
        public async Task Test_MultiThreadCalls_HandlersCalledTimesCorrect()
        {
            // Arrange
            var guid = Guid.NewGuid();

            var stateFactory = StateFactoryTestHelper.SetupStateFactory(guid, () => 

                new State<MyStates>(guid, MyStates.NotClicked)
                    .Define(b =>
                    {
                        b.From(MyStates.NotClicked).To(MyStates.Clicked)
                            .Changing(OnChangingHandlerForClicked)
                            .Changed(OnChangedHandlerForClicked);
                        
                        b.From(MyStates.Clicked).To(MyStates.NotClicked)
                            .Changing(OnChangingHandlerForNotClicked)
                            .Changed(OnChangedHandlerForNotClicked);
                    })
            );
            StateMachineManager.Default.SetStateFactory(stateFactory);

            // Act
            for (int i = 0; i < 10; i++)
            {
                if (StateMachineManager.GetState<MyStates>(guid).Current == MyStates.Clicked)
                {
                    Interlocked.Increment(ref CallTimes_NotClicked);
                    StateMachineManager.ChangeState(MyStates.NotClicked, guid);
                }
                else
                {
                    Interlocked.Increment(ref CallTimes_Clicked);
                    StateMachineManager.ChangeState(MyStates.Clicked, guid);
                }
            }
            
            StateMachineManager.Default.WaitAllHandlersProcessed(); //this is only needed for unit test to ensure that all handlers are run
            
            // Assert
            Assert.Equal(CallTimes_OnChangingHandlerForClicked, CallTimes_Clicked);
            Assert.Equal(CallTimes_OnChangingHandlerForNotClicked, CallTimes_NotClicked);
            Assert.Equal(CallTimes_OnChangedHandlerForClicked, CallTimes_Clicked);
            Assert.Equal(CallTimes_OnChangedHandlerForNotClicked, CallTimes_NotClicked);
        }

        
        private void OnChangedHandlerForNotClicked(MyStates previous, IState<MyStates> state)
        {
            Interlocked.Increment(ref CallTimes_OnChangedHandlerForNotClicked);
        }

        private void OnChangingHandlerForNotClicked(IState<MyStates> state, MyStates next)
        {
            Interlocked.Increment(ref CallTimes_OnChangingHandlerForNotClicked);
        }

        private void OnChangedHandlerForClicked(MyStates previous, IState<MyStates> state)
        {
            Interlocked.Increment(ref CallTimes_OnChangedHandlerForClicked);
        }

        private void OnChangingHandlerForClicked(IState<MyStates> state, MyStates next)
        {
            Interlocked.Increment(ref CallTimes_OnChangingHandlerForClicked);
        }

    }

}