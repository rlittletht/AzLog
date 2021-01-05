using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TCore.PostfixText;

namespace AzLog
{
    // This is the view that underlies a window. (Each window has a view)
    // filtering and sorting happens here.
    public class AzLogView
    {
        private List<int> m_pliale;
        private IComparer<int> m_icle;
        private AzLogModel m_azlm;
        private AzLogFilter m_azlf;
        private List<AzColorFilter> m_colorFilters = new List<AzColorFilter>();

        public int Length => m_pliale.Count;
        public AzLogFilter Filter => m_azlf;
        public List<AzColorFilter> ColorFilters => m_colorFilters;
        
        private AzLogWindow m_azlw;

        // when we change views on the azlw, we have to bump this number so that we know that any cached ListViewItems are now invalid 
        // (this is lazy invalidation).  for now, since we don't actually cache ListViewItems, this is a noop
        private int m_nLogViewGeneration;

        #region Construct/Destruct

        /* A Z  L O G  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogView
        	%%Qualified: AzLog.AzLogView.AzLogView
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogView(AzLogWindow azlw)
        {
            m_azlw = azlw;
            m_nLogViewGeneration = (new Random(System.Environment.TickCount)).Next(10000);
        }

        /* A Z  L O G  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogView
        	%%Qualified: AzLog.AzLogView.AzLogView
        	%%Contact: rlittle
        	
            Constructor used only by unit tests -- there is no attached window!
        ----------------------------------------------------------------------------*/
        public AzLogView()
        {
            m_azlw = null;
            m_nLogViewGeneration = (new Random(System.Environment.TickCount)).Next(10000);
        }

        #endregion

        #region Core Model / Update / Filter

        /* C L E A R  L O G */
        /*----------------------------------------------------------------------------
        	%%Function: ClearLog
        	%%Qualified: AzLog.AzLogView.ClearLog
        	%%Contact: rlittle

        	Clear our window
        ----------------------------------------------------------------------------*/
        public void ClearLog()
        {
            m_azlw.ClearLog();
        }

        /* S E T  F I L T E R */
        /*----------------------------------------------------------------------------
        	%%Function: SetFilter
        	%%Qualified: AzLog.AzLogView.SetFilter
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetFilter(AzLogFilter azlf)
        {
            m_azlf = azlf;
        }

        public void SetColorFilters(List<AzColorFilter> colorFilters)
        {
	        m_colorFilters = colorFilters;
        }

        public void AddColorFilter(AzLogFilter azlf, Color colorBack, Color colorFore)
        {
            m_colorFilters.Add(new AzColorFilter(azlf, colorBack, colorFore));
        }

        public bool FGetColorForItem(PostfixText.IValueClient client, out Color colorBack, out Color colorFore)
        {
            colorBack = Color.White;
            colorFore = Color.Black;

            foreach (AzColorFilter colorFilter in m_colorFilters)
            {
                if (colorFilter.Matches(client))
                {
                    colorBack = colorFilter.BackColor;
                    colorFore = colorFilter.ForeColor;

                    return true;
                }
            }

            return false;
        }

        public object SyncLock
        {
            get { return m_azlw.SyncLock; }
            set { m_azlw.SyncLock = value; }
        }

        /* A P P E N D  U P D A T E  R E G I O N */
        /*----------------------------------------------------------------------------
        	%%Function: AppendUpdateRegion
        	%%Qualified: AzLog.AzLogView.AppendUpdateRegion
        	%%Contact: rlittle
        	
            This is how the model tells us about new data -- it appends to our view
        ----------------------------------------------------------------------------*/
        public void AppendUpdateRegion(int iMin, int iMac)
        {
            m_azlw.AppendUpdateView(iMin, iMac);
        }

        /* B U M P  G E N E R A T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: BumpGeneration
        	%%Qualified: AzLog.AzLogView.BumpGeneration
        	%%Contact: rlittle
        	
            Whenever this is called, all of the cache ListViewItems become
            invalid and will be rebuilt.

            Callers should also invalidate their entire view to force a refetch
            of each listview item.
        ----------------------------------------------------------------------------*/
        public void BumpGeneration()
        {
            m_nLogViewGeneration++;
        }

