using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace MockFiles.Tests
{
    [TestClass]
    public class MockProviderTests
    {
        [TestMethod]
        public void TestRegisterStubCreatesJsonFileWithData()
        {
            const string file = "IBand.GetMembers.json";
            DeleteFile(file);

            var band = new Band();
            var members = band.GetMembers();
            MockProvider.RegisterStub(band, new Func<List<Member>>(band.GetMembers), members);

            Assert.IsTrue(File.Exists(file));

            var json = File.ReadAllText(file);
            var returnObj = (List<Member>)JsonConvert.DeserializeObject(json, typeof(List<Member>));
            Assert.AreEqual(members.Count, returnObj.Count);
        }

        [TestMethod]
        public void TestRegisterStubCreatesJsonFileWithDataAndParameters()
        {
            const string file = "IBand.GetMembersByStatus_Boolean.json";
            DeleteFile(file);

            var band = new Band();
            var members = band.GetMembersByStatus(true);
            MockProvider.RegisterStub(band, new Func<bool, List<Member>>(band.GetMembersByStatus), members);

            Assert.IsTrue(File.Exists(file));
        }

        [TestMethod]
        public void TestGetMockCreatesMockObject()
        {
            const string file = "IBand.GetMembers.json";
            DeleteFile(file);

            var band = new Band();
            var members = band.GetMembers();
            var activeMembers = band.GetMembersByStatus(true);
            MockProvider.RegisterStub(band, new Func<List<Member>>(band.GetMembers), members);
            //MockProvider.RegisterStub(band, new Func<bool, List<Member>>(band.GetMembersByStatus), activeMembers);

            var mockBand = MockProvider.GetMock<IBand>();
            var stubMembers = mockBand.GetMembers();
            //var stubActiveMembers = mockBand.GetMembersByStatus(true);

            Assert.AreEqual(members.Count, stubMembers.Count);
            //Assert.AreEqual(activeMembers.Count, stubActiveMembers.Count);

        }

        [TestMethod]
        public void TestGetMockCreatesMockObjectWithParameterMethods()
        {
            DeleteFile("IBand.GetMembersByStatus.json");
            DeleteFile("IBand.GetMembersByStatus_Boolean.json");

            var band = new Band();
            var members = band.GetMembers();
            var activeMembers = band.GetMembersByStatus(true);
            MockProvider.RegisterStub(band, new Func<bool, List<Member>>(band.GetMembersByStatus), activeMembers);

            var mockBand = MockProvider.GetMock<IBand>();
            var stubActiveMembers = mockBand.GetMembersByStatus(true);

            Assert.AreEqual(activeMembers.Count, stubActiveMembers.Count);

        }

        private static void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
