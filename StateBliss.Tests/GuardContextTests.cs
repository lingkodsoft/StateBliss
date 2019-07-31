using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task Test_GuardsOnChange()
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
            Assert.Equal(1, payOrderCommand.ValidateRequest_CallCount);
            Assert.Equal(1, payOrderCommand.PayToGateway_CallCount);
            Assert.Equal(1, payOrderCommand.PersistToRepo_CallCount);
            Assert.Equal(OrderState.Paid, ordersRepository.GetOrder(order.Id).State);
            Assert.Equal(OrderState.Paid, payOrderCommand.State.Current);
        }
        
        
        [Fact]
        public async Task Test_StateFactory()
        {
            // Arrange
            var guid = Guid.NewGuid();
            int stateIdValueFromRepo = (int)MyStates.NotClicked;

            StateMachineManager.Default.SetStateFactory((type, id) =>
            {
//              var stateGenericType = typeof(State<>).MakeGenericType(type);
//              var stateId =   Convert.ChangeType(Enum.ToObject(type, stateIdValueFromRepo), type);
//              var newState = (State)Activator.CreateInstance(stateGenericType, stateId);

                if (id != guid)
                {
                    return null;
                }
                
                var state0 = new State<MyStates>((MyStates)(object)stateIdValueFromRepo)
                    .Define(b =>
                    {
                        b.From(MyStates.NotClicked).To(MyStates.Clicked)
                            .Changed((s, n) =>
                            {
                                stateIdValueFromRepo = (int)Enum.ToObject(type, n.Current);
                            })
                            ;
                    });
                
                return state0;

            });
            
            // Act
            StateMachineManager.ChangeState(MyStates.Clicked, guid);
            await StateMachineManager.Default.WaitAllHandlersProcessed(); //this is only needed for unit test to ensure that all handlers are run

//            StateMachineManager.Guards(guid, MyStates.NotClicked, Guards<MyStates>.From(
//                OnTransitionedHandler1, 
//                OnTransitionedHandler2));

            
            // Assert
            Assert.Equal(MyStates.Clicked, StateMachineManager.GetState<MyStates>(guid).Current);
        }

