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

    public enum StatusEnum
    {
        NotClicked,
        Clicked
    }
    
    public class MyEntity
    {
        public int Id;
        public StatusEnum Status;
    }
    
    public class Test1
    {

        private IStateMachineManager _stateMachineManager;
        
        public void Run()
        {

            var entity = new MyEntity();
            
            var state = new State<MyEntity, StatusEnum>(entity, a => a.Status)
                .Define(b => 
                {
                    b.From(StatusEnum.NotClicked).To(StatusEnum.Clicked)
                        .OnTransitioned(this, a => a.ChangeText1);

                    b.From(StatusEnum.Clicked).To(StatusEnum.NotClicked)
                        .OnTransitioned(this, a => a.ChangeText1)
                        .OnTransitioning(this, a => a.ChangeText1);

                    b.OnEnter(StatusEnum.Clicked, this,a => a.ChangeText1);
                    b.OnExit(StatusEnum.NotClicked, this, a => a.ChangeText1);
                    
                });

            _stateMachineManager.Register(state);


            var nextStates = state.GetNextStates();
            state.Change(StatusEnum.Clicked);

        }
        
        public void ChangeText1()
        {
    
        }
    }


    
}