        /* L V I  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: LviItem
        	%%Qualified: AzLog.AzLogView.LviItem
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public ListViewItem LviItem(int i)
        {
            return m_azlm.LogEntry(m_pliale[i]).LviFetch(m_nLogViewGeneration, m_azlw.ViewSettings, this);
        }

        /* A Z L E  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: AzleItem
        	%%Qualified: AzLog.AzLogView.AzleItem
        	%%Contact: rlittle
        	
            Return an AzLogEntry for the given item in our view. 
        ----------------------------------------------------------------------------*/
        public AzLogEntry AzleItem(int i)
        {
            return m_azlm.LogEntry(m_pliale[i]);
        }

        public enum SearchKind
        {
            Forward,
            Backward,
            Nearest
        }

        public delegate bool FMatchDelegate(int entry, string search);
        
        /*----------------------------------------------------------------------------
			%%Function:FindEntryInView
			%%Qualified:AzLog.AzLogView.FindEntryInView

			nearest means we search forward and backward alternately looking for the
			item
        ----------------------------------------------------------------------------*/
        public static int FindEntryInViewCore(string s, int iStart, int iMax, SearchKind kind, int cMaxItemsToSearch, FMatchDelegate matchDelegate)
        {
	        if (iMax == 0)
		        return -1;
	        
	        int iLastReverse = kind == SearchKind.Forward ? iStart + 1 : 0;
	        int iLastForward = kind == SearchKind.Backward ? iStart - 1 : iMax - 1;
	        
	        if (cMaxItemsToSearch != -1)
	        {
		        iLastReverse = Math.Max(iLastReverse, iStart - cMaxItemsToSearch);
		        iLastForward = Math.Min(iLastForward, iStart + cMaxItemsToSearch);
	        }

	        int iForward = kind == SearchKind.Forward ? iStart : iStart + 1;
	        int iReverse = iStart;

	        while (iForward <= iLastForward || iReverse >= iLastReverse)
	        {
		        if (iReverse >= iLastReverse)
		        {
                    if (matchDelegate(iReverse, s))
	                    return iReverse;
                    
                    iReverse--;
		        }

		        if (iForward <= iLastForward)
		        {
			        if (matchDelegate(iForward, s))
				        return iForward;

			        iForward++;
		        }
	        }

	        return -1;
        }

        /*----------------------------------------------------------------------------
			%%Function:FindEntryInView
			%%Qualified:AzLog.AzLogView.FindEntryInView
        ----------------------------------------------------------------------------*/
        public int FindEntryInView(string s, int iStart, SearchKind kind = SearchKind.Forward, int cMaxItemsToSearch = -1)
        {
            return FindEntryInViewCore(
	            s, 
	            iStart, 
	            m_pliale.Count, 
	            kind, 
	            cMaxItemsToSearch,
	            (entry, search) => m_azlm.LogEntry(m_pliale[entry]).FMatchSearch(search, m_azlw.ViewSettings));
        
        }

        /*----------------------------------------------------------------------------
			%%Function:FindNearestEventTickcountInView
			%%Qualified:AzLog.AzLogView.FindNearestEventTickcountInView
        ----------------------------------------------------------------------------*/
        public int FindNearestEventTickcountInView(string sEventTickCount, int hint)
        {
	        return FindEntryInViewCore(
		        sEventTickCount,
		        hint,
		        m_pliale.Count,
		        SearchKind.Nearest,
		        100,
		        (entry, search) => String.Compare(m_azlm.LogEntry(m_pliale[entry]).GetColumn(AzLogEntry.LogColumn.EventTickCount), search) == 0);
        }

        class SelectionRestoreContext
        {
	        private List<string> m_plBackwardSave; // this will include the actual selected item
	        private List<string> m_plForwardSave;
	        
	        public delegate string FetchKeyDelegate(int i);

	        public static SelectionRestoreContext CreateRestoreContextFromListView(int iPreserve, int iMax, FetchKeyDelegate fetchKey)
	        {
		        SelectionRestoreContext restore = new SelectionRestoreContext();

		        restore.m_plBackwardSave = new List<string>();
		        restore.m_plForwardSave = new List<string>();

		        int i;
		        
		        for (i = iPreserve; i > Math.Max(0, i - 10); i--)
			        restore.m_plBackwardSave.Add(fetchKey(i));

		        i = iPreserve + 1;
		        
		        iMax = Math.Min(iMax, i + 10);
		        while (i < iMax)
		        {
			        restore.m_plForwardSave.Add(fetchKey(i++));
		        }

		        return restore;
	        }

	        public delegate int RestoreSearchDelegate(string sKey);
	        
