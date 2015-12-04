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

        public static AzLogCollection LoadCollection(string sRegRoot, string sName)
        {
            AzLogCollection azlc = new AzLogCollection(sName);
            azlc.Load(sRegRoot);

            return azlc;
        }

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

        private string m_sDefView;

        public void SetDefaultView(string sView)
        {
            m_sDefView = sView;

            // default view is autosaved unless we have never saved this collection
            if (m_ste != null)
                {
                m_ste.SetSValue("DefaultView", sView);
                m_ste.Save();
                }
        }

        public bool FAddDatasource(IAzLogDatasource iazlds)
        {
            m_pliazld.Add(iazlds);
            return true;
        }

        public bool FAddDatasource(string sDatasource, string sRegRoot)
        {
            IAzLogDatasource iazlds = AzLogDatasourceSupport.LoadDatasource(null, sRegRoot, sDatasource);
            if (iazlds != null)
                return FAddDatasource(iazlds);

            return false;
        }

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

                m_ste.SetSValue("DefaultView", m_sDefView);
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