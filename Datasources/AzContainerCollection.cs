using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzLog
{
    // each AzContainer is actually a BlobContainer AND a prefix (because a container
    // might carry logs for multiple websites or applications; this allows us to differentiate
    // and to filter by a particular prefix)
    class AzContainer
    {
        private CloudBlobContainer m_cbc;
        private string m_sApplication;  // this is the log prefix

        public AzContainer(CloudBlobContainer cbc, string sLogPrefix)
        {
            m_cbc = cbc;
            m_sApplication = sLogPrefix;
        }

        public CloudBlobContainer Container => m_cbc;
        public string Name => String.Format("{0}|{1}", m_cbc.Name, m_sApplication);
        public string ApplicationName => m_sApplication;
    }

    class AzContainerCollection
    {
        private string m_sAccountName;
        private string m_sAccountKey;

        public AzContainerCollection(string sAccountName, string sAccountKey)
        {
            m_sAccountName = sAccountName;
            m_sAccountKey = sAccountKey;
        }

        private List<AzContainer> m_plazc = null;
        private CloudBlobClient m_cblc = null;
        private CloudStorageAccount m_csa = null;

        public CloudBlobClient BlobClient => m_cblc;

        public string AccountName => m_sAccountName;

        public void Close()
        {

        }

        List<string> PlsApplicationsInBlobContainer(CloudBlobContainer cbc)
        {
            IEnumerable<IListBlobItem> blobs;

            blobs = cbc.ListBlobs(null, true, BlobListingDetails.None, null, null);
            string sLocalNamePrefix = String.Format("/{0}/", cbc.Name);
            HashSet<string> hshAppNames = new HashSet<string>();

            foreach (IListBlobItem blob in blobs)
                {
                string sLocalName = blob.Uri.LocalPath;
                if (!sLocalName.StartsWith(sLocalNamePrefix))
                    throw new Exception("URI localpath doesn't begin with BlobContainer name");

                string sAppName = blob.Uri.LocalPath.Substring(sLocalNamePrefix.Length);
                sAppName = sAppName.Substring(0, sAppName.IndexOf('/'));
                if (!hshAppNames.Contains(sAppName))
                    hshAppNames.Add(sAppName);
                }
            return new List<string>(hshAppNames.ToArray());
        }
        public List<string> PlsContainerNames()
        {
            string sConnectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                                     m_sAccountName, m_sAccountKey);

            try
                {
                m_csa = CloudStorageAccount.Parse(sConnectionString);

                if (m_csa == null)
                    return null;

                m_cblc = m_csa.CreateCloudBlobClient();
                if (m_cblc == null)
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
                IEnumerable<CloudBlobContainer> olcbc = m_cblc.ListContainers();

                m_plazc = new List<AzContainer>();
                pls = new List<string>();

                foreach (CloudBlobContainer cbc in olcbc)
                    {
                    List<string> plsAppNames = PlsApplicationsInBlobContainer(cbc);
                    foreach (string sAppName in plsAppNames)
                        {
                        AzContainer azc = new AzContainer(cbc, sAppName);
                        m_plazc.Add(azc);
                        pls.Add(azc.Name);
                        }
                    }
                }
            catch
                {
                return null;
                }

            return pls;
        }

        public AzContainer GetContainer(string sContainerName)
        {
            foreach (AzContainer azc in m_plazc)
                {
                if (azc.Name == sContainerName)
                    return azc;
                }
            return null;
        }

        public static bool TestConnection(string sAccountName, string sAccountKey)
        {
            AzContainerCollection acc = new AzContainerCollection(sAccountName, sAccountKey);
            if (acc.PlsContainerNames() == null)
                return false;

            return true;
        }
    }
}