using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using NUnit.Framework;
using TCore.Settings;

namespace AzLog
{
    public partial class AzLog : Form
    {
        private Settings.SettingsElt[] _rgsteeAccount = new[]
                                                            {
                                                            new Settings.SettingsElt("AccountKey", Settings.Type.Str, "", ""),
                                                            new Settings.SettingsElt("StorageType", Settings.Type.Str, "", ""),
                                                            new Settings.SettingsElt("StorageDomain", Settings.Type.Str, "", ""),
                                                            };
        public AzLog()
        {
            m_azles = new AzLogEntries();
            m_azlv = new AzLogView();
            
            m_azlv.BuildView(m_azles, new CompareLogEntryTickCount(m_azles));
            InitializeComponent();
            SetupListViewForLog(m_lvLog);
            PopulateAccounts();
        }

        public void PopulateAccounts()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Thetasoft\\AzLog");
            string[] rgs = rk.GetSubKeyNames();

            foreach (string s in rgs)
                {
                m_cbAccounts.Items.Add(s);
                }
            rk.Close();
        }

        private void DoAddEditAccount(object sender, EventArgs e)
        {
            if (m_cbAccounts.SelectedIndex == -1)
                AzAddAccount.AddStorageAccount(_rgsteeAccount);
            else
                AzAddAccount.EditStorageAccount((string)m_cbAccounts.SelectedItem, _rgsteeAccount);
        }

        private string KeyName => (string) String.Format("Software\\Thetasoft\\AzLog\\{0}", m_cbAccounts.SelectedItem);

        private AzTableCollection m_aztc;

        private void m_pbOpen_Click(object sender, EventArgs e)
        {
            Settings ste = new Settings(_rgsteeAccount, KeyName, "main");
            ste.Load();

            m_aztc = new AzTableCollection((string)m_cbAccounts.SelectedItem, ste.SValue("AccountKey"));
            List<string> pls = m_aztc.PlsTableNames();

            foreach (string s in pls)
                m_lbTables.Items.Add(s);
        }

        private AzTable m_azt = null;
        private AzLogEntries m_azles;
        private AzLogView m_azlv = null;

        private void DoSelectTable(object sender, EventArgs e)
        {
            ClearLog();
            m_azt = m_aztc.GetTable((string) m_lbTables.SelectedItem);
            // PopulateLog();
        }

