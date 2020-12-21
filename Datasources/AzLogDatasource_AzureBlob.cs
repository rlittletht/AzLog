using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TCore.Settings;

//
// AzureBlobs are a lot like text file logs, but we have a container that contains the logs (and
// presumably just contains logs).
//
// The Datasource will be the AzureBlob container. We will query the container for the blobs
// that are available and will assume a filename format like:
//      [AppName]/[YYYY/MM/DD]/HH/[InstanceID]-[ProcessID].applicationLog.csv
//
// The key for us to be able to fetch the appropriate logs is to fetch everything that matches the date/hour
// format. we will download these files [FUTURE: cache those files locally using some sort of checksum?],
// then parse those files into our log viewer entries

// in order to accommodate changing logs on the server, we will not cache the list of blob names
// in the container and will refetch them [FUTURE: we could cache the ones before today since things
// in the past shoudln't be changing]

namespace AzLog
{
    public class AzLogAzureBlob : IAzLogDatasource
    {
        private AzContainer m_azc = null;

        private AzContainerCollection m_azcc;
        private List<string> _mPlsContainers;

        private string m_sName;
        private string m_sAccountName;
        private string m_sContainerName;
        private int m_iDatasource;  // what is our iDatasource (for updating partitions, etc.)

        public List<string> Containers => _mPlsContainers;

        #region IAzLogDatasource implementation

        /*----------------------------------------------------------------------------
        	%%Function: GetSourceType
        	%%Qualified: AzLog.AzLogFile.GetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public DatasourceType GetSourceType()
        {
            return DatasourceType.AzureBlob;
        }

        /*----------------------------------------------------------------------------
        	%%Function: SetSourceType
        	%%Qualified: AzLog.AzLogFile.SetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetSourceType(DatasourceType dt)
        {
            if (dt != DatasourceType.AzureBlob)
                throw new Exception("cannot set datasourcetype on AzLogAzureBlob - must be blob");
        }

        /* S E T  D A T A S O U R C E  I N D E X */
        /*----------------------------------------------------------------------------
        	%%Function: SetDatasourceIndex
        	%%Qualified: AzLog.AzLogAzureBlob.SetDatasourceIndex
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
        	%%Qualified: AzLog.AzLogAzureBlob.ToString
        	%%Contact: rlittle
        	
            This is the display name (and the comparison identifier) for this
            datasource
        ----------------------------------------------------------------------------*/
        public override string ToString()
        {
            return String.Format("{0} [AzureBlob]", m_sName);
        }

        /* G E T  N A M E */
        /*----------------------------------------------------------------------------
        	%%Function: GetName
        	%%Qualified: AzLog.AzLogAzureLob.GetName
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
        	%%Qualified: AzLog.AzLogAzureBlob.SetName
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
            new Settings.SettingsElt("Container", Settings.Type.Str, "", ""),
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
        	%%Qualified: AzLog.AzLogAzureBlob.Save
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

            ste.SetSValue("AccountName", m_azcc.AccountName);
            ste.SetSValue("Container", m_azc.Name);
            ste.SetSValue("Type", AzLogDatasourceSupport.TypeToString(DatasourceType.AzureBlob));
            ste.Save();
        }

        /* F  L O A D */
        /*----------------------------------------------------------------------------
        	%%Function: FLoad
        	%%Qualified: AzLog.AzLogAzureBlob.FLoad
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
            m_sContainerName = ste.SValue("Container");
            m_sName = sName;

            return true;
        }

        /* F  O P E N */
        /*----------------------------------------------------------------------------
        	%%Function: FOpen
        	%%Qualified: AzLog.AzLogAzureBlob.FOpen
        	%%Contact: rlittle
        	
            Actually open the datasource, connecting to the source
        ----------------------------------------------------------------------------*/
        public bool FOpen(AzLogModel azlm, string sRegRoot)
        {
            try
                {
                OpenAccount(m_sAccountName, GetAccountKey(sRegRoot, m_sAccountName));
                OpenContainer(azlm, m_sContainerName);
                }
            catch (Exception exc)
                {
                MessageBox.Show(String.Format("Couldn't open azure datasource {0}: {1}", m_sName, exc.Message));
                return false;
                }
            return true;
        }

