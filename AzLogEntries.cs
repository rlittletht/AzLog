﻿using System;
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
        private int m_nGeneration; // this is going to pose a problem when multiple windows start at the same nGeneration -- we are likely to collide with multiple windows open, which means that they can
        // have separate views but they might have the same nGeneration (so we will be caching lviItem's from one view and thinking they are valid for other views)
        // the way to solve this *might* be to start each nGeneration with a random number, increasing the chances that we will not collide.

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


        public enum LogColumn : int
        {
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
            Message0 = 9,
            Message1 = 10,
            Message2 = 11,
            Message3 = 12,
            Message4 = 13,
            Message5 = 14,
            Message6 = 15,
            Message7 = 16,
            Message8 = 17
        };

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
                default:
                    return "";
                }
        }

        private ListViewItem m_lvi;

        public ListViewItem LviFetch(int nGeneration, AzLogViewSettings azlvs)
        {
            if (m_lvi == null || m_nGeneration != nGeneration)
                {
                m_lvi = new ListViewItem();

                for (int i = 0; i < azlvs.Columns.Count; i++)
                    {
                    m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());

                    m_lvi.SubItems[i].Text = GetColumn(azlvs.Columns[i].DataColumn);
                    m_lvi.Tag = this;
                    }
                m_nGeneration = nGeneration;
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

        public AzLogPartState GetPartState(DateTime dttm)
        {
            AzLogPartState azlps;

            lock (this)
                {
                azlps = m_azlps.GetPartState(dttm);
                }

            return azlps;
        }

        public void UpdatePart(DateTime dttmMin, DateTime dttmMac, AzLogPartState azpls)
        {
            lock (this)
                {
                m_azlps.SetPartState(dttmMin, dttmMac, azpls);
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
            return (int) (m_azlm.LogEntry(iRight).EventTickCount - m_azlm.LogEntry(iLeft).EventTickCount);
        }
    }


}