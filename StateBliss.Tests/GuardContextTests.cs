//using System;
//using System.Threading.Tasks;
//using Xunit;
//
//namespace StateBliss.Tests
//{
//    public class GuardContextTests : IDisposable
//    {
//        private IStateMachineManager _stateMachineManager;
//        
//        public void Dispose()
//        {
//            _stateMachineManager?.Stop();
//        }
//
//        [Fact]
//        public async Task Test_GuardsOnChange()
//        {
//            // Arrange
//            var ordersRepository = new OrdersRepository();
//            _stateMachineManager = new StateMachineManager();
//            var orderStateDefinition = new OrderStateDefinition(_stateMachineManager);
//            var payOrderCommandHandler = new PayOrderCommandHandler(orderStateDefinition, ordersRepository);
//            var uid = Guid.NewGuid();
//            var order = new Order
//            {
//                Id = 1,
//                State = OrderState.Initial,
//                Uid = uid
//            };
//            ordersRepository.InsertOrder(order);
//            
//            var payOrderCommand = new PayOrderCommand
//            {
//                Order = order
//            };
//            
//            StateMachineManager.SetDefaultStateFactory(StateFactoryTestHelper.StatesProvider);
//
//            // Act
//            payOrderCommandHandler.Handle(payOrderCommand);
//            await _stateMachineManager.WaitAllHandlersProcessedAsync();
//
//            // Assert
//            Assert.True(payOrderCommand.Succeeded);
//            Assert.Equal(1, payOrderCommand.ValidateRequest_CallCount);
//            Assert.Equal(1, payOrderCommand.PayToGateway_CallCount);
//            Assert.Equal(1, payOrderCommand.PersistToRepo_CallCount);
//            Assert.Equal(OrderState.Paid, ordersRepository.GetOrder(order.Id).State);
//            Assert.Equal(OrderState.Paid, payOrderCommand.State.Current);
//        }
//        
//        
//        [Fact]
//        public async Task Test_StateFactory()
//        {
//            // Arrange
//            var guid = Guid.NewGuid();
//
//            var stateFactory = StateFactoryTestHelper.SetupStateFactory(guid, () => 
//
//                new State<MyStates>(guid, MyStates.NotClicked)
//                    .Define(b => { b.From(MyStates.NotClicked).To(MyStates.Clicked); })
//            );
//            StateMachineManager.Default.SetStateFactory(stateFactory);
//            
//            // Act
//            StateMachineManager.ChangeState(MyStates.Clicked, guid);
//            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run
//            
//            // Assert
//            Assert.Equal(MyStates.Clicked, StateMachineManager.GetState<MyStates>(guid).Current);
//        }
//
//        [Fact]
//        public async Task Test_AddGuardsOnStateEntry()
//        {
//            // Arrange
//            var guid = Guid.NewGuid();
//            var hasChanged = false;
//            var stateFactory = StateFactoryTestHelper.SetupStateFactory(guid, () => 
//
//                new State<MyStates>(guid, MyStates.NotClicked)
//                    .Define(b =>
//                    {
//                        b.From(MyStates.NotClicked).To(MyStates.Clicked)
//                            .Changed((s, n) =>
//                            {
//                                hasChanged = true;
//                            });
//                    })
//            );
//            StateMachineManager.SetDefaultStateFactory(stateFactory);
//
//            var state = StateMachineManager.GetState<MyStates>(guid);
//            var context = new GuardContext<MyStates>();
//            
//            state.GuardsForEntry(MyStates.Clicked, Guards<MyStates>.From(context,
//                GuardHandler1, 
//                GuardHandler2));
//            
//            // Act
//            StateMachineManager.ChangeState(MyStates.Clicked, guid);
//            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run
//            
//            // Assert
//            Assert.Equal(MyStates.Clicked, state.Current);
//            Assert.True(hasChanged);
//            Assert.True((bool)context.Data["GuardHandler1"]);
//            Assert.True((bool)context.Data["GuardHandler2"]);
//        }
//        
//        
//        [Fact]
//        public async Task Test_AddGuardsOnStateExit()
//        {
//            // Arrange
//            var guid = Guid.NewGuid();
//            var hasChanged = false;
//            var stateFactory = StateFactoryTestHelper.SetupStateFactory(guid, () => 
//
//                new State<MyStates>(guid, MyStates.NotClicked)
//                    .Define(b =>
//                    {
//                        b.From(MyStates.NotClicked).To(MyStates.Clicked)
//                            .Changed((s, n) =>
//                            {
//                                hasChanged = true;
//                            });
//                    })
//            );
//            StateMachineManager.SetDefaultStateFactory(stateFactory);
//
//            var state = StateMachineManager.GetState<MyStates>(guid);
//            var context = new GuardContext<MyStates>();
//            
//            state.GuardsForExit(MyStates.NotClicked, Guards<MyStates>.From(context,
//                GuardHandler1, 
//                GuardHandler2));
//            
//            // Act
//            StateMachineManager.ChangeState(MyStates.Clicked, guid);
//            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run
//            
//            // Assert
//            Assert.Equal(MyStates.Clicked, state.Current);
//            Assert.True(hasChanged);
//            Assert.True((bool)context.Data["GuardHandler1"]);
//        }
//
//        [Fact]
//        public async Task Test_AddGuardsOnStateEdit()
//        {
//            // Arrange
//            var guid = Guid.NewGuid();
//            var stateFactory = StateFactoryTestHelper.SetupStateFactory(guid, () =>
//
//                new State<MyStates>(guid, MyStates.NotClicked)
//                    .Define(b =>
//                    {
//                        b.From(MyStates.NotClicked).To(MyStates.Clicked);
//                    })
//            );
//            StateMachineManager.SetDefaultStateFactory(stateFactory);
//
//            var state = StateMachineManager.GetState<MyStates>(guid);
//            var context = new GuardContext<MyStates>();
//
//            state.GuardsForExit(MyStates.NotClicked, Guards<MyStates>.From(context,
//                GuardHandler1,
//                GuardHandler2));
//
//            // Act
//            StateMachineManager.ChangeState(MyStates.NotClicked, guid);
//            await StateMachineManager.Default.WaitAllHandlersProcessedAsync(); //this is only needed for unit test to ensure that all handlers are run
//
//            // Assert
//            Assert.Equal(MyStates.NotClicked, state.Current);
//            Assert.True((bool)context.Data["GuardHandler1"]);
//        }
//
//        private void GuardHandler2(GuardContext<MyStates> context)
//        {
//            context.Continue = true;
//            context.Data["GuardHandler2"] = true;
//        }
//
//        private void GuardHandler1(GuardContext<MyStates> context)
//        {
//            context.Continue = true;
//            context.Data["GuardHandler1"] = true;
//        }
//
//        [Fact]
//        public async Task Test_GuardsOnStateDefine()
//        {
//            // Arrange
//            var ordersRepository = new OrdersRepository();
//            _stateMachineManager = new StateMachineManager();
//            var orderStateDefinition = new OrderStateDefinition(_stateMachineManager, ordersRepository);
//            var payOrderCommandHandler = new PayOrderCommandHandler(orderStateDefinition, ordersRepository);
//            var order = new Order
//            {
//                Id = 1,
//                State = OrderState.Initial
//            };
//            ordersRepository.InsertOrder(order);
//            
//            var payOrderCommand = new PayOrderCommand
//            {
//                GetStateWithGuards = true,
//                Order = order
//            };
//
//            // Act
//            payOrderCommandHandler.Handle(payOrderCommand);
//            await _stateMachineManager.WaitAllHandlersProcessedAsync();
//
//            // Assert
//            Assert.True(payOrderCommand.Succeeded);
//            Assert.Equal(1, payOrderCommand.ValidateRequest_CallCount);
//            Assert.Equal(1, payOrderCommand.PayToGateway_CallCount);
//            Assert.Equal(1, payOrderCommand.PersistToRepo_CallCount);
//            Assert.Equal(OrderState.Paid, ordersRepository.GetOrder(order.Id).State);
//            Assert.Equal(OrderState.Paid, payOrderCommand.State.Current);
//        }
//        
//   
//        
//
//        public class PayOrderCommandHandler
//        {
//            private readonly OrderStateDefinition _orderStateDefinition;
//            private readonly IStateMachineManager _stateMachineManager;
//            private readonly OrdersRepository _ordersRepository;
//
//            public PayOrderCommandHandler(IStateMachineManager stateMachineManager, OrdersRepository ordersRepository)
//            {
//                _stateMachineManager = stateMachineManager;
//                _ordersRepository = ordersRepository;
//            }
//            
//            public void Handle(PayOrderCommand cmd)
//            {
//                var context = new PaymentGuardContext
//                {
//                    Command = cmd
//                };
//
//                var state = _stateMachineManager.GetState<OrderState>(cmd.Order.Uid);
//                var hasChangedState = state.ChangeTo(OrderState.Paid, context);
//                cmd.State = (State<Order, OrderState>)state;
//                cmd.Succeeded = hasChangedState;
//            }
//
//            private void ValidateRequest(PaymentGuardContext context)
//            {
//                context.Command.ValidateRequest_CallCount++;
//                context.Continue = true;
//            }
//
//            private void PayToPaymentGateway(PaymentGuardContext context)
//            {
//                context.Command.PayToGateway_CallCount++;
//                context.Continue = true;
//            }
//
//            private void PersistOrderToRepository(PaymentGuardContext context)
//            {
//                context.Command.PersistToRepo_CallCount++;
//                _ordersRepository.UpdateOrder(context.Command.Order);
//                context.Continue = true;
//            }
//            
//        }
//        
//
//        
//        public class PayOrderWithGuardsCommandHandler
//        {
//            private readonly OrderStateDefinition _orderStateDefinition;
//            private readonly OrdersRepository _ordersRepository;
//
//            public PayOrderWithGuardsCommandHandler(OrderStateDefinition orderStateDefinition, OrdersRepository ordersRepository)
//            {
//                _orderStateDefinition = orderStateDefinition;
//                _ordersRepository = ordersRepository;
//            }
//            
//            public void Handle(PayOrderCommand cmd)
//            {
//                var context = new PaymentGuardContext
//                {
//                    Command = cmd
//                };
//
//                var state = cmd.GetStateWithGuards
//                    ? _orderStateDefinition.GetStateWithGuards(cmd.Order, () => context)
//                    : _orderStateDefinition.GetState(cmd.Order);
//
//                var hasChangedState = false;
//                if (cmd.GetStateWithGuards)
//                {
//                    hasChangedState = state.ChangeTo(OrderState.Paid);
//                }
//                else
//                {
//                    hasChangedState = state.ChangeTo(OrderState.Paid,
//                        Guards<OrderState>.From(context,
//                            ValidateRequest,
//                            PayToPaymentGateway,
//                            PersistOrderToRepository
//                        ));
//                }
//
//                cmd.State = state;
//                cmd.Succeeded = hasChangedState;
//            }
//
//            private void ValidateRequest(PaymentGuardContext context)
//            {
//                context.Command.ValidateRequest_CallCount++;
//                context.Continue = true;
//            }
//
//            private void PayToPaymentGateway(PaymentGuardContext context)
//            {
//                context.Command.PayToGateway_CallCount++;
//                context.Continue = true;
//            }
//
//            private void PersistOrderToRepository(PaymentGuardContext context)
//            {
//                context.Command.PersistToRepo_CallCount++;
//                _ordersRepository.UpdateOrder(context.Command.Order);
//                context.Continue = true;
//            }
//            
//        }
//
//        
//        public class OrderStateDefinition
//        {
//            private readonly IStateMachineManager _stateMachineManager;
//            private readonly OrdersRepository _ordersRepository;
//
//            public OrderStateDefinition(IStateMachineManager stateMachineManager, OrdersRepository ordersRepository = null)
//            {
//                _stateMachineManager = stateMachineManager;
//                _ordersRepository = ordersRepository;
//            }
//
//            public State<Order, OrderState> GetState(Order order)
//            {
//                var state = new State<Order, OrderState>(order, a => a.State)
//                    .Define(b =>
//                    {
//                        b.From(OrderState.Initial).To(OrderState.Paid);
//                        b.From(OrderState.Paid).To(OrderState.Processed);
//                        b.From(OrderState.Processed).To(OrderState.Delivered);
//                    });
//
//                _stateMachineManager.Register(state);
//                return state;
//            }
//
//            public State<Order, OrderState> GetStateWithGuards(Order order, Func<PaymentGuardContext> paidStateContextProvider)
//            {
//                var state = new State<Order, OrderState>(order, a => a.State)
//                    .Define(b =>
//                    {
//                        b.From(OrderState.Initial).To(OrderState.Paid);
//                        b.From(OrderState.Paid).To(OrderState.Processed);
//                        b.From(OrderState.Processed).To(OrderState.Delivered);
//
//                        b.OnEntering(OrderState.Paid, Guards<OrderState>.From(paidStateContextProvider(),
//                            ValidateRequest,
//                            PayToPaymentGateway, 
//                            PersistOrderToRepository
//                        ));
//
//                    });
//
////                //TODO: create a default statemanager
////                order.AsState(
////                    b =>
////                    {
////                        b.From(OrderState.Initial).To(OrderState.Paid);
////                        b.From(OrderState.Paid).To(OrderState.Processed);
////                        b.From(OrderState.Processed).To(OrderState.Delivered);
////
////                        b.OnEnter(OrderState.Paid, Guards<OrderState>.From(paidStateContextProvider(),
////                            ValidateRequest,
////                            PayToPaymentGateway, 
////                            PersistOrderToRepository
////                        ));
////
////                    }).ChangeTo(OrderState.Paid);
////                
////                _stateMachineManager.Register(state);
//                return state;
//            }
//            
//            //TODO: add global states trigger
//            
//            //TODO: add extensions
//
//            private void ValidateRequest(PaymentGuardContext context)
//            {
//                context.Command.ValidateRequest_CallCount++;
//                context.Continue = true;
//            }
//
//            private void PayToPaymentGateway(PaymentGuardContext context)
//            {
//                context.Command.PayToGateway_CallCount++;
//                context.Continue = true;
//            }
//
//            private void PersistOrderToRepository(PaymentGuardContext context)
//            {
//                context.Command.PersistToRepo_CallCount++;
//                _ordersRepository.UpdateOrder(context.Command.Order);
//                context.Continue = true;
//            }
//
//        }
//    }
//}