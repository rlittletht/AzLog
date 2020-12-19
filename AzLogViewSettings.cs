using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using NUnit.Framework;
using TCore.Settings;

namespace AzLog
{
    public class AzLogViewSettings
    {
        private string m_sName;
        private List<AzLogViewColumn> m_plazlvc;
        private List<int> m_pliazlvc; // the mapped tab order for saving later.  starts out with "identity"
        public string Name => m_sName;
        private bool m_fDirty;

        public class AzLogViewColumn
        {
            private string m_sName;
            private string m_sTitle;
            private int m_nWidth;
            private bool m_fVisible;
            private AzLogEntry.LogColumn m_azlvc;

            public string Name => m_sName;
            public AzLogEntry.LogColumn DataColumn => m_azlvc;

            public string Title
            {
                get { return m_sTitle; }
                set { m_sTitle = value; }
            }
            public int Width
            {
                get { return m_nWidth; }
                set { m_nWidth = value; }
            }

            public bool Visible
            {
                get { return m_fVisible; }
                set { m_fVisible = value; }
            }

            public override string ToString()
            {
                return m_sTitle;
            }

            public AzLogViewColumn(string sName, string sTitle, int nWidth, AzLogEntry.LogColumn azlvc, bool fVisible)
            {
                m_sName = sName;
                m_nWidth = nWidth;
                m_azlvc = azlvc;
                m_fVisible = fVisible;
                m_sTitle = sTitle;
            }

            public AzLogViewColumn Clone()
            {
                AzLogViewColumn azlvc = new AzLogViewColumn(m_sName, m_sTitle, m_nWidth, m_azlvc, m_fVisible);

                return azlvc;
            }
        }

        /* S E T  N A M E */
        /*----------------------------------------------------------------------------
            	%%Function: SetName
            	%%Qualified: AzLog.AzLogViewSettings.AzLogViewColumn.SetName
            	%%Contact: rlittle
            	
                Changing the name really is more than just setting the name string...or at
                least it could be, so make this an imperative function
        ----------------------------------------------------------------------------*/
        public void SetName(string sName)
        {
            m_sName = sName;
        }

        public override string ToString()
        {
            return m_sName;
        }

        public bool Dirty { get { return m_fDirty; } set { m_fDirty = value; } }
        #region Column Handling

        public AzLogViewColumn Column(int i)
        {
            return m_plazlvc[i];
        }

        public int ColumnCount()
        {
            return m_plazlvc.Count;
        }

        /* A D D  C O L U M N */
        /*----------------------------------------------------------------------------
        	%%Function: AddColumn
        	%%Qualified: AzLog.AzLogViewSettings.AddColumn
        	%%Contact: rlittle
        	
            Add the given column, making sure to keep the tab order mapping in synch
        ----------------------------------------------------------------------------*/
        public void AddColumn(AzLogViewColumn azlvc)
        {
            m_plazlvc.Add(azlvc);
            m_pliazlvc.Add(m_pliazlvc.Count);   // adds are always identity mapping because they are appended
            m_fDirty = true;
        }

        public void MoveColumn(int iSource, int iDest)
        {
            int iazlvc = m_pliazlvc[iSource];
            
            m_pliazlvc.RemoveAt(iSource);

            if (iDest > iSource)
                iDest--;

            m_pliazlvc.Insert(iDest, iazlvc);
            m_fDirty = true;
        }

        [TestCase("sMessage", 9, 9, 9, "Identity at the end")]
        [TestCase("PartitionKey", 0, 0, 0, "Identity at the beginning")]
        [TestCase("Level", 4, 4, 4, "Identity in the middle")]
        [TestCase("PartitionKey", 0, 4, 3, "Move later at the beginning")]
        [TestCase("Level", 4, 6, 5, "Move later in the middle")]
        [TestCase("sMessage", 9, 1, 1, "Move earlier at the end")]
        [TestCase("Level", 4, 1, 1, "Move earlier in the middle")]
        [Test]
        public void TestMove(string sColumn, int iazlvcExpected, int iazlvcDest, int iazlvcResult, string sTestDescription)
        {
            m_plazlvc.Clear();
            m_pliazlvc.Clear();

            SetDefault();

            int iazlvc = IazlvcTabOrderFromIazlvc(IazlvcFind(sColumn));

            Assert.AreEqual(iazlvcExpected, iazlvc, "{0} (source match)", sTestDescription);

            MoveColumn(iazlvc, iazlvcDest);
            int iazlvcNew = IazlvcTabOrderFromIazlvc(IazlvcFind(sColumn));
            Assert.AreEqual(iazlvcResult, iazlvcNew, "{0} (move result)", sTestDescription);
        }

