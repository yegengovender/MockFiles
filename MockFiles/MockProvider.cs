using System;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using MockFiles.Castle;

namespace MockFiles
{
    //http://kozmic.net/2009/03/20/castle-dynamic-proxy-tutorial-part-viii-interface-proxy-without-target/

    public static class MockProvider
    {
        /// <summary>
        /// Creates Json file from output of method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classInstance">Object that implements interface to be stubbed</param>
        /// <param name="func">Delegate definition of method on object</param>
        /// <param name="result">Output data that will be stored as stub in Json</param>
        public static void RegisterStub<T>(T classInstance, Delegate func, object result)
        {
            var className = typeof(T).GetInterfaces()[0].Name.Split('.').Last();
            var methodInfo = func.GetMethodInfo();

            FileHelper.CreateJsonFromMethod<T>(className, methodInfo, result);
        }


        public static T GetMock<T>() where T : class
        {
            var interfaceType = typeof(T);
            IInterceptor[] interceptors = GetInterceptors(interfaceType);
            var generator = new ProxyGenerator();
            var options = new ProxyGenerationOptions { Selector = new InterceptorSelector() };
            var proxy = generator.CreateInterfaceProxyWithoutTarget(interfaceType, options, interceptors);
            var x = proxy as T;
            return x;
        }

        private static MethodInterceptor[] GetInterceptors(Type interfaceType)
        {
            var className = interfaceType.Name;
            return interfaceType.GetMethods()
                .Select(method => new MethodInterceptor(method, FileHelper.GetObjectFromJson(className, method)))
                .ToArray();
        }

    }
}