	        public int FindNearestForRestore(RestoreSearchDelegate searchDelegate)
	        {
                // we have no idea what items that used to be around us are valid --
                // any of them could now be filtered out. We've got a max of 20 keys
                // to try to match (10 back and 10 forward, if they were available).

                while (m_plBackwardSave.Count > 0 || m_plForwardSave.Count > 0)
                {
	                int i;

	                if (m_plBackwardSave.Count > 0)
	                {
		                i = searchDelegate(m_plBackwardSave[0]);

		                if (i != -1)
			                return i;

		                m_plBackwardSave.RemoveAt(0);
	                }

	                if (m_plForwardSave.Count > 0)
	                {
		                i = searchDelegate(m_plForwardSave[0]);

		                if (i != -1)
			                return i;

		                m_plForwardSave.RemoveAt(0);
	                }
                }

                return -1;
	        }
        }
        
        /* R E B U I L D  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: RebuildView
        	%%Qualified: AzLog.AzLogView.RebuildView
        	%%Contact: rlittle
        	
            This is a complete, nuclear rebuild. 
        
			Whenever we rebuild the view, we want to try to center back on the same
			selected item as before. even if we can't find it, we'll find one around
			it
        ----------------------------------------------------------------------------*/
        public int RebuildView(int iPreserveSelection)
        {
	        SelectionRestoreContext restore = iPreserveSelection != -1
		        ? SelectionRestoreContext.CreateRestoreContextFromListView(
			        iPreserveSelection,
			        m_pliale.Count,
			        (_i) => m_azlm.LogEntry(m_pliale[_i]).GetColumn(AzLogEntry.LogColumn.EventTickCount))
		        : null;
	        
            m_pliale = new List<int>();

            for (int i = 0; i < m_azlm.LogCount; i++)
                {
                AppendItemToView(i);
                }
            m_pliale.Sort(m_icle);
            if (m_azlw != null)
                m_azlw.InvalWindowFull();

            if (restore != null)
            {
	            int hint = Math.Min(iPreserveSelection, m_pliale.Count);

                return restore.FindNearestForRestore((_search) => FindNearestEventTickcountInView(_search, hint));
            }
            
            return -1;
        }

        /*----------------------------------------------------------------------------
			%%Function:RebuildView
			%%Qualified:AzLog.AzLogView.RebuildView
        ----------------------------------------------------------------------------*/
        public int RebuildView()
        {
	        return RebuildView(-1);
        }
        
        private int m_cAsyncBegun = 0;

        public void BeginAsyncData()
        {
            if (m_azlw != null)
                {
                m_azlw.BeginDataAsync();
                m_cAsyncBegun++;
                }
        }

        public void CompleteAsyncData()
        {
            if (m_cAsyncBegun > 0)
                {
                m_cAsyncBegun--;
                if (m_azlw != null)
                    m_azlw.CompleteDataAsync();
                }
        }

        /* B U I L D  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: BuildView
        	%%Qualified: AzLog.AzLogView.BuildView
        	%%Contact: rlittle
        	
            Build the current view (or rebuild the current view). Use the given
            IComparer to sort items.

            (REMEMBER: the IComparer is always going to compare two indexes since
            it has to go through a level of indirection to get to the actual model
            data.  The difference is going to be calculated based on specific 
            underlying AzLogEntry fields)
        ----------------------------------------------------------------------------*/
        public void BuildView(AzLogModel azlm, IComparer<int> icle)
        {
            m_icle = icle;
            m_azlm = azlm;
            RebuildView();
        }

        /* A P P E N D  I T E M  T O  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AppendItemToView
        	%%Qualified: AzLog.AzLogView.AppendItemToView
        	%%Contact: rlittle
        	
            Append a single item to our view -- make sure it matches our filter
        ----------------------------------------------------------------------------*/
        private void AppendItemToView(int i)
        {
            AzLogEntry azle = m_azlm.LogEntry(i);
            if (m_azlf.FEvaluate(azle))
                m_pliale.Add(i);
        }

        /* A P P E N D  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: AppendView
        	%%Qualified: AzLog.AzLogView.AppendView
        	%%Contact: rlittle
        	
            Append iMin to iMac log entries. This really is just adding just the 
            indexes (raw) to the list and then re-sorting.

            Remember, all we store is a list of indexes into the underlying data 
            model.
        ----------------------------------------------------------------------------*/
        public void AppendView(int iMin, int iMac)
        {
            while (iMin < iMac)
                {
                // for each item, make sure it matches our filter criteria
                AppendItemToView(iMin);
                iMin++;
                }

            m_pliale.Sort(m_icle);
        }
        #endregion

        #region Unit Tests

