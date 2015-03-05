using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MockFiles
{
    internal class FileHelper
    {
        public static void CreateJsonFromMethod<T>(string className, MethodInfo methodInfo, object result)
        {
            var methodName = methodInfo.Name;
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            string paramsSuffix = ParamsSuffix(methodInfo.GetParameters());
            using (StreamWriter sw = new StreamWriter(MockFileName(className, methodName, paramsSuffix)))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, result);
            }
        }

        public static object GetObjectFromJson(string className, MethodInfo method)
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

    }
}