        void PopulateLog()
        {
            TableQuery<AzLogEntryEntity> tq =
                new TableQuery<AzLogEntryEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "2015102607"),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                                           "11e57bb3-4322-2ebf-93ff-00155d4417a2")));

            foreach (AzLogEntryEntity azle in m_azt.Table.ExecuteQuery(tq))
                {
                // m_lvLog.Items.Add(azle.LviFetch());
                }
        }

        void SetupListViewForLog(ListView lv)
        {
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[0].Text = "PartitionKey";
            lv.Columns[0].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[1].Text = "RowKey";
            lv.Columns[1].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[2].Text = "EventTickCount";
            lv.Columns[2].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[3].Text = "AppName";
            lv.Columns[3].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[4].Text = "Level";
            lv.Columns[4].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[5].Text = "EventID";
            lv.Columns[5].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[6].Text = "InstanceID";
            lv.Columns[6].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[7].Text = "Pid";
            lv.Columns[7].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[8].Text = "nTid";
            lv.Columns[8].Width = 64;
            lv.Columns.Add(new ColumnHeader());
            lv.Columns[9].Text = "sMessage";
            lv.Columns[9].Width = 256;

            lv.VirtualListSize = 0;
        }
        void ClearLog()
        {
            m_lvLog.Items.Clear();
        }

        static void FillMinMacFromStartEnd(string sStart, string sEnd, out DateTime dttmMin, out int nHourMin,
            out DateTime dttmMac, out int nHourMac)
        {
            DateTime dttmStart = DateTime.Parse(sStart);
            DateTime dttmEnd = DateTime.Parse(sEnd);

            dttmMin = new DateTime(dttmStart.Year, dttmStart.Month, dttmStart.Day);

            if (dttmEnd.Year == 1900 || (sEnd.IndexOf("/") == -1 && sEnd.IndexOf("-") == -1))
                {
                dttmMac = dttmMin;
                }
            else
                {
                dttmMac = new DateTime(dttmEnd.Year, dttmEnd.Month, dttmEnd.Day);
                }
            nHourMin = dttmStart.Hour;
            nHourMac = dttmEnd.Hour;
            }

        string SPartitionFromDate(DateTime dttm, int nHour)
        {
            return String.Format("{0:D4}{1:D2}{2:D2}{3:D2}", dttm.Year, dttm.Month, dttm.Day, nHour);
        }

        private delegate void SyncViewDel(int iFirstSync, int iMacSync);

        private object oSyncView = new Object();

        void SyncView(int iMin, int iMac)
        {
            // we have new items from iMin to iMac. Add them to the view
            m_azlv.AppendView(m_azles, iMin, iMac);
            m_lvLog.VirtualListSize = m_azlv.Length;
        }

        async Task<bool> FetchPartitionForDate(DateTime dttmMin, int nHourMin, DateTime dttmMac, int nHourMac)
        {
            m_azles.UpdatePart(dttmMin, nHourMin, dttmMac, nHourMac, AzLogPartState.Pending);

            TableQuery<AzLogEntryEntity> tq =
                new TableQuery<AzLogEntryEntity>().Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                                                           SPartitionFromDate(dttmMin, nHourMin)));

            TableQuerySegment<AzLogEntryEntity> azleSegment = null;

            while (azleSegment == null || azleSegment.ContinuationToken != null)
                {
                    azleSegment = await m_azt.Table.ExecuteQuerySegmentedAsync(tq, azleSegment?.ContinuationToken);
                    lock (oSyncView)
                        {
                        int iFirst = m_azles.Length;

                        m_azles.AddSegment(azleSegment);

                        int iLast = m_azles.Length;

                        if (m_lvLog.InvokeRequired)
                            m_lvLog.BeginInvoke(new SyncViewDel(SyncView), new object[] {iFirst, iLast});
                        else
                            SyncView(iFirst, iLast);
                        }
                }

            m_azles.UpdatePart(dttmMin, nHourMin, dttmMac, nHourMac, AzLogPartState.Complete);

            return true;
        }

        private void DoFetchLogEntries(object sender, EventArgs e)
        {
            // figure out the timespan being requested
            DateTime dttmMin, dttmMac;
            int nHourMin, nHourMac;

            FillMinMacFromStartEnd(m_ebStart.Text, m_ebEnd.Text, out dttmMin, out nHourMin, out dttmMac, out nHourMac);

            // we don't know what partition we're going to find this data in, so launch a query 
            // from the first partition for this date range
            FetchPartitionForDate(dttmMin, nHourMin, dttmMac, nHourMac);
        }

        private void GetListViewItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = m_azlv.LviItem(e.ItemIndex);
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
        private AzLogEntries m_azles;

        public CompareLogEntryTickCount(AzLogEntries azles)
        {
            m_azles = azles;
        }

        public int Compare(int iLeft, int iRight)
        {
            return (int) (m_azles.Item(iLeft).EventTickCount - m_azles.Item(iRight).EventTickCount);
        }
    }

    class AzLogView
    {
        private List<int> m_pliale;
        private IComparer<int> m_icle;
        private AzLogEntries m_azles;

        public int Length => m_pliale.Count;

        public ListViewItem LviItem(int i)
        {
            return m_azles.Item(m_pliale[i]).LviFetch();
        }

        public void BuildView(AzLogEntries azles, IComparer<int> icle)
        {
            m_icle = icle;
            m_azles = azles;
            m_pliale = new List<int>();

            for (int i = 0; i < m_azles.Length; i++)
                {
                m_pliale.Add(i);
                }
            m_pliale.Sort(icle);
        }

        public void AppendView(AzLogEntries azles, int iMin, int iMac)
        {
            while (iMin < iMac)
                m_pliale.Add(iMin++);

            m_pliale.Sort(m_icle);
        }
    }

    // we're going to store a lot of these, so let's make them smaller
    class AzLogEntry
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

    class AzLogEntryEntity : TableEntity
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
