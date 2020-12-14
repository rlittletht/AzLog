using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using TCore.Settings;

namespace AzLog
{
    // This implements adding/editing an Azure datasource
    public partial class AzAddDatasource_Azure : Form
    {
        private string m_sRegRoot;
        private Settings.SettingsElt[] _rgsteeAccount =
            {
                new Settings.SettingsElt("AccountKey", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("StorageType", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("StorageDomain", Settings.Type.Str, "", ""),
            };

        /* A Z  A D D  D A T A S O U R C E _  A Z U R E */
        /*----------------------------------------------------------------------------
        	%%Function: AzAddDatasource_Azure
        	%%Qualified: AzLog.AzAddDatasource_Azure.AzAddDatasource_Azure
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzAddDatasource_Azure(string sRegRoot, IAzLogDatasource iazlds = null)
        {
            m_sRegRoot = sRegRoot;
            m_iazlds = iazlds;

            InitializeComponent();
            PopulateAccounts();
            if (m_iazlds != null)
                {
                m_ebName.Text = m_iazlds.GetName();
                SelectCbItem(m_cbStorageType, AzLogDatasourceSupport.TypeToString(m_iazlds.GetSourceType()));
                }
        }

        void SelectCbItem(ComboBox cb, string sStringToFind)
        {
            int iSel;

            for (iSel = 0; iSel < cb.Items.Count; iSel++)
                {
                if (String.Compare(cb.Items[iSel].ToString(), sStringToFind, StringComparison.InvariantCultureIgnoreCase) == 0)
                    break;
                }
            if (iSel >= cb.Items.Count)
                return;

            cb.SelectedIndex = iSel;
        }

        /* P O P U L A T E  A C C O U N T S */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateAccounts
        	%%Qualified: AzLog.AzAddDatasource_Azure.PopulateAccounts
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void PopulateAccounts()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(String.Format("{0}\\AzureAccounts", m_sRegRoot));
            string[] rgs = rk.GetSubKeyNames();

            foreach (string s in rgs)
            {
                if (s != "Views")
                    m_cbAccounts.Items.Add(s);
            }
            rk.Close();
        }


        /* D O  A D D  E D I T  A C C O U N T */
        /*----------------------------------------------------------------------------
        	%%Function: DoAddEditAccount
        	%%Qualified: AzLog.AzAddDatasource_Azure.DoAddEditAccount
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoAddEditAccount(object sender, EventArgs e)
        {
            if (m_cbAccounts.SelectedIndex == -1)
                {
                AzAddAccount.AddStorageAccount(AzLogAzureTable.AccountSettingsDef());
                m_cbAccounts.Items.Clear();
                PopulateAccounts();
                }
            else
                AzAddAccount.EditStorageAccount((string) m_cbAccounts.SelectedItem, AzLogAzureTable.AccountSettingsDef());
        }

        private IAzLogDatasource m_iazlds;

        private string KeyName => (string)String.Format("{0}\\AzureAccounts\\{1}", m_sRegRoot, m_cbAccounts.SelectedItem);
        
        
        /* D O  O P E N  A C C O U N T */
        /*----------------------------------------------------------------------------
        	%%Function: DoOpenAccount
        	%%Qualified: AzLog.AzAddDatasource_Azure.DoOpenAccount
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoOpenAccount(object sender, EventArgs e)
        {
            if (m_cbStorageType.SelectedIndex == -1)
                return;

            m_lbTables.Items.Clear();

            Settings ste = new Settings(_rgsteeAccount, KeyName, "main");
            ste.Load();

            DatasourceType dt = AzLogDatasourceSupport.TypeFromString(m_cbStorageType.Text);

            if (dt == DatasourceType.AzureBlob)
                {
                AzLogAzureBlob azla = new AzLogAzureBlob();

                azla.OpenAccount((string) m_cbAccounts.SelectedItem, AzLogAzureBlob.GetAccountKey(m_sRegRoot, (string) m_cbAccounts.SelectedItem));
                foreach (string s in azla.Containers)
                    m_lbTables.Items.Add(s);

                m_iazlds = azla;
                }
            else if (dt == DatasourceType.AzureTable)
                {
                AzLogAzureTable azlt = new AzLogAzureTable();

                azlt.OpenAccount((string) m_cbAccounts.SelectedItem, AzLogAzureTable.GetAccountKey(m_sRegRoot, (string) m_cbAccounts.SelectedItem));

                foreach (string s in azlt.Tables)
                    m_lbTables.Items.Add(s);

                m_iazlds = azlt;
                }
        }

        /* D O  S E L E C T  T A B L E */
        /*----------------------------------------------------------------------------
        	%%Function: DoSelectTable
        	%%Qualified: AzLog.AzAddDatasource_Azure.DoSelectTable
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoSelectTable(object sender, EventArgs e)
        {
            m_iazlds.OpenContainer(null, (string)m_lbTables.SelectedItem);
        }

        /* C R E A T E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: CreateDatasource
        	%%Qualified: AzLog.AzAddDatasource_Azure.CreateDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static IAzLogDatasource CreateDatasource(string sRegRoot)
        {
            AzAddDatasource_Azure azads = new AzAddDatasource_Azure(sRegRoot);

            if (azads.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                azads.m_iazlds.SetName(azads.m_ebName.Text);

                return azads.m_iazlds;
                }
            return null;
        }

        public static void EditDatasource(string sRegRoot, IAzLogDatasource iazlds)
        {
            AzAddDatasource_Azure azads = new AzAddDatasource_Azure(sRegRoot, iazlds);

            if (azads.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                azads.m_iazlds.SetName(azads.m_ebName.Text);

                //return azads.m_iazlds;
                }
            //return null;
        }
    }
}
