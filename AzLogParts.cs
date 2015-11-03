using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;

namespace AzLog
{
    public enum AzLogPartState
    {
        Complete,
        Pending,
        Partial,
        Invalid
    }

    internal class AzLogPart
    {
        private DateTime m_dttmMin;
        private DateTime m_dttmMac;
        private AzLogPartState m_azlps;

        public AzLogPart() {}
        public AzLogPartState State => m_azlps;
        public DateTime DttmMin => m_dttmMin;
        public DateTime DttmMac => m_dttmMac;

        public bool Contains(DateTime dttm)
        {
            if (dttm >= m_dttmMin && dttm < m_dttmMac)
                return true;

            return false;
        }

        public static AzLogPart Create(DateTime dttmMin, DateTime dttmMac, AzLogPartState azlps)
        {
            if (dttmMin.Minute != 0 || dttmMac.Minute != 0)
                throw new Exception("all dates must be on even hours!");

            AzLogPart azlp = new AzLogPart();
            azlp.m_dttmMin = dttmMin;
            azlp.m_dttmMac = dttmMac;
            azlp.m_azlps = azlps;

            return azlp;
        }
    }


    internal class AzLogParts
    {
        // sorted list sucks for insert time, but it lets us binary search on our own to find the nearest
        // key...
        private SortedList<DateTime, AzLogPart> m_plazlp;

        public AzLogParts()
        {
            m_plazlp = new SortedList<DateTime, AzLogPart>();
        }

        int IazlpFindPart(DateTime dttmStart, out bool fMatched)
        {
            int iFirst = 0, iLim = m_plazlp.Count;
            fMatched = true;

            while (iFirst != iLim)
                {
                int iMid = iFirst + (iLim - iFirst)/2;
                int nCompare = m_plazlp.Keys[iMid].CompareTo(dttmStart);

                if (nCompare == 0)
                    return iMid;
                if (nCompare < 0)
                     iFirst = iMid + 1;
                else
                   iLim = iMid;
                }

            fMatched = false;
            return iFirst - 1;
        }

