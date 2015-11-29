using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;
using TCore.Settings;

namespace AzLog
{
    // import a text file into log entries
    class AzLogFile : IAzLogDatasource
    {
        private string m_sFilename;
        private int m_iDatasource;
        private string m_sName;

        public AzLogFile()
        {
            m_fDataLoaded = false;
        }

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
                new Settings.SettingsElt("FileName", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("LogTextFormat", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("Type", Settings.Type.Str, "", ""),
            };

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

            ste.SetSValue("FileName", m_sFilename);
            ste.SetSValue("Type", "TextFile");
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
            m_sFilename = ste.SValue("FileName");
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
            return true;
        }

        public void Close()
        {
            
        }
        #endregion

        #region Static helpers

        /* L O A D  A Z U R E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: LoadAzureDatasource
        	%%Qualified: AzLog.AzLogAzure.LoadAzureDatasource
        	%%Contact: rlittle
        	
            Loads this azure datasource
        ----------------------------------------------------------------------------*/
        public static AzLogFile LoadFileDatasource(AzLogModel azlm, string sRegRoot, string sName)
        {
            AzLogFile azlf = new AzLogFile();

            if (azlf.FLoad(azlm, sRegRoot, sName))
                return azlf;

            return null;
        }
        #endregion

        private bool m_fDataLoaded;

        private static int s_cChunkSize = 1024;

        /* F E T C H  P A R T I T I O N  F O R  D A T E */
        /*----------------------------------------------------------------------------
        	%%Function: FetchPartitionForDateAsync
        	%%Qualified: AzLog.AzLogModel.FetchPartitionForDateAsync
        	%%Contact: rlittle
        	
            for text files, its all or nothing (there's no query on top of the file)
            so when they ask for any data, they get all data. we will just remember
            internally if they have already been given all of our data
        ----------------------------------------------------------------------------*/
        public async Task<bool> FetchPartitionForDateAsync(AzLogModel azlm, DateTime dttm)
        {
            if (m_fDataLoaded)
                return true;

            return false;
#if no
            // since they asked for this dttm, at least tell them we're pending
            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Pending);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.BeginAsyncData();

            AzLogEntry[] rgazle = new AzLogEntry[s_cChunkSize]; // do 1k log entry chunks

            using (StreamReader sr = File.OpenText(m_sFilename))
                {
                int i = 0;

                while (!sr.EndOfStream)
                    {
                    string s = await sr.ReadLineAsync();

                    rgazle[i++] = AzleFromLine(s);

                    if (i >= s_cChunkSize)
                        {
                        int iFirst, iLast;
                        azlm.AddSegment(rgazle, i, out iFirst, out iLast);
                        }

                    }
                }
                while (azleSegment == null || azleSegment.ContinuationToken != null)
                {
                    azleSegment = await m_azt.Table.ExecuteQuerySegmentedAsync(tq, azleSegment?.ContinuationToken);
                    foreach (AzLogView azlv in azlm.Listeners)
                    {
                        lock (azlv.SyncLock)
                        {
                            int iFirst = azlm.Log.Length;

                            // TODO: Really, add the segment in the loop?!
                            azlm.AddSegment(azleSegment, out iFirst, out iLast);

                            int iLast = azlm.Log.Length;

                            azlv.AppendUpdateRegion(iFirst, iLast);
                        }
                    }

                    azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Complete);
                }
            foreach (AzLogView azlv in azlm.Listeners)
                azlv.CompleteAsyncData();

            return true;
#endif
        }
    }
}