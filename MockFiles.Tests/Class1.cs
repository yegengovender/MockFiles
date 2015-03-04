using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MockFiles.Tests
{
    class MyType
    {
        public MyType(int i) { this.Value = i; }

        public void SetValue(int i) { this.Value = i; }

        public void SetSumValue(int a, int b) { this.Value = a + b; }

        public int Value { get; set; }
    }
      public class DelegateBuilder
      {
          public static T BuildDelegate<T>(MethodInfo method, params object[] missingParamValues)
          {
              var queueMissingParams = new Queue<object>(missingParamValues);

              var dgtMi = typeof(T).GetMethod("Invoke");
              var dgtRet = dgtMi.ReturnType;
              var dgtParams = dgtMi.GetParameters();

              var paramsOfDelegate = dgtParams
                  .Select(tp => Expression.Parameter(tp.ParameterType, tp.Name))
                  .ToArray();

              var methodParams = method.GetParameters();

              if (method.IsStatic)
              {
                  var paramsToPass = methodParams
                      .Select((p, i) => CreateParam(paramsOfDelegate, i, p, queueMissingParams))
                      .ToArray();

                  var expr = Expression.Lambda<T>(
                      Expression.Call(method, paramsToPass),
                      paramsOfDelegate);

                  return expr.Compile();
              }
              else
              {
                  var paramThis = Expression.Convert(paramsOfDelegate[0], method.DeclaringType);

                  var paramsToPass = methodParams
                      .Select((p, i) => CreateParam(paramsOfDelegate, i + 1, p, queueMissingParams))
                      .ToArray();

                  var expr = Expression.Lambda<T>(
                      Expression.Call(paramThis, method, paramsToPass),
                      paramsOfDelegate);

                  return expr.Compile();
              }
          }

          private static Expression CreateParam(ParameterExpression[] paramsOfDelegate, int i, ParameterInfo callParamType, Queue<object> queueMissingParams)
          {
              if (i < paramsOfDelegate.Length)
                  return Expression.Convert(paramsOfDelegate[i], callParamType.ParameterType);

              if (queueMissingParams.Count > 0)
                  return Expression.Constant(queueMissingParams.Dequeue());

              if (callParamType.ParameterType.IsValueType)
                  return Expression.Constant(Activator.CreateInstance(callParamType.ParameterType));

              return Expression.Constant(null);
          }
      }

      public class MethodInterceptor : IInterceptor
      {
          private readonly Delegate _impl;

          public MethodInterceptor(Delegate @delegate)
          {
              _impl = @delegate;
          }

          public Delegate Delegate
          {
              get { return _impl; }
          }

          public void Intercept(IInvocation invocation)
          {
              var result = _impl.DynamicInvoke(invocation.Arguments);
              invocation.ReturnValue = result;
          }
      }

      public class MethodInterceptor2<T> : IInterceptor
      {
          private readonly Delegate _impl;

          public MethodInterceptor2(Func<T> @delegate)
          {
              _impl = @delegate;
          }

          public void Intercept(IInvocation invocation)
          {
              var result = _impl.DynamicInvoke(invocation.Arguments);
              invocation.ReturnValue = result;
          }
      }

      internal class DelegateSelector : IInterceptorSelector
      {
          public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
          {
              foreach (var interceptor in interceptors)
              {
                  var methodInterceptor = interceptor as MethodInterceptor;
                  if (methodInterceptor == null)
                      continue;
                  var d = methodInterceptor.Delegate;
                  if (IsEquivalent(d, method))
                      return new[] { interceptor };
              }
              throw new ArgumentException();
          }

          private static bool IsEquivalent(Delegate d, MethodInfo method)
          {
              var dm = d.Method;
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
      public class DelegateBuilder
      {
          public static T BuildDelegate<T>(MethodInfo method, params object[] missingParamValues)
          {
              var queueMissingParams = new Queue<object>(missingParamValues);

              var dgtMi = typeof(T).GetMethod("Invoke");
              var dgtRet = dgtMi.ReturnType;
              var dgtParams = dgtMi.GetParameters();

              var paramsOfDelegate = dgtParams
                  .Select(tp => Expression.Parameter(tp.ParameterType, tp.Name))
                  .ToArray();

              var methodParams = method.GetParameters();

              if (method.IsStatic)
              {
                  var paramsToPass = methodParams
                      .Select((p, i) => CreateParam(paramsOfDelegate, i, p, queueMissingParams))
                      .ToArray();

                  var expr = Expression.Lambda<T>(
                      Expression.Call(method, paramsToPass),
                      paramsOfDelegate);

                  return expr.Compile();
              }
              else
              {
                  var paramThis = Expression.Convert(paramsOfDelegate[0], method.DeclaringType);

                  var paramsToPass = methodParams
                      .Select((p, i) => CreateParam(paramsOfDelegate, i + 1, p, queueMissingParams))
                      .ToArray();

                  var expr = Expression.Lambda<T>(
                      Expression.Call(paramThis, method, paramsToPass),
                      paramsOfDelegate);

                  return expr.Compile();
              }
          }

          private static Expression CreateParam(ParameterExpression[] paramsOfDelegate, int i, ParameterInfo callParamType, Queue<object> queueMissingParams)
          {
              if (i < paramsOfDelegate.Length)
                  return Expression.Convert(paramsOfDelegate[i], callParamType.ParameterType);

              if (queueMissingParams.Count > 0)
                  return Expression.Constant(queueMissingParams.Dequeue());

              if (callParamType.ParameterType.IsValueType)
                  return Expression.Constant(Activator.CreateInstance(callParamType.ParameterType));

              return Expression.Constant(null);
          }
      }

      public static class MockProvider
      {
          public static T GetMock<T>() where T : class
          {
              var interfaceType = typeof(T);
              IInterceptor[] interceptors = GetInterceptors(interfaceType);
              var generator = new ProxyGenerator();
              var options = new ProxyGenerationOptions { Selector = new DelegateSelector() };
              var proxy = generator.CreateInterfaceProxyWithoutTarget(interfaceType, options, interceptors);
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

              using (StreamWriter sw = new StreamWriter(string.Format(@"{0}.{1}.json", className, methodName)))
              using (JsonWriter writer = new JsonTextWriter(sw))
              {
                  serializer.Serialize(writer, result);
              }
          }

          private static Delegate GetDelegateFromMethod<T>(MethodInfo method, T returnValue) where T : class
          {

              List<Type> args = new List<Type>(method.GetParameters().Select(p => p.ParameterType));
              Type delegateType;
              if (method.ReturnType == typeof(void))
              {
                  delegateType = Expression.GetActionType(args.ToArray());
              }
              else
              {
                  args.Add(method.ReturnType);
                  delegateType = Expression.GetFuncType(args.ToArray());
              }
              Delegate d = Delegate.CreateDelegate(delegateType, null, method);
              var x = d.GetInvocationList();
              return d;
          }
      }
  }
