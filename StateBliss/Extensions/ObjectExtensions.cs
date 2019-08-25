using System;

namespace StateBliss
{
    internal static class ObjectExtensions
    {
        public static void WhenSome<T>(this T target, Action<T> action)
        {
            if (target != null)
            {
                action(target);
            }
        }
    }
}