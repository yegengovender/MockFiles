using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using MockFiles.Castle;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
            var methodName = func.GetMethodInfo().Name;

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            string paramsSuffix = ParamsSuffix(func.GetMethodInfo().GetParameters());
            using (StreamWriter sw = new StreamWriter(MockFileName(className, methodName, paramsSuffix)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }
        }

        private static string MockFileName(string className, string methodName, string paramsSuffix)
        {
            return string.Format(@"{0}.{1}{2}.json", className, methodName, paramsSuffix);
        }

        private static string ParamsSuffix(ParameterInfo[] getParameters)
        {
            var sb = new StringBuilder();

            foreach (var parameterInfo in getParameters)
            {
                sb.Append("_" + parameterInfo.ParameterType.Name);
            }

            return sb.ToString();
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
                .Select(method => new MethodInterceptor(method, GetObjectFromJson(className, method)))
                .ToArray();
        }

        private static object GetObjectFromJson(string className, MethodInfo method)
        {
            var methodName = method.Name;
            var returnType = method.ReturnType;
            string paramsSuffix = ParamsSuffix(method.GetParameters());
            var file = MockFileName(className, methodName, paramsSuffix);

            var returnObj = File.Exists(file)
                ? JsonConvert.DeserializeObject(File.ReadAllText(file), returnType)
                : new Exception(string.Format("Json File was not created for method [{0}] in type [{1}]", methodName, className));

            return returnObj;
        }
    }
}
