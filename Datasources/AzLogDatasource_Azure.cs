﻿using System;
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
        	%%Qualified: AzLog.AzLogAzureTable.Save
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
            ste.SetSValue("Type", AzLogDatasourceSupport.TypeToString(DatasourceType.AzureTable));
            ste.Save();
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
            m_sTableName = ste.SValue("TableName");
            m_sName = sName;

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
        public static AzLogAzureTable LoadAzureDatasource(AzLogModel azlm, string sRegRoot, string sName)
        {
            AzLogAzureTable azla = new AzLogAzureTable();

            if (azla.FLoad(azlm, sRegRoot, sName))
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