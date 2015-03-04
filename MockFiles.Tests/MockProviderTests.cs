using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Castle.DynamicProxy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;

namespace MockFiles.Tests
{
    [TestClass]
    public class MockProviderTests
    {
        [TestMethod]
        public void TestRegisterStubCreatesJsonFileWithData()
        {
            var file = "IBand.GetMembers.json";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            var band = new Band();
            var members = band.GetMembers();
            MockProvider.RegisterStub(band, new Func<List<Member>>(band.GetMembers), members);

            Assert.IsTrue(File.Exists(file));

            var json = File.ReadAllText(file);
            var returnObj = (List<Member>)JsonConvert.DeserializeObject(json, typeof(List<Member>));
            Assert.AreEqual(members.Count, returnObj.Count);
        }

        [TestMethod]
        public void TestGetMockCreatesMockObject()
        {
            var file = "IBand.GetMembers.json";
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            var band = new Band();
            var members = band.GetMembers();
            var activeMembers = band.GetMembersByStatus(true);
            MockProvider.RegisterStub(band, new Func<List<Member>>(band.GetMembers), members);
            MockProvider.RegisterStub(band, new Func<bool, List<Member>>(band.GetMembersByStatus), activeMembers);

            var mockBand = MockProvider.GetMock<IBand>();
            //var stubMembers = mockBand.GetMembers();
            var stubActiveMembers = mockBand.GetMembersByStatus(true);

            //Assert.AreEqual(members.Count, stubMembers.Count);
            Assert.AreEqual(activeMembers.Count, stubActiveMembers.Count);

        }

        [TestMethod]
        public void TryBuildExpressions()
        {
            Type type = typeof(MyType);

            var mi = type.GetMethod("SetValue");

            var obj1 = new MyType(1);
            var obj2 = new MyType(2);

            var action = DelegateBuilder.BuildDelegate<Action<object, int>>(mi);

            action(obj1, 3);
            action(obj2, 4);

            Console.WriteLine(obj1.Value);
            Console.WriteLine(obj2.Value);

            // Sample passing a default value for the 2nd param of SetSumValue.
            var mi2 = type.GetMethod("SetSumValue");

            var action2 = DelegateBuilder.BuildDelegate<Action<object, int>>(mi2, 10);

            action2(obj1, 3);
            action2(obj2, 4);

            Console.WriteLine(obj1.Value);
            Console.WriteLine(obj2.Value);

            // Sample without passing a default value for the 2nd param of SetSumValue.
            // It will just use the default int value that is 0.
            var action3 = DelegateBuilder.BuildDelegate<Action<object, int>>(mi2);

            action3(obj1, 3);
            action3(obj2, 4);

            Console.WriteLine(obj1.Value);
            Console.WriteLine(obj2.Value);
        }

        [TestMethod]
        public void Scratch()
        {
            Type type = typeof(IBand);

            var mi = type.GetMethod("GetMembersByStatus");

            var action = DelegateBuilder.BuildDelegate<Action<object, int>>(mi);
        }

        [TestMethod]
        public void Scratch2()
        {
            string name = "DefineMethodOverrideExample";
            AssemblyName asmName = new AssemblyName(name);
            AssemblyBuilder ab =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    asmName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = ab.DefineDynamicModule(name, name + ".dll");

            TypeBuilder tb =
                mb.DefineType("C", TypeAttributes.Public, typeof(A));
            tb.AddInterfaceImplementation(typeof(I));

            // Build the method body for the explicit interface  
            // implementation. The name used for the method body  
            // can be anything. Here, it is the name of the method, 
            // qualified by the interface name. 
            //
            MethodBuilder mbIM = tb.DefineMethod("I.M",
                MethodAttributes.Private | MethodAttributes.HideBySig |
                    MethodAttributes.NewSlot | MethodAttributes.Virtual |
                    MethodAttributes.Final,
                null,
                Type.EmptyTypes);
            ILGenerator il = mbIM.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "The I.M implementation of C");
            il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine",
                new Type[] { typeof(string) }));
            il.Emit(OpCodes.Ret);

            // DefineMethodOverride is used to associate the method  
            // body with the interface method that is being implemented. 
            //
            tb.DefineMethodOverride(mbIM, typeof(I).GetMethod("M"));

            MethodBuilder mbM = tb.DefineMethod("M",
                MethodAttributes.Public | MethodAttributes.ReuseSlot |
                    MethodAttributes.Virtual | MethodAttributes.HideBySig,
                null,
                Type.EmptyTypes);
            il = mbM.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "Overriding A.M from C.M");
            il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine",
                new Type[] { typeof(string) }));
            il.Emit(OpCodes.Ret);

            Type tc = tb.CreateType();

            // Save the emitted assembly, to examine with Ildasm.exe.
            ab.Save(name + ".dll");

            Object test = Activator.CreateInstance(tc);

            MethodInfo mi = typeof(I).GetMethod("M");
            mi.Invoke(test, null);

            mi = typeof(A).GetMethod("M");
            mi.Invoke(test, null);
        }
    }
}
