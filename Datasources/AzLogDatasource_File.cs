using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TCore.Settings;
using TCore.XmlSettings;

namespace AzLog
{
    // import a text file into log entries
    class AzLogFile : IAzLogDatasource
    {
        private string m_sFilename;
        private int m_iDatasource;
        private string m_sName;
        private DateTime m_dttmFirst = DateTime.MaxValue;
        private DateTime m_dttmLast = DateTime.MinValue;

        public AzLogFile()
        {
            m_fDataLoaded = false;
        }

        #region IAzLogDatasource implementation

        /*----------------------------------------------------------------------------
        	%%Function: GetSourceType
        	%%Qualified: AzLog.AzLogFile.GetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public DatasourceType GetSourceType()
        {
            return DatasourceType.TextFile;
        }

        /*----------------------------------------------------------------------------
        	%%Function: SetSourceType
        	%%Qualified: AzLog.AzLogFile.SetSourceType
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetSourceType(DatasourceType dt)
        {
            throw new Exception("cannot set datasourcetype on LogFile - must be text");
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

        static XmlDescription<AzLogFile> CreateXmlDescription()
        {
	        return XmlDescriptionBuilder<AzLogFile>
		        .Build("http://www.thetasoft.com/schemas/AzLog/datasource/2020", "Datasource")
		        .DiscardAttributesWithNoSetter()
		        .DiscardUnknownAttributes()
		        .AddAttribute("type", null, null)
		        .AddChildElement("TextDatasource")
		        .AddChildElement("FileName", AzLogFile.GetFilename, AzLogFile.SetFilename)
		        .AddElement("LogTextFormat", null, AzLogFile.SetLogTextFormat);
        }
        
        /* S A V E */
        /*----------------------------------------------------------------------------
        	%%Function: Save
        	%%Qualified: AzLog.AzLogAzureTable.Save
        	%%Contact: rlittle
        	
            Save this datasource to the registry
        ----------------------------------------------------------------------------*/
        public void Save(Collection collection)
        {
	        XmlDescription<AzLogFile> descriptor = CreateXmlDescription();

            if (string.IsNullOrEmpty(m_sName))
                throw new Exception("Cannot save empty datasource name");

            using (WriteFile<AzLogFile> writeFile = collection.CreateSettingsWriteFile<AzLogFile>(m_sName))
	            writeFile.SerializeSettings(descriptor, this);
        }

        public static string GetDatasourceType(AzLogFile model) => AzLogDatasourceSupport.TypeToString(DatasourceType.TextFile);

        public static void SetFilename(AzLogFile azlf, string filename, RepeatContext<AzLogFile>.RepeatItemContext repeatItem) => azlf.m_sFilename = filename;
        public static string GetFilename(AzLogFile azlf, RepeatContext<AzLogFile>.RepeatItemContext repeatItem) => azlf.m_sFilename;

        public static void SetLogTextFormat(AzLogFile azlf, string format, RepeatContext<AzLogFile>.RepeatItemContext repeatItem) => azlf.m_tlc = TextLogConverter.CreateFromConfig(format);
        
        /*----------------------------------------------------------------------------
			%%Function:FLoad
			%%Qualified:AzLog.AzLogFile.FLoad

            Load the information about this datasource, but don't actually open it
            (doesn't ping the net or validate information)
        ----------------------------------------------------------------------------*/
        public bool FLoad(AzLogModel azlm, Collection.FileDescription file)
        {
	        XmlDescription<AzLogFile> descriptor = CreateXmlDescription();

	        ReadFile<AzLogFile> readFile = Collection.CreateSettingsReadFile<AzLogFile>(file);
	        
	        readFile.DeSerialize(descriptor, this);

	        m_sName = file.Name;

            return true;
        }

        public void OpenContainer(AzLogModel azlm, string sName)
        {
            throw new Exception("NYI");
        }
        private TextLogConverter m_tlc;

        /* F  O P E N */
        /*----------------------------------------------------------------------------
        	%%Function: FOpen
        	%%Qualified: AzLog.AzLogAzureTable.FOpen
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
        	%%Qualified: AzLog.AzLogAzureTable.LoadAzureDatasource
        	%%Contact: rlittle
        	
            Loads this azure datasource
        ----------------------------------------------------------------------------*/
        public static AzLogFile LoadFileDatasource(AzLogModel azlm, Collection.FileDescription file)
        {
            AzLogFile azlf = new AzLogFile();

            if (azlf.FLoad(azlm, file))
                return azlf;

            return null;
        }
        #endregion

        private bool m_fDataLoaded;
        private bool m_fDataLoading; // this means that someone has already requested and we are parsing. if they ask for another partition of data, just return since we are getting all the partitions.

        private static int s_cChunkSize = 8192;

        /* A Z L E  F R O M  L I N E */
        /*----------------------------------------------------------------------------
        	%%Function: AzleFromLine
        	%%Qualified: AzLog.AzLogFile.AzleFromLine
        	%%Contact: rlittle
        	
            We pass in nLine to retain some relationship of ordering to the original
            log file. Since we are converting Seconds into TickCounts, the level 
            of granularity is low. this means the many of the least significant digits
            of the tickcount are always zero.

            we can take advantage of that and put the line number into those low digits
            and that way log lines that have identical date/time will still get 
            differentiated by their tickcount.
        ----------------------------------------------------------------------------*/
        AzLogEntry AzleFromLine(string sLine, int nLine)
        {
            return m_tlc.ParseLine(sLine, nLine);
        }

        public bool FGetMinMacDateTime(AzLogModel azlm, out DateTime dttmMin, out DateTime dttmMac)
        {
            if (!m_fDataLoaded)
            {
                LoadCompleteModel(azlm).Wait();

                m_fDataLoaded = true;
                m_fDataLoading = false;
            }

            dttmMin = m_dttmFirst;
            dttmMac = m_dttmLast.AddDays(1);

            return true;
        }
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async Task LoadCompleteModel(AzLogModel azlm)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
            AzLogEntry[] rgazle = new AzLogEntry[s_cChunkSize]; // do 1k log entry chunks

            using (StreamReader sr = File.OpenText(m_sFilename))
            {
                int iLine = 0;
                int cChunk = 0;
                int iFirst, iLast;

                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    AzLogEntry azle = null;
                    try
                    {
                        azle = AzleFromLine(s, iLine++);
                    }
                    catch
                    {
                    }

                    if (azle == null)
                        continue;

                    DateTime dttm = AzLogModel.DttmFromPartition(azle.Partition);
                    if (m_dttmFirst > dttm)
                        m_dttmFirst = dttm;
                    if (m_dttmLast < dttm)
                        m_dttmLast = dttm;

                    rgazle[cChunk++] = azle;

                    if (cChunk >= s_cChunkSize)
                    {
                        azlm.AddSegment(rgazle, cChunk, out iFirst, out iLast);
                        cChunk = 0;
                    }
                }
                if (cChunk > 0)
                    azlm.AddSegment(rgazle, cChunk, out iFirst, out iLast);
            }
        }

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
            if (m_fDataLoaded || m_fDataLoading)
                return true;

            m_fDataLoading = true;

            // since they asked for this dttm, at least tell them we're pending
            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Pending);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.BeginAsyncData();

            await LoadCompleteModel(azlm);

            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Complete);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.CompleteAsyncData();

            m_fDataLoaded = true;
            m_fDataLoading = false;
            return true;
        }
    }
}