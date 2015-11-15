using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using TCore.ListViewSupp;

namespace AzLog
{
    public partial class AzLogWindow : Form
    {
        private AzLogView m_azlv = null;
        private AzLogModel m_azlm;

        public AzLogView View => m_azlv;
    
        /* A Z  L O G  W I N D O W */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogWindow
        	%%Qualified: AzLog.AzLogWindow.AzLogWindow
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogWindow()
        {
            InitializeComponent();
            // SetupListViewForLog(m_lvLog);
            PopulateViewList();
            // SetView("");
        }

        public AzLogViewSettings ViewSettings => m_azlvs;
        private ILogClient m_ilc;

        /* C R E A T E  N E W  W I N D O W */
        /*----------------------------------------------------------------------------
        	%%Function: CreateNewWindow
        	%%Qualified: AzLog.AzLogWindow.CreateNewWindow
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogWindow CreateNewWindow(AzLogModel azlm, string sViewName, ILogClient ilc)
        {
            AzLogWindow azlw = new AzLogWindow();

            azlw.SetView(sViewName);
            azlw.m_azlv = new AzLogView(azlw);
            azlw.m_ilc = ilc;
            azlw.m_azlm = azlm;
            azlw.m_azlv.BuildView(azlw.m_azlm, new CompareLogEntryTickCount(azlw.m_azlm));

            return azlw;
        }

        private int IViewFromName(string sName)
        {
            int i;

            for (i = 0; i < m_cbView.Items.Count; i++)
                {
                AzLogViewSettings azlvs = (AzLogViewSettings) m_cbView.Items[i];
                if (String.Compare(azlvs.Name, sName, true) == 0)
                    break;
                }
            return (i >= m_cbView.Items.Count) ? -1 : i;
        }

        private AzLogViewSettings m_azlvs;

        public void SetView(string sViewName)
        {
            int iView = IViewFromName(sViewName);

            if (iView != -1)
                {
                m_cbView.SelectedIndex = iView;
                m_azlvs = (AzLogViewSettings) m_cbView.Items[iView];
                }
            else
                {
                AzLogViewSettings azlvs = new AzLogViewSettings(sViewName);
                m_azlvs = azlvs;
                m_cbView.SelectedIndex = -1;
                }

            SetupListViewForLog(m_azlvs);
        }

        /* G E T  L I S T  V I E W  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: GetListViewItem
        	%%Qualified: AzLog.AzLogWindow.GetListViewItem
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void GetListViewItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = m_azlv.LviItem(e.ItemIndex);
        }

        /* P O P U L A T E  V I E W  L I S T */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateViewList
        	%%Qualified: AzLog.AzLogWindow.PopulateViewList
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void PopulateViewList()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Thetasoft\\AzLog\\Views");
            if (rk != null)
                {
                string[] rgs = rk.GetSubKeyNames();

                foreach (string s in rgs)
                    {
                    AzLogViewSettings azlvs = new AzLogViewSettings(s);
                    m_cbView.Items.Add(azlvs);
                    }
                rk.Close();
                }
            m_cbView.Items.Add(new AzLogViewSettings("<New...>"));
        }

