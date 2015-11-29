using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzLog
{
    // we're going to store a lot of these, so let's make them smaller, more convenient that AzLogEntryEntitys
    // (also we don't want to be bound strcitly to the azure log format)
    public class AzLogEntry : AzLogFilter.ILogFilterItem
    {
        private string m_sPartition;
        private Guid m_guidRowKey;
        private Int64 m_nEventTickCount;
        private string m_sAppName;
        private string m_sLevel;
        private int m_nEventID;
        private int m_nInstanceID;
        private int m_nPid;
        private int m_nTid;
        private string m_sMessage;
        private string[] m_rgsMessageParts;

        public enum LogColumn : int
        {
            Nil = -1,
            Partition = 0,
            RowKey = 1,
            EventTickCount = 2,
            AppName = 3,
            Level = 4,
            EventID = 5,
            InstanceID = 6,
            Pid = 7,
            Tid = 8,
            Message = 9,
            Message0 = 10,
            Message1 = 11,
            Message2 = 12,
            Message3 = 13,
            Message4 = 14,
            Message5 = 15,
            Message6 = 16,
            Message7 = 17,
            Message8 = 18,
            Message9 = 19
        };

        private ListViewItem m_lvi; // this is valid if the requested generation matches the generation we have cached...

        private int m_nGeneration;
        // this is going to pose a problem when multiple windows start at the same nGeneration -- we are likely to collide with multiple windows open, which means that they can
        // have separate views but they might have the same nGeneration (so we will be caching lviItem's from one view and thinking they are valid for other views)
        // the way to solve this *might* be to start each nGeneration with a random number, increasing the chances that we will not collide.
        // REVIEW: this might already be solved...

        #region Accessors

        public string Partition
        {
            get { return m_sPartition; }
            set { m_sPartition = value; }
        }

        public Guid RowKey
        {
            get { return m_guidRowKey; }
            set { m_guidRowKey = value; }
        }

        public Int64 EventTickCount
        {
            get { return m_nEventTickCount; }
            set { m_nEventTickCount = value; }
        }

        public string ApplicationName
        {
            get { return m_sAppName; }
            set { m_sAppName = value; }
        }

        public string Level
        {
            get { return m_sLevel; }
            set { m_sLevel = value; }
        }

        public int EventId
        {
            get { return m_nEventID; }
            set { m_nEventID = value; }
        }

        public string InstanceId
        {
            get { return m_nInstanceID.ToString("X8"); }
            set { m_nInstanceID = int.Parse(value, NumberStyles.AllowHexSpecifier); }
        }

        public int Pid
        {
            get { return m_nPid; }
            set { m_nPid = value; }
        }

        public int Tid
        {
            get { return m_nTid; }
            set { m_nTid = value; }
        }

        public string Message
        {
            get { return m_sMessage; }
            set { m_sMessage = value; }
        }

        public string Message0 => m_rgsMessageParts.Length <= 0 ? null : m_rgsMessageParts[0];
        public string Message1 => m_rgsMessageParts.Length <= 1 ? null : m_rgsMessageParts[1];
        public string Message2 => m_rgsMessageParts.Length <= 2 ? null : m_rgsMessageParts[2];
        public string Message3 => m_rgsMessageParts.Length <= 3 ? null : m_rgsMessageParts[3];
        public string Message4 => m_rgsMessageParts.Length <= 4 ? null : m_rgsMessageParts[4];
        public string Message5 => m_rgsMessageParts.Length <= 5 ? null : m_rgsMessageParts[5];
        public string Message6 => m_rgsMessageParts.Length <= 6 ? null : m_rgsMessageParts[6];
        public string Message7 => m_rgsMessageParts.Length <= 7 ? null : m_rgsMessageParts[7];
        public string Message8 => m_rgsMessageParts.Length <= 8 ? null : m_rgsMessageParts[8];
        public string Message9 => m_rgsMessageParts.Length <= 9 ? null : m_rgsMessageParts[9];

        public object OGetValue(AzLogFilter.AzLogFilterValue.ValueType vt, AzLogFilter.AzLogFilterValue.DataSource ds, LogColumn lc)
        {
            if (ds == AzLogFilter.AzLogFilterValue.DataSource.DttmRow)
                {
                // get the datetime from the partition
                return AzLogModel.DttmFromPartition(Partition);
                }
            if (ds == AzLogFilter.AzLogFilterValue.DataSource.Column)
                return GetColumn(lc);

            throw new Exception("bad datasource in OGetValue under AzLogEntry");
        }

        public string GetColumn(LogColumn lc)
        {
            switch (lc)
                {
                case LogColumn.Partition:
                    return Partition;
                case LogColumn.RowKey:
                    return RowKey.ToString();
                case LogColumn.EventTickCount:
                    return m_nEventTickCount.ToString();
                case LogColumn.AppName:
                    return m_sAppName;
                case LogColumn.Level:
                    return m_sLevel;
                case LogColumn.EventID:
                    return m_nEventID.ToString();
                case LogColumn.InstanceID:
                    return m_nInstanceID.ToString();
                case LogColumn.Pid:
                    return m_nPid.ToString();
                case LogColumn.Tid:
                    return m_nTid.ToString();
                case LogColumn.Message:
                    return m_sMessage;
                case LogColumn.Message0:
                    return Message0;
                case LogColumn.Message1:
                    return Message1;
                case LogColumn.Message2:
                    return Message2;
                case LogColumn.Message3:
                    return Message3;
                case LogColumn.Message4:
                    return Message4;
                case LogColumn.Message5:
                    return Message5;
                case LogColumn.Message6:
                    return Message6;
                case LogColumn.Message7:
                    return Message7;
                case LogColumn.Message8:
                    return Message8;
                case LogColumn.Message9:
                    return Message9;
                default:
                    return "";
                }
        }

        /* L V I  F E T C H */
        /*----------------------------------------------------------------------------
        	%%Function: LviFetch
        	%%Qualified: AzLog.AzLogEntry.LviFetch
        	%%Contact: rlittle
        	
            Fetch a list view item appropriate for the given LogView.

            We only cache one lvi at a time right now, which means that if two
            windows decide to fetch the same lvi, we are going to be missing this
            cache every single time because we will be flip-flopping between two
            generations

            TODO: Fix this by having more than one cache LVI (to support multiple
            windows)
        ----------------------------------------------------------------------------*/
        public ListViewItem LviFetch(int nGeneration, AzLogViewSettings azlvs)
        {
            if (m_lvi == null || m_nGeneration != nGeneration)
                {
                m_lvi = new ListViewItem();

                for (int i = 0; i < azlvs.ColumnCount(); i++)
                    {
                    m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    m_lvi.SubItems[i].Text = GetColumn(azlvs.Column(i).DataColumn);
                    m_lvi.Tag = this;
                    }
                m_nGeneration = nGeneration;
                }
            return m_lvi;
        }
        #endregion

        #region Construct/Destruct

        public AzLogEntry() {}

        /* C R E A T E */
        /*----------------------------------------------------------------------------
        	%%Function: Create
        	%%Qualified: AzLog.AzLogEntry.Create
        	%%Contact: rlittle
        	
            Create a log entry from its constituent parts. This will parse
            sMessage into all its subparts too (split by \t).

            FUTURE: If this gets too slow, we can make this "opt in" to parse the
            message
        ----------------------------------------------------------------------------*/
        public static AzLogEntry Create(string sPartition, Guid guidRowKey, Int64 nEventTickCount, string sAppName,
            string sLevel, int nEventId, string sInstanceId, int nPid, int nTid, string sMessage)
        {
            AzLogEntry azle = new AzLogEntry();
            azle.Partition = sPartition;
            azle.RowKey = guidRowKey;
            azle.EventTickCount = nEventTickCount;
            azle.ApplicationName = sAppName;
            azle.Level = sLevel;
            azle.EventId = nEventId;
            azle.InstanceId = sInstanceId;
            azle.Pid = nPid;
            azle.Tid = nTid;
            azle.Message = sMessage;
            azle.m_rgsMessageParts = sMessage.Split('\t');

            return azle;
        }

        /* C R E A T E */
        /*----------------------------------------------------------------------------
        	%%Function: Create
        	%%Qualified: AzLog.AzLogEntry.Create
        	%%Contact: rlittle
        	
            Create the log entry from the Azure entity.
        ----------------------------------------------------------------------------*/
        public static AzLogEntry Create(AzLogEntryEntity azlee)
        {
            return AzLogEntry.Create(azlee.PartitionKey, Guid.Parse(azlee.RowKey), azlee.EventTickCount,
                                     azlee.ApplicationName, azlee.Level, azlee.EventId, azlee.InstanceId, azlee.Pid,
                                     azlee.Tid, azlee.Message);
        }
        #endregion
    }

    // We need an uber collection that supports virtualized set of AzLogEntries. Preferably partitioned. Supporting filtered and sorted views that doesn't change the underlying collection
    // we also need to know the ranges of data that are "complete", "pending", or "partial"
    // The we will use PartitionKey + RowKey as the master key for uniqueness (to allow filling of partial data)
    //
    // for now we will partition the data by day + hour (using the partitions)
    public class AzLogEntries
    {
        // this collection must only grow and only append. all views depend on that.
        private List<AzLogEntry> m_plale;
        private AzLogParts m_azlps;

        // the underlying length is the same regardless of the number of parts -- its just the number
        // of log entries
        public int Length => m_plale.Count;

        public AzLogEntries()
        {
            m_plale = new List<AzLogEntry>();
            m_azlps = new AzLogParts();

        }
        public AzLogEntry Item(int i)
        {
            return m_plale[i];
        }

        public AzLogPartState GetPartState(DateTime dttm)
        {
            AzLogPartState azlps;

            lock (this)
                {
                azlps = m_azlps.GetPartState(dttm);
                }

            return azlps;
        }

        public void UpdatePart(DateTime dttmMin, DateTime dttmMac, Int32 grfDatasource, AzLogPartState azpls)
        {
            lock (this)
                {
                m_azlps.SetPartState(dttmMin, dttmMac, grfDatasource, azpls);
                }
        }

        public void AddLogEntry(AzLogEntry azle)
        {
            lock (this)
                {
                m_plale.Add(azle);
                }
        }

        public void AddSegment(TableQuerySegment<AzLogEntryEntity> qsazle)
        {
            lock (this)
                {
                foreach (AzLogEntryEntity azlee in qsazle.Results)
                    {
                    m_plale.Add(AzLogEntry.Create(azlee));
                    }
                }
        }

    }

    // ============================================================================
    // C O M P A R E  L O G  E N T R Y  T I C K  C O U N T
    // ============================================================================
    internal class CompareLogEntryTickCount : IComparer<int>
    {
        private AzLogModel m_azlm;

        public CompareLogEntryTickCount(AzLogModel azlm)
        {
            m_azlm = azlm;
        }

        public int Compare(int iLeft, int iRight)
        {
            long n = (m_azlm.LogEntry(iLeft).EventTickCount - m_azlm.LogEntry(iRight).EventTickCount);

            if (n > 0)
                return 1;
            if (n == 0)
                return 0;

            return -1;
        }
    }
}