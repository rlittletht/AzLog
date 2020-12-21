using System;
using System.Collections.Generic;
using AzLog;
using Microsoft.Win32;
using TCore.Settings;

namespace AzLog
{
    internal class AzLogCollection
    {
        private string m_sName;
        private List<IAzLogDatasource> m_pliazld;
        private string m_sDefaultView;

        /* L O A D  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: LoadCollection
        	%%Qualified: AzLog.AzLogCollection.LoadCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogCollection LoadCollection(string sRegRoot, string sName)
        {
            AzLogCollection azlc = new AzLogCollection(sName);
            azlc.Load(sRegRoot);

            return azlc;
        }

        /* A Z  L O G  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogCollection
        	%%Qualified: AzLog.AzLogCollection.AzLogCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogCollection(string sName)
        {
            m_sName = sName;
            m_pliazld = new List<IAzLogDatasource>();
        }

        public List<IAzLogDatasource> Sources => m_pliazld;

        private Settings.SettingsElt[] _rgsteeCollection =
            {
                new Settings.SettingsElt("DefaultView", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("Datasources", Settings.Type.StrArray, new string[] {}, new string[] {}),
            };

        private Settings m_ste;

        /* S E T  D E F A U L T  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: SetDefaultView
        	%%Qualified: AzLog.AzLogCollection.SetDefaultView
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetDefaultView(string sView)
        {
            m_sDefaultView = sView;

            // default view is autosaved unless we have never saved this collection
            if (m_ste != null)
                {
                m_ste.SetSValue("DefaultView", sView);
                m_ste.Save();
                }
        }

        public string DefaultView => m_sDefaultView;

        /* F  A D D  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: FAddDatasource
        	%%Qualified: AzLog.AzLogCollection.FAddDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public bool FAddDatasource(IAzLogDatasource iazlds)
        {
            m_pliazld.Add(iazlds);
            return true;
        }

        /* F  A D D  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: FAddDatasource
        	%%Qualified: AzLog.AzLogCollection.FAddDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public bool FAddDatasource(string sDatasource, string sRegRoot)
        {
            IAzLogDatasource iazlds = AzLogDatasourceSupport.LoadDatasource(null, sRegRoot, sDatasource);
            if (iazlds != null)
                return FAddDatasource(iazlds);

            return false;
        }

        /* R E M O V E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: RemoveDatasource
        	%%Qualified: AzLog.AzLogCollection.RemoveDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void RemoveDatasource(IAzLogDatasource iazlds)
        {
            string sDatasource = iazlds.ToString();

            for (int i = 0; i < m_pliazld.Count; i++)
                {
                if (string.Compare(sDatasource, m_pliazld[i].ToString(), true) == 0)
                    {
                    m_pliazld[i].Close();
                    m_pliazld.RemoveAt(i);
                    return;
                    }
                }
        }

        public void Load(string sRegRoot)
        {
            string sKeyName = String.Format("{0}\\Collections\\{1}", sRegRoot, m_sName);
            m_ste = new Settings(_rgsteeCollection, sKeyName, "main");
            m_ste.Load();

            m_sDefaultView = m_ste.SValue("DefaultView");
            string[] rgs = m_ste.RgsValue("Datasources");

            foreach (string s in rgs)
                {
                FAddDatasource(s, sRegRoot);
                }

        }

        public void Save(string sRegRoot)
        {
            string sKeyName = String.Format("{0}\\Collections\\{1}", sRegRoot, m_sName);

            if (m_ste == null)
                {
                m_ste = new Settings(_rgsteeCollection, sKeyName, "main");
                m_ste.Load();

                m_ste.SetSValue("DefaultView", m_sDefaultView);
                }

            string[] rgs = new string[m_pliazld.Count];

            int i = 0;
            foreach (IAzLogDatasource iazlds in m_pliazld)
                rgs[i++] = iazlds.GetName();

            m_ste.SetRgsValue("Datasources", rgs);
            m_ste.Save();
        }

    }
}