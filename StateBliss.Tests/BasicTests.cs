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
            StateMachineManager.Default.Register(new [] { typeof(BasicTests).Assembly });
            var currentState = AuthenticationState.Unauthenticated;
            
            // Act
            StateMachineManager.Default.Trigger(currentState, AuthenticationState.Authenticated, (1, "test"));
            
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
                throw new NotImplementedException();
            }
        }

    }

    public enum AuthenticationState
    {
        Unauthenticated,
        Authenticated
    }
}