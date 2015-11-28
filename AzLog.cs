using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using NUnit.Framework;
using TCore.Settings;

namespace AzLog
{
    public interface ILogClient
    {
        void SetDefaultView(string s);
    }

    public partial class AzLog : Form, ILogClient
    {
        private Settings.SettingsElt[] _rgsteeAccount =
            {
                new Settings.SettingsElt("AccountKey", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("StorageType", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("StorageDomain", Settings.Type.Str, "", ""),
            };

        private Settings.SettingsElt[] _rgsteeApp =
            {
                new Settings.SettingsElt("DefaultView", Settings.Type.Str, "", ""),
            };

        private string m_sDefaultView;

        private AzLogModel m_azlm;
        private Settings m_ste;
        private AzLogAzure m_azla;

        public AzLog()
        {
            m_azlm = new AzLogModel();
            
            InitializeComponent();
            PopulateAccounts();
            m_ste = new Settings(_rgsteeApp, "Software\\Thetasoft\\AzLog", "App");
            m_ste.Load();
            m_sDefaultView = m_ste.SValue("DefaultView");
            m_azla = new AzLogAzure();
        }

        public void SetDefaultView(string sView)
        {
            m_ste.SetSValue("DefaultView", sView);
            m_ste.Save();
        }

        public void PopulateAccounts()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("Software\\Thetasoft\\AzLog");
            string[] rgs = rk.GetSubKeyNames();

            foreach (string s in rgs)
                {
                if (s != "Views")
                    m_cbAccounts.Items.Add(s);
                }
            rk.Close();
        }

        private void DoAddEditAccount(object sender, EventArgs e)
        {
            if (m_cbAccounts.SelectedIndex == -1)
                AzAddAccount.AddStorageAccount(_rgsteeAccount);
            else
                AzAddAccount.EditStorageAccount((string)m_cbAccounts.SelectedItem, _rgsteeAccount);
        }

        private string KeyName => (string) String.Format("Software\\Thetasoft\\AzLog\\{0}", m_cbAccounts.SelectedItem);

        private void m_pbOpen_Click(object sender, EventArgs e)
        {
            Settings ste = new Settings(_rgsteeAccount, KeyName, "main");
            ste.Load();

            m_azla.OpenAccount((string) m_cbAccounts.SelectedItem, ste.SValue("AccountKey"));
            foreach (string s in m_azla.Tables)
                m_lbTables.Items.Add(s);
        }
        private void DoSelectTable(object sender, EventArgs e)
        {
            m_azla.OpenTable(m_azlm, (string) m_lbTables.SelectedItem);
            m_azlm.AttachDatasource(m_azla);
            // PopulateLog();
        }

        private void DoFetchLogEntries(object sender, EventArgs e)
        {
            // figure out the timespan being requested
            DateTime dttmMin, dttmMac;

            AzLogModel.FillMinMacFromStartEnd(m_ebStart.Text, m_ebEnd.Text, out dttmMin, out dttmMac);

            AzLogFilter azlf = new AzLogFilter();

            // create a basic filter based on the range they asked for
            azlf.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.DateTime, AzLogFilter.AzLogFilterValue.DataSource.DttmRow, AzLogEntry.LogColumn.Nil, 
                                                          AzLogFilter.AzLogFilterCondition.CmpOp.Gte, dttmMin));
            azlf.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.DateTime, AzLogFilter.AzLogFilterValue.DataSource.DttmRow, AzLogEntry.LogColumn.Nil, 
                                                          AzLogFilter.AzLogFilterCondition.CmpOp.Lt, dttmMac));
            azlf.Add(AzLogFilter.AzLogFilterOperation.OperationType.And);

            AzLogWindow azlw = AzLogWindow.CreateNewWindow(m_azlm, m_sDefaultView, azlf, this);

            azlw.Show();

            m_azlm.AddView(azlw.View);

            // we don't know what partition we're going to find this data in, so launch a query 
            // from the first partition for this date range
            //m_azlm.FFetchPartitionsForDateRange(dttmMin, nHourMin, dttmMac, nHourMac);
            m_azlm.FFetchPartitionsForDateRange(dttmMin, dttmMac);
        }

#if nomore
        void PopulateLog()
        {
            TableQuery<AzLogEntryEntity> tq =
                new TableQuery<AzLogEntryEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "2015102607"),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                                                           "11e57bb3-4322-2ebf-93ff-00155d4417a2")));

            foreach (AzLogEntryEntity azle in m_azt.Table.ExecuteQuery(tq))
                {
                // m_lvLog.Items.Add(azle.LviFetch());
                }
        }
#endif // nomore




    }

}
