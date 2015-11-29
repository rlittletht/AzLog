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
        public AzAddDatasource_Azure(string sRegRoot)
        {
            m_sRegRoot = sRegRoot;

            InitializeComponent();
            PopulateAccounts();
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
                AzAddAccount.AddStorageAccount(AzLogAzure.AccountSettingsDef());
            else
                AzAddAccount.EditStorageAccount((string)m_cbAccounts.SelectedItem, AzLogAzure.AccountSettingsDef());
        }

        private AzLogAzure m_azla;

        private string KeyName => (string)String.Format("{0}\\AzureAccounts\\{1}", m_sRegRoot, m_cbAccounts.SelectedItem);

        /* D O  O P E N  A C C O U N T */
        /*----------------------------------------------------------------------------
        	%%Function: DoOpenAccount
        	%%Qualified: AzLog.AzAddDatasource_Azure.DoOpenAccount
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoOpenAccount(object sender, EventArgs e)
        {
            Settings ste = new Settings(_rgsteeAccount, KeyName, "main");
            ste.Load();

            m_azla = new AzLogAzure();

            m_azla.OpenAccount((string)m_cbAccounts.SelectedItem, AzLogAzure.GetAccountKey(m_sRegRoot, (string)m_cbAccounts.SelectedItem));
            foreach (string s in m_azla.Tables)
                m_lbTables.Items.Add(s);
        }

        /* D O  S E L E C T  T A B L E */
        /*----------------------------------------------------------------------------
        	%%Function: DoSelectTable
        	%%Qualified: AzLog.AzAddDatasource_Azure.DoSelectTable
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoSelectTable(object sender, EventArgs e)
        {
            m_azla.OpenTable(null, (string)m_lbTables.SelectedItem);
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
                azads.m_azla.SetName(azads.m_ebName.Text);
                return azads.m_azla;
                }
            return null;
        }
    }
}
