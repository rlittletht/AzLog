using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCore.Settings;

namespace AzLog
{
    public partial class AzAddAccount : Form
    {
        public AzAddAccount(Settings.SettingsElt[] rgsteeAccount)
        {
            InitializeComponent();
            this.CancelButton = m_pbCancel;

            _rgsteeAccount = rgsteeAccount;
            for (int i = 0; i < _rgsteeAccount.Length; i++)
                {
                if (_rgsteeAccount[i].sRePath == "AccountKey")
                    _rgsteeAccount[i].oref = m_ebAccountKey;
                else if (_rgsteeAccount[i].sRePath == "StorageType")
                    _rgsteeAccount[i].oref = m_cbStorageType;
                else if (_rgsteeAccount[i].sRePath == "StorageDomain")
                    _rgsteeAccount[i].oref = m_ebAccountDomain;
                }
        }

        public void LoadSettings(string sAccountName)
        {
            Settings ste = new Settings(_rgsteeAccount, String.Format("Software\\Thetasoft\\AzLog\\{0}", sAccountName), sAccountName);

            ste.Load();
            m_ebAccountName.Text = sAccountName;
        }

        private Settings.SettingsElt[] _rgsteeAccount;

        private void DoSave(object sender, EventArgs e)
        {
            string sAccountName = m_ebAccountName.Text;

            Settings ste = new Settings(_rgsteeAccount, String.Format("Software\\Thetasoft\\AzLog\\{0}", sAccountName), sAccountName);
            ste.Save();
            this.Close();
        }

        public static bool EditStorageAccount(string sAccountName, Settings.SettingsElt[] rgsteeAccount)
        {
            AzAddAccount azaa = new AzAddAccount(rgsteeAccount);
            azaa.LoadSettings(sAccountName);
            azaa.ShowDialog();
                        
            return true;
        }

        public static bool AddStorageAccount(Settings.SettingsElt[] rgsteeAccount)
        {
            AzAddAccount azaa = new AzAddAccount(rgsteeAccount);

            return azaa.ShowDialog() == System.Windows.Forms.DialogResult.OK;
        }

        private void DoCancel(object sender, EventArgs e)
        {
            
        }

        private void DoTest(object sender, EventArgs e)
        {
            if (AzTableCollection.TestConnection(m_ebAccountName.Text, m_ebAccountKey.Text))
                MessageBox.Show("Table client successfully created", "AzLog", MessageBoxButtons.OK);
            else
                MessageBox.Show("FAILED to create table client", "AzLog", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
    }
}