        public int IazlvcTabOrderFromIazlvc(int iazlvc)
        {
            for (int i = 0; i < m_pliazlvc.Count; i++)
                if (m_pliazlvc[i] == iazlvc)
                    return i;

            return -1;
        }
        public int IazlvcFind(string sName)
        {
            for (int i = 0; i < m_plazlvc.Count; i++)
                {
                if (string.Compare(m_plazlvc[i].Name, sName, true) == 0)
                    return i;
                }
            return -1;
        }
        #endregion

        public AzLogViewSettings Clone()
        {
            AzLogViewSettings azlvs = new AzLogViewSettings(m_sName);

            azlvs.Dirty = true;
            azlvs.m_plazlvc.Clear();
            azlvs.m_pliazlvc.Clear();

            int i;
            for (i = 0; i < ColumnCount(); i++)
                {
                // BE CAREFUL! we want the new clone to have its columns in the order of
                // the tab order, NOT in the list order. we do this because the new view
                // is likely to be used to recreate a listview whereas the current view
                // reflects the loaded view PLUS any manipulation done by the current
                // window that the view is attached to
                azlvs.AddColumn(m_plazlvc[m_pliazlvc[i]].Clone());
                }
            return azlvs;
        }

        public AzLogViewSettings(string sName)
        {
            m_plazlvc = new List<AzLogViewColumn>();
            m_pliazlvc = new List<int>();
            m_sName = sName;
            Load();
            m_fDirty = false;
        }

        public AzLogViewSettings()
        {
            m_plazlvc = new List<AzLogViewColumn>();
            m_pliazlvc = new List<int>();
        }

        #region File I/O
        private Settings.SettingsElt[] _rgsteeColumn =
            {
                new Settings.SettingsElt("TabOrder", Settings.Type.Int, 0, ""),
                new Settings.SettingsElt("Width", Settings.Type.Int, 64, ""),
                new Settings.SettingsElt("DataLogColumn", Settings.Type.Int, 9, ""),
                new Settings.SettingsElt("Visible", Settings.Type.Bool, true, ""),
                new Settings.SettingsElt("Title", Settings.Type.Str, "", "")
            };

        /* K E Y  S E T T I N G S */
        /*----------------------------------------------------------------------------
        	%%Function: KeySettings
        	%%Qualified: AzLog.AzLogViewSettings.KeySettings
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private string KeySettings()
        {
            return String.Format("Software\\Thetasoft\\AzLog\\Views\\{0}", m_sName);

        }

        private string KeySettingsParent()
        {
            return "Software\\Thetasoft\\AzLog\\Views";
        }

        /* L O A D */
        /*----------------------------------------------------------------------------
        	%%Function: Load
        	%%Qualified: AzLog.AzLogViewSettings.Load
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void Load()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(KeySettings());

            if (rk == null)
                {
                SetDefault();
                return;
                }
            string[] rgs = rk.GetSubKeyNames();
            Settings ste;
            SortedList<int, Settings> mpnste = new SortedList<int, Settings>();

            foreach (string sColumn in rgs)
                {
                string sKey = String.Format("{0}\\{1}", KeySettings(), sColumn);
                ste = new Settings(_rgsteeColumn, sKey, sColumn);
                ste.Load();
                int nTabOrder = ste.NValue("TabOrder");
                mpnste.Add(nTabOrder, ste);
                }

            foreach (int nKey in mpnste.Keys)
                {
                ste = mpnste[nKey];

                int nWidth = ste.NValue("Width");
                AzLogEntry.LogColumn azlc = (AzLogEntry.LogColumn) ste.NValue("DataLogColumn");
                bool fVisible = ste.FValue("Visible");
                string sTitle = ste.SValue("Title");

                if (sTitle == "")
                    sTitle = (string) ste.Tag;

                AddLogViewColumn((string) ste.Tag, sTitle, nWidth, azlc, fVisible);
                }

            rk.Close();
            m_fDirty = false;
        }

        /* S A V E */
        /*----------------------------------------------------------------------------
        	%%Function: Save
        	%%Qualified: AzLog.AzLogViewSettings.Save
        	%%Contact: rlittle
        	
            Save is a little tricky. We load the settings into a new listview, which
            means that the taborder is the same as the column order. on save, though,
            the listview could have been reordered by the user, which means that the
            the tab order isn't necessarily the same as our column ordering.

            the client will tell use the tab ordering via an rgsColumns

            TODO: All this wacky "watch the user moving the columns" is unnecessary.
            Just use the DisplayIndex int he column header (see GetOrderedHeaders in
            ListViewSupp.cs)
        ----------------------------------------------------------------------------*/
        public void Save()
        {
            RegistryKey rk = Settings.RkEnsure(KeySettingsParent());
            
            // build a tab order reverse mapping
            int[] mpTabOrder = new int[m_pliazlvc.Count];

            for (int i = 0; i < m_pliazlvc.Count; i++)
                mpTabOrder[m_pliazlvc[i]] = i;

            rk.DeleteSubKeyTree(m_sName, false);
            rk.Close();

            rk = Settings.RkEnsure(KeySettings());
            
            for (int i = 0; i < ColumnCount(); i++)
                {
                AzLogViewColumn azlvc = Column(i);

                string sKey = String.Format("{0}\\{1}", KeySettings(), azlvc.Name);
                Settings ste = new Settings(_rgsteeColumn, sKey, azlvc.Name);

                ste.SetNValue("TabOrder", mpTabOrder[i]);
                ste.SetNValue("Width", azlvc.Width);
                ste.SetNValue("DataLogColumn", ((int) azlvc.DataColumn));
                ste.SetFValue("Visible", azlvc.Visible);
                ste.SetSValue("Title", azlvc.Title);

                ste.Save();
                }
            rk.Close();
            m_fDirty = false;
        }
        #endregion

