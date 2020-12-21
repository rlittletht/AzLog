using System;
using System.Collections.Generic;
using System.Drawing.Design;
using AzLog;
using NUnit.Framework;


namespace AzLog
{
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
            private AzLogEntry.LogColumn m_lcCopy; // this allows us to parse the column into two places
            private Sep m_sep;
            private int m_cch; // for fixed width

            public Sep Separator
            {
                get { return m_sep; }
                set { m_sep = value; }
            }

            public int Cch
            {
                get { return m_cch; }
                set { m_cch = value; }
            }

            public AzLogEntry.LogColumn Column
            {
                get { return m_lc; }
                set { m_lc = value; }
            }

            public AzLogEntry.LogColumn ColumnCopy
            {
                get { return m_lcCopy; }
                set { m_lcCopy = value; }
            }

            public TextLogColumn()
            {
                m_lcCopy = m_lc = AzLogEntry.LogColumn.Nil;
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

        /* F  P A R S E  F I X E D  W I D T H  C O L U M N */
        /*----------------------------------------------------------------------------
            %%Function: FParseFixedWidthColumn
            %%Qualified: AzLog.AzLogFile.TextLogConverter.FParseFixedWidthColumn
            %%Contact: rlittle
    
        ----------------------------------------------------------------------------*/
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
                    dttm = dttm.ToUniversalTime();

                    azle.SetColumn(AzLogEntry.LogColumn.Partition, dttm.ToString("yyyyMMddHH"));
                    azle.EventTickCount = dttm.Ticks + nLine;
                    if (tlc.ColumnCopy != AzLogEntry.LogColumn.Nil)
                        azle.SetColumn(tlc.ColumnCopy, dttm.ToString("MM/dd/yyyy HH:mm:ss"));

                    // and manufacture a partition as well
                    azle.SetColumn(AzLogEntry.LogColumn.Partition, AzLogModel.SPartitionFromDate(dttm, dttm.Hour));
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
//                    if (ilc < m_pltlc.Count)
//                        return null;
                    break;
                    }
                }

            // 
            return azle;
        }

        static int ParseColumn(string sConfig, int ichFirst, ref int ichNext, int ichMac)
        {
            if (sConfig[ichNext] == '`')
            {
                // `LogColumn` is a named version for the column. convert this to a physical index
                ichNext++;
                while (ichNext < ichMac && sConfig[ichNext] != '`')
                    ichNext++;

                if (sConfig[ichNext] != '`')
                    throw new Exception("bad config format - no matching ` for column name");

                string sColumnName = sConfig.Substring(ichFirst + 1, ichNext - ichFirst - 1);
                ichNext++;
                if (ichNext >= ichMac)
                    throw new Exception("bad config format -- no terminating separator");

                return (int) AzLogEntry.GetColumnIndexByName(sColumnName);
            }

            while (ichNext < ichMac && char.IsDigit(sConfig[ichNext]))
                ichNext++;

            if (ichNext >= ichMac)
                throw new Exception("bad config format -- no terminating separator");

            return int.Parse(sConfig.Substring(ichFirst, ichNext - ichFirst));
        }

        /*----------------------------------------------------------------------------
            %%Function: CreateFromConfig
            %%Qualified: AzLog.TextLogConverter.CreateFromConfig
            %%Contact: rlittle

            Config language:

            This config defines how to parse a text line into an AzLogEntry, which
            has a set of predefined columns, as well as custom columns. See
            AzLogEntry.LogColummn for the set of constants we are mapping to.

            Column definition:

            [0-9]+  this is the column we are mapping to
            ["+"]   an optional "+" character, which means this column is copied to
                    another builtin column:
                    [0-9]+  the other column we are mapping to
            [,:t?]  what is the separator character
                    , means its a comma
                    : means its a colon
                    t means its \t
                    ? means we have a fixed width, with the width following:
                    [0-9]+  the fixed width, followed by a space (to terminate the
                            width)
        ----------------------------------------------------------------------------*/
        public static TextLogConverter CreateFromConfig(string sConfig)
        {
            TextLogConverter tlc = new TextLogConverter();

            int ich, ichNext;
            int ichMac = sConfig.Length;

            ich = 0;
            while (ich < ichMac)
            {
                ichNext = ich;
                int nCol = -1;

                nCol = ParseColumn(sConfig, ich, ref ichNext, ichMac);

                int nColCopy = -1;

                if (sConfig[ichNext] == '+') // this means they want to copy this field to another column too
                {
                    int ichFirstCopy = ++ichNext;

                    nColCopy = ParseColumn(sConfig, ichFirstCopy, ref ichNext, ichMac);
#if no
                    while (ichNext < ichMac && char.IsDigit(sConfig[ichNext]))
                        ichNext++;

                    if (ichNext >= ichMac)
                        throw new Exception("bad config format -- no terminating separator after column copy");

                    nColCopy = int.Parse(sConfig.Substring(ichFirstCopy, ichNext - ichFirstCopy));
#endif // no
                }

                char chSep = sConfig[ichNext];
                TextLogColumn tlcc = new TextLogColumn();
                tlcc.Column = (AzLogEntry.LogColumn)nCol;
                if (nColCopy != -1)
                    tlcc.ColumnCopy = (AzLogEntry.LogColumn)nColCopy;

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

#pragma warning disable format // @formatter:off — disable formatter after this line
        //                                                   0    1      2     3     4     5    6     7     8     9     10    11     12   13    14    15    16    17    18   19  
        //                                                  Part  Row   Tick  App   Level EvID  Inst  Pid   Tid   MsgR  Msg0  Msg1  Msg2  Msg3  Msg4  Msg5  Msg6  Msg7  Msg8  Msg9
        [TestCase("`AppName`t", "AppName", null, null, null, "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`t", " AppName", null, null, null, "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`t", "  AppName  ", null, null, null, "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

        [TestCase("`Message9`t", "AppName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "AppName")]

        [TestCase("`AppName`t`Level`t", "AppName\tInformation", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`t`Level`t", " AppName\tInformation", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`t`Level`t", " AppName \tInformation", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`t`Level`t", " AppName \t Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`t`Level`t", " AppName \t Information ", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

        [TestCase("`AppName`,`Level`,", "AppName,Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`,`Level`,", " AppName,Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`,`Level`,", " AppName ,Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`,`Level`,", " AppName , Information", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`,`Level`,", " AppName , Information ", null, null, null, "AppName", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`,`Level`,", " \"App,Name\" , Information ", null, null, null, "App,Name", "Information", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

        [TestCase("`AppName`?2 ", "AppName", null, null, null, "Ap", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]
        [TestCase("`AppName`?2 `Level`,", "AppName", null, null, null, "Ap", "pName", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)]

        [TestCase("`Nil`?6 `Level`:`Message1`:`Message0`t`Tid`t`Message2`t`Message3`t`Message4`t", "TcRec Information: -30257 : cc6689cf-f523-4495-9762-c110ef99da32\t13\t0A7AF47C\t10/26/2015 18:12:23\tOnPacketReceived",
            null, null, null, null, "Information", null, null, null, "13", null, "cc6689cf-f523-4495-9762-c110ef99da32", "-30257", "0A7AF47C", "10/26/2015 18:12:23",
            "OnPacketReceived", null, null, null, null, null)]

        [TestCase("`Nil`?6 `Level`:`Message1`:`Message0`t`Tid`t`Message2`t`EventTickCount`+`Message3`t`Message4`t", "TcRec Information: -30257 : cc6689cf-f523-4495-9762-c110ef99da32\t13\t0A7AF47C\t10/26/2015 18:12:23\tOnPacketReceived",
            "2015102701", null, "635815051430000000", null, "Information", null, null, null, "13", null, "cc6689cf-f523-4495-9762-c110ef99da32", "-30257", "0A7AF47C",
            "10/27/2015 01:12:23", "OnPacketReceived", null, null, null, null, null)]

        [TestCase("`EventTickCount`,", "10/26/2015 18:12:23", "2015102701", null, "635815051430000000", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null)]

#if NoDirectIndex
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
            null, null, null, null, "Information", null, null, null, "13", null, "cc6689cf-f523-4495-9762-c110ef99da32", "-30257", "0A7AF47C", "10/26/2015 18:12:23",
            "OnPacketReceived", null, null, null, null, null)]
        [TestCase("20?6 4:11:10t8t12t2+13t14t", "TcRec Information: -30257 : cc6689cf-f523-4495-9762-c110ef99da32\t13\t0A7AF47C\t10/26/2015 18:12:23\tOnPacketReceived",
            "2015102701", null, "635815051430000000", null, "Information", null, null, null, "13", null, "cc6689cf-f523-4495-9762-c110ef99da32", "-30257", "0A7AF47C",
            "10/27/2015 01:12:23", "OnPacketReceived", null, null, null, null, null)]
        [TestCase("2,", "10/26/2015 18:12:23", "2015102701", null, "635815051430000000", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null)]
#endif // NoDirectIndex

#pragma warning restore format // @formatter:on — disable formatter after this line
        [Test]
        public static void TestParseLine(string sConfig, string sLine, string sPartitionExpected, string sRowKeyExpected, string sTickCountExpected, string sAppNameExpected,
            string sLevelExpected, string sEventIDExpected, string sInstanceIDExpected, string sPidExpected, string sTidExpected, string sMessageRawExpected,
            string sMessage0Expected,
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
}