        /* G E T  P A R T  S T A T E */
        /*----------------------------------------------------------------------------
        	%%Function: GetPartState
        	%%Qualified: AzLog.AzLogParts.GetPartState
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogPartState GetPartState(DateTime dttm)
        {
            if (dttm.Minute != 0)
                throw new Exception("dates must be on hour boundaries");

            bool fMatched;
            int iazlp = IazlpFindPart(dttm, out fMatched);
            if (fMatched)
                {
                return m_plazlp.Values[iazlp].State;
                }
            else if (iazlp == -1)
                {
                return AzLogPartState.Invalid;
                }
            else
                {
                AzLogPart azlp = m_plazlp.Values[iazlp];

                if (azlp.Contains(dttm))
                    return azlp.State;

                return AzLogPartState.Invalid;
                }
        }


        /* S E T  P A R T  S T A T E */
        /*----------------------------------------------------------------------------
        	%%Function: SetPartState
        	%%Qualified: AzLog.AzLogParts.SetPartState
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetPartState(DateTime dttmMin, DateTime dttmMac, AzLogPartState azlps)
        {
            bool fMatched;
            int iazlp = IazlpFindPart(dttmMin, out fMatched);

            if (!fMatched && iazlp == -1)
                {
                // just insert a new one
                AzLogPart azlpNew = AzLogPart.Create(dttmMin, dttmMac, azlps);
                m_plazlp.Add(dttmMin, azlpNew);
                return;
                }

            AzLogPart azlp = m_plazlp.Values[iazlp];
            // does this part start *before* we do? if so, we need to split
            AzLogPart azlpPre = null;
            AzLogPart azlpReplace = null;
            AzLogPart azlpPost = null;
            bool fReplace = false;

            if (azlp.DttmMin < dttmMin)
                {
                // if it doesn't contain us, then just add a new part and be done
                if (!azlp.Contains(dttmMin))
                    {
                    AzLogPart azlpNew = AzLogPart.Create(dttmMin, dttmMac, azlps);
                    m_plazlp.Add(dttmMin, azlpNew);
                    // CAN'T JUST RETURN HERE because this new part might overlap a following part.
                    }
                else
                    {
                    fReplace = true;
                    // ok, there's some overlap.
                    azlpPre = AzLogPart.Create(azlp.DttmMin, dttmMin, azlp.State);

                    // now create the replacement part that covers what we want.
                    if (azlp.DttmMac > dttmMac)
                        {
                        // the end of the current node extends beyond what we want. need to create a replace and a post part
                        azlpReplace = AzLogPart.Create(dttmMin, dttmMac, azlps);
                        azlpPost = AzLogPart.Create(dttmMac, azlp.DttmMac, azlp.State);
                        }
                    else
                        {
                        // the end of the matched block is *not* beyond our end. create a replacement that extends to where
                        // we want
                        // NOTE!! This might create overlap with the following block. we will need to coalesce!!
                        azlpReplace = AzLogPart.Create(dttmMin, dttmMac, azlps);
                        // there is no post because we just extended
                        }
                    }
                }
            else
                {
                if (fMatched)
                    {
                    azlpReplace = AzLogPart.Create(dttmMin, dttmMac, azlps);
                    fReplace = true;
                    }
                else
                    {
                    // the matched block does *NOT* begin before us, so just create a new pre block for us, and then coalesce
                    // any following blocks
                    azlpPre = AzLogPart.Create(dttmMin, dttmMac, azlps);
                    }
                }
            // ok, do the operations
            if (fReplace)
                m_plazlp.RemoveAt(iazlp);

            if (azlpPre != null)
                m_plazlp.Add(azlpPre.DttmMin, azlpPre);

            if (azlpReplace != null)
                m_plazlp.Add(azlpReplace.DttmMin, azlpReplace);

            if (azlpPost != null)
                m_plazlp.Add(azlpPost.DttmMin, azlpPost);

            // at this point, iazlp points to at least the new item, and maybe some overlapping items. fix those (coalescing)
            CoalesceLogParts(iazlp);
        }

        /* C O A L E S C E  L O G  P A R T S */
        /*----------------------------------------------------------------------------
        	%%Function: CoalesceLogParts
        	%%Qualified: AzLog.AzLogParts.CoalesceLogParts
        	%%Contact: rlittle
        	
            coalesce
        ----------------------------------------------------------------------------*/
        private void CoalesceLogParts(int iazlp)
        {
            AzLogPart azlp, azlpNext;

            while (iazlp < m_plazlp.Count - 1)
                {
                azlp = m_plazlp.Values[iazlp];
                azlpNext = m_plazlp.Values[iazlp + 1];
                
                if (azlp.DttmMac > azlpNext.DttmMin)
                    {
                    // we overlap the next guy.  crop the next guy so he doesn't overlap us
                    AzLogPart azlpNew = null;

                    if (azlp.DttmMac < azlpNext.DttmMac)
                        {
                        // there is still some left
                        azlpNew = AzLogPart.Create(azlp.DttmMac, azlpNext.DttmMac, azlpNext.State);
                        }
                    m_plazlp.RemoveAt(iazlp + 1);
                    if (azlpNew != null)
                        m_plazlp.Add(azlpNew.DttmMin, azlpNew);
                    }
                else
                    {
                    iazlp++;
                    }
                }
        }

        #region Unit Tests

        private class CoalesceLogPartsBuilder
        {
            public int nDayMin;
            public int nMonthMin;
            public int nYearMin;
            public int nHourMin;
            public int nDayMax;
            public int nMonthMax;
            public int nYearMax;
            public int nHourMax;
            public int nState;
        }