        #region Initialization

        public struct DefaultColumnDef
        {
            public string sName;
            public AzLogEntry.LogColumn lc;
            public int nWidthDefault;
            public bool fVisibleDefault;

            public DefaultColumnDef(string sNameIn, AzLogEntry.LogColumn lcIn, int nWidthDefaultIn, bool fVisibleDefIn = true)
            {
                sName = sNameIn;
                lc = lcIn;
                nWidthDefault = nWidthDefaultIn;
                fVisibleDefault = fVisibleDefIn;
            }
        }

        private DefaultColumnDef[] _rgdcd = new DefaultColumnDef[]
        {
            new DefaultColumnDef("PartitionKey", AzLogEntry.LogColumn.Partition, 64),
            new DefaultColumnDef("RowKey", AzLogEntry.LogColumn.RowKey, 64),
            new DefaultColumnDef("EventTickCount", AzLogEntry.LogColumn.EventTickCount, 64),
            new DefaultColumnDef("AppName", AzLogEntry.LogColumn.AppName, 64),
            new DefaultColumnDef("Level", AzLogEntry.LogColumn.Level, 64),
            new DefaultColumnDef("EventID", AzLogEntry.LogColumn.EventID, 64),
            new DefaultColumnDef("InstanceID", AzLogEntry.LogColumn.InstanceID, 64),
            new DefaultColumnDef("Pid", AzLogEntry.LogColumn.Pid, 64),
            new DefaultColumnDef("nTid", AzLogEntry.LogColumn.Tid, 64),
            new DefaultColumnDef("UlsTimestamp", AzLogEntry.LogColumn.UlsTimestamp, 64, false),
            new DefaultColumnDef("UlsArea", AzLogEntry.LogColumn.UlsArea, 64, false),
            new DefaultColumnDef("UlsCategory", AzLogEntry.LogColumn.UlsCategory, 64, false),
            new DefaultColumnDef("UlsCorrelation", AzLogEntry.LogColumn.UlsCorrelation, 64, false),
            new DefaultColumnDef("UlsEventID", AzLogEntry.LogColumn.UlsEventID, 64, false),
            new DefaultColumnDef("sMessage", AzLogEntry.LogColumn.Message, 64),
            new DefaultColumnDef("sMessage0", AzLogEntry.LogColumn.Message0, 64, false),
            new DefaultColumnDef("sMessage1", AzLogEntry.LogColumn.Message1, 64, false),
            new DefaultColumnDef("sMessage2", AzLogEntry.LogColumn.Message2, 64, false),
            new DefaultColumnDef("sMessage3", AzLogEntry.LogColumn.Message3, 64, false),
            new DefaultColumnDef("sMessage4", AzLogEntry.LogColumn.Message4, 64, false),
            new DefaultColumnDef("sMessage5", AzLogEntry.LogColumn.Message5, 64, false),
            new DefaultColumnDef("sMessage6", AzLogEntry.LogColumn.Message6, 64, false),
            new DefaultColumnDef("sMessage7", AzLogEntry.LogColumn.Message7, 64, false),
            new DefaultColumnDef("sMessage8", AzLogEntry.LogColumn.Message8, 64, false),
            new DefaultColumnDef("sMessage9", AzLogEntry.LogColumn.Message9, 64, false)
        };

