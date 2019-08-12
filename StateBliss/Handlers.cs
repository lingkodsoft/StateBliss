//using System;
//
//namespace StateBliss
//{
//    public static class Handlers
//    {
//        //IHandlersInfo<TState, TContext> handlers
////        public static IHandlersInfo<TState, TContext> From<TState, TContext>(params OnStateEventHandler<TState, TContext>[] actions)
////            where TState : Enum
////            where TContext : StateContext
////        {
////            return new HandlersInfo<TState, TContext>(actions);
////        }
////        
////        
////        public static IHandlersInfo<TState, StateContext> From<TState>(
////            Func<HandlerStateContext<TState>> contextProvider,
////            params OnStateEventHandler<TState, HandlerStateContext<TState>>[] actions)
////            where TState : Enum
////        {
////            return new HandlersInfo<TState, StateContext>(actions);
////        }
////        
////        
//        public static IHandlersInfo<TState, TContext> From<TState, TContext>(
//            Func<TContext> contextProvider,
//            params OnStateEventHandler<TState, TContext>[] actions)
//            where TState : Enum
//            where TContext : HandlerStateContext<TState>
//
//        {
//            throw new NotImplementedException();
////            return new HandlersInfo<TState, StateContext>(actions);
//        }
//        
////        
////        public static IHandlersInfo<TState, StateContext> From<TState, TContext>(
////            Func<HandlerStateContext<TState>> contextProvider,
////            params OnStateEventHandler<TState, HandlerStateContext<TState>>[] actions)
////            where TContext : HandlerStateContext<TState>
////            where TState : Enum
////        {
////            return new HandlersInfo<TState, StateContext>(actions);
////        }
//        
////        
////        public static IHandlersInfo<TContext> From<TContext>(TContext command, 
////            params OnStateEventHandler<TContext>[] actions)
////            where TContext : StateContext
////        {
////            return new HandlersInfo<TContext>(command, actions);
////        }
////        
////        public static IHandlersInfo<TContext> From<TContext>(Func<TContext> contextProvider, 
////            params OnStateEventHandler<TContext>[] actions)
////            where TContext : StateContext
////        {
////            return new HandlersInfo<TContext>(contextProvider, actions);
////        }
//    }
//}