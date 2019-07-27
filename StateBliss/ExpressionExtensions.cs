using System;
using System.Linq.Expressions;
using System.Reflection;

namespace StateBliss
{
    public static class ExpressionExtensions
    {
        public static string GetMethodName<T, TDelegate>(this Expression<Func<T, TDelegate>> handlerName)
            where TDelegate : Delegate
        {
            var unaryExpression = (UnaryExpression) handlerName.Body;
            var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
            var constantExpression = (ConstantExpression) methodCallExpression.Object;
            var methodInfoExpression = (MethodInfo)constantExpression.Value;
            return methodInfoExpression.Name;
        }
    }
}