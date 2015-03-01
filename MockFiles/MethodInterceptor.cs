using System;
using Castle.DynamicProxy;

namespace MockFiles
{
    internal class MethodInterceptor : IInterceptor
    {
        private readonly Delegate _impl;

        public MethodInterceptor(Delegate @delegate)
        {
            _impl = @delegate;
        }

        public void Intercept(IInvocation invocation)
        {
            var result = _impl.DynamicInvoke(invocation.Arguments);
            invocation.ReturnValue = result;
        }
    }
}