//        private TContext OnTransitionedHandler1<TContext>() where TContext : GuardContext<TStataus>
//        {
//            throw new NotImplementedException();
//        }

        [Fact]
        public async Task Test_GuardsOnStateDefine()
        {
            // Arrange
            var ordersRepository = new OrdersRepository();
            _stateMachineManager = new StateMachineManager();
            var orderStateDefinition = new OrderStateDefinition(_stateMachineManager, ordersRepository);
            var payOrderCommandHandler = new PayOrderCommandHandler(orderStateDefinition, ordersRepository);
            var order = new Order
            {
                Id = 1,
                State = OrderState.Initial
            };
            ordersRepository.InsertOrder(order);
            
            var payOrderCommand = new PayOrderCommand
            {
                GetStateWithGuards = true,
                Order = order
            };

            // Act
            payOrderCommandHandler.Handle(payOrderCommand);
            await _stateMachineManager.WaitAllHandlersProcessed();

            // Assert
            Assert.True(payOrderCommand.Succeeded);
            Assert.Equal(1, payOrderCommand.ValidateRequest_CallCount);
            Assert.Equal(1, payOrderCommand.PayToGateway_CallCount);
            Assert.Equal(1, payOrderCommand.PersistToRepo_CallCount);
            Assert.Equal(OrderState.Paid, ordersRepository.GetOrder(order.Id).State);
            Assert.Equal(OrderState.Paid, payOrderCommand.State.Current);
        }
        
        public class PayOrderCommand
        {
            public Order Order { get; set; }
            public State<Order, OrderState> State { get; set; }
            public bool GetStateWithGuards { get; set; }
            public int PersistToRepo_CallCount;
            public int PayToGateway_CallCount;
            public int ValidateRequest_CallCount;
            public bool Succeeded { get; set; }
        }
        
        public class PaymentGuardContext : GuardContext<OrderState>
        {
            public PayOrderCommand Command { get; set; }
        }
        
        public class PayOrderCommandHandler
        {
            private readonly OrderStateDefinition _orderStateDefinition;
            private readonly OrdersRepository _ordersRepository;

            public PayOrderCommandHandler(OrderStateDefinition orderStateDefinition, OrdersRepository ordersRepository)
            {
                _orderStateDefinition = orderStateDefinition;
                _ordersRepository = ordersRepository;
            }
            
            public void Handle(PayOrderCommand cmd)
            {
                var context = new PaymentGuardContext
                {
                    Command = cmd
                };

                var state = cmd.GetStateWithGuards
                    ? _orderStateDefinition.GetStateWithGuards(cmd.Order, () => context)
                    : _orderStateDefinition.GetState(cmd.Order);

                var hasChangedState = false;
                if (cmd.GetStateWithGuards)
                {
                    hasChangedState = state.ChangeTo(OrderState.Paid);
                }
                else
                {
                    hasChangedState = state.ChangeTo(OrderState.Paid,
                        Guards<OrderState>.From(context,
                            ValidateRequest,
                            PayToPaymentGateway,
                            PersistOrderToRepository
                        ));
                }

                cmd.State = state;
                cmd.Succeeded = hasChangedState;
            }

            private void ValidateRequest(PaymentGuardContext context)
            {
                context.Command.ValidateRequest_CallCount++;
                context.Continue = true;
            }

            private void PayToPaymentGateway(PaymentGuardContext context)
            {
                context.Command.PayToGateway_CallCount++;
                context.Continue = true;
            }

            private void PersistOrderToRepository(PaymentGuardContext context)
            {
                context.Command.PersistToRepo_CallCount++;
                _ordersRepository.UpdateOrder(context.Command.Order);
                context.Continue = true;
            }
            
        }
        
        public class Order
        {
            public int Id { get; set; }
            public OrderState State { get; set; }
        }
        
        public class OrdersRepository
        {
            private readonly List<Order> _orders = new List<Order>();
        
            public IReadOnlyList<Order> GetOrders()
            {
                return _orders;
            }
        
            public Order GetOrder(int id)
            {
                return _orders.FirstOrDefault(a => a.Id == id);
            }
        
            public void InsertOrder(Order order)
            {
                _orders.Add(order);
            }
        
            public void UpdateOrder(Order order)
            {
                var existingOrder = _orders.FirstOrDefault(a => a.Id == order.Id);
                if (existingOrder == null)
                {
                    throw new Exception($"Order {order.Id} does not exists.");
                }
                _orders.Remove(existingOrder);
                _orders.Add(order);
            }
        }
        
        public enum OrderState
        {
            Initial,
            Paid,
            Processing,
            Processed,
            Delivered
        }
        
        public class OrderStateDefinition
        {
            private readonly IStateMachineManager _stateMachineManager;
            private readonly OrdersRepository _ordersRepository;

            public OrderStateDefinition(IStateMachineManager stateMachineManager, OrdersRepository ordersRepository = null)
            {
                _stateMachineManager = stateMachineManager;
                _ordersRepository = ordersRepository;
            }

            public State<Order, OrderState> GetState(Order order)
            {
                var state = new State<Order, OrderState>(order, a => a.State)
                    .Define(b =>
                    {
                        b.From(OrderState.Initial).To(OrderState.Paid);
                        b.From(OrderState.Paid).To(OrderState.Processed);
                        b.From(OrderState.Processed).To(OrderState.Delivered);
                    });

                _stateMachineManager.Register(state);
                return state;
            }

            public State<Order, OrderState> GetStateWithGuards(Order order, Func<PaymentGuardContext> paidStateContextProvider)
            {
                var state = new State<Order, OrderState>(order, a => a.State)
                    .Define(b =>
                    {
                        b.From(OrderState.Initial).To(OrderState.Paid);
                        b.From(OrderState.Paid).To(OrderState.Processed);
                        b.From(OrderState.Processed).To(OrderState.Delivered);

                        b.OnEnter(OrderState.Paid, Guards<OrderState>.From(paidStateContextProvider(),
                            ValidateRequest,
                            PayToPaymentGateway, 
                            PersistOrderToRepository
                        ));

                    });

//                //TODO: create a default statemanager
//                order.AsState(
//                    b =>
//                    {
//                        b.From(OrderState.Initial).To(OrderState.Paid);
//                        b.From(OrderState.Paid).To(OrderState.Processed);
//                        b.From(OrderState.Processed).To(OrderState.Delivered);
//
//                        b.OnEnter(OrderState.Paid, Guards<OrderState>.From(paidStateContextProvider(),
//                            ValidateRequest,
//                            PayToPaymentGateway, 
//                            PersistOrderToRepository
//                        ));
//
//                    }).ChangeTo(OrderState.Paid);
//                
//                _stateMachineManager.Register(state);
                return state;
            }
            
            //TODO: add global states trigger
            
            //TODO: add extensions

            private void ValidateRequest(PaymentGuardContext context)
            {
                context.Command.ValidateRequest_CallCount++;
                context.Continue = true;
            }

            private void PayToPaymentGateway(PaymentGuardContext context)
            {
                context.Command.PayToGateway_CallCount++;
                context.Continue = true;
            }

            private void PersistOrderToRepository(PaymentGuardContext context)
            {
                context.Command.PersistToRepo_CallCount++;
                _ordersRepository.UpdateOrder(context.Command.Order);
                context.Continue = true;
            }

        }
    }

}