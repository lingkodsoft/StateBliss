using System;
using System.Threading.Tasks;
using Xunit;

namespace StateBliss.Tests
{
    public class GuardContextTests : IDisposable
    {
        private IStateMachineManager _stateMachineManager;
        
        public void Dispose()
        {
            _stateMachineManager?.Stop();
        }

        [Fact]
        public async Task Test_OnTransitioningHandlerCalled()
        {
            // Arrange
            var ordersRepository = new OrdersRepository();
            _stateMachineManager = new StateMachineManager();
            var orderStateDefinition = new OrderStateDefinition(_stateMachineManager);
            var payOrderCommandHandler = new PayOrderCommandHandler(orderStateDefinition, ordersRepository);
            var order = new Order
            {
                Id = 1,
                State = OrderState.Initial
            };
            ordersRepository.InsertOrder(order);
            
            var payOrderCommand = new PayOrderCommand
            {
                Order = order
            };

            // Act
            payOrderCommandHandler.Handle(payOrderCommand);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(payOrderCommand.Succeeded);
            Assert.Equal(OrderState.Paid, ordersRepository.GetOrder(order.Id).State);
            Assert.Equal(OrderState.Paid, payOrderCommand.State.Current);
            
        }
        
    }

}