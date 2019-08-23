using System;
using Xunit;

namespace StateBliss.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Tests()
        {
            var definitions = new[] { new DefineOrderState() };
//            var findStates = new[] { new FindOrderState() }; 
                
            StateMachineManager.Default.Register(definitions);
            
            StateMachineManager.Default.Trigger(OrderTestState.Paid, 1, (1, "test"));
            
            
            //StateMachineManager.Default.Define()
            
        }

        public class DefineOrderState : StateHandlerDefinition<OrderTestState>
        {
            public override void Define(IStateFromBuilder<OrderTestState> builder)
            {
                builder.From(OrderTestState.Initial).To(OrderTestState.Paid)
                    .Changing(this, a => a.ChangingHandler1);

            }

            private void ChangingHandler1<TTriggerContext>(StateChangeInfo<OrderTestState, TTriggerContext> changeinfo)
            {
                
                throw new NotImplementedException();
            }
        }
//
//        public class FindOrderState : IFindState<OrderTestState>
//        {
//            public FindStateResult<OrderTestState> Find(object id)
//            {
//                throw new System.NotImplementedException();
//            }
//
////            public FindStateResult Find<TState>(object id) where TState : Enum
////            {
////                throw new NotImplementedException();
////            }
////            
//            
//        }
    }

    public enum OrderTestState
    {
        Initial,
        Paid
    }
}