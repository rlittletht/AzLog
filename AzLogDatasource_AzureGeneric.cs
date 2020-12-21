using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TCore.Settings;

namespace AzLog
{
    // generic front end for Azure Storage accounts, allows listing of Tables and Blobs to feed into the Datasource objects
    public class AzLogAzudreGeneric : IAzLogDatasource // not really a full implementation, but enough to save to the registry...
    {
        private List<string> m_plsContainers;

        private global::AzLog.DatasourceType m_dt;

        private string m_sName;
        private string m_sAccountName;
        private string m_sAccountKey;
        private string m_sContainer;


        public List<string> Containers => m_plsContainers;

        #region Static helpers
        /* G E T  A C C O U N T  K E Y */
        /*----------------------------------------------------------------------------
        	%%Function: GetAccountKey
        	%%Qualified: AzLog.AzLogAzureTable.GetAccountKey
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
        	%%Qualified: AzLog.AzLogAzureTable.LoadAzureDatasource
        	%%Contact: rlittle
        	
            Loads this azure datasource
        ----------------------------------------------------------------------------*/
        public static AzLogAzudreGeneric LoadAzureDatasource(AzLogModel azlm, string sRegRoot, string sName)
        {
            AzLogAzureGeneric azla = new AzLogAzureGeneric();

            if (azla.FLoad(azlm, sRegRoot, sName))
                return azla;

            return null;
        }
        #endregion

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

        #region IAzLogDatasource Implementation
        /*----------------------------------------------------------------------------
        	%%Function: GetSourceType
        	%%Qualified: AzLog.AzLogFile.GetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public DatasourceType GetSourceType()
        {
            return m_dt;
        }

        /*----------------------------------------------------------------------------
        	%%Function: SetSourceType
        	%%Qualified: AzLog.AzLogFile.SetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetSourceType(DatasourceType dt)
        {
            m_dt = dt;
        }

        /*----------------------------------------------------------------------------
        	%%Function: SetName
        	%%Qualified: AzLog.AzLogAzureGeneric.SetName
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetName(string sName)
        {
            m_sName = sName;
        }

        /* F  L O A D */
        /*----------------------------------------------------------------------------
        	%%Function: FLoad
        	%%Qualified: AzLog.AzLogAzureTable.FLoad
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
            m_sContainer = ste.SValue("TableName");
            m_dt = AzLogDatasourceSupport.TypeFromString(ste.SValue("Type"));
            m_sName = sName;

            return true;
        }

        public void Save(string sRegRoot)
        {
            if (string.IsNullOrEmpty(m_sName))
                throw new Exception("Cannot save empty datasource name");

            string sKey = String.Format("{0}\\Datasources\\{1}", sRegRoot, m_sName);

            // save everything we need to be able to recreate ourselves
            Settings ste = new Settings(_rgsteeDatasource, sKey, "ds");

            ste.SetSValue("AccountName", m_sAccountName);
            ste.SetSValue("TableName", m_sContainer);
            ste.SetSValue("Type", AzLogDatasourceSupport.TypeToString(m_dt));
            ste.Save();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public int GetDatasourceIndex()
        {
            throw new NotImplementedException();
        }

        public void SetDatasourceIndex(int i)
        {
            throw new NotImplementedException();
        }

        public bool FOpen(AzLogModel azlm, string sRegRoot)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FetchPartitionForDateAsync(AzLogModel azlm, DateTime dttm)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Account / Table Support

        public void OpenContainer(string sContainer)
        {
            m_sContainer = sContainer;
        }

        public List<string> PlsNames(string sAccountName, string sAccountKey, DatasourceType st)
        {
            string sConnectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                                     sAccountName, sAccountKey);
            CloudTableClient ctc = null;
            CloudBlobClient cbc = null;
            List<string> pls = new List<string>();

            try
            {
                CloudStorageAccount csa = CloudStorageAccount.Parse(sConnectionString);

                if (csa == null)
                    return null;

                if (st == DatasourceType.AzureTable)
                    {
                    ctc = csa.CreateCloudTableClient();
                    if (ctc == null)
                        return null;
                    }
                else if (st == DatasourceType.AzureBlob)
                    {
                    cbc = csa.CreateCloudBlobClient();
                    if (cbc == null)
                        return null;
                    }
                
                }
            catch
                {
                return null;
                }

            try
                {
                if (st == DatasourceType.AzureTable)
                    {
                    IEnumerable<CloudTable> plct = ctc.ListTables();

                    foreach (CloudTable ct in plct)
                        {
                        pls.Add(ct.Name);
                        }
                    }
                else if (st == DatasourceType.AzureBlob)
                    {
                    IEnumerable<CloudBlobContainer> plcbc = cbc.ListContainers("");

                    foreach (CloudBlobContainer cbContainer in plcbc)
                        {
                        pls.Add(cbContainer.Uri.ToString());
                        }
                    }
                }
            catch
                {
                return null;
                }

            return pls;
        }

        /* O P E N  A C C O U N T */
        /*----------------------------------------------------------------------------
        	%%Function: OpenAccount
        	%%Qualified: AzLog.AzLogModel.OpenAccount
        	%%Contact: rlittle
        	
            Open the given Azure account and populate the list of tables that we 
            know about
        ----------------------------------------------------------------------------*/
        public void OpenAccount(string sAccountName, string sAccountKey, DatasourceType st)
        {
            m_plsContainers = PlsNames(sAccountName, sAccountKey, st);
        }

        public void Close()
        {
        }
        #endregion
    }
}