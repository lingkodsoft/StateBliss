using System;

namespace StateBliss
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
        
        
        
    }

    public enum MyStates
    {
        NotClicked,
        Clicked
    }
    
    public class MyEntity
    {
        public int Id;
        public MyStates Status;
    }
    
    public class Test1
    {

        private IStateMachineManager _stateMachineManager;
        
        public void Run()
        {

            var entity = new MyEntity();
            
            var state = new State<MyEntity, MyStates>(entity, a => a.Status)
                .Define(b => 
                {
                    b.From(MyStates.NotClicked).To(MyStates.Clicked)
                        .OnTransitioned(this, a => a.OnTransitionedHandler1);

                    b.From(MyStates.Clicked).To(MyStates.NotClicked)
                        .OnTransitioned(this, a => a.OnTransitionedHandler2)
                        .OnTransitioning(this, a => a.OnTransitioningHandler2);

                    b.OnEnter(MyStates.Clicked, this,a => a.OnEnterHandler1);
                    b.OnExit(MyStates.NotClicked, this, a => a.OnExitHandler1);
                    b.DisableSameStateTransitionFor(MyStates.NotClicked, MyStates.NotClicked);

                });

            _stateMachineManager.Register(state);

            state.Change(MyStates.Clicked);
        }

        private void OnExitHandler1(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        private void OnEnterHandler1(MyStates next, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        private void OnTransitioningHandler2(IState<MyStates> state, MyStates next)
        {
            throw new NotImplementedException();
        }


        private void OnTransitionedHandler2(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }

        private void OnTransitionedHandler1(MyStates previous, IState<MyStates> state)
        {
            throw new NotImplementedException();
        }
    }


    
}