using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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

            m_tlc = TextLogConverter.CreateFromConfig(ste.SValue("LogTextFormat"));

            return true;
        }

        private TextLogConverter m_tlc;

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

        class TextLogConverter
        {
            // AzLogEntry has a number of predefined columns (defined by the Azure diagnostics
            // schema) as well as a raw message column which is further split into szMessage0...szMessage9)

            // we have to map how to parse a line of text into an AzLogEntry
            // each column will define how it separates itself from the next column

            public class TextLogColumn
            {
                public enum Sep
                {
                    Colon = 0,
                    Comma = 1,
                    Tab = 2,
                    FixedWidth = 3
                }

                private static char[] mpsepch = {':', ',', '\t', ' '};

                public static char CharFromSep(Sep sep)
                {
                    return mpsepch[(int) sep];
                }

                private AzLogEntry.LogColumn m_lc;
                private Sep m_sep;
                private int m_cch; // for fixed width

                public Sep Separator {
                    get { return m_sep; }
                    set { m_sep = value; }
                }

                public int Cch
                {
                    get { return m_cch; }
                    set { m_cch = value; }
                }

                public AzLogEntry.LogColumn Column {
                    get { return m_lc; }
                    set { m_lc = value; }
                }

            }

            private List<TextLogColumn> m_pltlc;

            public TextLogConverter()
            {
                m_pltlc = new List<TextLogColumn>();
            }

            /* F  P A R S E  C O M M A  C O L U M N */
            /*----------------------------------------------------------------------------
            	%%Function: FParseCommaColumn
            	%%Qualified: AzLog.AzLogFile.TextLogConverter.FParseCommaColumn
            	%%Contact: rlittle
            	
                on entry, ich points to the first non-space character of the column
                (which might be a quote)

                on exit, ichEnd will point to the matching quote character; ichLast will
                point to the character before the next comma separator.

                return value is true if we should continue parsing this line; else false
                to fail
            ----------------------------------------------------------------------------*/
            private static bool FParseQuotedCommaColumn(string sLine, int ich, ref int ichLast, ref int ichEnd, int ichMac, bool fLastColumn)
            {
                if (sLine[ich] == '"')
                    {
                    // need to find the matching quote
                    ichLast = ich + 1;
                    while (ichLast < ichMac)
                        {
                        ichLast = sLine.IndexOf('"', ichLast);
                        if (ichLast == -1)
                            return false; // no matching quote

                        if (ichLast + 1 >= ichMac || sLine[ichLast + 1] != '"')
                            break;
                        // we encoutered a double quote in the column -- skip it looking for the real terminating quote
                        ichLast += 2;
                        }
                    // at this point ichLim points to the terminating quote
                    ichEnd = ichLast;
                    // and now find the terminating comma
                    if (fLastColumn)
                        ichLast = sLine.Length - 1;
                    else
                        {
                        ichLast = sLine.IndexOf(",", ichLast);
                        if (ichLast == -1)
                            return false; // couldn't find separator
                        ichLast--;
                        }
                    }
                return true;
            }

            private static bool FParseFixedWidthColumn(string sLine, int cch, int ich, ref int ichLast, ref int ichEnd, int ichMac, bool fLastColumn)
            {
                if (ich + cch >= ichMac)
                    return false;

                ichEnd = ich + cch - 1;
                ichLast = ich + cch;

                return true;
            }

            /* P A R S E  L I N E */
            /*----------------------------------------------------------------------------
            	%%Function: ParseLine
            	%%Qualified: AzLog.AzLogFile.TextLogConverter.ParseLine
            	%%Contact: rlittle
            	
            ----------------------------------------------------------------------------*/
            public AzLogEntry ParseLine(string sLine, int nLine)
            {
                AzLogEntry azle = new AzLogEntry();

                azle.InitMessageParts(10);

                int ilc = 0;
                int ich = 0;
                int ichLast = 0;

                int ichMac = sLine.Length;

                while (ilc < m_pltlc.Count)
                    {
                    ichLast = 0;

                    TextLogColumn tlc = m_pltlc[ilc];

                    // by definition we are already positioned at the beginning of this column except for possible whitespace
                    // skip leading whitespace exc
                    while (ich < ichMac && sLine[ich] == ' ')
                        ich++;

                    if (ich >= ichMac)
                        {
                        if (ilc < m_pltlc.Count)
                            return null;
                        break;
                        }
                    int ichEnd = 0;

                    // look for the separator
                    if (tlc.Separator == TextLogColumn.Sep.Comma)
                        {
                        if (!FParseQuotedCommaColumn(sLine, ich, ref ichLast, ref ichEnd, ichMac, ilc + 1 == m_pltlc.Count))
                            return null;

                        // else, we'll calculate ichLim below
                        }
                    else if (tlc.Separator == TextLogColumn.Sep.FixedWidth)
                        {
                        if (!FParseFixedWidthColumn(sLine, tlc.Cch, ich, ref ichLast, ref ichEnd, ichMac, ilc + 1 == m_pltlc.Count))
                            return null;
                        }

                    if (ichLast == 0)
                        {
                        if (ilc + 1 == m_pltlc.Count)
                            {
                            ichEnd = ichLast = sLine.Length - 1;
                            }
                        else
                            {
                            char chSep = TextLogColumn.CharFromSep(tlc.Separator);
                            ichLast = sLine.IndexOf(chSep, ich);
                            if (ichLast == -1)
                                return null; // couldn't find the separator for this column
                            ichLast--; // we want this to point to the last char, not to the separator
                            ichEnd = ichLast;
                            }
                        }
                    // at this point we have ich pointing to start of string; ichEnd pointing to last char before the separator (maybe a quote),
                    // at ichLast pointing to just before the separator (never a quote)
                    //, lets trim the string
                    int ichFirst = ich;
                    

                    if (sLine[ichFirst] == '"' && sLine[ichEnd] == '"')
                        {
                        ichFirst++;
                        ichEnd--;
                        }

                    while (ichEnd > ichFirst && sLine[ichEnd] == ' ')
                        ichEnd--;

                    // now we have the string
                    if (tlc.Column == AzLogEntry.LogColumn.EventTickCount)
                        {
                        DateTime dttm = DateTime.Parse(sLine.Substring(ichFirst, ichEnd - ichFirst + 1));
                        azle.SetColumn(AzLogEntry.LogColumn.Partition, dttm.ToString("yyyyMMddHH"));
                        azle.EventTickCount = dttm.Ticks + nLine;
                        }
                    else
                        {
                        azle.SetColumn(tlc.Column, sLine.Substring(ichFirst, ichEnd - ichFirst + 1));
                        }
                    if (tlc.Separator == TextLogColumn.Sep.FixedWidth)
                        ich = ichLast;
                    else
                        ich = ichLast + 2;
                    ilc++;
                    if (ich >= ichMac)
                        {
                        if (ilc < m_pltlc.Count)
                            return null;
                        break;
                        }
                    }

                // 
                return azle;
            }
            
            #region Unit Tests
            [TestCase("\"test\"", 0, true, 5, 5, true)]
            [TestCase("\"test\",\"test\"", 7, true, 12, 12, true)]
            [TestCase("\"t,st\"", 0, true, 5, 5, true)]
            [TestCase("\"test \"", 0, true, 6, 6, true)]
            [TestCase("\"t\"\"t \"", 0, true, 6, 6, true)]
            [TestCase("\"test \" ", 0, true, 7, 6, true)]
            [TestCase("\"test\", test", 0, false, 5, 5, true)]
            [TestCase("\"test\" , test", 0, false, 6, 5, true)]
            [TestCase("\"t,st\" , test", 0, false, 6, 5, true)]
            [Test]
            public static void TestParseQuotedCommaColumn(string sLine, int ichStart, bool fLastColumn, int ichLastExpected, int ichEndExpected, bool fExpected)
            {
                int ichEnd = 0;
                int ichLast = 0;

                bool fResult = FParseQuotedCommaColumn(sLine, ichStart, ref ichLast, ref ichEnd, sLine.Length, fLastColumn);
                Assert.AreEqual(ichLastExpected, ichLast);
                Assert.AreEqual(ichEndExpected, ichEnd);
                Assert.AreEqual(fExpected, fResult);
            }

            public static TextLogConverter CreateFromConfig(string sConfig)
            {
                TextLogConverter tlc = new TextLogConverter();

                int ich, ichNext;
                int ichMac = sConfig.Length;

                ich = 0;
                while (ich < ichMac)
                    {
                    ichNext = ich;
                    while (ichNext < ichMac && char.IsDigit(sConfig[ichNext]))
                        ichNext++;

                    if (ichNext >= ichMac)
                        throw new Exception("bad config format -- no terminating separator");

                    int nCol = int.Parse(sConfig.Substring(ich, ichNext - ich));

                    char chSep = sConfig[ichNext];
                    TextLogColumn tlcc = new TextLogColumn();
                    tlcc.Column = (AzLogEntry.LogColumn) nCol;
                    if (chSep == ',')
                        tlcc.Separator = TextLogColumn.Sep.Comma;
                    else if (chSep == ':')
                        tlcc.Separator = TextLogColumn.Sep.Colon;
                    else if (chSep == 't')
                        tlcc.Separator = TextLogColumn.Sep.Tab;
                    else if (chSep == '?')
                        {
                        tlcc.Separator = TextLogColumn.Sep.FixedWidth;
                        
                        int ichFirst = ++ichNext;

                        // the next digits are the cch
                        while (ichNext < ichMac && char.IsDigit(sConfig[ichNext]))
                            ichNext++;

                        if (ichFirst >= ichNext)
                            throw new Exception("bad config format -- no length for fixed width column");

                        if (sConfig[ichNext] != ' ')
                            throw new Exception("bad config format -- fixed width column width was not terminated with a space");

                        tlcc.Cch = int.Parse(sConfig.Substring(ichFirst, ichNext - ichFirst));
                        }
                    else
                        {
                        throw new Exception("bad config format. unknown separator");
                        }

                    tlc.m_pltlc.Add(tlcc);
                    ich = ichNext + 1;
                    }

                return tlc;
            }

            static private void AssertAzleEqual(AzLogEntry azle, AzLogEntry azleExpected)
            {
                Assert.AreEqual(azleExpected.Partition, azle.Partition);
                Assert.AreEqual(azleExpected.RowKey, azle.RowKey);
                Assert.AreEqual(azleExpected.EventTickCount, azle.EventTickCount);
                Assert.AreEqual(azleExpected.ApplicationName, azle.ApplicationName);
                Assert.AreEqual(azleExpected.Level, azle.Level);
                Assert.AreEqual(azleExpected.EventId, azle.EventId);
                Assert.AreEqual(azleExpected.InstanceId, azle.InstanceId);
                Assert.AreEqual(azleExpected.Pid, azle.Pid);
                Assert.AreEqual(azleExpected.Tid, azle.Tid);
                Assert.AreEqual(azleExpected.Message, azle.Message);
                Assert.AreEqual(azleExpected.Message0, azle.Message0);
                Assert.AreEqual(azleExpected.Message1, azle.Message1);
                Assert.AreEqual(azleExpected.Message2, azle.Message2);
                Assert.AreEqual(azleExpected.Message3, azle.Message3);
                Assert.AreEqual(azleExpected.Message4, azle.Message4);
                Assert.AreEqual(azleExpected.Message5, azle.Message5);
                Assert.AreEqual(azleExpected.Message6, azle.Message6);
                Assert.AreEqual(azleExpected.Message7, azle.Message7);
                Assert.AreEqual(azleExpected.Message8, azle.Message8);
                Assert.AreEqual(azleExpected.Message9, azle.Message9);
            }
            //                                                   0    1      2     3     4     5    6     7     8     9     10    11     12   13    14    15    16    17    18   19  
            //                                                  Part  Row   Tick  App   Level EvID  Inst  Pid   Tid   MsgR  Msg0  Msg1  Msg2  Msg3  Msg4  Msg5  Msg6  Msg7  Msg8  Msg9
            [TestCase("3t", "AppName", null, null, null, "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3t", " AppName", null, null, null, "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3t", "  AppName  ", null, null, null, "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

            [TestCase("19t", "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "AppName")]

            [TestCase("3t4t", "AppName\tInformation", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3t4t", " AppName\tInformation", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3t4t", " AppName \tInformation", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3t4t", " AppName \t Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3t4t", " AppName \t Information ", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

            [TestCase("3,4,", "AppName,Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3,4,", " AppName,Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3,4,", " AppName ,Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3,4,", " AppName , Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3,4,", " AppName , Information ", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3,4,", " \"App,Name\" , Information ", null, null, null, "App,Name", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

            [TestCase("3?2 ", "AppName", null, null, null, "Ap", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [TestCase("3?2 4,", "AppName", null, null, null, "Ap", "pName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

            [TestCase("20?6 4:11:10t8t12t13t14t", "TcRec Information: -30257 : cc6689cf-f523-4495-9762-c110ef99da32\t13\t0A7AF47C\t10/26/2015 18:12:23\tOnPacketReceived",
                null, null, null, null, "Information", null, null, null, "13", null, "cc6689cf-f523-4495-9762-c110ef99da32", "-30257", "0A7AF47C", "10/26/2015 18:12:23", "OnPacketReceived", null,null,null,null,null)]

            [TestCase("2,", "10/26/2015 18:12:23", "2015102618", null, "635814799430000000", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
            [Test]
            public static void TestParseLine(string sConfig, string sLine, string sPartitionExpected, string sRowKeyExpected, string sTickCountExpected, string sAppNameExpected,
                string sLevelExpected, string sEventIDExpected, string sInstanceIDExpected, string sPidExpected, string sTidExpected, string sMessageRawExpected, string sMessage0Expected,
                string sMessage1Expected, string sMessage2Expected, string sMessage3Expected, string sMessage4Expected, string sMessage5Expected, string sMessage6Expected,
                string sMessage7Expected, string sMessage8Expected, string sMessage9Expected)
            {
                TextLogConverter tlc = TextLogConverter.CreateFromConfig(sConfig);
                AzLogEntry azle = tlc.ParseLine(sLine, 0);
                AzLogEntry azleExpected = new AzLogEntry();
                azleExpected.InitMessageParts(10);

                azleExpected.SetColumn(AzLogEntry.LogColumn.Partition, sPartitionExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.RowKey, sRowKeyExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.EventTickCount, sTickCountExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.AppName, sAppNameExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Level, sLevelExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.EventID, sEventIDExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.InstanceID, sInstanceIDExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Pid, sPidExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Tid, sTidExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message, sMessageRawExpected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message0, sMessage0Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message1, sMessage1Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message2, sMessage2Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message3, sMessage3Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message4, sMessage4Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message5, sMessage5Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message6, sMessage6Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message7, sMessage7Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message8, sMessage8Expected);
                azleExpected.SetColumn(AzLogEntry.LogColumn.Message9, sMessage9Expected);


                AssertAzleEqual(azle, azleExpected);
            }
            #endregion
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
            if (m_fDataLoaded)
                return true;

            // since they asked for this dttm, at least tell them we're pending
            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Pending);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.BeginAsyncData();

            AzLogEntry[] rgazle = new AzLogEntry[s_cChunkSize]; // do 1k log entry chunks

            using (StreamReader sr = File.OpenText(m_sFilename))
                {
                int iLine = 0;
                int cChunk = 0;
                int iFirst, iLast;

                while (!sr.EndOfStream)
                    {
                    string s = await sr.ReadLineAsync();

                    AzLogEntry azle = AzleFromLine(s, iLine++);

                    if (azle == null)
                        continue;

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
            azlm.UpdatePart(dttm, dttm.AddHours(1.0), AzLogParts.GrfDatasourceForIDatasource(m_iDatasource), AzLogPartState.Complete);

            foreach (AzLogView azlv in azlm.Listeners)
                azlv.CompleteAsyncData();

            m_fDataLoaded = true;
            return true;
        }
    }
}