﻿using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace MockFiles.Castle
{
    public class MethodInterceptor : IInterceptor
      {
        private readonly MethodInfo _methodInfo;
        private readonly object _returnValue;

        public MethodInfo Info
        {
            get { return _methodInfo; }
        }

        public MethodInterceptor(MethodInfo methodInfo, object returnValue)
        {
            _methodInfo = methodInfo;
            _returnValue = returnValue;
        }

        public void Intercept(IInvocation invocation)
        {
            if (_returnValue.GetType() == typeof(Exception))
            {
                throw new Exception(((Exception)_returnValue).Message);
            }
            invocation.ReturnValue = _returnValue;
        }
      }
}
