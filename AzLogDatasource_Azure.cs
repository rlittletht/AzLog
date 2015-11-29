using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TCore.Settings;

namespace AzLog
{
    public class AzLogAzure : IAzLogDatasource
    {
        private AzTable m_azt = null;

        private AzTableCollection m_aztc;
        private List<string> m_plsTables;

        private string m_sName;
        private string m_sAccountName;
        private string m_sTableName;
        private int m_iDatasource;  // what is our iDatasource (for updating partitions, etc.)

        public List<string> Tables => m_plsTables;

        #region IAzLogDatasource implementation

        /* S E T  D A T A S O U R C E  I N D E X */
        /*----------------------------------------------------------------------------
        	%%Function: SetDatasourceIndex
        	%%Qualified: AzLog.AzLogAzure.SetDatasourceIndex
        	%%Contact: rlittle
        	
            When we update partitions with Complete/pending, we need to know what
            our index is.
        ----------------------------------------------------------------------------*/
        public void SetDatasourceIndex(int i)
        {
            m_iDatasource = i;
        }

        public int GetDatasourceIndex()
        {
            return m_iDatasource;
        }

        /* T O  S T R I N G */
        /*----------------------------------------------------------------------------
        	%%Function: ToString
        	%%Qualified: AzLog.AzLogAzure.ToString
        	%%Contact: rlittle
        	
            This is the display name (and the comparison identifier) for this
            datasource
        ----------------------------------------------------------------------------*/
        public override string ToString()
        {
            return String.Format("{0} [Azure]", m_sName);
        }

        /* G E T  N A M E */
        /*----------------------------------------------------------------------------
        	%%Function: GetName
        	%%Qualified: AzLog.AzLogAzure.GetName
        	%%Contact: rlittle
        	
            The raw name of this datasource
        ----------------------------------------------------------------------------*/
        public string GetName()
        {
            return m_sName;
        }

        /* S E T  N A M E */
        /*----------------------------------------------------------------------------
        	%%Function: SetName
        	%%Qualified: AzLog.AzLogAzure.SetName
        	%%Contact: rlittle
        	
            Allows setting of the name through the interface
        ----------------------------------------------------------------------------*/
        public void SetName(string sName)
        {
            m_sName = sName;
        }

