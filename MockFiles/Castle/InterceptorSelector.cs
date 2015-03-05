using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace MockFiles.Castle
{
    internal class InterceptorSelector : IInterceptorSelector
    {
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            foreach (var interceptor in interceptors)
            {
                var methodInterceptor = interceptor as MethodInterceptor;
                if (methodInterceptor == null)
                    continue;
                var d = methodInterceptor.Info;
                if (IsEquivalent(d, method))
                    return new[] { interceptor };
            }
            throw new ArgumentException();
        }

        private static bool IsEquivalent(MethodInfo dm, MethodInfo method)
        {
            if (!method.ReturnType.IsAssignableFrom(dm.ReturnType))
                return false;
            var parameters = method.GetParameters();
            var dp = dm.GetParameters();
            if (parameters.Length != dp.Length)
                return false;
            for (int i = 0; i < parameters.Length; i++)
            {
                //BUG: does not take into account modifiers (like out, ref...)
                if (!parameters[i].ParameterType.IsAssignableFrom(dp[i].ParameterType))
                    return false;
            }
            return true;
        }
    }
}