        private static CoalesceLogPartsBuilder ParseLogPartsBuilderFromString(string sLogParts)
        {
            CoalesceLogPartsBuilder clp = new CoalesceLogPartsBuilder();
            
            int i2 = 0;
            int i2Next = sLogParts.IndexOf("/", i2);
            int i2NextT;
            i2NextT = sLogParts.IndexOf("-", i2);
            if (i2NextT < i2Next && i2NextT != -1)
                {
                // this has day and month parts too
                clp.nMonthMin = int.Parse(sLogParts.Substring(i2, i2NextT - i2));
                i2 = i2NextT + 1;
                i2NextT = sLogParts.IndexOf("-", i2);
                if (i2NextT >= i2Next)
                    throw new Exception("input string in valid format");
                clp.nDayMin = int.Parse(sLogParts.Substring(i2, i2NextT - i2));
                i2 = i2NextT + 1;
                // fallthrough
                }
            else
                {
                clp.nMonthMin = clp.nDayMin = 1;
                }
            clp.nYearMin = int.Parse(sLogParts.Substring(i2, i2Next - i2));
            i2 = i2Next + 1;
            i2Next = sLogParts.IndexOf("/", i2);
            clp.nHourMin = int.Parse(sLogParts.Substring(i2, i2Next - i2));
            i2 = i2Next + 1;
            i2Next = sLogParts.IndexOf("/", i2);
            i2NextT = sLogParts.IndexOf("-", i2);
            if (i2NextT < i2Next && i2NextT != -1)
                {
                // this has day and month parts too
                clp.nMonthMax = int.Parse(sLogParts.Substring(i2, i2NextT - i2));
                i2 = i2NextT + 1;
                i2NextT = sLogParts.IndexOf("-", i2);
                if (i2NextT >= i2Next)
                    throw new Exception("input string in valid format");
                clp.nDayMax = int.Parse(sLogParts.Substring(i2, i2NextT - i2));
                i2 = i2NextT + 1;
                // fallthrough
                }
            else
                {
                clp.nMonthMax = clp.nDayMax = 1;
                }
            clp.nYearMax = int.Parse(sLogParts.Substring(i2, i2Next - i2));
            i2 = i2Next + 1;
            i2Next = sLogParts.IndexOf("/", i2);
            clp.nHourMax = int.Parse(sLogParts.Substring(i2, i2Next - i2));
            i2 = i2Next + 1;
            i2Next = sLogParts.IndexOf("/", i2);
            clp.nState = int.Parse(sLogParts.Substring(i2, i2Next - i2));

            return clp;
        }

        [Test]
        static public void TestParseLogPartsBuilderFromStringYear()
        {
            CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString("2003/0/2004/1/2/");
            Assert.AreEqual(2003, clp.nYearMin);
            Assert.AreEqual(1, clp.nMonthMin);
            Assert.AreEqual(1, clp.nDayMin);
            Assert.AreEqual(2004, clp.nYearMax);
            Assert.AreEqual(1, clp.nMonthMax);
            Assert.AreEqual(1, clp.nDayMax);
            Assert.AreEqual(0, clp.nHourMin);
            Assert.AreEqual(1, clp.nHourMax);
            Assert.AreEqual(2, clp.nState);
        }

        [Test]
        static public void TestParseLogPartsBuilderFromStringFullDate()
        {
            CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString("1-1-2003/0/1-2-2003/0/2/");
            Assert.AreEqual(2003, clp.nYearMin);
            Assert.AreEqual(1, clp.nMonthMin);
            Assert.AreEqual(1, clp.nDayMin);
            Assert.AreEqual(2003, clp.nYearMax);
            Assert.AreEqual(1, clp.nMonthMax);
            Assert.AreEqual(2, clp.nDayMax);
            Assert.AreEqual(0, clp.nHourMin);
            Assert.AreEqual(0, clp.nHourMax);
            Assert.AreEqual(2, clp.nState);
        }

