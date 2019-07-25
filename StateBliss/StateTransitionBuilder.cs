using System;
using System.Linq.Expressions;

namespace StateBliss
{
    public class StateTransitionBuilder<TStatus> where TStatus : Enum
    {
        public IStateTo From(TStatus state)
        {
            throw new NotImplementedException();
        }

        public void OnEnter<T>(TStatus state, T target, Expression<Func<T, Action>> func)
        {
            throw new NotImplementedException();
        }
        
        public void OnExit<T>(TStatus state, T target, Expression<Func<T, Action>> func)
        {
            throw new NotImplementedException();
        }
    }
}