        [TestCase("11-15-2015 10:00", new Int64[] {60, 200, 30, 230})]
        [TestCase("11-15-2015 10:00", new Int64[] {20, 200, 230, 430})]
        [TestCase("11-15-2015 10:00",
            new Int64[] {635814444358372408, 635814457244493004, 635814444361184850, 635814444364310162})]
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

        [TestCase("11-15-2015 10:00", "test",
            new string[] {"test", "test", null, null, null, null, null, null, null, null, null})]
        [TestCase("11-15-2015 10:00", "test\tparm1",
            new string[] {"test\tparm1", "test", "parm1", null, null, null, null, null, null, null, null})]
        [TestCase("11-15-2015 10:00", "test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9",
            new string[]
                {
                    "test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9", "test", "parm1", "parm2",
                    "parm3", "parm4", "parm5", "parm6", "parm7", "parm8", "parm9"
                })]
        [TestCase("11-15-2015 10:00", "test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9\tparm10",
            new string[]
                {
                    "test\tparm1\tparm2\tparm3\tparm4\tparm5\tparm6\tparm7\tparm8\tparm9\tparm10", "test", "parm1", "parm2",
                    "parm3", "parm4", "parm5", "parm6", "parm7", "parm8", "parm9"
                })]
        [TestCase("11-15-2015 10:00", "test\tparm1\t\tparm3",
            new string[] {"test\tparm1\t\tparm3", "test", "parm1", "", "parm3", null, null, null, null, null, null})]
        [Test]
        public void TestMessageSplit(string sPartition, string sMessage, string[] rgsExpected)
        {
            AzLogModel azlm = new AzLogModel();
            m_azlf = new AzLogFilter(); // just get a default empty one

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

        [TestCase("11-15-2015 10:00", new Int64[] {60, 200, 30, 230}, "11-15-2015 12:00",
            new long[] {360, 300, 330, 430})]
        [TestCase("11-15-2015 12:00", new Int64[] {360, 300, 330, 430}, "11-15-2015 10:00",
            new long[] {60, 200, 30, 230})]
        [TestCase("11-15-2015 10:00", new Int64[] {20, 200, 230, 430}, "11-15-2015 12:00",
            new long[] {220, 400, 430, 530})]
        [TestCase("11-15-2015 12:00", new Int64[] {220, 400, 430, 530}, "11-15-2015 10:00",
            new long[] {20, 200, 230, 430})]
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

        [TestCase(new[]{"1", "2", "3"}, "1", 0, SearchKind.Forward, -1, 0)]
        [TestCase(new[] { "1", "2", "3" }, "2", 0, SearchKind.Forward, -1, 1)]
        [TestCase(new[] { "1", "2", "3" }, "3", 0, SearchKind.Forward, -1, 2)]
        [TestCase(new[] { "1", "2", "3" }, "1", 1, SearchKind.Forward, -1, -1)]
        [TestCase(new string[] { }, "1", 0, SearchKind.Forward, -1, -1)]
        [TestCase(new[] { "1", "2", "3" }, "1", 2, SearchKind.Backward, -1, 0)]
        [TestCase(new[] { "1", "2", "3" }, "2", 2, SearchKind.Backward, -1, 1)]
        [TestCase(new[] { "1", "2", "3" }, "3", 2, SearchKind.Backward, -1, 2)]
        [TestCase(new[] { "1", "2", "3" }, "3", 1, SearchKind.Backward, -1, -1)]
        
        [TestCase(new[] { "1", "2", "3" }, "3", 1, SearchKind.Nearest, -1, 2)]
        [TestCase(new[] { "1", "2", "3" }, "3", 0, SearchKind.Nearest, -1, 2)]
        [TestCase(new[] { "1", "2", "3" }, "3", 0, SearchKind.Nearest, 1, -1)]
        [TestCase(new[] { "1", "2", "3" }, "1", 1, SearchKind.Nearest, -1, 0)]
        [TestCase(new[] { "1", "2", "3" }, "1", 2, SearchKind.Nearest, -1, 0)]
        [TestCase(new[] { "1", "2", "3" }, "1", 2, SearchKind.Nearest, 1, -1)]
        [Test]
        public static void TestFind(string[] domain, string search, int iStart, SearchKind kind, int cEntries, int iExpected)
        {
	        int iActual = FindEntryInViewCore(
		        search,
		        iStart,
		        domain.Length,
		        kind,
		        cEntries,
		        (_i, _s) => String.Compare(domain[_i], _s) == 0);

	        Assert.AreEqual(iExpected, iActual);
        }
        #endregion
    }
}