        public DefaultColumnDef[] DefaultColumns => _rgdcd;


        public static string GetColumnName(AzLogEntry.LogColumn lc)
        {
            switch (lc)
                {
                case AzLogEntry.LogColumn.Partition:
                    return "PartitionKey";
                case AzLogEntry.LogColumn.RowKey:
                    return "RowKey";
                case AzLogEntry.LogColumn.EventTickCount:
                    return "EventTickCount";
                case AzLogEntry.LogColumn.AppName:
                    return "AppName";
                case AzLogEntry.LogColumn.Level:
                    return "Level";
                case AzLogEntry.LogColumn.EventID:
                    return "EventID";
                case AzLogEntry.LogColumn.InstanceID:
                    return "InstanceID";
                case AzLogEntry.LogColumn.Pid:
                    return "Pid";
                case AzLogEntry.LogColumn.Tid:
                    return "Tid";
                case AzLogEntry.LogColumn.Message:
                    return "sMessage";
                case AzLogEntry.LogColumn.Message0:
                    return "sMessage0";
                case AzLogEntry.LogColumn.Message1:
                    return "sMessage1";
                case AzLogEntry.LogColumn.Message2:
                    return "sMessage2";
                case AzLogEntry.LogColumn.Message3:
                    return "sMessage3";
                case AzLogEntry.LogColumn.Message4:
                    return "sMessage4";
                case AzLogEntry.LogColumn.Message5:
                    return "sMessage5";
                case AzLogEntry.LogColumn.Message6:
                    return "sMessage6";
                case AzLogEntry.LogColumn.Message7:
                    return "sMessage7";
                case AzLogEntry.LogColumn.Message8:
                    return "sMessage8";
                case AzLogEntry.LogColumn.Message9:
                    return "sMessage9";
                default:
                    return "";
                }
        }
        /* S E T  D E F A U L T */
        /*----------------------------------------------------------------------------
        	%%Function: SetDefault
        	%%Qualified: AzLog.AzLogViewSettings.SetDefault
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetDefault()
        {
            foreach (DefaultColumnDef dcd in _rgdcd)
                {
                if (dcd.fVisibleDefault)
                    AddLogViewColumn(dcd.sName, dcd.sName, dcd.nWidthDefault, dcd.lc, true);
                }
        }

        /* A D D  L O G  V I E W  C O L U M N */
        /*----------------------------------------------------------------------------
        	%%Function: AddLogViewColumn
        	%%Qualified: AzLog.AzLogViewSettings.AddLogViewColumn
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void AddLogViewColumn(string sName, string sTitle, int nWidth, AzLogEntry.LogColumn azlc, bool fVisible)
        {
            AddColumn(new AzLogViewColumn(sName, sTitle, nWidth, azlc, fVisible));
            m_fDirty = true;
        }

        #endregion 

        /* A Z L V C  F R O M  N A M E */
        /*----------------------------------------------------------------------------
        	%%Function: AzlvcFromName
        	%%Qualified: AzLog.AzLogViewSettings.AzlvcFromName
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogViewColumn AzlvcFromName(string sName)
        {
            foreach (AzLogViewColumn azlvc in m_plazlvc)
                {
                if (String.Compare(azlvc.Name, sName, true /*fIgnoreCase*/) == 0)
                    return azlvc;
                }
            return null;
        }

        public void SetColumnSize(string sName, int nWidth)
        {
            AzLogViewColumn azlvc = AzlvcFromName(sName);

            if (azlvc != null)
                azlvc.Width = nWidth;
            m_fDirty = true;
        }

        // TODO: Need to make this work for Show. Right now it assumes hide always.
        public bool ShowHideColumn(string sName, bool fVisible)
        {
            AzLogViewColumn azlvc = AzlvcFromName(sName);

            if (azlvc != null)
                {
                // we can't just remove this from m_plazlvc -- we also have to update our tab order mapping
                // because we just changed all the indexes
                int iazlvc = m_plazlvc.IndexOf(azlvc);

                for (int i = 0; i < m_pliazlvc.Count; i++)
                    {
                    if (m_pliazlvc[i] == iazlvc)
                        m_pliazlvc.RemoveAt(i);
                    else if (m_pliazlvc[i] > iazlvc)
                        m_pliazlvc[i]--;
                    }

                m_plazlvc.RemoveAt(iazlvc);
                m_fDirty = true;
//                bool fVisibleSav = azlvc.Visible;
                //azlvc.Visible = fVisible;
                //return fVisibleSav;
                }
            return false;
        }
    }
}