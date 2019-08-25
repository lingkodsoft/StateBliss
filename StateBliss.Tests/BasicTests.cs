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
            StateMachineManager.Register(new [] { typeof(BasicTests).Assembly }); //Register at bootstrap of your application, i.e. Startup
            var currentState = AuthenticationState.Unauthenticated;
            var data = new Dictionary<string, object>();
            
            // Act
            var changeInfo = StateMachineManager.Trigger(currentState, AuthenticationState.Authenticated, data);
            
            // Assert
            Assert.True(changeInfo.StateChangedSucceeded);
            Assert.Equal("ChangingHandler1", changeInfo.Data["key1"]);
            Assert.Equal("ChangingHandler2", changeInfo.Data["key2"]);
        }

        public class AuthenticationStateDefinition : StateDefinition<AuthenticationState>
        {
            public override void Define(IStateFromBuilder<AuthenticationState> builder)
            {
                builder.From(AuthenticationState.Unauthenticated).To(AuthenticationState.Authenticated)
                    .Changing(this, a => a.ChangingHandler1)
                    .Changed(this, a => a.ChangedHandler1);

                builder.OnEntering(AuthenticationState.Authenticated, this, a => a.OnEnteringHandler1);
                builder.OnEntered(AuthenticationState.Authenticated, this, a => a.OnEnteredHandler1);
                
                builder.OnExiting(AuthenticationState.Unauthenticated, this, a => a.OnExitingHandler1);
                builder.OnExited(AuthenticationState.Authenticated, this, a => a.OnExitedHandler1);
                
                builder.OnEditing(AuthenticationState.Authenticated, this, a => a.OnEditingHandler1);
                builder.OnEdited(AuthenticationState.Authenticated, this, a => a.OnEditedHandler1);
                
                builder.ThrowExceptionWhenDiscontinued = true;
            }

            private void OnEditedHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                
            }

            private void OnExitedHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                
            }

            private void OnEnteredHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                
            }

            private void OnEditingHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
            }

            private void OnExitingHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
            }

            private void OnEnteringHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
                // changeinfo.Continue = false; //this will prevent changing the state
            }

            private void ChangedHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
            }

            private void ChangingHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
                var data = changeinfo.DataAs<Dictionary<string, object>>();
                data["key1"] = "ChangingHandler1";
            }
        }
        
        //this class gets regitered automatically by calling StateMachineManager.Register
        public class AnotherAuthenticationStateDefinition : StateDefinition<AuthenticationState>
        {
            public override void Define(IStateFromBuilder<AuthenticationState> builder)
            {
                builder.From(AuthenticationState.Unauthenticated).To(AuthenticationState.Authenticated)
                    .Changing(this, a => a.ChangingHandler2);

            }

            private void ChangingHandler2(StateChangeGuardInfo<AuthenticationState> changeinfo)
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