using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MockFiles
{
    //http://kozmic.net/2009/03/20/castle-dynamic-proxy-tutorial-part-viii-interface-proxy-without-target/

    public static class MockProvider
    {
        public static T GetMock<T>() where T : class
        {
            var interfaceType = typeof(T);
            IInterceptor[] interceptors = GetInterceptors(interfaceType);
            var generator = new ProxyGenerator();
            var proxy = generator.CreateInterfaceProxyWithoutTarget(interfaceType, new ProxyGenerationOptions(), interceptors);
            var x = proxy as T;
            return x;
        }

        private static MethodInterceptor[] GetInterceptors(Type interfaceType)
        {
            var interceptors = new List<MethodInterceptor>();

            foreach (var method in interfaceType.GetMethods())
            {
                var className = interfaceType.Name;
                var methodName = method.Name;
                var returnType = method.ReturnType;
                var file = string.Format("{0}.{1}.json", className, methodName);
                if (!File.Exists(file)) continue;

                interceptors.Add(GetMethodInterceptor(file, returnType));
            }
            return interceptors.ToArray();
        }

        private static MethodInterceptor GetMethodInterceptor(string file, Type returnType)
        {
            var json = File.ReadAllText(file);
            var returnObj = JsonConvert.DeserializeObject(json, returnType);
            var interceptor = new Func<object, object>(z => returnObj);
            return new MethodInterceptor(interceptor);
        }

        /// <summary>
        /// Creates Json file from output of method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="classInstance"></param>
        /// <param name="func"></param>
        /// <param name="result"></param>
        public static void RegisterStub<T>(T classInstance, Delegate func, object result)
        {
            var className = typeof(T).GetInterfaces()[0].Name.Split('.').Last();
            var methodName = func.GetMethodInfo().Name;

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            using (StreamWriter sw = new StreamWriter(string.Format(@"{0}.{1}.json", className, methodName)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }
        }
    }
}
