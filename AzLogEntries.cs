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
    // we're going to store a lot of these, so let's make them smaller
    public class AzLogEntry
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

        
        private ListViewItem m_lvi;

        public ListViewItem LviFetch()
        {
            if (m_lvi == null)
                {
                m_lvi = new ListViewItem();

                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());                      

                m_lvi.SubItems[0].Text = Partition;
                m_lvi.SubItems[1].Text = RowKey.ToString();
                m_lvi.SubItems[2].Text = m_nEventTickCount.ToString();
                m_lvi.SubItems[3].Text = m_sAppName;
                m_lvi.SubItems[4].Text = m_sLevel;
                m_lvi.SubItems[5].Text = m_nEventID.ToString();
                m_lvi.SubItems[6].Text = m_nInstanceID.ToString();
                m_lvi.SubItems[7].Text = m_nPid.ToString();
                m_lvi.SubItems[8].Text = m_nTid.ToString();
                m_lvi.SubItems[9].Text = m_sMessage;

                m_lvi.Tag = this;
                }
            return m_lvi;
        }
        public AzLogEntry()
        { }

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

            return azle;
        }

        public static AzLogEntry Create(AzLogEntryEntity azlee)
        {
            return AzLogEntry.Create(azlee.PartitionKey, Guid.Parse(azlee.RowKey), azlee.EventTickCount,
                                     azlee.ApplicationName, azlee.Level, azlee.EventId, azlee.InstanceId, azlee.Pid,
                                     azlee.Tid, azlee.Message);
        }
    }
    
    // We need an uber collection that supports virtualized set of AzLogEntries. Preferably partitioned. Supporting filtered and sorted views that doesn't change the underlying collection
    // we also need to know the ranges of data that are "complete", "pending", or "partial"
    // The we will use PartitionKey + RowKey as the master key for uniqueness (to allow filling of partial data)
    //
    // for now we will partition the data by day + hour (using the partitions)
    class AzLogEntries
    {
        // this collection must only grow and only append. all views depend on that.
        private List<AzLogEntry> m_plale;
        private AzLogParts m_azlps;

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

        public void UpdatePart(DateTime dttmMin, int nHourMin, DateTime dttmMac, int nHourMac, AzLogPartState azpls)
        {
            lock (this)
                {
                m_azlps.SetPartState(dttmMin, dttmMac, nHourMin, nHourMac, azpls);
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
    class CompareLogEntryTickCount : IComparer<int>
    {
        private AzLogModel m_azlm;

        public CompareLogEntryTickCount(AzLogModel azlm)
        {
            m_azlm = azlm;
        }

        public int Compare(int iLeft, int iRight)
        {
            return (int) (m_azlm.LogEntry(iLeft).EventTickCount - m_azlm.LogEntry(iRight).EventTickCount);
        }
    }


}