        [Test]
        public static void TestParseLogPartsBuilderFromStringCollection()
        {
            List<CoalesceLogPartsBuilder> plclp =
                ParseLogPartsCollectionBuilderString("2003/0/2003/1/1/|2003/1/2003/2/1/|2003/2/2003/4/1/|");

            Assert.AreEqual(2003, plclp[0].nYearMin);
            Assert.AreEqual(2003, plclp[0].nYearMax);
            Assert.AreEqual(0, plclp[0].nHourMin);
            Assert.AreEqual(1, plclp[0].nHourMax);
            Assert.AreEqual(1, plclp[0].nState);

            Assert.AreEqual(2003, plclp[1].nYearMin);
            Assert.AreEqual(2003, plclp[1].nYearMax);
            Assert.AreEqual(1, plclp[1].nHourMin);
            Assert.AreEqual(2, plclp[1].nHourMax);
            Assert.AreEqual(1, plclp[1].nState);

            Assert.AreEqual(2003, plclp[2].nYearMin);
            Assert.AreEqual(2003, plclp[2].nYearMax);
            Assert.AreEqual(2, plclp[2].nHourMin);
            Assert.AreEqual(4, plclp[2].nHourMax);
            Assert.AreEqual(1, plclp[2].nState);
        }

        [Test]
        public static void TestParseLogPartsBuildFromEmptyStringCollection()
        {
            List<CoalesceLogPartsBuilder> plclp = ParseLogPartsCollectionBuilderString("|");
            Assert.AreEqual(0, plclp.Count);
        }

        static private List<CoalesceLogPartsBuilder> ParseLogPartsCollectionBuilderString(string sLogPartsCollection)
        {
            int i = 0;
            List<CoalesceLogPartsBuilder> plclp = new List<CoalesceLogPartsBuilder>();

            while (i < sLogPartsCollection.Length)
                {
                int iNext = sLogPartsCollection.IndexOf("|", i);
                if (iNext == -1)
                    break;

                if (iNext > i)
                    {
                    string s = sLogPartsCollection.Substring(i, iNext - i);
                    CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString(s);

                    plclp.Add(clp);
                    }
                i = iNext + 1;
                }
            return plclp;
        }

        private static AzLogPartState AzlpsFromNum(int n)
        {
            if (n == 0)
                return AzLogPartState.Invalid;
            else if (n == 1)
                return AzLogPartState.Complete;
            else if (n == 2)
                return AzLogPartState.Pending;
            else if (n == 3)
                return AzLogPartState.Partial;

            throw new Exception("invalid azlps num");
        }

        private static AzLogParts AzlpsFromClp(List<CoalesceLogPartsBuilder> plclp)
        {
            AzLogParts azlps = new AzLogParts();
            foreach (CoalesceLogPartsBuilder clp in plclp)
                {
                AzLogPart azlp = AzLogPart.Create(new DateTime(clp.nYearMin, clp.nMonthMin, clp.nDayMin, clp.nHourMin, 0, 0),
                                                  new DateTime(clp.nYearMax, clp.nMonthMax, clp.nDayMax, clp.nHourMax, 0, 0),
                                                  AzlpsFromNum(clp.nState));
                azlps.m_plazlp.Add(azlp.DttmMin, azlp);
                }
            return azlps;
        }

        static private void AssertEqualClpAzlps(List<CoalesceLogPartsBuilder> plclpExpected, AzLogParts azlps, string sTest)
        {
            Assert.AreEqual(plclpExpected.Count, azlps.m_plazlp.Values.Count);

            for (int iclp = 0; iclp < plclpExpected.Count; iclp++)
                {
                CoalesceLogPartsBuilder clp = plclpExpected[iclp];

                Assert.AreEqual(new DateTime(clp.nYearMin, clp.nMonthMin, clp.nDayMin, clp.nHourMin, 0, 0), azlps.m_plazlp.Values[iclp].DttmMin,
                                "{0}: index {1} match(min) check", sTest, iclp);
                Assert.AreEqual(new DateTime(clp.nYearMax, clp.nMonthMax, clp.nDayMax, clp.nHourMax, 0 , 0), azlps.m_plazlp.Values[iclp].DttmMac,
                                "{0}: index {1} match(max) check", sTest, iclp);
                Assert.AreEqual(AzlpsFromNum(clp.nState), azlps.m_plazlp.Values[iclp].State,
                                "{0}: index {1} match check(state)", sTest, iclp);
                }
        }

