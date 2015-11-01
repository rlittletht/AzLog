using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzLog
{
    class AzTable
    {
        private CloudTable m_act;

        public AzTable(CloudTable act)
        {
            m_act = act;
        }

        public CloudTable Table => m_act;

    }

    class AzTableCollection
    {
        private string m_sAccountName;
        private string m_sAccountKey;

        public AzTableCollection(string sAccountName, string sAccountKey)
        {
            m_sAccountName = sAccountName;
            m_sAccountKey = sAccountKey;
        }

        private List<CloudTable> m_plct = null;
        private CloudTableClient m_ctc = null;
        private CloudStorageAccount m_csa = null;

        public List<string> PlsTableNames()
        {
            string sConnectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                                     m_sAccountName, m_sAccountKey);

            try
                {
                m_csa = CloudStorageAccount.Parse(sConnectionString);

                if (m_csa == null)
                    return null;

                m_ctc = m_csa.CreateCloudTableClient();
                if (m_ctc == null)
                    return null;

                // trye to enumerate the tables
                }
            catch 
                {
                return null;
                }

            List<string> pls;
            try
                {
                IEnumerable<CloudTable> plct = m_ctc.ListTables();

                m_plct = new List<CloudTable>();
                pls = new List<string>();

                foreach (CloudTable ct in plct)
                    {
                    m_plct.Add(ct);
                    pls.Add(ct.Name);
                    }
                }    
            catch
                {
                return null;
                }

            return pls;
        }

        public AzTable GetTable(string sTableName)
        {
            foreach (CloudTable ct in m_plct)
                {
                if (ct.Name == sTableName)
                    return new AzTable(ct);
                }
            return null;
        }

        public static bool TestConnection(string sAccountName, string sAccountKey)
        {
            AzTableCollection azt = new AzTableCollection(sAccountName, sAccountKey);
            if (azt.PlsTableNames() == null)
                return false;

            return true;
        }
    }
}