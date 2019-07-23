using System;

namespace StateBliss
{
    public class ActionInfo
    {
        public Guid Id { get; }
        private Action _method;
        private readonly string _methodName;
        private object _target;

        public ActionInfo(Guid id, string methodName, object target)
        {
            Id = id;
            _methodName = methodName;
            _target = target;
            CreateDelegateFromInstance();
        }

        private void CreateDelegateFromInstance()
        {
            _method = Delegate.CreateDelegate(typeof (Action), _target, _methodName) as Action;
        }

        public void Execute()
        {
            _method.Invoke();
        }
    }
}