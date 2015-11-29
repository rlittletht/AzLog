using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AzLog
{
    // The model holds all of the data for all the views. This model is an aggregate of all the
    // different datasources that have been added/opened. 
    //
    // Some datasoures are queryable for more data (like Azure), and some are just static
    // "imports" of data (like files).  The model doesn't care. If you ask if for more data
    // it will query all the datasources that can be queried and assumes that the files are
    // already fully loaded. If your file based logging splits into separate files based 
    // on time, then you will have to manually open/import more files
    public class AzLogModel
    {
        // this deals with partitioning our data and allowing us to have a virtual log
        private AzLogEntries m_azles;

        // these are all the views that we are currently attached to. all of our data updates need
        // to propagate to them
        private List<AzLogView> m_plazlvListeners;

        // this is the underlying count of log entries
        public int LogCount => m_azles.Length;

        // TODO: Temporary during refactor!
        public List<AzLogView> Listeners => m_plazlvListeners;
        public AzLogEntries Log => m_azles;

        /* L O G  E N T R Y */
        /*----------------------------------------------------------------------------
        	%%Function: LogEntry
        	%%Qualified: AzLog.AzLogModel.LogEntry
        	%%Contact: rlittle
        	
            Get the actual log entry at index i
        ----------------------------------------------------------------------------*/
        public AzLogEntry LogEntry(int i)
        {
            return m_azles.Item(i);
        }

        #region Construct/Deconstruct
        /* A Z  L O G  M O D E L */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogModel
        	%%Qualified: AzLog.AzLogModel.AzLogModel
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogModel()
        {
            m_azles = new AzLogEntries();
            m_plazlvListeners = new List<AzLogView>();
            m_pliazldsSources = new List<IAzLogDatasource>();
        }
        #endregion

        #region Views
        /* A D D  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AddView
        	%%Qualified: AzLog.AzLogModel.AddView
        	%%Contact: rlittle
        	
            add this view to our list of listeners
        ----------------------------------------------------------------------------*/
        public void AddView(AzLogView azlv)
        {
            m_plazlvListeners.Add(azlv);
        }

        /* R E M O V E  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: RemoveView
        	%%Qualified: AzLog.AzLogModel.RemoveView
        	%%Contact: rlittle
        	
            remove this view from our listeners
        ----------------------------------------------------------------------------*/
        public void RemoveView(AzLogView azlv)
        {
            for (int i = 0; i < m_plazlvListeners.Count; i++)
                if (m_plazlvListeners[i] == azlv)
                    {
                    m_plazlvListeners.RemoveAt(i);;
                    return;
                    }
        }
        #endregion

 
        #region Data Retrieval

        private List<IAzLogDatasource> m_pliazldsSources;
         
        /* A T T A C H  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: AttachDatasource
        	%%Qualified: AzLog.AzLogModel.AttachDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void AttachDatasource(IAzLogDatasource iazlds)
        {
            string sName = iazlds.GetName();

            // make sure its not already there
            foreach (IAzLogDatasource iazldsT in m_pliazldsSources)
                if (String.Compare(iazldsT.GetName(), sName, true) == 0)
                    throw new Exception("attaching the same datasource more than once");

            m_pliazldsSources.Add(iazlds);
            iazlds.SetDatasourceIndex(m_pliazldsSources.Count - 1);
        }

        /* U P D A T E  P A R T */
        /*----------------------------------------------------------------------------
        	%%Function: UpdatePart
        	%%Qualified: AzLog.AzLogModel.UpdatePart
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void UpdatePart(DateTime dttmMin, DateTime dttmMac, Int32 grfDatasource, AzLogPartState azlps)
        {
            m_azles.UpdatePart(dttmMin, dttmMac, grfDatasource, azlps);
        }

        /* A D D  S E G M E N T */
        /*----------------------------------------------------------------------------
        	%%Function: AddSegment
        	%%Qualified: AzLog.AzLogModel.AddSegment
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void AddSegment(TableQuerySegment<AzLogEntryEntity> azleSegment)
        {
            m_azles.AddSegment(azleSegment);
        }

        /* F E T C H  P A R T I T I O N S  F O R  D A T E  R A N G E */
        /*----------------------------------------------------------------------------
        	%%Function: FFetchPartitionsForDateRange
        	%%Qualified: AzLog.AzLogModel.FFetchPartitionsForDateRange
        	%%Contact: rlittle
        	
            Fetch all the partitions in the given range.
        ----------------------------------------------------------------------------*/
        public bool FFetchPartitionsForDateRange(DateTime dttmMin, DateTime dttmMac)
        {
            for (int i = 0; i < m_pliazldsSources.Count; i++)
                {
                IAzLogDatasource iazlds = m_pliazldsSources[i];

                while (dttmMin < dttmMac)
                    {
                    if (m_azles.GetPartState(dttmMin) != AzLogPartState.Complete)
                        {
#if NOMORE // the marking of Pending happens in the FetchPartitionForDateAsync
    // if this ever comes back, then we will have to get the correct IDataSource.
                    m_azles.UpdatePart(dttmMin, dttmMin.AddHours(1), AzLogParts.GrfDatasourceForIDatasource(1), AzLogPartState.Pending);
#endif
                        iazlds.FetchPartitionForDateAsync(this, dttmMin);
                        }
                    dttmMin = dttmMin.AddHours(1);
                    }
                }
            return true;
        }

        public void FetchPartitionForDateAsync(DateTime dttmMin)
        {
            FFetchPartitionsForDateRange(dttmMin, dttmMin.AddHours(1));
        }

        /* S  P A R T I T I O N  F R O M  D A T E */
        /*----------------------------------------------------------------------------
        	%%Function: SPartitionFromDate
        	%%Qualified: AzLog.AzLogModel.SPartitionFromDate
        	%%Contact: rlittle
        	
            Create a valid partition name for the given dttm and hour
        ----------------------------------------------------------------------------*/
        public static string SPartitionFromDate(DateTime dttm, int nHour)
        {
            return String.Format("{0:D4}{1:D2}{2:D2}{3:D2}", dttm.Year, dttm.Month, dttm.Day, nHour);
        }

        /* D T T M  F R O M  P A R T I T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: DttmFromPartition
        	%%Qualified: AzLog.AzLogModel.DttmFromPartition
        	%%Contact: rlittle
        	
            Convert a partition string into a datetime.
        ----------------------------------------------------------------------------*/
        public static DateTime DttmFromPartition(string sPartition)
        {
            return new DateTime(Int32.Parse(sPartition.Substring(0, 4)), Int32.Parse(sPartition.Substring(4, 2)), Int32.Parse(sPartition.Substring(6, 2)),
                                Int32.Parse(sPartition.Substring(8, 2)), 0, 0);
        }

        [TestCase("1995050501", "5/5/1995 1:00")]
        [TestCase("1995050601", "5/6/1995 1:00")]
        [TestCase("1995010523", "1/5/1995 23:00")]
        [TestCase("1995050500", "5/5/1995 0:00")]
        [Test]
        public static void TestDttmFromPartition(string sPartition, string sExpected)
        {
            DateTime dttmExpected = DateTime.Parse(sExpected);

            Assert.AreEqual(dttmExpected, DttmFromPartition(sPartition));
        }

        /* F I L L  M I N  M A C  F R O M  S T A R T  E N D */
        /*----------------------------------------------------------------------------
        	%%Function: FillMinMacFromStartEnd
        	%%Qualified: AzLog.AzLogModel.FillMinMacFromStartEnd
        	%%Contact: rlittle
        	
            Just a helper to parse a start and end date/time into real DateTimes

            (allows a shorthand on the end date to be used)
        ----------------------------------------------------------------------------*/
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
#endregion
        
#region TestSupport
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

            azles.UpdatePart(dttmPartition, dttmPartition.AddHours(1), AzLogParts.GrfDatasourceForIDatasource(1), AzLogPartState.Complete);
        }
#endregion

    }

    // this is the raw data coming back from azure. the type is kept in sync with the format of the azure log data
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
        
        public AzLogEntryEntity() { }

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

        public string Message
        {
            get { return m_sMessage; }
            set { m_sMessage = value; }
        }


    }
}