        /*----------------------------------------------------------------------------
        	%%Function: OpenContainer
        	%%Qualified: AzLog.AzLogAzureBlob.OpenContainer
        	%%Contact: rlittle
        	
            clear all of the views since we are now opening a new blob.
        ----------------------------------------------------------------------------*/
        public void OpenContainer(AzLogModel azlm, string sContainerName)
        {
            if (azlm != null)
                {
                foreach (AzLogView azlv in azlm.Listeners)
                    azlv.ClearLog();
                }

            m_azc = m_azcc.GetContainer(sContainerName);
        }

        /*----------------------------------------------------------------------------
        	%%Function: FGetMinMacDateTime
        	%%Qualified: AzLog.AzLogAzureBlob.FGetMinMacDateTime
        	
        ----------------------------------------------------------------------------*/
        public bool FGetMinMacDateTime(AzLogModel azlm, out DateTime dttmMin, out DateTime dttmMax)
        {
            dttmMin = DateTime.MaxValue;
            dttmMax = DateTime.MinValue;

            return false;
        }
        #endregion

        #region Static helpers
        /* G E T  A C C O U N T  K E Y */
        /*----------------------------------------------------------------------------
            %%Function: GetAccountKey
            %%Qualified: AzLog.AzLogAzureBlob.GetAccountKey
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
        	%%Qualified: AzLog.AzLogAzureBlob.LoadAzureDatasource
        	%%Contact: rlittle
        	
            Loads this azure datasource
        ----------------------------------------------------------------------------*/
        public static AzLogAzureBlob LoadAzureDatasource(AzLogModel azlm, string sRegRoot, string sName)
        {
            AzLogAzureBlob azla = new AzLogAzureBlob();

            if (azla.FLoad(azlm, sRegRoot, sName))
                return azla;

            return null;
        }
        #endregion

        #region Account / Blob Support
        /* O P E N  A C C O U N T */
        /*----------------------------------------------------------------------------
        	%%Function: OpenAccount
        	%%Qualified: AzLog.AzLogModel.OpenAccount
        	%%Contact: rlittle
        	
            Open the given Azure account and populate the list of containers that we 
            know about
        ----------------------------------------------------------------------------*/
        public void OpenAccount(string sAccountName, string sAccountKey)
        {
            m_azcc = new AzContainerCollection(sAccountName, sAccountKey);
            _mPlsContainers = m_azcc.PlsContainerNames();
        }

        public void Close()
        {
            m_azcc.Close();
        }
        #endregion

        private List<Uri> m_pluriAvailableBlobs;

        List<Uri> GetBlobList(DateTime dttm)
        {
            List<Uri> pluri = new List<Uri>();

            IEnumerable<IListBlobItem> blobs;

            string sPrefix = String.Format("{0}/{1}", m_azc.ApplicationName, dttm.ToString("yyyy/MM/dd/HH"));

            blobs = m_azc.Container.ListBlobs(sPrefix, true, BlobListingDetails.None, null, null);

            foreach (IListBlobItem blob in blobs)
                {
                pluri.Add(blob.Uri);
                }
            return pluri;
        }
        
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
            List<Uri> pluri = GetBlobList(dttm);

            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Pending);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.BeginAsyncData();

            foreach (Uri uri in pluri)
            {
                ICloudBlob icb = m_azcc.BlobClient.GetBlobReferenceFromServer(uri);
                string sTempFile = TCore.Util.Filename.SBuildTempFilename(null, null);

                icb.DownloadToFile(sTempFile, FileMode.Create);
                // at this point, we have the file

            }

            // now we can download the blobs -- create CloudBlob for each URI, then DownloadToFile...
            // need to do this async....
#if no
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
                azleSegment = await m_azc.Container.ExecuteQuerySegmentedAsync(tq, azleSegment?.ContinuationToken);
                int iFirst, iLast;

                azlm.AddSegment(azleSegment, out iFirst, out iLast);

                azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Complete);
                }
            foreach (AzLogView azlv in azlm.Listeners)
                azlv.CompleteAsyncData();
#endif
            return true;
        }

    }
}