        [TestCase("2003/0/2003/1/1/|2003/1/2003/2/1/|2003/2/2003/4/1/|", 1, "2003/0/2003/1/1/|2003/1/2003/2/1/|2003/2/2003/4/1/|", "Identity coalesce")]
        [TestCase("2003/0/2003/1/1/|2003/1/2003/4/1/|2003/2/2003/4/1/|", 1, "2003/0/2003/1/1/|2003/1/2003/4/1/|", "Simple combine, wholly subsumed")]
        [TestCase("2003/0/2003/1/1/|2003/1/2003/3/2/|2003/2/2003/4/1/|", 1, "2003/0/2003/1/1/|2003/1/2003/3/2/|2003/3/2003/4/1/|", "Overlap with next with split next")]
        [TestCase("2003/0/2003/1/1/|2003/1/2003/5/2/|2003/2/2003/3/1/|2003/3/2003/5/1/|", 1, "2003/0/2003/1/1/|2003/1/2003/5/2/|", "Wholle subsume, superset of next")]
        [TestCase("2003/0/2003/6/3/|2003/1/2003/5/2/|2003/2/2003/3/1/|2003/3/2003/5/1/|", 0, "2003/0/2003/6/3/|", "Combine with next 2 elements")]
        [TestCase("2003/0/2003/6/3/|1-1-2003/22/1-2-2003/1/2/|1-1-2003/23/1-2-2003/0/1/|1-2-2003/1/1-2-2003/2/3/|", 0, "2003/0/2003/6/3/|1-1-2003/22/1-2-2003/1/2/|1-2-2003/1/1-2-2003/2/3/|", "Test combine with 25th hour")]
        [Test]
        static public void TestCoalesceLogParts(string sLogParts, int iFirst, string sLogPartsExpected, string sTest)
        {
            // setup the state to coalesce
            List<CoalesceLogPartsBuilder> plclp = ParseLogPartsCollectionBuilderString(sLogParts);
            AzLogParts azlps = AzlpsFromClp(plclp);
            azlps.CoalesceLogParts(iFirst);

            List<CoalesceLogPartsBuilder> plclpExpected = ParseLogPartsCollectionBuilderString(sLogPartsExpected);
            AssertEqualClpAzlps(plclpExpected, azlps, sTest);
        }

