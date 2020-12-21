using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
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
        private Int32 m_grfPending; // a bitmask of pending datasources.  when zero, then no longer pending

        public AzLogPart() {}
        public AzLogPartState State => m_azlps;
        public DateTime DttmMin => m_dttmMin;
        public DateTime DttmMac => m_dttmMac;
        public Int32 GrfPending { get { return m_grfPending; } set { m_grfPending = value; } }
        
        public bool Contains(DateTime dttm)
        {
            if (dttm >= m_dttmMin && dttm < m_dttmMac)
                return true;

            return false;
        }

        public static AzLogPart Create(DateTime dttmMin, DateTime dttmMac, Int32 grfPending, AzLogPartState azlps)
        {
            if (dttmMin.Minute != 0 || dttmMac.Minute != 0)
                throw new Exception("all dates must be on even hours!");

            AzLogPart azlp = new AzLogPart();
            azlp.m_dttmMin = dttmMin;
            azlp.m_dttmMac = dttmMac;
            azlp.m_azlps = azlps;

            // we carry along the pending bits here because coalesce needs to see them.
            azlp.m_grfPending = grfPending;

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

        /* I A Z L P  F I N D  P A R T */
        /*----------------------------------------------------------------------------
        	%%Function: IazlpFindPart
        	%%Qualified: AzLog.AzLogParts.IazlpFindPart
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
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


        /* G R F  D A T A S O U R C E  F O R  I  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: GrfDatasourceForIDatasource
        	%%Qualified: AzLog.AzLogParts.GrfDatasourceForIDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static Int32 GrfDatasourceForIDatasource(int iDatasource)
        {
            return 1 << iDatasource;
        }

        /* G R F  A L L  D A T A S O U R C E S */
        /*----------------------------------------------------------------------------
        	%%Function: GrfAllDatasources
        	%%Qualified: AzLog.AzLogParts.GrfAllDatasources
        	%%Contact: rlittle
        	
            return a grf that includes *all* datasources given the count of 
            datasources
        ----------------------------------------------------------------------------*/
        public static Int32 GrfAllDatasources(int cDatasources)
        {
            return (1 << (cDatasources + 1)) - 1;
        }

        /* S E T  P A R T  S T A T E */
        /*----------------------------------------------------------------------------
        	%%Function: SetPartState
        	%%Qualified: AzLog.AzLogParts.SetPartState
        	%%Contact: rlittle
        	
            Every caller must supply a grfDataSource to let us know who this azlps
            applies to. Pass -1 if you want to just clobber the state for everyone.

            (This is really used to allow setting "pending" for all datasources, or
            for setting complete on pending partitions

            (BE CAREFUL, if you set pending for a datasource that doesn't exist, then
            it will NEVER be set to complete because we will never get a complete
            for that datasource).

            So, to set pending, pass in grf == (1 << (cDataSources + 1)) - 1. (this
            will set bits for every existing datasource).

            Then for each complete, the datasource will send in GrfDatasourceForIDatasource()
        ----------------------------------------------------------------------------*/
        public void SetPartState(DateTime dttmMin, DateTime dttmMac, Int32 grfDataSource, AzLogPartState azlps)
        {
            DateTime dttmDeferred;
            int iazlpFirst = -1;

            // we might not be able to update the whole block at once.  sometimes we will have 
            // to defer a block to avoid having to coalesce
            while (dttmMin < dttmMac)
                {
                bool fMatched;
                int iazlp = IazlpFindPart(dttmMin, out fMatched);

                dttmDeferred = dttmMac; // assume we aren't going to defer anything

                if (!fMatched && iazlp == -1)
                    {
                    // just insert a new one, being careful of overlap (which we would have to 
                    // defer)

                    // check to see if we overlap the first item in the list -- if so, defer
                    // the overlapping part
                    if (m_plazlp.Count > 0 && m_plazlp.Values[0].DttmMin < dttmMac)
                        {
                        AzLogPart azlpNew = AzLogPart.Create(dttmMin, m_plazlp.Values[0].DttmMin, azlps != AzLogPartState.Pending ? 0 : grfDataSource, azlps);
                        m_plazlp.Add(dttmMin, azlpNew);
                        dttmMin = azlpNew.DttmMac;
                        if (iazlpFirst == -1)
                            iazlpFirst = 0;

                        continue;
                        }
                    else
                        {
                        AzLogPart azlpNew = AzLogPart.Create(dttmMin, dttmMac, azlps != AzLogPartState.Pending ? 0 : grfDataSource, azlps);
                        m_plazlp.Add(dttmMin, azlpNew);
                        return;
                    }
                }

                AzLogPart azlp = m_plazlp.Values[iazlp];
                // does this part start *before* we do? if so, we need to split
                AzLogPart azlpPre = null;
                AzLogPart azlpReplace = null;
                AzLogPart azlpReplace2 = null;
                AzLogPart azlpPost = null;
                bool fReplace = false;

                if (azlp.DttmMin < dttmMin)
                    {
                    // if it doesn't contain us, then just add a new part and be done
                    if (!azlp.Contains(dttmMin))
                        {
                        AzLogPart azlpNew;

                        // THIS REQUIRES COALESCE TO BE VERY SMART.  THis is just the case where our start isn't inside
                        // any existing items, so we wanted (before) to just insert a big item here and let coalesce deal with it.

                        // instead. let's insert the part that doesn't overlap the next item, and defer the part that matches the
                        // next item in the list so we can do coalesces work for them

                        // get the next item so we can defer any overlap
                        if (iazlp + 1 < m_plazlp.Count && dttmMac > m_plazlp.Values[iazlp + 1].DttmMin)
                            {
                            AzLogPart azlpNext = m_plazlp.Values[iazlp + 1];

                            azlpNew = AzLogPart.Create(dttmMin, azlpNext.DttmMin, grfDataSource, azlps);
                            dttmDeferred = azlpNew.DttmMac;
                            }
                        else
                            {
                            azlpNew = AzLogPart.Create(dttmMin, dttmMac, grfDataSource, azlps);
                            }

                        m_plazlp.Add(dttmMin, azlpNew);
                        // CAN'T JUST RETURN HERE because this new part might overlap a following part.
                        }
                    else
                        {
                        fReplace = true;
                        // ok, there's some overlap.
                        azlpPre = AzLogPart.Create(azlp.DttmMin, dttmMin, azlp.GrfPending, azlp.State);

                        // now create the replacement part that covers what we want.
                        if (azlp.DttmMac > dttmMac)
                            {
                            // the end of the current node extends beyond what we want. need to create a replace and a post part
                            azlpReplace = AzLogParts.MergeAzlp(AzLogPart.Create(dttmMin, dttmMac, azlp.GrfPending, azlp.State), grfDataSource, azlps);
                            azlpPost = AzLogPart.Create(dttmMac, azlp.DttmMac, azlp.GrfPending, azlp.State);
                            }
                        else
                            {
                            // the end of the matched block is *not* beyond our end. create a replacement that extends to where
                            // we want.  if we end up overlapping the next item, then defer the remainder to handle on the next pass
                            azlpReplace = AzLogParts.MergeAzlp(AzLogPart.Create(dttmMin, azlp.DttmMac, azlp.GrfPending, azlp.State), grfDataSource, azlps);
                            if (azlp.DttmMac < dttmMac)
                                dttmDeferred = azlp.DttmMac;    // defer the rest of our update to another pass (this allows us to match another block)
                            }
                        }
                    }
                else
                    {
                    // we matched dttmMin with an existing partition
                    if (fMatched)
                        {
                        if (azlp.DttmMac > dttmMac)
                            {
                            azlpPost = AzLogPart.Create(dttmMac, azlp.DttmMac, azlp.GrfPending, azlp.State);
                            azlpReplace = AzLogParts.MergeAzlp(AzLogPart.Create(dttmMin, dttmMac, azlp.GrfPending, azlp.State), grfDataSource, azlps);
                            }
                        else
                            {
                            azlpReplace = AzLogParts.MergeAzlp(AzLogPart.Create(dttmMin, azlp.DttmMac, azlp.GrfPending, azlp.State), grfDataSource, azlps);
                            }

                        if (azlp.DttmMac < dttmMac)
                            dttmDeferred = azlp.DttmMac;    // defer the rest of our update to another pass
                        fReplace = true;
                        }
                    else
                        {
                        // the matched block does *NOT* begin before us, so just create a new pre block for us, and then coalesce
                        // any following blocks
                        azlpPre = AzLogPart.Create(dttmMin, dttmMac, grfDataSource, azlps);
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
                if (iazlpFirst == -1)
                    iazlpFirst = iazlp;

                CoalesceLogParts(iazlpFirst); // always coalesce from the first point we inserted into
                dttmMin = dttmDeferred;
                }
        }

        /* M E R G E  A Z L P */
        /*----------------------------------------------------------------------------
        	%%Function: MergeAzlp
        	%%Qualified: AzLog.AzLogParts.MergeAzlp
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static private AzLogPart MergeAzlp(AzLogPart azlpOld, Int32 grfPendingNew, AzLogPartState azlpsNew)
        {
            AzLogPartState azlps;
            Int32 grfPending = 0;

            if (azlpOld.State == AzLogPartState.Pending)
                {
                if (azlpsNew == AzLogPartState.Pending)
                    {
                    azlps = AzLogPartState.Pending;
                    grfPending = azlpOld.GrfPending | grfPendingNew;
                    }
                else
                    {
                    grfPending = azlpOld.GrfPending & ~grfPendingNew; // mask out the bit they are setting
                    if (grfPending == 0)
                        {
                        azlps = azlpsNew;
                        }
                    else
                        {
                        // else it just stays the same but with the grfPending reduced
                        // NOTE: if this was reporting "partial" and the next one reports
                        // "complete", then we will LOSE the fact that one of the updates
                        // was partial.  we ASSUME that the model is the one dictating 
                        // partial v. complete updates, and not the datasources.
                        //
                        // If a datasource reports partial, but the model knows that it
                        // was querying partial data from at least one datasource and
                        // will report that the part is still "partial", hence we don't
                        // have to worry about partial v. complete
                        azlps = azlpOld.State;
                        }
                    }
                }
            else
                {
                if (azlpsNew == AzLogPartState.Pending)
                    {
                    grfPending = grfPendingNew;
                    azlps = AzLogPartState.Pending;
                    }
                else
                    {
                    grfPending = 0;
                    azlps = azlpsNew;
                    }
                }
            return AzLogPart.Create(azlpOld.DttmMin, azlpOld.DttmMac, grfPending, azlps);
        }

        /* C O A L E S C E  L O G  P A R T S */
        /*----------------------------------------------------------------------------
        	%%Function: CoalesceLogParts
        	%%Qualified: AzLog.AzLogParts.CoalesceLogParts
        	%%Contact: rlittle
        	
            coalesce

            NOTE: coming INTO this routine, we might have state != PENDING with 
            grfPending bits set -- this is communicating the intent of UpdatePart
            to Coalesce (to allow us to properly coalesce).

            ON EXIT, ALL parts that are State != Pending will have grfPending == 0x00

            NOTE: Coalesce SHOULD NEVER encounter a situation where there is overlap
            between items with different States/GrfPending.  ALL OF THAT should have
            been handled by SetPartState
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
                    if (azlp.State != azlpNext.State || (azlp.State == AzLogPartState.Pending && azlp.GrfPending != azlpNext.GrfPending))
                        throw new Exception("Coalesce encountered overlap with different state!");

                    // we overlap the next guy.  crop the next guy so he doesn't overlap us
                    AzLogPart azlpNew = null;

                    if (azlp.DttmMac < azlpNext.DttmMac)
                        {
                        // there is still some left
                        azlpNew = AzLogPart.Create(azlp.DttmMac, azlpNext.DttmMac, azlpNext.GrfPending, azlpNext.State);
                        }
                    m_plazlp.RemoveAt(iazlp + 1);
                    if (azlpNew != null)
                        m_plazlp.Add(azlpNew.DttmMin, azlpNew);
                    }
                else if (azlp.DttmMac == azlpNext.DttmMin && azlp.State == azlpNext.State && (azlp.State != AzLogPartState.Pending || azlp.GrfPending == azlpNext.GrfPending))
                    {
                    // we can combine
                    AzLogPart azlpNew = AzLogPart.Create(azlp.DttmMin, azlpNext.DttmMac, azlp.GrfPending, azlp.State);

                    m_plazlp.RemoveAt(iazlp);
                    m_plazlp.RemoveAt(iazlp);
                    m_plazlp.Add(azlpNew.DttmMin, azlpNew);

                    // and continue with the same iazlp to process any more overlaps
                    }
                else
                    {
                    if (azlp.State != AzLogPartState.Pending)
                        azlp.GrfPending = 0;

                    iazlp++;
                    }
                }
            if (m_plazlp.Values[m_plazlp.Count - 1].State != AzLogPartState.Pending)
                m_plazlp.Values[m_plazlp.Count - 1].GrfPending = 0;
        }

        #region Unit Tests

        [TestCase(AzLogPartState.Pending, 0x01, AzLogPartState.Pending, 0x02, AzLogPartState.Pending, 0x03)]
        [TestCase(AzLogPartState.Pending, 0x03, AzLogPartState.Complete, 0x02, AzLogPartState.Pending, 0x01)]
        [TestCase(AzLogPartState.Pending, 0x03, AzLogPartState.Partial, 0x02, AzLogPartState.Pending, 0x01)]
        [TestCase(AzLogPartState.Pending, 0x03, AzLogPartState.Partial, 0x03, AzLogPartState.Partial, 0x00)]
        [TestCase(AzLogPartState.Pending, 0x03, AzLogPartState.Complete, 0x01, AzLogPartState.Pending, 0x02)]
        [Test]
        public static void TestMergeAzlp(AzLogPartState azlpsOld, Int32 grfPendingOld, AzLogPartState azlpsNew, Int32 grfPendingNew, AzLogPartState azlpsExpected,
            Int32 grfPendingExpected)
        {
            AzLogPart azlpOld = AzLogPart.Create(new DateTime(2015, 1, 1), new DateTime(2015, 1, 2), grfPendingOld, azlpsOld);

            AzLogPart azlpResult = AzLogParts.MergeAzlp(azlpOld, grfPendingNew, azlpsNew);
            Assert.AreEqual(grfPendingExpected, azlpResult.GrfPending);
            Assert.AreEqual(azlpsExpected, azlpResult.State);
        }

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
            public Int32 grfPending;
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
            clp.grfPending = Convert.ToInt32(sLogParts.Substring(i2, i2Next - i2), 16);
            i2 = i2Next + 1;
            i2Next = sLogParts.IndexOf("/", i2);
            clp.nState = int.Parse(sLogParts.Substring(i2, i2Next - i2));

            return clp;
        }

        [Test]
        static public void TestParseLogPartsBuilderFromStringYear()
        {
            CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString("2003/0/2004/1/0x01/2/");
            Assert.AreEqual(2003, clp.nYearMin);
            Assert.AreEqual(1, clp.nMonthMin);
            Assert.AreEqual(1, clp.nDayMin);
            Assert.AreEqual(2004, clp.nYearMax);
            Assert.AreEqual(1, clp.nMonthMax);
            Assert.AreEqual(1, clp.nDayMax);
            Assert.AreEqual(0, clp.nHourMin);
            Assert.AreEqual(1, clp.nHourMax);
            Assert.AreEqual(1, clp.grfPending);
            Assert.AreEqual(2, clp.nState);
        }

        [Test]
        static public void TestParseLogPartsBuilderFromStringFullDate()
        {
            CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString("1-1-2003/0/1-2-2003/0/0x03/2/");
            Assert.AreEqual(2003, clp.nYearMin);
            Assert.AreEqual(1, clp.nMonthMin);
            Assert.AreEqual(1, clp.nDayMin);
            Assert.AreEqual(2003, clp.nYearMax);
            Assert.AreEqual(1, clp.nMonthMax);
            Assert.AreEqual(2, clp.nDayMax);
            Assert.AreEqual(0, clp.nHourMin);
            Assert.AreEqual(0, clp.nHourMax);
            Assert.AreEqual(0x03, clp.grfPending);
            Assert.AreEqual(2, clp.nState);
        }

        [Test]
        public static void TestParseLogPartsBuilderFromStringCollection()
        {
            List<CoalesceLogPartsBuilder> plclp =
                ParseLogPartsCollectionBuilderString("2003/0/2003/1/0x01/1/|2003/1/2003/2/0x02/1/|2003/2/2003/4/0x03/1/|");

            Assert.AreEqual(2003, plclp[0].nYearMin);
            Assert.AreEqual(2003, plclp[0].nYearMax);
            Assert.AreEqual(0, plclp[0].nHourMin);
            Assert.AreEqual(1, plclp[0].nHourMax);
            Assert.AreEqual(0x01, plclp[0].grfPending);
            Assert.AreEqual(1, plclp[0].nState);

            Assert.AreEqual(2003, plclp[1].nYearMin);
            Assert.AreEqual(2003, plclp[1].nYearMax);
            Assert.AreEqual(1, plclp[1].nHourMin);
            Assert.AreEqual(2, plclp[1].nHourMax);
            Assert.AreEqual(0x02, plclp[1].grfPending);
            Assert.AreEqual(1, plclp[1].nState);

            Assert.AreEqual(2003, plclp[2].nYearMin);
            Assert.AreEqual(2003, plclp[2].nYearMax);
            Assert.AreEqual(2, plclp[2].nHourMin);
            Assert.AreEqual(4, plclp[2].nHourMax);
            Assert.AreEqual(0x03, plclp[2].grfPending);
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
                                                  clp.grfPending, AzlpsFromNum(clp.nState));
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
                Assert.AreEqual(clp.grfPending, azlps.m_plazlp.Values[iclp].GrfPending);
                }
        }

        [TestCase("2003/0/2003/1/0x01/1/|2003/2/2003/3/0x01/1/|2003/4/2003/5/0x01/1/|", 1, "2003/0/2003/1/0x01/1/|2003/2/2003/3/0x00/1/|2003/4/2003/5/0x00/1/|", "Identity coalesce")]
        [TestCase("2003/0/2003/1/0x01/1/|2003/1/2003/2/0x01/1/|2003/2/2003/3/0x01/1/|", 1, "2003/0/2003/1/0x01/1/|2003/1/2003/3/0x00/1/|", "coalesce")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/1/2003/2/0x01/2/|2003/2/2003/3/0x03/2/|", 1, "2003/0/2003/1/0x01/2/|2003/1/2003/2/0x01/2/|2003/2/2003/3/0x03/2/|", "no coalesce - grfpending")]
        [TestCase("2003/0/2003/1/0x01/1/|2003/1/2003/4/0x01/1/|2003/2/2003/4/0x01/1/|", 1, "2003/0/2003/1/0x01/1/|2003/1/2003/4/0x00/1/|", "Simple combine, wholly subsumed")]

        [TestCase("2003/0/2003/1/0x01/1/|2003/1/2003/3/0x01/1/|2003/2/2003/4/0x01/1/|", 1, "2003/0/2003/1/0x01/1/|2003/1/2003/4/0x00/1/|", "Overlap with next with coalesce")]
        [TestCase("2003/0/2003/1/0x01/1/|2003/1/2003/5/0x01/1/|2003/2/2003/3/0x01/1/|2003/3/2003/5/0x01/1/|", 1, "2003/0/2003/1/0x01/1/|2003/1/2003/5/0x00/1/|", "Wholle subsume, superset of next")]
        [TestCase("2003/0/2003/6/0x01/2/|2003/1/2003/5/0x01/2/|2003/2/2003/3/0x01/2/|2003/3/2003/5/0x01/2/|", 0, "2003/0/2003/6/0x01/2/|", "Combine with next 3 elements")]
        [TestCase("2003/0/2003/6/0x01/3/|1-1-2003/22/1-2-2003/1/0x01/3/|1-1-2003/23/1-2-2003/0/0x01/3/|1-2-2003/1/1-2-2003/2/0x01/3/|", 0, "2003/0/2003/6/0x00/3/|1-1-2003/22/1-2-2003/2/0x00/3/|", "Test combine with 25th hour")]
#if NOT_VALID // these used to be valid when we would allow overlapping regions to coalesce, but that is now taken care of in SetPart, so these have different semantics above
        [TestCase("2003/0/2003/1/0x01/1/|2003/1/2003/3/0x01/1/|2003/2/2003/4/0x01/3/|", 1, "2003/0/2003/1/0x01/1/|2003/1/2003/3/0x00/1/|2003/3/2003/4/0x00/3/|", "Overlap with next with split next")]
        [TestCase("2003/0/2003/1/0x01/1/|2003/1/2003/5/0x01/2/|2003/2/2003/3/0x01/1/|2003/3/2003/5/0x01/1/|", 1, "2003/0/2003/1/0x01/1/|2003/1/2003/5/0x01/2/|", "Wholle subsume, superset of next")]
        [TestCase("2003/0/2003/6/0x01/3/|2003/1/2003/5/0x01/2/|2003/2/2003/3/0x01/1/|2003/3/2003/5/0x01/1/|", 0, "2003/0/2003/6/0x00/3/|", "Combine with next 2 elements")]
        [TestCase("2003/0/2003/6/0x01/3/|1-1-2003/22/1-2-2003/1/0x01/3/|1-1-2003/23/1-2-2003/0/0x01/3/|1-2-2003/1/1-2-2003/2/0x01/3/|", 0, "2003/0/2003/6/0x00/3/|1-1-2003/22/1-2-2003/1/0x01/3/|1-2-2003/1/1-2-2003/2/0x00/3/|", "Test combine with 25th hour")]
#endif
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

        // All of these are a single datasource -- simple update with state == invalid which is a direct replace
        [TestCase("|", "2003/0/2003/1/0x00/0/", "2003/0/2003/1/0x00/0/|", "insert into an empty list")]
        [TestCase("2003/0/2003/1/0x00/0/|", "2003/0/2003/1/0x00/1/", "2003/0/2003/1/0x00/1/|", "update a single item in a single item list")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/ 2/2003/ 3/0x00/0/|", "2003/0/2003/1/0x00/1/", "2003/0/2003/1/0x00/1/|2003/2/2003/3/0x00/0/|", "update a single item in a non single item list")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/ 2/2003/ 3/0x00/0/|2003/ 3/2003/ 4/0x00/0/|2003/ 4/2003/ 5/0x00/0/|", "2003/ 2/2003/ 4/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 2/2003/ 4/0x00/1/|2003/ 4/2003/ 5/0x00/0/|", "update an item that coalesces 2 items into 1, starting with a match")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/ 1/2003/ 3/0x00/0/|2003/ 3/2003/ 4/0x00/0/|2003/ 4/2003/ 5/0x00/0/|", "2003/ 2/2003/ 4/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 1/2003/ 2/0x00/0/|2003/ 2/2003/ 4/0x00/1/|2003/ 4/2003/ 5/0x00/0/|", "update an item that coalesces 2 items into 1, splitting the first item")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/13/2003/14/0x00/0/|2003/15/2003/16/0x00/0/|", "2003/ 9/2003/13/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/13/0x00/1/|2003/13/2003/14/0x00/0/|2003/15/2003/16/0x00/0/|", "update an item that starts before a match and ends exactly at the following")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/13/2003/14/0x00/0/|2003/14/2003/15/0x00/0/|", "2003/ 9/2003/13/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/13/0x00/1/|2003/13/2003/15/0x00/0/|", "update an item that starts before a match and ends exactly at the following, with coalescing after")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/16/0x00/0/|2003/16/2003/17/0x00/3/|", "2003/ 9/2003/14/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/14/0x00/1/|2003/15/2003/16/0x00/0/|2003/16/2003/17/0x00/3/|", "update an item that starts before a match and ends after the following (but not matching the following following)")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/16/0x00/0/|2003/16/2003/17/0x00/0/|", "2003/ 9/2003/14/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/14/0x00/1/|2003/15/2003/17/0x00/0/|", "update an item that starts before a match and ends after the following (but not matching the following following), with later coalescing")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/16/0x00/0/|2003/16/2003/17/0x00/3/|", "2003/ 9/2003/15/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/15/0x00/1/|2003/15/2003/16/0x00/0/|2003/16/2003/17/0x00/3/|", "update an item that starts before a match and ends after the following (and the ending matches the beginning of the following following")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/16/0x00/1/|2003/16/2003/17/0x00/1/|", "2003/ 9/2003/15/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/17/0x00/1/|", "update an item that starts before a match and ends after the following (and the ending matches the beginning of the following following, with complete coalescing")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/16/0x00/0/|2003/16/2003/17/0x00/0/|", "2003/ 9/2003/15/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/15/0x00/1/|2003/15/2003/17/0x00/0/|", "update an item that starts before a match and ends after the following (and the ending matches the beginning of the following following, with later coalescing")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/17/0x00/0/|2003/17/2003/18/0x00/0/|", "2003/11/2003/17/0x00/1/", "2003/0/2003/1/0x00/0/|2003/10/2003/11/0x00/0/|2003/11/2003/17/0x00/1/|2003/17/2003/18/0x00/0/|", "update an item that starts within a match and ends exactly at the end of the following")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/14/2003/15/0x00/0/|2003/16/2003/17/0x00/0/|", "2003/ 1/2003/ 2/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 1/2003/ 2/0x00/1/|2003/10/2003/13/0x00/0/|2003/14/2003/15/0x00/0/|2003/16/2003/17/0x00/0/|", "insert an item that starts before a match")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/13/2003/14/0x00/0/|2003/14/2003/15/0x00/0/|", "2003/ 1/2003/ 2/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 1/2003/ 2/0x00/1/|2003/10/2003/15/0x00/0/|", "insert an item that starts before a match, with coalescing later")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/14/2003/15/0x00/0/|2003/16/2003/17/0x00/0/|", "2003/ 9/2003/11/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/11/0x00/1/|2003/11/2003/13/0x00/0/|2003/14/2003/15/0x00/0/|2003/16/2003/17/0x00/0/|", "update an item that starts before a match and ends within the following")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/13/2003/14/0x00/0/|2003/14/2003/15/0x00/0/|", "2003/ 9/2003/11/0x00/1/", "2003/0/2003/1/0x00/0/|2003/ 9/2003/11/0x00/1/|2003/11/2003/15/0x00/0/|", "update an item that starts before a match and ends within the following, with coalescing after")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/17/0x00/3/|2003/17/2003/18/0x00/0/|", "2003/11/2003/16/0x00/1/", "2003/0/2003/1/0x00/0/|2003/10/2003/11/0x00/0/|2003/11/2003/16/0x00/1/|2003/16/2003/17/0x00/3/|2003/17/2003/18/0x00/0/|", "update an item that starts within a match and ends within the following")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/17/0x00/0/|2003/17/2003/18/0x00/0/|", "2003/11/2003/16/0x00/1/", "2003/0/2003/1/0x00/0/|2003/10/2003/11/0x00/0/|2003/11/2003/16/0x00/1/|2003/16/2003/18/0x00/0/|", "update an item that starts within a match and ends within the following, with later coalescing")]
        [TestCase("2003/0/2003/1/0x00/0/|2003/10/2003/13/0x00/0/|2003/15/2003/17/0x00/0/|2003/17/2003/18/0x00/0/|", "2003/11/2003/18/0x00/1/", "2003/0/2003/1/0x00/0/|2003/10/2003/11/0x00/0/|2003/11/2003/18/0x00/1/|", "update an item that starts within a match and ends exactly at the end of the following following (also at the end of the list)")]
        // Same as above, but going from Pending to Complete, single datasource
        [TestCase("|", "2003/0/2003/1/0x01/1/", "2003/0/2003/1/0x00/1/|", "1DS: insert into an empty list")]
        [TestCase("2003/0/2003/1/0x01/2/|", "2003/0/2003/1/0x01/1/", "2003/0/2003/1/0x00/1/|", "1DS: update a single item in a single item list")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/ 2/2003/ 3/0x01/2/|", "2003/0/2003/1/0x01/1/", "2003/0/2003/1/0x00/1/|2003/2/2003/3/0x01/2/|", "1DS: update a single item in a non single item list")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/ 2/2003/ 3/0x01/2/|2003/ 3/2003/ 4/0x01/2/|2003/ 4/2003/ 5/0x01/2/|", "2003/ 2/2003/ 4/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 2/2003/ 4/0x00/1/|2003/ 4/2003/ 5/0x01/2/|", "1DS: update an item that coalesces 2 items into 1, starting with a match")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/ 1/2003/ 3/0x01/2/|2003/ 3/2003/ 4/0x01/2/|2003/ 4/2003/ 5/0x01/2/|", "2003/ 2/2003/ 4/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 1/2003/ 2/0x01/2/|2003/ 2/2003/ 4/0x00/1/|2003/ 4/2003/ 5/0x01/2/|", "1DS: update an item that coalesces 2 items into 1, splitting the first item")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/13/2003/14/0x01/2/|2003/14/2003/15/0x01/2/|", "2003/ 9/2003/13/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/13/0x00/1/|2003/13/2003/15/0x01/2/|", "1DS: update an item that starts before a match and ends exactly at the following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/15/2003/16/0x01/2/|2003/16/2003/17/0x01/2/|", "2003/ 9/2003/14/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/14/0x00/1/|2003/15/2003/17/0x01/2/|", "1DS: update an item that starts before a match and ends after the following (but not matching the following following)")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/15/2003/16/0x01/2/|2003/16/2003/17/0x01/2/|", "2003/ 9/2003/15/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/15/0x00/1/|2003/15/2003/17/0x01/2/|", "1DS: update an item that starts before a match and ends after the following (and the ending matches the beginning of the following following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/15/2003/17/0x01/2/|2003/17/2003/18/0x01/2/|", "2003/11/2003/17/0x01/1/", "2003/0/2003/1/0x01/2/|2003/10/2003/11/0x01/2/|2003/11/2003/17/0x00/1/|2003/17/2003/18/0x01/2/|", "1DS: update an item that starts within a match and ends exactly at the end of the following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/13/2003/14/0x01/2/|2003/14/2003/15/0x01/2/|", "2003/ 1/2003/ 2/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 1/2003/ 2/0x00/1/|2003/10/2003/15/0x01/2/|", "1DS: insert an item that starts before a match")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/13/2003/14/0x01/2/|2003/14/2003/15/0x01/2/|", "2003/ 9/2003/11/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/11/0x00/1/|2003/11/2003/15/0x01/2/|", "1DS: update an item that starts before a match and ends within the following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/15/2003/17/0x01/2/|2003/17/2003/18/0x01/2/|", "2003/11/2003/16/0x01/1/", "2003/0/2003/1/0x01/2/|2003/10/2003/11/0x01/2/|2003/11/2003/16/0x00/1/|2003/16/2003/18/0x01/2/|", "1DS: update an item that starts within a match and ends within the following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/15/2003/17/0x01/2/|2003/17/2003/18/0x01/2/|", "2003/11/2003/18/0x01/1/", "2003/0/2003/1/0x01/2/|2003/10/2003/11/0x01/2/|2003/11/2003/18/0x00/1/|", "1DS: update an item that starts within a match and ends exactly at the end of the following following (also at the end of the list)")]

        // Try multiple datasource
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x01/2/|2003/13/2003/14/0x01/2/|2003/14/2003/15/0x01/2/|", "2003/ 9/2003/11/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/11/0x00/1/|2003/11/2003/15/0x01/2/|", "1DS: update an item that starts before a match and ends within the following")]

        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x03/2/|2003/13/2003/14/0x01/2/|2003/14/2003/15/0x01/2/|", "2003/ 9/2003/11/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/10/0x00/1/|2003/10/2003/11/0x02/2/|2003/11/2003/13/0x03/2/|2003/13/2003/15/0x01/2/|", "2DS: update an item that starts before a match and ends within the following")]

        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x03/2/|2003/13/2003/14/0x01/2/|2003/14/2003/15/0x01/2/|", "2003/ 9/2003/11/0x01/1/", "2003/0/2003/1/0x01/2/|2003/ 9/2003/10/0x00/1/|2003/10/2003/11/0x02/2/|2003/11/2003/13/0x03/2/|2003/13/2003/15/0x01/2/|", "2DS: update an item that starts before a match and ends within the following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x03/2/|2003/15/2003/17/0x03/2/|2003/17/2003/18/0x01/2/|", "2003/11/2003/16/0x01/1/", "2003/0/2003/1/0x01/2/|2003/10/2003/11/0x03/2/|2003/11/2003/13/0x02/2/|2003/13/2003/15/0x00/1/|2003/15/2003/16/0x02/2/|2003/16/2003/17/0x03/2/|2003/17/2003/18/0x01/2/|", "2DS: update an item that starts within a match and ends within the following")]
        [TestCase("2003/0/2003/1/0x01/2/|2003/10/2003/13/0x03/2/|2003/15/2003/17/0x03/2/|2003/17/2003/18/0x03/2/|", "2003/11/2003/18/0x01/1/", "2003/0/2003/1/0x01/2/|2003/10/2003/11/0x03/2/|2003/11/2003/13/0x02/2/|2003/13/2003/15/0x00/1/|2003/15/2003/18/0x02/2/|", "2DS: update an item that starts within a match and ends exactly at the end of the following following (also at the end of the list)")]

        // try some of the above but spanning another item between following and following following
        [TestCase("2003/1/2003/4/0x03/2/|", "2003/1/2003/2/0x01/1/", "2003/1/2003/2/0x02/2/|2003/2/2003/4/0x03/2/|", "2DS: update an item that matches the start and splits that item. both still pending, different grfDS")]
        [TestCase("2003/1/2003/4/0x03/2/|", "2003/1/2003/5/0x01/1/", "2003/1/2003/4/0x02/2/|2003/4/2003/5/0x00/1/|", "2DS: update an item that matches the start and extends beyond item. one still pending, new part complete")]
        [TestCase("2003/1/2003/4/0x03/2/|2003/4/2003/5/0x01/2/|", "2003/1/2003/5/0x01/1/", "2003/1/2003/4/0x02/2/|2003/4/2003/5/0x00/1/|", "2DS: update an item that matches the start and extends beyond the next tiem. one still pending, new part complete")]
        [TestCase("2003/1/2003/4/0x03/2/|", "2003/2/2003/4/0x01/1/", "2003/1/2003/2/0x03/2/|2003/2/2003/4/0x02/2/|", "2DS: update an item that starts within the match and ends at that item end both still pending, different grfDS")]
        [TestCase("2003/1/2003/4/0x00/1/|", "2003/1/2003/2/0x01/2/", "2003/1/2003/2/0x01/2/|2003/2/2003/4/0x00/1/|", "2DS: update an item that matches the start and splits that item. complete initially. now 1 pending, 1 complete")]
        [TestCase("2003/1/2003/3/0x0f/2/|2003/3/2003/5/0x07/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "2003/1/2003/5/0x01/1/", "2003/1/2003/3/0x0e/2/|2003/3/2003/5/0x06/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "MDS: update 2 items, neither complete")]
        [TestCase("2003/1/2003/3/0x0f/2/|2003/3/2003/5/0x07/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "2003/1/2003/7/0x01/1/", "2003/1/2003/3/0x0e/2/|2003/3/2003/5/0x06/2/|2003/5/2003/7/0x02/2/|2003/7/2003/9/0x01/2/|", "MDS: update 3 items, neither complete")]
        [TestCase("2003/1/2003/3/0x0f/2/|2003/3/2003/5/0x07/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "2003/1/2003/9/0x01/1/", "2003/1/2003/3/0x0e/2/|2003/3/2003/5/0x06/2/|2003/5/2003/7/0x02/2/|2003/7/2003/9/0x00/1/|", "MDS: update 4 items, one complete")]

        [TestCase("2003/1/2003/3/0x0f/2/|2003/3/2003/5/0x07/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "2003/2/2003/5/0x01/1/", "2003/1/2003/2/0x0f/2/|2003/2/2003/3/0x0e/2/|2003/3/2003/5/0x06/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "MDS: update 2 items, starting int he middle of the first match, neither complete")]
        [TestCase("2003/1/2003/3/0x0f/2/|2003/3/2003/5/0x07/2/|2003/5/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "2003/2/2003/6/0x01/1/", "2003/1/2003/2/0x0f/2/|2003/2/2003/3/0x0e/2/|2003/3/2003/5/0x06/2/|2003/5/2003/6/0x02/2/|2003/6/2003/7/0x03/2/|2003/7/2003/9/0x01/2/|", "MDS: update 3 items, starting int he middle of the first match, ending withing a later item, neither complete")]

        [TestCase("2003/3/2003/6/0x03/2/|", "2003/1/2003/2/0x01/1/", "2003/1/2003/2/0x00/1/|2003/3/2003/6/0x03/2/|", "2DS: insert an item at the front of the list, no overlap")]
        [TestCase("2003/3/2003/6/0x03/2/|", "2003/1/2003/4/0x01/1/", "2003/1/2003/3/0x00/1/|2003/3/2003/4/0x02/2/|2003/4/2003/6/0x03/2/|", "2DS: insert an item at the front of the list, overlap within next item")]
        [TestCase("2003/3/2003/6/0x03/2/|", "2003/1/2003/6/0x01/1/", "2003/1/2003/3/0x00/1/|2003/3/2003/6/0x02/2/|", "2DS: insert an item at the front of the list, overlap entire next item")]
        [TestCase("2003/3/2003/6/0x03/2/|", "2003/1/2003/7/0x01/1/", "2003/1/2003/3/0x00/1/|2003/3/2003/6/0x02/2/|2003/6/2003/7/0x00/1/|", "2DS: insert an item at the front of the list, overlap entire next item with post")]

        // Now some complex updates because of grfDatasource differences
        [TestCase("2003/0/2003/1/0x03/2/|", "2003/0/2003/1/0x01/1/", "2003/0/2003/1/0x02/2/|", "update a single item in a single item list -- Pending 0x03 with Complete 0x01 == Pending 0x02")]
        [TestCase("2003/0/2003/1/0x02/2/|", "2003/0/2003/1/0x02/1/", "2003/0/2003/1/0x00/1/|", "update a single item in a single item list -- Pending 0x02 with Complete 0x02 == Complete 0x00")]
        [Test]
        static public void TestSetPartState(string sLogParts, string sPartUpdate, string sLogPartsExpected, string sTest)
        {
            List<CoalesceLogPartsBuilder> plclp = ParseLogPartsCollectionBuilderString(sLogParts);
            AzLogParts azlps = AzlpsFromClp(plclp);
            CoalesceLogPartsBuilder clp = ParseLogPartsBuilderFromString(sPartUpdate);
            azlps.SetPartState(new DateTime(clp.nYearMin, 1, 1, clp.nHourMin, 0, 0), new DateTime(clp.nYearMax, 1, 1, clp.nHourMax, 0, 0), clp.grfPending, AzlpsFromNum(clp.nState));
            
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