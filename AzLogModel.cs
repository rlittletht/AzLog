using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework.Constraints;

namespace AzLog
{
    public class AzLogModel
    {
        private AzTable m_azt = null;
        private AzLogEntries m_azles;

        public int LogCount => m_azles.Length;

        public AzLogEntry LogEntry(int i)
        {
            return m_azles.Item(i);
        }

        private List<AzLogView> m_plazlvListeners = new List<AzLogView>();

        public void AddView(AzLogView azlv)
        {
            m_plazlvListeners.Add(azlv);
        }

        public void RemoveView(AzLogView azlv)
        {
            for (int i = 0; i < m_plazlvListeners.Count; i++)
                if (m_plazlvListeners[i] == azlv)
                    {
                    m_plazlvListeners.RemoveAt(i);;
                    return;
                    }
        }
        public void OpenTable(string sTableName)
        {
            foreach (AzLogView azlv in m_plazlvListeners)
                azlv.ClearLog();

            m_azt = m_aztc.GetTable(sTableName);
            // PopulateLog();
        }

        private AzTableCollection m_aztc;
        private List<string> m_plsTables;

        public List<string> Tables => m_plsTables;
         
        public void OpenAccount(string sAccountName, string sAccountKey)
        {
            m_aztc = new AzTableCollection(sAccountName, sAccountKey);
            m_plsTables = m_aztc.PlsTableNames();

        }


        public AzLogModel()
        {
            m_azles = new AzLogEntries();
        }

        public void AddTestDataPartition(DateTime dttmPartition, Int64[] rgTickCount, string []rgs)
        {
            AzLogEntries azles = new AzLogEntries();
            for (int i = 0; i < rgTickCount.Length; i++)
                {
                Int64 nTickCount = rgTickCount[i];
                string s = rgs == null ? "msg" : rgs[i];

                AzLogEntry azle = AzLogEntry.Create(dttmPartition.ToString("yyyyMMddHH"), Guid.NewGuid(), nTickCount, "testdata", "Informational",
                                                    1, "2", 3, 4, s);

                m_azles.AddLogEntry(azle);
                }

            azles.UpdatePart(dttmPartition, dttmPartition.AddHours(1), AzLogPartState.Complete);
        }

        public async Task<bool> FetchPartitionsForDateRange(DateTime dttmMin, DateTime dttmMac)
        {

            while (dttmMin < dttmMac)
                {
                if (m_azles.GetPartState(dttmMin) != AzLogPartState.Complete)
                    {
                    m_azles.UpdatePart(dttmMin, dttmMin.AddHours(1), AzLogPartState.Pending);
                    FetchPartitionForDate(dttmMin);
                    }
                dttmMin = dttmMin.AddHours(1);
                }
            return true;
        }

        public async Task<bool> FetchPartitionForDate(DateTime dttm)
        {
            TableQuery<AzLogEntryEntity> tq =
                new TableQuery<AzLogEntryEntity>().Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                                                           SPartitionFromDate(dttm, dttm.Hour)));

            TableQuerySegment<AzLogEntryEntity> azleSegment = null;
            m_azles.UpdatePart(dttm, dttm.AddHours(1.0), AzLogPartState.Pending);

            while (azleSegment == null || azleSegment.ContinuationToken != null)
                {
                azleSegment = await m_azt.Table.ExecuteQuerySegmentedAsync(tq, azleSegment?.ContinuationToken);
                foreach (AzLogView azlv in m_plazlvListeners)
                    {
                    lock (azlv.OSyncView)
                        {
                        int iFirst = m_azles.Length;

                        m_azles.AddSegment(azleSegment);

                        int iLast = m_azles.Length;

                        azlv.UpdateViewRegion(iFirst, iLast);
                        }
                    }

                m_azles.UpdatePart(dttm, dttm.AddHours(1.0), AzLogPartState.Complete);
                }
            return true;
        }

        string SPartitionFromDate(DateTime dttm, int nHour)
        {
            return String.Format("{0:D4}{1:D2}{2:D2}{3:D2}", dttm.Year, dttm.Month, dttm.Day, nHour);
        }

        public static void FillMinMacFromStartEnd(string sStart, string sEnd, out DateTime dttmMin, out DateTime dttmMac)
        {
            DateTime dttmStart = DateTime.Parse(sStart);
            DateTime dttmEnd = DateTime.Parse(sEnd);

            if (dttmEnd.Year == 1900 || (sEnd.IndexOf("/") == -1 && sEnd.IndexOf("-") == -1))
                {
                dttmEnd = dttmStart.AddHours(dttmEnd.Hour - dttmStart.Hour);
                }

            dttmStart = dttmStart.AddSeconds(-dttmStart.Second);
            dttmMin = dttmStart.AddMinutes(-dttmStart.Minute);

            dttmEnd = dttmEnd.AddSeconds(-dttmEnd.Second);
            dttmMac = dttmEnd.AddMinutes(-dttmEnd.Minute);
        }

    }

    public class AzLogEntryEntity : TableEntity
    {
        // also implicitly present
        // Partition
        // RowKey

        private Int64 m_nEventTickCount;
        private string m_sAppName;
        private string m_sLevel;
        private int m_nEventID;
        private int m_nInstanceID;
        private int m_nPid;
        private int m_nTid;
        private string m_sMessage;


        public AzLogEntryEntity()
        {
            
        }

        public Int64 EventTickCount   {
            get { return m_nEventTickCount; }
            set { m_nEventTickCount = value; }
        }
        public string ApplicationName   {
            get { return m_sAppName; }
            set { m_sAppName = value; }
        }
        public string Level   {
            get { return m_sLevel; }
            set { m_sLevel = value; }
        }

        public int EventId   {
            get { return m_nEventID; }
            set { m_nEventID = value; }
        }

        public string InstanceId   {
            get { return m_nInstanceID.ToString("X8"); }
            set { m_nInstanceID = int.Parse(value, NumberStyles.AllowHexSpecifier); }
        }
        public int Pid   {
            get { return m_nPid; }
            set { m_nPid = value; }
        }

        public int Tid   {
            get { return m_nTid; }
            set { m_nTid = value; }
        }
#if num
        public string EventId   {
            get { return m_nEventID.ToString(); }
            set { m_nEventID = int.Parse(value); }
        }
#endif

        public string Message
        {
            get { return m_sMessage; }
            set { m_sMessage = value; }
        }


    }
}
