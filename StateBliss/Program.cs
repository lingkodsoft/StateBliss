using System;

namespace StateBliss
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var stateMachineManager = new StateMachineManager();
            stateMachineManager.Start();
            stateMachineManager.OnHandlerException += (sender, t) =>
            {
                Console.WriteLine($"{t.exception}, state type: {t.state.GetType()}, fromState: {t.fromState}, toState: {t.toState}");
            };
            
            new Test1(stateMachineManager).Run();
            
            Console.Read();
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

        private IStateMachineManager _stateMachineManager;

        public Test1(IStateMachineManager stateMachineManager)
        {
            _stateMachineManager = stateMachineManager;
        }
        
        public void Run()
        {

            var entity = new MyEntity();
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Status, "myState1")
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .OnTransitioned(this, a => a.OnTransitionedHandler1);

                    b.From(MyStates.Clicked).To(MyStates.NotClicked)
                        .OnTransitioned(this, a => a.OnTransitionedHandler2)
                        .OnTransitioning(this, a => a.OnTransitioningHandler2);

                    b.OnEnter(MyStates.Clicked, this,a => a.OnEnterHandler1);
                    b.OnExit(MyStates.NotClicked, this, a => a.OnExitHandler1);
                    
                    b.DisableSameStateTransitionFor(MyStates.Clicked);

                });

            _stateMachineManager.Register(state);

            var previousState = state.Current;
            
            state.ChangeTo(MyStates.NotClicked);
            
            state.ChangeTo(MyStates.Clicked);
            Console.WriteLine($"New state: {state.Current}, from: {previousState}");

            try
            {
                state.ChangeTo(MyStates.NotAllowedState);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name);
            }
            
            try
            {
                state.ChangeTo(MyStates.Clicked);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name);
            }

        }

        private void OnExitHandler1(MyStates previous, IState<MyStates> state)
        {
            Console.WriteLine($"OnExitHandler1 previous:{previous}, {state.Current}");
        }

        private void OnEnterHandler1(MyStates next, IState<MyStates> state)
        {
            Console.WriteLine($"OnEnterHandler1 next:{next}, {state.Current}");
        }

        private void OnTransitioningHandler2(IState<MyStates> state, MyStates next)
        {
            Console.WriteLine($"OnTransitioningHandler2 next:{next}, {state.Current}");
        }

        private void OnTransitionedHandler2(MyStates previous, IState<MyStates> state)
        {
            Console.WriteLine($"OnTransitionedHandler2 next:{previous}, {state.Current}");
            throw new Exception("Test error");
        }

        private void OnTransitionedHandler1(MyStates previous, IState<MyStates> state)
        {
            Console.WriteLine($"OnTransitionedHandler2 next:{previous}, {state.Current}");
        }
    }


    
}