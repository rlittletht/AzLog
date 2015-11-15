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
using NUnit.Framework;
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

            azlw.m_lvLog.VirtualListSize = azlw.m_azlv.Length;

            return azlw;
        }

        public void CloseWindow()
        {
            m_azlm.RemoveView(m_azlv);
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
            SetupContextMenu();
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

        public void AppendUpdateViewCore(int iMin, int iMac)
        {
            // we have new items from iMin to iMac. Add them to the view
            m_azlv.AppendView(m_azlm, iMin, iMac);
            m_lvLog.VirtualListSize = m_azlv.Length;
        }

        public void AppendUpdateView(int iMin, int iMac)
        {
            if (m_lvLog.InvokeRequired)
                m_lvLog.BeginInvoke(new AzLogWindow.SyncViewDel(AppendUpdateViewCore), new object[] {iMin, iMac});
            else
                AppendUpdateViewCore(iMin, iMac);
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
            AzLogViewSettings azlvs = (AzLogViewSettings) m_cbView.SelectedItem;

            if (azlvs.Name == "<New...>")
                {
                CreateNewView();
                }
        }

        private void DoViewSave(object sender, EventArgs e)
        {
            AzLogViewSettings azlvs = (AzLogViewSettings) m_cbView.SelectedItem;

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

        private void SetupContextMenu()
        {
            m_ctxmHeader.Items.Clear();

            ToolStripMenuItem tsmi = new ToolStripMenuItem {Text = "Remove Column...", Tag = null, Checked = false};

            tsmi.Click += HandleRemoveHeaderItem;

            m_ctxmHeader.Items.Add(tsmi);
            tsmi = new ToolStripMenuItem {Text = "-------------------", Enabled = false};

            m_ctxmHeader.Items.Add(tsmi);

            foreach (AzLogViewSettings.DefaultColumnDef dcd in m_azlvs.DefaultColumns)
                {
                AzLogViewSettings.AzLogViewColumn azlvc = m_azlvs.AzlvcFromName(dcd.sName);
                bool fVisible = true;

                if (azlvc == null || !azlvc.Visible)
                    fVisible = false;

                tsmi = new ToolStripMenuItem {Text = dcd.sName, Tag = dcd, Checked = fVisible};

                tsmi.Click += HandleSelectHeaderItem;

                m_ctxmHeader.Items.Add(tsmi);
                }
        }

        /* H A N D L E  C O N T E X T  O P E N I N G */
        /*----------------------------------------------------------------------------
        	%%Function: HandleContextOpening
        	%%Qualified: AzLog.AzLogWindow.HandleContextOpening
        	%%Contact: rlittle
        	
            This is going to get executed every time they go to open a context menu.
            We want to know *where* they are invoking from, so we do some clever
            hacker in HeaderSup.ColumnHeaderFromContextOpening(...) -- it figures out
            (based on the client rects of the column header items) where they are 
            clicking and which column header is appropriate.

            NOTE: IF it locates a ColumnHeader and returns it to you, then 
            ColumnHeaderFromContextOpeneing CANCELS the context menu operation
            from here from happening and assumes you are going to invoke the context
            menu yourself (which is why we conly call m_ctxmHeader.Show() when
            we get a columnheader back - this allows us to show our ContextMenu
            for our Header columns instead of the context menu for the rest of the 
            listview)

            ALSO NOTE that we squirrel away the matched columnheader into the first
            menu item in the context menu -- in our case, that's the
            "Remove this column" menu item, so it follows that it needs to know
            what "this" column is.  (Other routings may choose to look here for this
            information, so let's make sure to clear the Tag in times when we aren't
            matched...)
        ----------------------------------------------------------------------------*/
        private void HandleContextOpening(object sender, CancelEventArgs e)
        {
            if (m_hs == null)
                m_hs = new HeaderSupp();

            ColumnHeader ch = m_hs.ColumnHeaderFromContextOpening(m_lvLog, sender, e);

            if (ch != null)
                {
                m_ctxmHeader.Tag = ch;
                m_ctxmHeader.Items[0].Text = "Remove column " + ch.Text;
                m_ctxmHeader.Show(Control.MousePosition);
                }
            else {}
        }

        private void HandleSelectHeaderItem(object sender, EventArgs e)
        {
            // first, figure out what column they right clicked on (this is useful if they are asking
            // to show a column -- this will tell us where to insert the column before)
            // this information is squirrelled away in the Tag of the context menu

            ColumnHeader ch = (ColumnHeader) m_ctxmHeader.Tag;

            // are we removing or checking?
            ToolStripMenuItem tsmi = (ToolStripMenuItem) sender;
            AzLogViewSettings.DefaultColumnDef dcd = (AzLogViewSettings.DefaultColumnDef) tsmi.Tag;

            if (tsmi.Checked)
                {
                // we are removing a column
                tsmi.Checked = false;
                RemoveHeader(dcd.sName);
                }
            else
                {
                tsmi.Checked = true;
                AddHeader(dcd, ch.Text);
                }
        }

        private void AddHeader(AzLogViewSettings.DefaultColumnDef dcd, string sColumnInsertBefore)
        {
            int iazlvcInsert = m_azlvs.IazlvcFind(sColumnInsertBefore);
            int iazlvc = m_azlvs.IazlvcFind(dcd.sName);

            if (iazlvc == -1)
                {
                // we are adding this column
                m_azlvs.AddLogViewColumn(dcd.sName, dcd.nWidthDefault, dcd.lc, true);
                iazlvc = m_azlvs.IazlvcFind(dcd.sName);
                }
            else
                {
                m_azlvs.ShowHideColumn(dcd.sName, true);
                }

            m_azlvs.MoveColumn(iazlvc, iazlvcInsert);

            int c = m_lvLog.VirtualListSize;

            SetupListViewForLog(m_azlvs);
            m_azlv.BumpGeneration();
            m_lvLog.VirtualListSize = c;
        }

        private void RemoveHeader(string sColumnName)
        {
            m_azlvs.ShowHideColumn(sColumnName, false);

            int c = m_lvLog.VirtualListSize;

            SetupListViewForLog(m_azlvs);
            m_azlv.BumpGeneration();
            m_lvLog.VirtualListSize = c;
        }

        private void HandleRemoveHeaderItem(object sender, EventArgs e)
        {
            RemoveHeader(((ColumnHeader) ((((ToolStripMenuItem) sender).GetCurrentParent()).Tag)).Text);
            // or we could just get this from m_ctxmHeader.Tag...
        }

        private void HandleFormClosed(object sender, FormClosedEventArgs e)
        {
            CloseWindow();
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

        private int m_nLogViewGeneration;
                    // when we change views on the azlw, we have to bump this number so that we know that any cached ListViewItems are now invalid 

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

        public void UpdateViewRegion(int iMin, int iMac)
        {
            m_azlw.AppendUpdateView(iMin, iMac);
        }

        // public bool InvokeRequired { get { return m_azlw.InvokeRequired; } }

        public AzLogView(AzLogWindow azlw)
        {
            m_azlw = azlw;
            m_nLogViewGeneration = (new Random(System.Environment.TickCount)).Next(10000);
        }

        // only used for unit tests
        public AzLogView()
        {
            m_azlw = null;
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

        public AzLogEntry AzleItem(int i)
        {
            return m_azlm.LogEntry(m_pliale[i]);
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

        [TestCase("11-15-2015 10:00", new Int64[] {60, 200, 30, 230})]
        [TestCase("11-15-2015 10:00", new Int64[] {20, 200, 230, 430})]
        [TestCase("11-15-2015 10:00", new Int64[] {635814444358372408, 635814457244493004, 635814444361184850, 635814444364310162})]
        [Test]
        public void TestSort(string sPartition, Int64[] rg)
        {
            AzLogModel azlm = new AzLogModel();

            azlm.AddTestDataPartition(DateTime.Parse(sPartition), rg, null);
            List<Int64> pls = new List<Int64>();

            foreach (Int64 n in rg)
                pls.Add(n);

            pls.Sort();

            BuildView(azlm, new CompareLogEntryTickCount(azlm));

            for (int i = 0; i < pls.Count; i++)
                Assert.AreEqual(pls[i], AzleItem(i).EventTickCount);
        }

        [TestCase("11-15-2015 10:00", "test", new string[] {"test", "test", null, null, null, null, null, null, null, null, null})]
        [TestCase("11-15-2015 10:00", "test\tparm1", new string[] {"test\tparm1", "test", "parm1", null, null, null, null, null, null, null, null})]
        [TestCase("11-15-2015 10:00", "test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9", new string[] {"test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9", "test", "parm1", "parm2", "parm3", "parm4", "parm5", "parm6", "parm7", "parm8", "parm9"})]
        [TestCase("11-15-2015 10:00", "test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9\tparm10", new string[] {"test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9\tparm10", "test", "parm1", "parm2", "parm3", "parm4", "parm5", "parm6", "parm7", "parm8", "parm9"})]
        [TestCase("11-15-2015 10:00", "test\tparm1\t\tparm3", new string[] {"test\tparm1\t\tparm3", "test", "parm1", "", "parm3", null, null, null, null, null, null})]
        [Test]
        public void TestMessageSplit(string sPartition, string sMessage, string []rgsExpected)
        {
            AzLogModel azlm = new AzLogModel();

            azlm.AddTestDataPartition(DateTime.Parse(sPartition), new Int64[] {10}, new string[] {sMessage});

            BuildView(azlm, new CompareLogEntryTickCount(azlm));

            AzLogEntry azle = AzleItem(0);

            Assert.AreEqual(rgsExpected[0], azle.Message);
            Assert.AreEqual(rgsExpected[1], azle.Message0);
            Assert.AreEqual(rgsExpected[2], azle.Message1);
            Assert.AreEqual(rgsExpected[3], azle.Message2);
            Assert.AreEqual(rgsExpected[4], azle.Message3);
            Assert.AreEqual(rgsExpected[5], azle.Message4);
            Assert.AreEqual(rgsExpected[6], azle.Message5);
            Assert.AreEqual(rgsExpected[7], azle.Message6);
            Assert.AreEqual(rgsExpected[8], azle.Message7);
            Assert.AreEqual(rgsExpected[9], azle.Message8);
        }

        [TestCase("11-15-2015 10:00", new Int64[] {60, 200, 30, 230}, "11-15-2015 12:00", new long[] {360, 300, 330, 430})]
        [TestCase("11-15-2015 12:00", new Int64[] {360, 300, 330, 430}, "11-15-2015 10:00", new long[] {60, 200, 30, 230})]
        [TestCase("11-15-2015 10:00", new Int64[] {20, 200, 230, 430}, "11-15-2015 12:00", new long[] {220, 400, 430, 530})]
        [TestCase("11-15-2015 12:00", new Int64[] {220, 400, 430, 530}, "11-15-2015 10:00", new long[] {20, 200, 230, 430})]
        [Test]
        public void TestSortTwoPartitions(string sPartition, Int64[] rg, string sPartition2, Int64[] rg2)
        {
            AzLogModel azlm = new AzLogModel();

            azlm.AddTestDataPartition(DateTime.Parse(sPartition), rg, null);
            azlm.AddTestDataPartition(DateTime.Parse(sPartition2), rg2, null);
            List<Int64> pls = new List<Int64>();

            foreach (Int64 n in rg)
                pls.Add(n);

            foreach (Int64 n in rg2)
                pls.Add(n);

            pls.Sort();

            BuildView(azlm, new CompareLogEntryTickCount(azlm));

            for (int i = 0; i < pls.Count; i++)
                Assert.AreEqual(pls[i], AzleItem(i).EventTickCount);
        }
    }
}

