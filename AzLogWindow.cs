using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public AzLogViewSettings ViewSettings => m_azlvs;
        private ILogClient m_ilc; // allows setting of default view
        private AzLogViewSettings m_azlvs;

        #region Construct/Destruct

        /* A Z  L O G  W I N D O W */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogWindow
        	%%Qualified: AzLog.AzLogWindow.AzLogWindow
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogWindow()
        {
            InitializeComponent();
            PopulateViewList();
        }


        /* C R E A T E  N E W  W I N D O W */
        /*----------------------------------------------------------------------------
        	%%Function: CreateNewWindow
        	%%Qualified: AzLog.AzLogWindow.CreateNewWindow
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogWindow CreateNewWindow(AzLogModel azlm, string sViewName, AzLogFilter azlf, ILogClient ilc)
        {
            AzLogWindow azlw = new AzLogWindow();

            if (String.IsNullOrEmpty(sViewName))
                sViewName = "Default";

            azlw.SetView(sViewName);
            azlw.m_azlv = new AzLogView(azlw);
            azlw.m_azlv.SetFilter(azlf);

            // propagate the filter values we were given into the textboxes on our window
            AzLogFilter.AzLogFilterCondition azlfc;

            azlfc = azlf.Operations[0].Condition;
            if (azlfc.LHS.Source == AzLogFilter.AzLogFilterValue.DataSource.DttmRow && azlfc.Comparison == AzLogFilter.AzLogFilterCondition.CmpOp.Gte)
                azlw.m_ebStart.Text = azlfc.RHS.Dttm(null).ToLocalTime().ToString("MM/dd/yy HH:mm");

            azlfc = azlf.Operations[1].Condition;
            if (azlfc.LHS.Source == AzLogFilter.AzLogFilterValue.DataSource.DttmRow && azlfc.Comparison == AzLogFilter.AzLogFilterCondition.CmpOp.Lt)
                azlw.m_ebEnd.Text = azlfc.RHS.Dttm(null).ToLocalTime().ToString("MM/dd/yy HH:mm");

            azlw.m_ilc = ilc;
            azlw.m_azlm = azlm;
            azlw.m_azlv.BuildView(azlw.m_azlm, new CompareLogEntryTickCount(azlw.m_azlm));
            // azlw.m_lvLog.VirtualListSize = azlw.m_azlv.Length;
            azlw.m_lvLog.SetVirtualListSize(azlw.m_azlv.Length);
            return azlw;
        }

        /* C L O S E  W I N D O W */
        /*----------------------------------------------------------------------------
        	%%Function: CloseWindow
        	%%Qualified: AzLog.AzLogWindow.CloseWindow
        	%%Contact: rlittle
        	
            Handle removing this from the underlying model (so we don't get any more
            notifications)
        ----------------------------------------------------------------------------*/
        public void CloseWindow()
        {
            m_azlm.RemoveView(m_azlv);
        }


        /* H A N D L E  F O R M  C L O S E D */
        /*----------------------------------------------------------------------------
        	%%Function: HandleFormClosed
        	%%Qualified: AzLog.AzLogWindow.HandleFormClosed
        	%%Contact: rlittle
        	
            Handle the form closed event
        ----------------------------------------------------------------------------*/
        private void HandleFormClosed(object sender, FormClosedEventArgs e)
        {
            CloseWindow();
        }

        #endregion

        #region View Settings/Selection

        /* D I R T Y  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: DirtyView
        	%%Qualified: AzLog.AzLogWindow.DirtyView
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DirtyView(bool fDirty)
        {
            m_pbSave.Enabled = fDirty;
            m_azlvs.Dirty = fDirty;
        }

        /* C R E A T E  N E W  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: CreateNewView
        	%%Qualified: AzLog.AzLogWindow.CreateNewView
        	%%Contact: rlittle
        	
            Create a new view copied from the current view. Ask the user to give us
            a name for the new view
        ----------------------------------------------------------------------------*/
        private void CreateNewView()
        {
            string sName;
            if (TCore.UI.InputBox.ShowInputBox("New view name", "View name", "", out sName))
                {
                // create a new view based on the current view

                AzLogViewSettings azlvs = m_azlvs.Clone();
                azlvs.SetName(sName);
                m_cbView.Items.Insert(m_cbView.Items.Count - 1, azlvs);
                m_cbView.SelectedIndex = m_cbView.Items.Count - 2;
                //m_azlvs = azlvs;
                }
        }

        /* C H A N G E  V I E W  S E L E C T E D */
        /*----------------------------------------------------------------------------
        	%%Function: ChangeViewSelected
        	%%Qualified: AzLog.AzLogWindow.ChangeViewSelected
        	%%Contact: rlittle
        	
            Handle a change to the selected view dropdown.
        ----------------------------------------------------------------------------*/
        private void ChangeViewSelected(object sender, EventArgs e)
        {
            AzLogViewSettings azlvs = (AzLogViewSettings) m_cbView.SelectedItem;

            if (azlvs.Name == "<New...>")
                {
                CreateNewView();
                DirtyView(true);
                }
            else
                {
                SetView(azlvs.Name);
                }
        }

        /* D O  V I E W  S A V E */
        /*----------------------------------------------------------------------------
        	%%Function: DoViewSave
        	%%Qualified: AzLog.AzLogWindow.DoViewSave
        	%%Contact: rlittle
        	
            Save all the current view settings
        ----------------------------------------------------------------------------*/
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

            for (int i = 0; i < m_azlvs.ColumnCount(); i++)
                {
                string sName = m_azlvs.Column(i).Name;

                // only sync the width here. all else sync during manipulation
                if (rgColumns.ContainsKey(sName))
                    m_azlvs.Column(i).Width = m_lvLog.Columns[rgColumns[sName]].Width;
                }

            m_azlvs.Save();
            m_ilc.SetDefaultView(m_azlvs.Name);
            DirtyView(false);
        }

        /* I  V I E W  F R O M  N A M E */
        /*----------------------------------------------------------------------------
        	%%Function: IViewFromName
        	%%Qualified: AzLog.AzLogWindow.IViewFromName
        	%%Contact: rlittle
        	
            Return the indext to the named view (in our combobox of views)
        ----------------------------------------------------------------------------*/
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

        /* S E T  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: SetView
        	%%Qualified: AzLog.AzLogWindow.SetView
        	%%Contact: rlittle
        	
            Set the current view to the given name. This will handle loading the 
            view settings from the registry and setting up the current view to match
            the selected view.
        ----------------------------------------------------------------------------*/
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

            SetupListViewForView(m_azlvs);
            SetupContextMenu();
            DirtyView(true);
            
        }


        /* S E T U P  L I S T  V I E W  F O R  L O G */
        /*----------------------------------------------------------------------------
        	%%Function: SetupListViewForView
        	%%Qualified: AzLog.AzLogWindow.SetupListViewForView
        	%%Contact: rlittle
        	
            Initialize the view to match the given view
        ----------------------------------------------------------------------------*/
        private void SetupListViewForView(AzLogViewSettings azlvs)
        {
            int i;

            m_lvLog.BeginUpdate();

            m_lvLog.Columns.Clear();

            //for (i = m_lvLog.Columns.Count - 1; i >= 0; --i)
            //m_lvLog.Columns.RemoveAt(i);

            for (i = 0; i < azlvs.ColumnCount(); i++)
                {
                AzLogViewSettings.AzLogViewColumn azlvc = azlvs.Column(i);

                m_lvLog.Columns.Add(new ColumnHeader());
                m_lvLog.Columns[i].Text = azlvc.Title;
                m_lvLog.Columns[i].Tag = azlvc.Name;
                m_lvLog.Columns[i].Width = azlvc.Width;
                }

            if (m_azlv != null)
                m_azlv.BumpGeneration();
            m_lvLog.EndUpdate();
            // m_lvLog.VirtualListSize = 0;
        }

        #endregion

        #region View Contents

        /* C L E A R  L O G */
        /*----------------------------------------------------------------------------
        	%%Function: ClearLog
        	%%Qualified: AzLog.AzLogWindow.ClearLog
        	%%Contact: rlittle
        	
            This is only going to happen if someone changes the underlying data model
            (its something that the model expects to call back into our window)
        ----------------------------------------------------------------------------*/
        public void ClearLog()
        {
            m_lvLog.Items.Clear();
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


        private void DoFetchLogEntries(object sender, EventArgs e)
        {
            // figure out the timespan being requested
            DateTime dttmMin, dttmMac;

            AzLogModel.FillMinMacFromStartEnd(m_ebStart.Text, m_ebEnd.Text, out dttmMin, out dttmMac);

            // we don't know what partition we're going to find this data in, so launch a query 
            // from the first partition for this date range
            // m_azlm.FFetchPartitionsForDateRange(dttmMin, nHourMin, dttmMac, nHourMac);
            m_azlm.FetchPartitionForDateAsync(dttmMin);
        }

        public delegate void SyncViewDel(int iFirstSync, int iMacSync);

        private object _syncLock = new Object();

        public object SyncLock
        {
            get { return _syncLock; }
            set { _syncLock = value; }
        }

        /* A P P E N D  U P D A T E  V I E W  C O R E */
        /*----------------------------------------------------------------------------
        	%%Function: AppendUpdateViewCore
        	%%Qualified: AzLog.AzLogWindow.AppendUpdateViewCore
        	%%Contact: rlittle
        	
            Actually append new items to our view. This adds the items to our views
            list of log entries (and re-sorts). Then updates our virtual list size

            TODO: Try to keep the view scrolled to the same place.
        ----------------------------------------------------------------------------*/
        public void AppendUpdateViewCore(int iMin, int iMac)
        {
            // we have new items from iMin to iMac. Add them to the view
            m_lvLog.BeginUpdate();
            m_azlv.AppendView(iMin, iMac);
            // m_lvLog.VirtualListSize = m_azlv.Length;
            m_lvLog.SetVirtualListSize(m_azlv.Length);
            m_lvLog.EndUpdate();
        }

        /* I N V A L  W I N D O W  F U L L */
        /*----------------------------------------------------------------------------
        	%%Function: InvalWindowFull
        	%%Qualified: AzLog.AzLogWindow.InvalWindowFull
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void InvalWindowFull()
        {
            m_azlv.BumpGeneration();
            m_lvLog.BeginUpdate();
//            m_lvLog.VirtualListSize = m_azlv.Length;
            m_lvLog.SetVirtualListSize(m_azlv.Length);
            m_lvLog.EndUpdate();
        }

        private int m_cDataPending = 0;

        /* B E G I N  D A T A  A S Y N C */
        /*----------------------------------------------------------------------------
        	%%Function: BeginDataAsync
        	%%Qualified: AzLog.AzLogWindow.BeginDataAsync
        	%%Contact: rlittle
        	
            Notify this window that there is async data coming
        ----------------------------------------------------------------------------*/
        public void BeginDataAsync()
        {
            if (m_cDataPending++ > 0)
                m_pgbMain.Show();
        }

        /* C O M P L E T E  D A T A  A S Y N C */
        /*----------------------------------------------------------------------------
        	%%Function: CompleteDataAsync
        	%%Qualified: AzLog.AzLogWindow.CompleteDataAsync
        	%%Contact: rlittle
        	
            Notify this window that we are done with pending async data
        ----------------------------------------------------------------------------*/
        public void CompleteDataAsync()
        {
            if (--m_cDataPending <= 0)
                {
                m_cDataPending = 0;
                m_pgbMain.Hide();
                }
        }

        /* A P P E N D  U P D A T E  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AppendUpdateView
        	%%Qualified: AzLog.AzLogWindow.AppendUpdateView
        	%%Contact: rlittle
        	
            Deal with a request to append new data to our view. This marshals to the
            UI thread
        ----------------------------------------------------------------------------*/
        public void AppendUpdateView(int iMin, int iMac)
        {
            if (m_lvLog.InvokeRequired)
                m_lvLog.BeginInvoke(new AzLogWindow.SyncViewDel(AppendUpdateViewCore), new object[] {iMin, iMac});
            else
                AppendUpdateViewCore(iMin, iMac);
        }

        #endregion

        #region Column Headers / Context Menus

        private HeaderSupp m_hs; // this is the guts of dealing with right clicking on the header region

        /* S E T U P  C O N T E X T  M E N U */
        /*----------------------------------------------------------------------------
        	%%Function: SetupContextMenu
        	%%Qualified: AzLog.AzLogWindow.SetupContextMenu
        	%%Contact: rlittle
        	
            Setup the context menu for the listview. This creates the "add/remove
            column" options.
        ----------------------------------------------------------------------------*/
        private void SetupContextMenu()
        {
            m_ctxmHeader.Items.Clear();

            ToolStripMenuItem tsmi = new ToolStripMenuItem {Text = "Remove Column...", Tag = null, Checked = false};
            tsmi.Click += HandleRemoveHeaderItem;
            m_ctxmHeader.Items.Add(tsmi);

            tsmi = new ToolStripMenuItem {Text = "Rename column...", Tag = null, Checked = false};
            tsmi.Click += HandleRenameHeaderItem;
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
            else
                {
                // we aren't in the column headers. now customize our context menu
                Point ptLocal = m_lvLog.PointToClient(Cursor.Position);
                ListViewItem lvi = m_lvLog.GetItemAt(ptLocal.X, ptLocal.Y);
                AzLogEntry azle = (AzLogEntry) lvi.Tag;

                ch = TCore.ListViewSupp.HeaderSupp.ColumnFromPoint(m_lvLog, ptLocal.X);
                AzLogViewSettings.AzLogViewColumn azlvc = m_azlvs.AzlvcFromName((string)ch.Tag);
                ContextMenuContext cmc = new ContextMenuContext();
                cmc.azle = azle;
                cmc.lc = azlvc.DataColumn;
                m_ctxmListViewLog.Items[0].Tag = cmc;
                m_ctxmListViewLog.Items[0].Text = String.Format("Filter to this {0}", ch.Text);
                m_ctxmListViewLog.Items[1].Tag = cmc;
                m_ctxmListViewLog.Items[1].Text = String.Format("Filter out this {0}", ch.Text);
                m_ctxmListViewLog.Items[2].Tag = cmc;
                m_ctxmListViewLog.Items[2].Text = String.Format("Color this {0}", ch.Text);
            }
        }

        struct ContextMenuContext
        {
            public AzLogEntry.LogColumn lc;
            public AzLogEntry azle;
        }

        /* H A N D L E  S E L E C T  H E A D E R  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: HandleSelectHeaderItem
        	%%Qualified: AzLog.AzLogWindow.HandleSelectHeaderItem
        	%%Contact: rlittle
        	
            Deal with selecting an item on the header context menu.
        ----------------------------------------------------------------------------*/
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
                AddHeader(dcd, (string) ch.Tag);
                }
        }

        /* A D D  H E A D E R */
        /*----------------------------------------------------------------------------
        	%%Function: AddHeader
        	%%Qualified: AzLog.AzLogWindow.AddHeader
        	%%Contact: rlittle
        	
            Add the named header to the view. This handles adding the column
            and invalidating the current view so it will get rebuild with the
            correct list view items.
        ----------------------------------------------------------------------------*/
        private void AddHeader(AzLogViewSettings.DefaultColumnDef dcd, string sColumnInsertBefore)
        {
            lock (SyncLock)
                {
                int iazlvcInsert = m_azlvs.IazlvcFind(sColumnInsertBefore);
                int iazlvc = m_azlvs.IazlvcFind(dcd.sName);

                if (iazlvc == -1)
                    {
                    // we are adding this column
                    m_azlvs.AddLogViewColumn(dcd.sName, dcd.sName, dcd.nWidthDefault, dcd.lc, true);
                    iazlvc = m_azlvs.IazlvcFind(dcd.sName);
                    }
                else
                    {
                    m_azlvs.ShowHideColumn(dcd.sName, true);
                    }

                m_azlvs.MoveColumn(iazlvc, iazlvcInsert);

                int c = m_lvLog.VirtualListSize;

                SetupListViewForView(m_azlvs);
                m_azlv.BumpGeneration();
                m_lvLog.SetVirtualListSize(c);
                // m_lvLog.VirtualListSize = c;
                }
            DirtyView(true);
        }

        /* R E M O V E  H E A D E R */
        /*----------------------------------------------------------------------------
        	%%Function: RemoveHeader
        	%%Qualified: AzLog.AzLogWindow.RemoveHeader
        	%%Contact: rlittle
        	
            Remove the named header from the list of columns displayed

            This deals with invalidating the current view.
        ----------------------------------------------------------------------------*/
        private void RemoveHeader(string sColumnName)
        {
            lock (SyncLock)
                {
                m_azlvs.ShowHideColumn(sColumnName, false);

                int c = m_lvLog.VirtualListSize;

                SetupListViewForView(m_azlvs);
                m_azlv.BumpGeneration();
                m_lvLog.SetVirtualListSize(c);
                //m_lvLog.VirtualListSize = c;
                }
            DirtyView(true);
        }

        /* H A N D L E  R E M O V E  H E A D E R  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: HandleRemoveHeaderItem
        	%%Qualified: AzLog.AzLogWindow.HandleRemoveHeaderItem
        	%%Contact: rlittle
        	
            Menu item handler for removing the current column
        ----------------------------------------------------------------------------*/
        private void HandleRemoveHeaderItem(object sender, EventArgs e)
        {
            RemoveHeader(((ColumnHeader) ((((ToolStripMenuItem) sender).GetCurrentParent()).Tag)).Text);
            DirtyView(true);
            // or we could just get this from m_ctxmHeader.Tag...
        }

        /* H A N D L E  R E N A M E  H E A D E R  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: HandleRenameHeaderItem
        	%%Qualified: AzLog.AzLogWindow.HandleRenameHeaderItem
        	%%Contact: rlittle
        	
            Rename the text of the column. The actual column name remains in the tag
        ----------------------------------------------------------------------------*/
        private void HandleRenameHeaderItem(object sender, EventArgs e)
        {
            ColumnHeader ch = (ColumnHeader) ((((ToolStripMenuItem) sender).GetCurrentParent()).Tag);

            string sName;
            if (TCore.UI.InputBox.ShowInputBox("New column name", "Column name", ch.Text, out sName))
                {
                m_azlvs.AzlvcFromName((string) ch.Tag).Title = sName;
                ch.Text = sName;
                }
            DirtyView(true);
        }

        /* D O  C O L U M N  R E O R D E R */
        /*----------------------------------------------------------------------------
        	%%Function: DoColumnReorder
        	%%Qualified: AzLog.AzLogWindow.DoColumnReorder
        	%%Contact: rlittle
        	
            Handle a drag of the column header (reordering the columns)
        ----------------------------------------------------------------------------*/
        private void DoColumnReorder(object sender, ColumnReorderedEventArgs e)
        {
            m_azlvs.MoveColumn(e.OldDisplayIndex, e.NewDisplayIndex);
                // just notify it of the move, this doesn't change anything until we save because the listview already did the move for us.
            DirtyView(true);
            // really, this is just about remembering the tab order...
        }

        /* N O T I F Y  C O L U M N  W I D T H  C H A N G E D */
        /*----------------------------------------------------------------------------
        	%%Function: NotifyColumnWidthChanged
        	%%Qualified: AzLog.AzLogWindow.NotifyColumnWidthChanged
        	%%Contact: rlittle
        
            Just make note of the fact that the column widths changed
        ----------------------------------------------------------------------------*/
        private void NotifyColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            AzLogViewSettings.AzLogViewColumn azlvc = m_azlvs.AzlvcFromName((string) m_lvLog.Columns[e.ColumnIndex].Tag);

            if (azlvc != null)
                {
                if (azlvc.Width != m_lvLog.Columns[e.ColumnIndex].Width)
                    {
                    azlvc.Width = m_lvLog.Columns[e.ColumnIndex].Width;
                    DirtyView(true);
                    }
                }
            else if (m_lvLog.Columns[e.ColumnIndex].Tag != null)
                throw (new Exception("no column found!"));
        }

        #endregion

        #region Filtering

        /* B U M P  D T T M  F I L T E R */
        /*----------------------------------------------------------------------------
        	%%Function: BumpDttmFilter
        	%%Qualified: AzLog.AzLogWindow.BumpDttmFilter
        	%%Contact: rlittle
        	
            Bump forward or backward the datetime filter by the given nHours. fStart
            determines if we are bumping the start or end of the range
        ----------------------------------------------------------------------------*/
        private void BumpDttmFilter(TextBox eb, int nHours, bool fStart)
        {
            // figure out the timespan being requested
            DateTime dttmMin, dttmMac;

            AzLogModel.FillMinMacFromStartEnd(eb.Text, eb.Text, out dttmMin, out dttmMac);

            // update our filter so we will see this

            // we intimately know which items to edit. if this ever turns out to be false, then we will have to find
            // the right parts of the filter, or just rebuild it. but that will be slower (ack)
            AzLogFilter.AzLogFilterOperation azlfo = m_azlv.Filter.Operations[fStart ? 0 : 1]; // start is first item, end is last

            if (azlfo.Op != AzLogFilter.AzLogFilterOperation.OperationType.Value ||
                !azlfo.Condition.RHS.FEvaluate(AzLogFilter.AzLogFilterCondition.CmpOp.Eq, new AzLogFilter.AzLogFilterValue(dttmMac), null))
                {
                throw new Exception("did not find correct end date value in filter");
                }

            // now add an hour to the end
            dttmMac = dttmMac.AddHours(nHours);
            azlfo.Condition.RHS.OValue = dttmMac; // update our filter to include the new date range, otherwise the model will get new data and we won't show it
            m_azlv.RebuildView();
            // 10/26/2015 9:00

            eb.Text = dttmMac.ToLocalTime().ToString("MM/dd/yyyy HH:mm");

            // at this point, all our changes were to "dttmMac" (dttmMin started out same as dttmMac...so we could just 
            // blissfully use dttmMac.  But for the fetch, it matters if we are growing or shrinking our range
            if (fStart)
                {
                if (nHours < 0)
                    m_azlm.FFetchPartitionsForDateRange(dttmMac, dttmMin); // i know, confusing. but we modified dttmMac...
                }
            else
                {
                if (nHours > 0)
                    m_azlm.FFetchPartitionsForDateRange(dttmMin, dttmMac);
                }
        }

        private void EndBumpForward(object sender, EventArgs e)         { BumpDttmFilter(m_ebEnd, 1, false); }
        private void EndBumpFastForward(object sender, EventArgs e)     { BumpDttmFilter(m_ebEnd, 12, false); }
        private void EndBumpRewind(object sender, EventArgs e)          { BumpDttmFilter(m_ebEnd, -1, false); }
        private void EndBumpFastRewind(object sender, EventArgs e)      { BumpDttmFilter(m_ebEnd, -12, false); }
        private void StartBumpFastForward(object sender, EventArgs e)   { BumpDttmFilter(m_ebStart, 12, true); }
        private void StartBumpForward(object sender, EventArgs e)       { BumpDttmFilter(m_ebStart, 1, true); }
        private void StartBumpReverse(object sender, EventArgs e)       { BumpDttmFilter(m_ebStart, -1, true); }
        private void StartBumpFastReverse(object sender, EventArgs e)   { BumpDttmFilter(m_ebStart, -12, true); }

        /* F I L T E R  T O  C O N T E X T */
        /*----------------------------------------------------------------------------
        	%%Function: CreateFilterToContext
        	%%Qualified: AzLog.AzLogWindow.CreateFilterToContext
        	%%Contact: rlittle
        	
            Grabt he ContextMenuContext that we stashed away when the context menu
            was popping up and then make a filter for that.

            (we can't evaluate the context here because the mousemove that happened 
            to select the context menu item will screw up the context of *where* they
            were when they right clicked for the context menu.
        ----------------------------------------------------------------------------*/
        private void CreateFilterToContext(object sender, EventArgs e)
        {
            ContextMenuContext cmc = (ContextMenuContext)((ToolStripMenuItem) sender).Tag;

            m_azlv.Filter.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.String, AzLogFilter.AzLogFilterValue.DataSource.Column, cmc.lc, AzLogFilter.AzLogFilterCondition.CmpOp.Eq, cmc.azle.GetColumn(cmc.lc)));
            m_azlv.Filter.Add(AzLogFilter.AzLogFilterOperation.OperationType.And);
            m_azlv.RebuildView();
        }

        /*----------------------------------------------------------------------------
        	%%Function: CreateColorContext
        	%%Qualified: AzLog.AzLogWindow.CreateColorContext
        	
            we're going to get called for the color item, so the parent of this item
            should have our tag
        ----------------------------------------------------------------------------*/
        private void CreateColorContext(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem) sender;
            ToolStripMenuItem menuItem = colorThisToolStripMenuItem;

            ContextMenuContext cmc = (ContextMenuContext)(menuItem).Tag;

            AzLogFilter filter = new AzLogFilter();
            filter.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.String, AzLogFilter.AzLogFilterValue.DataSource.Column, cmc.lc, AzLogFilter.AzLogFilterCondition.CmpOp.Eq, cmc.azle.GetColumn(cmc.lc)));
            m_azlv.AddColorFilter(filter, subMenuItem.BackColor, subMenuItem.ForeColor);
            m_azlv.RebuildView();
        }

        #endregion

        /* D O  E D I T  R E M O V E  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: DoEditRemoveFilters
        	%%Qualified: AzLog.AzLogWindow.DoEditRemoveFilters
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoEditRemoveFilters(object sender, EventArgs e)
        {
            AzLogFilter azlfNew = AzEditFilters.EditFilters(m_azlv.Filter.Clone());

            if (azlfNew != null)
                m_azlv.SetFilter(azlfNew);
        }

        /* D O  F E T C H */
        /*----------------------------------------------------------------------------
        	%%Function: DoFetch
        	%%Qualified: AzLog.AzLogWindow.DoFetch
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoFetch(object sender, EventArgs e)
        {
            DateTime dttmMin, dttmMac;

            AzLogModel.FillMinMacFromStartEnd(m_ebStart.Text, m_ebEnd.Text, out dttmMin, out dttmMac);

            // update our filter so we will see this

            // we intimately know which items to edit. if this ever turns out to be false, then we will have to find
            // the right parts of the filter, or just rebuild it. but that will be slower (ack)
            AzLogFilter.AzLogFilterOperation azlfo = m_azlv.Filter.Operations[0]; // start is first item, end is last


            if (azlfo.Op != AzLogFilter.AzLogFilterOperation.OperationType.Value || azlfo.Condition.LHS.Source != AzLogFilter.AzLogFilterValue.DataSource.DttmRow)
                {
                throw new Exception("did not find correct value type in filter");
                }

            azlfo.Condition.RHS.OValue = dttmMin; // update our filter to include the new date range, otherwise the model will get new data and we won't show it

            azlfo = m_azlv.Filter.Operations[1]; // start is first item, end is last


            if (azlfo.Op != AzLogFilter.AzLogFilterOperation.OperationType.Value || azlfo.Condition.LHS.Source != AzLogFilter.AzLogFilterValue.DataSource.DttmRow)
                {
                throw new Exception("did not find correct value type in filter");
                }

            azlfo.Condition.RHS.OValue = dttmMac; // update our filter to include the new date range, otherwise the model will get new data and we won't show it
            m_azlv.RebuildView();

            m_azlm.FFetchPartitionsForDateRange(dttmMin, dttmMac);
        }

        private void CreateFilterOutContext(object sender, EventArgs e)
        {
            ContextMenuContext cmc = (ContextMenuContext)((ToolStripMenuItem)sender).Tag;

            m_azlv.Filter.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.String, AzLogFilter.AzLogFilterValue.DataSource.Column, cmc.lc, AzLogFilter.AzLogFilterCondition.CmpOp.SNe, cmc.azle.GetColumn(cmc.lc)));
            m_azlv.Filter.Add(AzLogFilter.AzLogFilterOperation.OperationType.And);
            m_azlv.RebuildView();
        }

        private void DoFilterSave(object sender, EventArgs e)
        {

        }
    }
}

 
