using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace AzLog
{
    public class AzLogAzure
    {
        private AzTable m_azt = null;

        private AzTableCollection m_aztc;
        private List<string> m_plsTables;

        public List<string> Tables => m_plsTables;

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
            foreach (AzLogView azlv in azlm.Listeners)
                azlv.ClearLog();

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
            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogPartState.Pending);

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

                azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogPartState.Complete);
            }
            return true;
        }

    }
}