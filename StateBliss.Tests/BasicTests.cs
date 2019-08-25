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
            StateMachineManager.Register(new [] { typeof(BasicTests).Assembly }); //Register at bootstrap of yor application, i.e. Startup
            var currentState = AuthenticationState.Unauthenticated;
            var data = new Dictionary<string, object>();
            
            // Act
            var changeInfo = StateMachineManager.Trigger(currentState, AuthenticationState.Authenticated, data);
            
            // Assert
            Assert.True(changeInfo.StateChangedSucceeded);
            Assert.Equal("ChangingHandler1", changeInfo.Data["key1"]);
            Assert.Equal("ChangingHandler2", changeInfo.Data["key2"]);
        }

        public class DefineAuthenticationState : StateDefinition<AuthenticationState>
        {
            public override void Define(IStateFromBuilder<AuthenticationState> builder)
            {
                builder.From(AuthenticationState.Unauthenticated).To(AuthenticationState.Authenticated)
                    .Changing(this, a => a.ChangingHandler1)
                    .Changed(this, a => a.ChangedHandler1);

                builder.OnEntering(AuthenticationState.Authenticated, this, a => a.OnEnteringHandler1);
                builder.OnExiting(AuthenticationState.Unauthenticated, this, a => a.OnExitingHandler1);
                builder.OnEditing(AuthenticationState.Authenticated, this, a => a.OnEditingHandler1);

                builder.ThrowExceptionWhenDiscontinued = true;
            }

            private void OnEditingHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
               // changeinfo.Continue = false;
            }

            private void OnExitingHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
               // changeinfo.Continue = false;
            }

            private void OnEnteringHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
                //changeinfo.Continue = false;
            }

            private void ChangedHandler1(StateChangeInfo<AuthenticationState> changeinfo)
            {
                // throw new NotImplementedException();
            }

            private void ChangingHandler1(StateChangeGuardInfo<AuthenticationState> changeinfo)
            {
                var data = changeinfo.DataAs<Dictionary<string, object>>();
                data["key1"] = "ChangingHandler1";
            }
        }

        
        public class DefineAuthenticationState2 : StateDefinition<AuthenticationState>
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