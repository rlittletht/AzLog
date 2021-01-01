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
using TCore.XmlSettings;

namespace AzLog
{
    public class AzLogAzureTable : IAzLogDatasource
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

        /*----------------------------------------------------------------------------
        	%%Function: GetSourceType
        	%%Qualified: AzLog.AzLogFile.GetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public DatasourceType GetSourceType()
        {
            return DatasourceType.AzureTable;
        }

        /*----------------------------------------------------------------------------
        	%%Function: SetSourceType
        	%%Qualified: AzLog.AzLogFile.SetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetSourceType(DatasourceType dt)
        {
            if (dt != DatasourceType.AzureTable)
                throw new Exception("cannot set datasourcetype on AzLogAzureTable - must be table");
        }
        /* S E T  D A T A S O U R C E  I N D E X */
        /*----------------------------------------------------------------------------
        	%%Function: SetDatasourceIndex
        	%%Qualified: AzLog.AzLogAzureTable.SetDatasourceIndex
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
        	%%Qualified: AzLog.AzLogAzureTable.ToString
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
        	%%Qualified: AzLog.AzLogAzureTable.GetName
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
        	%%Qualified: AzLog.AzLogAzureTable.SetName
        	%%Contact: rlittle
        	
            Allows setting of the name through the interface
        ----------------------------------------------------------------------------*/
        public void SetName(string sName)
        {
            m_sName = sName;
        }

        public static Settings.SettingsElt[] AccountSettingsDef()
        {
            return new Settings.SettingsElt[]
                       {
                           new Settings.SettingsElt("AccountKey", Settings.Type.Str, "", ""),
                           new Settings.SettingsElt("StorageType", Settings.Type.Str, "", ""),
                           new Settings.SettingsElt("StorageDomain", Settings.Type.Str, "", ""),
                       };

        }

        static XmlDescription<AzLogAzureTable> CreateXmlDescriptor()
        {
	        return XmlDescriptionBuilder<AzLogAzureTable>
		        .Build("http://www.thetasoft.com/schemas/AzLog/datasource/2020", "Datasource")
		        .DiscardAttributesWithNoSetter()
		        .DiscardUnknownAttributes()
		        .AddAttribute("type", GetDatasourceType, null)
		        .AddChildElement("AzureTableDatasource")
		        .AddChildElement("AccountName", GetAccountName, SetAccountName)
		        .AddElement("TableName", GetTableName, SetTableName);
        }

        /* S A V E */
        /*----------------------------------------------------------------------------
        	%%Function: Save
        	%%Qualified: AzLog.AzLogAzureTable.Save
        	%%Contact: rlittle
        	
            Save this datasource
        ----------------------------------------------------------------------------*/
        public void Save(Collection collection)
        {
	        XmlDescription<AzLogAzureTable> descriptor = CreateXmlDescriptor();

            if (string.IsNullOrEmpty(m_sName))
                throw new Exception("Cannot save empty datasource name");

            m_sTableName = m_azt.Table.Name;
            m_sAccountName = m_aztc.AccountName;
            
            using (WriteFile<AzLogAzureTable> writeFile = collection.CreateSettingsWriteFile<AzLogAzureTable>(m_sName))
	            writeFile.SerializeSettings(descriptor, this);
        }

        public static string GetDatasourceType(AzLogAzureTable model) => AzLogDatasourceSupport.TypeToString(DatasourceType.AzureTable);
        public static string GetAccountName(AzLogAzureTable model) => model.m_sAccountName;
        public static void SetAccountName(AzLogAzureTable model, string accountName) => model.m_sAccountName = accountName;
        public static string GetTableName(AzLogAzureTable model) => model.m_sTableName;
        public static void SetTableName(AzLogAzureTable model, string tableName) => model.m_sTableName = tableName;
        
        /* F  L O A D */
        /*----------------------------------------------------------------------------
        	%%Function: FLoad
        	%%Qualified: AzLog.AzLogAzureTable.FLoad
        	%%Contact: rlittle
        	
            Load the information about this datasource, but don't actually open it
            (doesn't ping the net or validate information)
        ----------------------------------------------------------------------------*/
        public bool FLoad(AzLogModel azlm, Collection.FileDescription file)
        {
	        XmlDescription<AzLogAzureTable> descriptor = CreateXmlDescriptor();

            ReadFile<AzLogAzureTable> readFile = Collection.CreateSettingsReadFile<AzLogAzureTable>(file);

	        readFile.DeSerialize(descriptor, this);

	        m_sName = file.Name;
	        
	        return true;
        }

        /* F  O P E N */
        /*----------------------------------------------------------------------------
        	%%Function: FOpen
        	%%Qualified: AzLog.AzLogAzureTable.FOpen
        	%%Contact: rlittle
        	
            Actually open the datasource, connecting to the source
        ----------------------------------------------------------------------------*/
        public bool FOpen(AzLogModel azlm, string sRegRoot)
        {
            try
            {
                OpenAccount(m_sAccountName, GetAccountKey(sRegRoot, m_sAccountName));
                OpenContainer(azlm, m_sTableName);
            }
            catch (Exception exc)
            {
                MessageBox.Show(String.Format("Couldn't open azure datasource {0}: {1}", m_sName, exc.Message));
                return false;
            }
            return true;
        }

        /*----------------------------------------------------------------------------
        	%%Function: FGetMinMacDateTime
        	%%Qualified: AzLog.AzLogAzureTable.FGetMinMacDateTime
        	
            we cannot automatically detect date range (yet). return false.
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
        public static AzLogAzureTable LoadAzureDatasource(AzLogModel azlm, Collection.FileDescription file)
        {
            AzLogAzureTable azla = new AzLogAzureTable();

            if (azla.FLoad(azlm, file))
                return azla;

            return null;
        }
        #endregion

        #region Account / Table Support

        /* O P E N  T A B L E */
        /*----------------------------------------------------------------------------
        	%%Function: OpenSource
        	%%Qualified: AzLog.AzLogModel.OpenSource
        	%%Contact: rlittle
        	
            clear all of the views since we are now opening a new table.
        ----------------------------------------------------------------------------*/
        public void OpenContainer(AzLogModel azlm, string sTableName)
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
                int iFirst, iLast;

                azlm.AddSegment(azleSegment, out iFirst, out iLast);

                azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Complete);
                }
            foreach (AzLogView azlv in azlm.Listeners)
                azlv.CompleteAsyncData();

            return true;
        }

    }
}