using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzLog
{
    public partial class AzLogWindow : Form
    {
        private AzLogView m_azlv = null;
        private AzLogModel m_azlm;

        public AzLogWindow()
        {
            InitializeComponent();
            SetupListViewForLog(m_lvLog);
        }

        public AzLogView View => m_azlv;

        private void GetListViewItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = m_azlv.LviItem(e.ItemIndex);
        }

        public static AzLogWindow CreateNewWindow(AzLogModel azlm)
        {
            AzLogWindow azlw = new AzLogWindow();
            azlw.m_azlv = new AzLogView(azlw);
            azlw.m_azlm = azlm;
            azlw.m_azlv.BuildView(azlw.m_azlm, new CompareLogEntryTickCount(azlw.m_azlm));

            return azlw;
        }

        private void SetupListViewForLog(ListView lv)
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

        public void ClearLog()
        {
            m_lvLog.Items.Clear();
        }

        
        private void DoFetchLogEntries(object sender, EventArgs e)
        {
            // figure out the timespan being requested
            DateTime dttmMin, dttmMac;

            AzLogModel.FillMinMacFromStartEnd(m_ebStart.Text, m_ebEnd.Text, out dttmMin, out dttmMac);

            // we don't know what partition we're going to find this data in, so launch a query 
            // from the first partition for this date range
            // m_azlm.FetchPartitionsForDateRange(dttmMin, nHourMin, dttmMac, nHourMac);
            m_azlm.FetchPartitionForDate(dttmMin);
        }

        public delegate void SyncViewDel(int iFirstSync, int iMacSync);

        private object oSyncView = new Object();

        public object OSyncView
        {
            get { return oSyncView; }
            set { oSyncView = value; }
        }

        public void SyncViewCore(int iMin, int iMac)
        {
            // we have new items from iMin to iMac. Add them to the view
            m_azlv.AppendView(m_azlm, iMin, iMac);
            m_lvLog.VirtualListSize = m_azlv.Length;
        }

        public void SyncView(int iMin, int iMac)
        {
            if (m_lvLog.InvokeRequired)
                m_lvLog.BeginInvoke(new AzLogWindow.SyncViewDel(SyncViewCore), new object[] {iMin, iMac});
            else
                SyncViewCore(iMin, iMac);
        }

//        public bool InvokeRequired { get { return m_lvLog.InvokeRequired; } }
    }

    public class AzLogView
    {
        private List<int> m_pliale;
        private IComparer<int> m_icle;
        private AzLogModel m_azlm;

        public int Length => m_pliale.Count;

        private object m_oSyncView;
        private AzLogWindow m_azlw;

        public void ClearLog()
        {
            m_azlw.ClearLog();
        }
        public object OSyncView
        {
            get { return m_azlw.OSyncView; }
            set { m_azlw.OSyncView = value; }
        }

        public void SyncView(int iMin, int iMac)
        {
            m_azlw.SyncView(iMin, iMac);
        }
        // public bool InvokeRequired { get { return m_azlw.InvokeRequired; } }

        public AzLogView(AzLogWindow azlw)
        {
            m_azlw = azlw;
        }

        public ListViewItem LviItem(int i)
        {
            return m_azlm.LogEntry(m_pliale[i]).LviFetch();
        }

        public void BuildView(AzLogModel azlm, IComparer<int> icle)
        {
            m_icle = icle;
            m_azlm = azlm;
            m_pliale = new List<int>();

            for (int i = 0; i < m_azlm.LogCount; i++)
                {
                m_pliale.Add(i);
                }
            m_pliale.Sort(icle);
        }

        public void AppendView(AzLogModel azlm, int iMin, int iMac)
        {
            while (iMin < iMac)
                m_pliale.Add(iMin++);

            m_pliale.Sort(m_icle);
        }
    }
}