        /* S E T U P  L I S T  V I E W  F O R  L O G */
        /*----------------------------------------------------------------------------
        	%%Function: SetupListViewForLog
        	%%Qualified: AzLog.AzLogWindow.SetupListViewForLog
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void SetupListViewForLog(AzLogViewSettings azlvs)
        {
            int i;
            for (i = m_lvLog.Columns.Count - 1; i >= 0; --i)
                m_lvLog.Columns.RemoveAt(i);

            for (i = 0; i < azlvs.Columns.Count; i++)
                {
                AzLogViewSettings.AzLogViewColumn azlvc = azlvs.Columns[i];

                m_lvLog.Columns.Add(new ColumnHeader());
                m_lvLog.Columns[i].Text = azlvc.Name;
                m_lvLog.Columns[i].Width = azlvc.Width;
                }

            m_lvLog.VirtualListSize = 0;
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

        private AzLogViewSettings m_azlvsCurrent;

        private void CreateNewView()
        {
            string sName;
            if (TCore.UI.InputBox.ShowInputBox("New view name", "View name", "", out sName))
                {
                // create a new view based on the current view

                AzLogViewSettings azlvs = m_azlvs.Clone();
                azlvs.SetName(sName);
                m_cbView.Items.Add(azlvs);
                m_cbView.SelectedIndex = m_cbView.Items.Count - 1;

                m_azlvs = azlvs;

                }
        }

        private void ChangeViewSelected(object sender, EventArgs e)
        {
            AzLogViewSettings azlvs = (AzLogViewSettings)m_cbView.SelectedItem;

            if (azlvs.Name == "<New...>")
                {
                CreateNewView();
                }
        }

        private void DoViewSave(object sender, EventArgs e)
        {
            AzLogViewSettings azlvs = (AzLogViewSettings)m_cbView.SelectedItem;

            if (azlvs == null || azlvs.Name == "<New...>")
                {
                CreateNewView();
                }

            Dictionary<string, int> rgColumns = new Dictionary<string, int>();

            for (int i = 0; i < m_lvLog.Columns.Count; i++)
                {
                rgColumns.Add(m_lvLog.Columns[i].Text, i);
                }

            for (int i = 0; i < m_azlvs.Columns.Count; i++)
                {
                string sName = m_azlvs.Columns[i].Name;

                // only sync the width here. all else sync during manipulation
                if (rgColumns.ContainsKey(sName))
                    m_azlvs.Columns[i].Width = m_lvLog.Columns[rgColumns[sName]].Width;
                }

            m_azlvs.Save();
            m_ilc.SetDefaultView(m_azlvs.Name);
        }

        private void DoColumnReorder(object sender, ColumnReorderedEventArgs e)
        {
            m_azlvs.MoveColumn(e.OldDisplayIndex, e.NewDisplayIndex);
        }

        private void DoHideColumnClick(object sender, EventArgs e)
        {
            m_azlvs.MoveColumn(1, 1);
        }

        private HeaderSupp m_hs;

        private void HandleContextOpening(object sender, CancelEventArgs e)
        {
            if (m_hs == null)
                m_hs = new HeaderSupp();

            ColumnHeader ch = m_hs.ColumnHeaderFromContextOpening(m_lvLog, sender, e);

            if (ch != null)
                {
                ctxMenuHeader.Tag = ch;
                ctxMenuHeader.Items[0].Text = "Command for Header " + ch.Text;
                ctxMenuHeader.Show(Control.MousePosition);
                }
        }

        private void blahToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_azlvs.ShowHideColumn(((ColumnHeader)((((ToolStripMenuItem) sender).GetCurrentParent()).Tag)).Text, false);
            int c = m_lvLog.VirtualListSize;
            
            SetupListViewForLog(m_azlvs);
            m_azlv.BumpGeneration();
            m_lvLog.VirtualListSize = c;
        }

        //        public bool InvokeRequired { get { return m_lvLog.InvokeRequired; } }
    }

    public class AzLogView
    {
        private List<int> m_pliale;
        private IComparer<int> m_icle;
        private AzLogModel m_azlm;
        private int m_nGeneration;

        public int Length => m_pliale.Count;

        private object m_oSyncView;
        private AzLogWindow m_azlw;
        private int m_nLogViewGeneration; // when we change views on the azlw, we have to bump this number so that we know that any cached ListViewItems are now invalid 
        // (this is lazy invalidation).  for now, since we don't actually cache ListViewItems, this is a noop
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
            m_nLogViewGeneration = (new Random(System.Environment.TickCount)).Next(10000);
        }

        public void BumpGeneration()
        {
            m_nLogViewGeneration++;
        }

        public ListViewItem LviItem(int i)
        {
            return m_azlm.LogEntry(m_pliale[i]).LviFetch(m_nLogViewGeneration, m_azlw.ViewSettings);
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