        private Settings.SettingsElt[] _rgsteeDatasource =
            {
                new Settings.SettingsElt("AccountName", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("TableName", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("Type", Settings.Type.Str, "", ""),
            };

        public static Settings.SettingsElt[] AccountSettingsDef()
        {
            return new Settings.SettingsElt[]
                       {
                           new Settings.SettingsElt("AccountKey", Settings.Type.Str, "", ""),
                           new Settings.SettingsElt("StorageType", Settings.Type.Str, "", ""),
                           new Settings.SettingsElt("StorageDomain", Settings.Type.Str, "", ""),
                       };

        }

        /* S A V E */
        /*----------------------------------------------------------------------------
        	%%Function: Save
        	%%Qualified: AzLog.AzLogAzure.Save
        	%%Contact: rlittle
        	
            Save this datasource to the registry
        ----------------------------------------------------------------------------*/
        public void Save(string sRegRoot)
        {
            if (string.IsNullOrEmpty(m_sName))
                throw new Exception("Cannot save empty datasource name");

            string sKey = String.Format("{0}\\Datasources\\{1}", sRegRoot, m_sName);

            // save everything we need to be able to recreate ourselves
            Settings ste = new Settings(_rgsteeDatasource, sKey, "ds");

            ste.SetSValue("AccountName", m_aztc.AccountName);
            ste.SetSValue("TableName", m_azt.Table.Name);
            ste.SetSValue("Type", "AzureTableStorage");
            ste.Save();
        }

        /* F  L O A D */
        /*----------------------------------------------------------------------------
        	%%Function: FLoad
        	%%Qualified: AzLog.AzLogAzure.FLoad
        	%%Contact: rlittle
        	
            Load the information about this datasource, but don't actually open it
            (doesn't ping the net or validate information)
        ----------------------------------------------------------------------------*/
        public bool FLoad(AzLogModel azlm, string sRegRoot, string sName)
        {
            string sKey = String.Format("{0}\\Datasources\\{1}", sRegRoot, sName);

            // save everything we need to be able to recreate ourselves
            Settings ste = new Settings(_rgsteeDatasource, sKey, "ds");

            ste.Load();
            m_sAccountName = ste.SValue("AccountName");
            m_sTableName = ste.SValue("TableName");
            m_sName = sName;

            return true;
        }

        /* F  O P E N */
        /*----------------------------------------------------------------------------
        	%%Function: FOpen
        	%%Qualified: AzLog.AzLogAzure.FOpen
        	%%Contact: rlittle
        	
            Actually open the datasource, connecting to the source
        ----------------------------------------------------------------------------*/
        public bool FOpen(AzLogModel azlm, string sRegRoot)
        {
            try
            {
                OpenAccount(m_sAccountName, GetAccountKey(sRegRoot, m_sAccountName));
                OpenTable(azlm, m_sTableName);
            }
            catch (Exception exc)
            {
                MessageBox.Show(String.Format("Couldn't open azure datasource {0}: {1}", m_sName, exc.Message));
                return false;
            }
            return true;
        }
        #endregion

        #region Static helpers
        /* G E T  A C C O U N T  K E Y */
        /*----------------------------------------------------------------------------
        	%%Function: GetAccountKey
        	%%Qualified: AzLog.AzLogAzure.GetAccountKey
        	%%Contact: rlittle
        	
            gets the account key from the registry
        ----------------------------------------------------------------------------*/
        public static string GetAccountKey(string sRegRoot, string sAccountName)
        {
            Settings ste = new Settings(AccountSettingsDef(), String.Format("{0}\\AzureAccounts\\{1}", sRegRoot, sAccountName), "main");
            ste.Load();

            return ste.SValue("AccountKey");
        }

        /* L O A D  A Z U R E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: LoadAzureDatasource
        	%%Qualified: AzLog.AzLogAzure.LoadAzureDatasource
        	%%Contact: rlittle
        	
            Loads this azure datasource
        ----------------------------------------------------------------------------*/
        public static AzLogAzure LoadAzureDatasource(AzLogModel azlm, string sRegRoot, string sName)
        {
            AzLogAzure azla = new AzLogAzure();

            if (azla.FLoad(azlm, sRegRoot, sName))
                return azla;

            return null;
        }
        #endregion

        #region Account / Table Support

        /* O P E N  T A B L E */
        /*----------------------------------------------------------------------------
        	%%Function: OpenTable
        	%%Qualified: AzLog.AzLogModel.OpenTable
        	%%Contact: rlittle
        	
            clear all of the views since we are now opening a new table.
        ----------------------------------------------------------------------------*/
        public void OpenTable(AzLogModel azlm, string sTableName)
        {
            if (azlm != null)
                {
                foreach (AzLogView azlv in azlm.Listeners)
                    azlv.ClearLog();
                }

            m_azt = m_aztc.GetTable(sTableName);
        }

        /* O P E N  A C C O U N T */
        /*----------------------------------------------------------------------------
        	%%Function: OpenAccount
        	%%Qualified: AzLog.AzLogModel.OpenAccount
        	%%Contact: rlittle
        	
            Open the given Azure account and populate the list of tables that we 
            know about
        ----------------------------------------------------------------------------*/
        public void OpenAccount(string sAccountName, string sAccountKey)
        {
            m_aztc = new AzTableCollection(sAccountName, sAccountKey);
            m_plsTables = m_aztc.PlsTableNames();
        }

        public void Close()
        {
            m_aztc.Close();
        }
        #endregion

        /* F E T C H  P A R T I T I O N  F O R  D A T E */
        /*----------------------------------------------------------------------------
        	%%Function: FetchPartitionForDateAsync
        	%%Qualified: AzLog.AzLogModel.FetchPartitionForDateAsync
        	%%Contact: rlittle
        	
            Fetch the partition for the given dttm (assumes that the hour is also
            filled in)
        ----------------------------------------------------------------------------*/
        public async Task<bool> FetchPartitionForDateAsync(AzLogModel azlm, DateTime dttm)
        {
            TableQuery<AzLogEntryEntity> tq =
                new TableQuery<AzLogEntryEntity>().Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                                                           AzLogModel.SPartitionFromDate(dttm, dttm.Hour)));

            TableQuerySegment<AzLogEntryEntity> azleSegment = null;
            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Pending);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.BeginAsyncData();

            while (azleSegment == null || azleSegment.ContinuationToken != null)
                {
                azleSegment = await m_azt.Table.ExecuteQuerySegmentedAsync(tq, azleSegment?.ContinuationToken);
                foreach (AzLogView azlv in azlm.Listeners)
                    {
                    lock (azlv.SyncLock)
                        {
                        int iFirst = azlm.Log.Length;

                        // TODO: Really, add the segment in the loop?!
                        azlm.AddSegment(azleSegment);

                        int iLast = azlm.Log.Length;

                        azlv.AppendUpdateRegion(iFirst, iLast);
                        }
                    }

                azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Complete);
                }
            foreach (AzLogView azlv in azlm.Listeners)
                azlv.CompleteAsyncData();

            return true;
        }

    }
}