using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AzLog
{
    partial class AzLog
    {
        //static AzLogEntries BuildTestData1()
        //{
        //AzLogEntries
        //}

#region Constructor Tests
        [Test]
        public void TestAzLogEntryConstructor()
        {
            AzLogEntry azle = AzLogEntry.Create("Partition", Guid.Empty, 1, "AppName", "Level", 2, "a000", 3, 4,
                                                "Message");

            Assert.AreEqual("Partition", azle.Partition);
            Assert.AreEqual(Guid.Empty, azle.RowKey);
            Assert.AreEqual(1, azle.EventTickCount);
            Assert.AreEqual("AppName", azle.ApplicationName);
            Assert.AreEqual("Level", azle.Level);
            Assert.AreEqual(2, azle.EventId);
            Assert.AreEqual("0000A000", azle.InstanceId);
            Assert.AreEqual(3, azle.Pid);
            Assert.AreEqual(4, azle.Tid);
            Assert.AreEqual("Message", azle.Message);
        }

        [Test]
        public void TestAzLogEntryEntityConstructor()
        {
            AzLogEntryEntity azlee = new AzLogEntryEntity();

            azlee.PartitionKey = "Partition";
            azlee.RowKey = Guid.Empty.ToString();
            azlee.InstanceId = "a000";
            azlee.EventTickCount = 1;
            azlee.EventId = 2;
            azlee.Level = "Level";
            azlee.ApplicationName = "AppName";
            azlee.Pid = 3;
            azlee.Tid = 4;
            azlee.Message = "Message";

            AzLogEntry azle = AzLogEntry.Create(azlee);
            Assert.AreEqual("Partition", azle.Partition);
            Assert.AreEqual(Guid.Empty, azle.RowKey);
            Assert.AreEqual(1, azle.EventTickCount);
            Assert.AreEqual("AppName", azle.ApplicationName);
            Assert.AreEqual("Level", azle.Level);
            Assert.AreEqual(2, azle.EventId);
            Assert.AreEqual("0000A000", azle.InstanceId);
            Assert.AreEqual(3, azle.Pid);
            Assert.AreEqual(4, azle.Tid);
            Assert.AreEqual("Message", azle.Message);
        }
#endregion // ConstructorTests

        [TestCase("1/1/2003 8:00", "1/1/2003 9:00", "1/1/2003", 8, "1/1/2003", 9)]
        [TestCase("1/1/2003 12:00", "1/1/2003 13:00", "1/1/2003", 12, "1/1/2003", 13)]
        [TestCase("1/1/2003 0:00", "1/1/2003 2:00", "1/1/2003", 0, "1/1/2003", 2)]
        [TestCase("1/1/2003 8:00", "9:00", "1/1/2003", 8, "1/1/2003", 9)]
        [TestCase("1/1/2003 8:00", "1/2/2003 9:00", "1/1/2003", 8, "1/2/2003", 9)]
        [Test]
        public static void TestFillMinMacFromStartEnd(string sStart, string sEnd, string sDttmMinExpected, int nHourMinExpected,
                    string sDttmMacExpected, int nHourMacExpected)
        {
            DateTime dttmMin = DateTime.Parse(sDttmMinExpected);
            DateTime dttmMac = DateTime.Parse(sDttmMacExpected);
            DateTime dttmMinActual, dttmMacActual;
            int nHourMinActual, nHourMacActual;

            FillMinMacFromStartEnd(sStart, sEnd, out dttmMinActual, out nHourMinActual, out dttmMacActual, out nHourMacActual);
            Assert.AreEqual(dttmMin, dttmMinActual);
            Assert.AreEqual(dttmMac, dttmMacActual);
            Assert.AreEqual(nHourMinExpected, nHourMinActual);
            Assert.AreEqual(nHourMacExpected, nHourMacActual);
        }
    }
}
