using System;
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
            
            // Act
            StateMachineManager.Trigger(currentState, AuthenticationState.Authenticated, (1, "test"));
            
            // Assert
        }

        public class DefineAuthenticationState : StateHandlerDefinition<AuthenticationState>
        {
            public override void Define(IStateFromBuilder<AuthenticationState> builder)
            {
                builder.From(AuthenticationState.Unauthenticated).To(AuthenticationState.Authenticated)
                    .Changing(this, a => a.ChangingHandler1);

            }

            private void ChangingHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                
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
                
            }
        }
    }

    public enum AuthenticationState
    {
        Unauthenticated,
        Authenticated
    }
}