        [TestCase("|", "2003/0/2003/1/0/", "2003/0/2003/1/0/|", "insert into an empty list")]
        [TestCase("2003/0/2003/1/0/|", "2003/0/2003/1/1/", "2003/0/2003/1/1/|", "update a single item in a single item list")]
        [TestCase("2003/0/2003/1/0/|2003/2/2003/3/0/|", "2003/0/2003/1/1/", "2003/0/2003/1/1/|2003/2/2003/3/0/|", "update a single item in a non single item list")]
        [TestCase("2003/0/2003/1/0/|2003/2/2003/3/0/|2003/3/2003/4/0/|2003/4/2003/5/0/|","2003/2/2003/4/1/", "2003/0/2003/1/0/|2003/2/2003/4/1/|2003/4/2003/5/0/|","update an item that coalesces 2 items into 1, starting with a match")]
        [TestCase("2003/0/2003/1/0/|2003/1/2003/3/0/|2003/3/2003/4/0/|2003/4/2003/5/0/|","2003/2/2003/4/1/", "2003/0/2003/1/0/|2003/1/2003/2/0/|2003/2/2003/4/1/|2003/4/2003/5/0/|","update an item that coalesces 2 items into 1, splitting the first item")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/13/2003/14/0/|2003/14/2003/15/0/|", "2003/1/2003/2/1/", "2003/0/2003/1/0/|2003/1/2003/2/1/|2003/10/2003/13/0/|2003/13/2003/14/0/|2003/14/2003/15/0/|", "insert an item that starts before a match")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/13/2003/14/0/|2003/14/2003/15/0/|", "2003/9/2003/11/1/", "2003/0/2003/1/0/|2003/9/2003/11/1/|2003/11/2003/13/0/|2003/13/2003/14/0/|2003/14/2003/15/0/|", "update an item that starts before a match and ends within the following")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/13/2003/14/0/|2003/14/2003/15/0/|", "2003/9/2003/13/1/", "2003/0/2003/1/0/|2003/9/2003/13/1/|2003/13/2003/14/0/|2003/14/2003/15/0/|", "update an item that starts before a match and ends exactly at the following")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/15/2003/16/0/|2003/16/2003/17/0/|", "2003/9/2003/14/1/", "2003/0/2003/1/0/|2003/9/2003/14/1/|2003/15/2003/16/0/|2003/16/2003/17/0/|", "update an item that starts before a match and ends after the following (but not matching the following following)")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/15/2003/16/0/|2003/16/2003/17/0/|", "2003/9/2003/15/1/", "2003/0/2003/1/0/|2003/9/2003/15/1/|2003/15/2003/16/0/|2003/16/2003/17/0/|", "update an item that starts before a match and ends after the following (and the ending matches the beginning of the following following")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/15/2003/17/0/|2003/17/2003/18/0/|", "2003/11/2003/16/1/", "2003/0/2003/1/0/|2003/10/2003/11/0/|2003/11/2003/16/1/|2003/16/2003/17/0/|2003/17/2003/18/0/|", "update an item that starts within a match and ends within the following")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/15/2003/17/0/|2003/17/2003/18/0/|", "2003/11/2003/17/1/", "2003/0/2003/1/0/|2003/10/2003/11/0/|2003/11/2003/17/1/|2003/17/2003/18/0/|", "update an item that starts within a match and ends exactly at the end of the following")]
        [TestCase("2003/0/2003/1/0/|2003/10/2003/13/0/|2003/15/2003/17/0/|2003/17/2003/18/0/|", "2003/11/2003/18/1/", "2003/0/2003/1/0/|2003/10/2003/11/0/|2003/11/2003/18/1/|", "update an item that starts within a match and ends exactly at the end of the following following (also at the end of the list)")]
        // try some of the above but spanning another item between following and following following
        [Test]
        static public void TestSetPartState(string sLogParts, string sPartUpdate, string sLogPartsExpected, string sTest)
        {
            List<CoalesceLogPartsBuilder> plclp = ParseLogPartsCollectionBuilderString(sLogParts);
            AzLogParts azlps = AzlpsFromClp(plclp);
            CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString(sPartUpdate);
            azlps.SetPartState(new DateTime(clp.nYearMin, 1, 1, clp.nHourMin, 0, 0), new DateTime(clp.nYearMax, 1, 1, clp.nHourMax, 0, 0), AzlpsFromNum(clp.nState));
            
            List<CoalesceLogPartsBuilder> plclpExpected = ParseLogPartsCollectionBuilderString(sLogPartsExpected);
            AssertEqualClpAzlps(plclpExpected, azlps, sTest);
        }

        [TestCase(new int[] { 1, 2, 3}, 1, 0, true)]
        [TestCase(new int[] { 1, 2, 3}, 2, 1, true)]
        [TestCase(new int[] { 1, 2, 3}, 3, 2, true)]
        [TestCase(new int[] { 1, 2}, 1, 0, true)]
        [TestCase(new int[] { 1, 2}, 2, 1, true)]
        [TestCase(new int[] { 1, 3, 5}, 2, 0, false)]
        [TestCase(new int[] { 1, 3, 5}, 0, -1, false)]
        [TestCase(new int[] { 1, 3, 5}, 4, 1, false)]
        [TestCase(new int[] { 1, 3, 5}, 6, 2, false)]
        [TestCase(new int[] { 1, 3}, 0, -1, false)]
        [TestCase(new int[] { 1, 3}, 2, 0, false)]
        [TestCase(new int[] { 1, 3}, 4, 1, false)]
        [Test]
        static public void TestFindPart(int[] rgnKeys, int nFind, int iExpected, bool fMatchExpected)
        {
            AzLogParts azlp = new AzLogParts();
            foreach (int n in rgnKeys)
                azlp.m_plazlp.Add(new DateTime(2000 + n, 1, 1), null);

            bool fMatched;
            int iResult = azlp.IazlpFindPart(new DateTime(2000 + nFind, 1, 1), out fMatched);
            Assert.AreEqual(iExpected, iResult);
            Assert.AreEqual(fMatchExpected, fMatched);
        }
#endregion

    }

}