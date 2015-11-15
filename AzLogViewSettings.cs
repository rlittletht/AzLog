using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using TCore.Settings;

namespace AzLog
{
    public class AzLogViewSettings
    {
        private string m_sName;
        private List<AzLogViewColumn> m_plazlvc;

        public string Name => m_sName;

        public class AzLogViewColumn
        {
            private string m_sName;
            private int m_nWidth;
            private bool m_fVisible;
            private AzLogEntry.LogColumn m_azlvc;

            public string Name => m_sName;
            public AzLogEntry.LogColumn DataColumn => m_azlvc;

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
                return m_sName;
            }

            public AzLogViewColumn(string sName, int nWidth, AzLogEntry.LogColumn azlvc, bool fVisible)
            {
                m_sName = sName;
                m_nWidth = nWidth;
                m_azlvc = azlvc;
                m_fVisible = fVisible;
            }

            public AzLogViewColumn Clone()
            {
                AzLogViewColumn azlvc = new AzLogViewColumn(m_sName, m_nWidth, m_azlvc, m_fVisible);

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

        public List<AzLogViewColumn> Columns => m_plazlvc;

        public void MoveColumn(int iSource, int iDest)
        {
            AzLogViewColumn azlvc = m_plazlvc[iSource];
            m_plazlvc.RemoveAt(iSource);

            if (iDest > iSource)
                iDest--;

            m_plazlvc.Insert(iDest, azlvc);
        }

        public AzLogViewSettings Clone()
        {
            AzLogViewSettings azlvs = new AzLogViewSettings(m_sName);
            int i;
            for (i = 0; i < Columns.Count; i++)
                {
                azlvs.Columns.Add(Columns[i].Clone());
                }
            return azlvs;
        }

        public AzLogViewSettings(string sName)
        {
            m_plazlvc = new List<AzLogViewColumn>();
            m_sName = sName;
            Load();
        }

        private Settings.SettingsElt[] _rgsteeColumn =
            {
                new Settings.SettingsElt("TabOrder", Settings.Type.Int, 0, ""),
                new Settings.SettingsElt("Width", Settings.Type.Int, 64, ""),
                new Settings.SettingsElt("DataLogColumn", Settings.Type.Int, 9, ""),
                new Settings.SettingsElt("Visible", Settings.Type.Bool, 1, ""),
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

                AddLogViewColumn((string) ste.Tag, nWidth, azlc, fVisible);
                }

            rk.Close();
        }

        /* S A V E */
        /*----------------------------------------------------------------------------
        	%%Function: Save
        	%%Qualified: AzLog.AzLogViewSettings.Save
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void Save()
        {
            RegistryKey rk = Settings.RkEnsure(KeySettingsParent());

            rk.DeleteSubKeyTree(m_sName, false);
            rk.Close();

            rk = Settings.RkEnsure(KeySettings());

            int i = 0;
            for (i = 0; i < Columns.Count; i++)
                {
                AzLogViewColumn azlvc = Columns[i];

                string sKey = String.Format("{0}\\{1}", KeySettings(), azlvc.Name);
                Settings ste = new Settings(_rgsteeColumn, sKey, azlvc.Name);

                ste.SetNValue("TabOrder", i);
                ste.SetNValue("Width", azlvc.Width);
                ste.SetNValue("DataLogColumn", ((int) azlvc.DataColumn));
                ste.SetFValue("Visible", azlvc.Visible);

                ste.Save();
                }
            rk.Close();
        }

        /* S E T  D E F A U L T */
        /*----------------------------------------------------------------------------
        	%%Function: SetDefault
        	%%Qualified: AzLog.AzLogViewSettings.SetDefault
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetDefault()
        {
            AddLogViewColumn("PartitionKey", 64, AzLogEntry.LogColumn.Partition, true);
            AddLogViewColumn("RowKey", 64, AzLogEntry.LogColumn.RowKey, true);
            AddLogViewColumn("EventTickCount", 64, AzLogEntry.LogColumn.EventTickCount, true);
            AddLogViewColumn("AppName", 64, AzLogEntry.LogColumn.AppName, true);
            AddLogViewColumn("Level", 64, AzLogEntry.LogColumn.Level, true);
            AddLogViewColumn("EventID", 64, AzLogEntry.LogColumn.EventID, true);
            AddLogViewColumn("InstanceID", 64, AzLogEntry.LogColumn.InstanceID, true);
            AddLogViewColumn("Pid", 64, AzLogEntry.LogColumn.Pid, true);
            AddLogViewColumn("nTid", 64, AzLogEntry.LogColumn.Tid, true);
            AddLogViewColumn("sMessage", 256, AzLogEntry.LogColumn.Message, true);
        }

        public void AddLogViewColumn(string sName, int nWidth, AzLogEntry.LogColumn azlc, bool fVisible)
        {
            m_plazlvc.Add(new AzLogViewColumn(sName, nWidth, azlc, fVisible));
        }

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
        }

        public bool ShowHideColumn(string sName, bool fVisible)
        {
            AzLogViewColumn azlvc = AzlvcFromName(sName);

            if (azlvc != null)
                {
                m_plazlvc.Remove(azlvc);
//                bool fVisibleSav = azlvc.Visible;
                //azlvc.Visible = fVisible;
                //return fVisibleSav;
                }
            return false;
        }
    }
}