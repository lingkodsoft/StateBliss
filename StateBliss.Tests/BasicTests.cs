using System;
using System.Collections.Generic;
using Xunit;

namespace StateBliss.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Tests()
        {
            // Arrange
            StateMachineManager.Register(new [] { typeof(BasicTests).Assembly });
            var currentState = AuthenticationState.Unauthenticated;
            var data = new Dictionary<string, object>();
            
            // Act
            StateMachineManager.Trigger(currentState, AuthenticationState.Authenticated, data);
            
            // Assert
            Assert.Equal("ChangingHandler1", data["key1"]);
            Assert.Equal("ChangingHandler2", data["key2"]);
        }

        public class DefineAuthenticationState : StateHandlerDefinition<AuthenticationState>
        {
            public override void Define(IStateFromBuilder<AuthenticationState> builder)
            {
                builder.From(AuthenticationState.Unauthenticated).To(AuthenticationState.Authenticated)
                    .Changing(this, a => a.ChangingHandler1)
        
                    .Changed(this, a => a.ChangedHandler1)
                    
                    ;

                builder.OnEntering(AuthenticationState.Authenticated, this, a => a.OnEnteringHandler1);
            }

            private void OnEnteringHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                
            }

            private void ChangedHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
               // throw new NotImplementedException();
            }

            private void ChangingHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                var data = changeinfo.DataAs<Dictionary<string, object>>();
                data["key1"] = "ChangingHandler1";
            }
        }

        
        public class DefineAuthenticationState2 : StateHandlerDefinition<AuthenticationState>
        {
            public override void Define(IStateFromBuilder<AuthenticationState> builder)
            {
                builder.From(AuthenticationState.Unauthenticated).To(AuthenticationState.Authenticated)
                    .Changing(this, a => a.ChangingHandler2);

            }

            private void ChangingHandler2(StateChangeInfo<AuthenticationState> changeinfo)
            {
                var data = changeinfo.DataAs<Dictionary<string, object>>();
                data["key2"] = "ChangingHandler2";
            }
        }
    }

    public enum AuthenticationState
    {
        Unauthenticated,
        Authenticated
    }
}