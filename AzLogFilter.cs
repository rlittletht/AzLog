// ============================================================================
// A Z  L O G  F I L T E R
// ============================================================================

// AzLogFilter - apply filter criteria to a datasource to evaluate match/no match.
// This can get called A LOT, so performance will be important (each row in the model
// will call this to see if it should be added to the view
//
// Also, we will try to create some filtering at the query/data model level if possible
// to limit the amount of data coming down (at the very least, date ranges)
//
// There will ba a filter for each view, and potentially a filter for the model
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AzLog
{
    public class AzLogFilter
    {
        // ============================================================================
        // A Z  L O G  F I L T E R  O P E R A T I O N
        // ============================================================================
        public class AzLogFilterOperation
        {
            public enum OperationType
            {
                And,
                Or,
                Value
            }

            private OperationType m_lfo;
            private AzLogFilterCondition m_azlfc;

            public AzLogFilterOperation(OperationType lfo, AzLogFilterCondition azlfc)
            {
                m_lfo = lfo;
                m_azlfc = azlfc;
            }

            /* S  D E S C R I B E */
            /*----------------------------------------------------------------------------
            	%%Function: SDescribe
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterOperation.SDescribe
            	%%Contact: rlittle
            	
                Return a human readable version of this operation
            ----------------------------------------------------------------------------*/
            public string SDescribe()
            {
                if (m_lfo == OperationType.And)
                    return "And";
                if (m_lfo == OperationType.Or)
                    return "Or";
                return m_azlfc.SDescribe();
            }

            /* C L O N E */
            /*----------------------------------------------------------------------------
            	%%Function: Clone
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterOperation.Clone
            	%%Contact: rlittle
            	
                return a deep clone of this operation
            ----------------------------------------------------------------------------*/
            public AzLogFilterOperation Clone()
            {
                return new AzLogFilterOperation(m_lfo, m_azlfc?.Clone());
            }

            public AzLogFilterCondition Condition => m_azlfc;
            public OperationType Op => m_lfo;
        }

        // WE STORE THIS LIST in POSTFIX
        // (operation)
        // or 
        // (operation) (operation) (and)
        private List<AzLogFilterOperation> m_plazlfo;
        private Guid m_idFilter;
                     // this changes every time we change the filter conditions. this allows us to store just the id for data parts to know what (if any) filters were applied

        public Guid ID => m_idFilter;

        // NOTE: don't save any filter id with the data part if the only thing in the filter is date ranges -- every data part that comes back is unfiltered at that point

        #region Construct/Deconstruct

        /* A Z  L O G  F I L T E R */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogFilter
        	%%Qualified: AzLog.AzLogFilter.AzLogFilter
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogFilter()
        {
            m_plazlfo = new List<AzLogFilterOperation>();
            InvalFilterID();
        }

        /* C L O N E */
        /*----------------------------------------------------------------------------
        	%%Function: Clone
        	%%Qualified: AzLog.AzLogFilter.Clone
        	%%Contact: rlittle
        	
            Do a deep clone of all the operations in this filter. 
        ----------------------------------------------------------------------------*/
        public AzLogFilter Clone()
        {
            AzLogFilter azlfNew = new AzLogFilter();

            foreach (AzLogFilterOperation azlfo in m_plazlfo)
                azlfNew.m_plazlfo.Add(azlfo.Clone());

            return azlfNew;
        }
        #endregion

        #region Model Manipulation
        /* A D D */
        /*----------------------------------------------------------------------------
        	%%Function: Add
        	%%Qualified: AzLog.AzLogFilter.Add
        	%%Contact: rlittle
        	
            Add a single log filter condition
        ----------------------------------------------------------------------------*/
        public void Add(AzLogFilterCondition azlfc)
        {
            InvalFilterID();
            m_plazlfo.Add(new AzLogFilterOperation(AzLogFilterOperation.OperationType.Value, azlfc));
        }

        /* A D D */
        /*----------------------------------------------------------------------------
        	%%Function: Add
        	%%Qualified: AzLog.AzLogFilter.Add
        	%%Contact: rlittle
        	
            add a boolean log filter operation
        ----------------------------------------------------------------------------*/
        public void Add(AzLogFilterOperation.OperationType ot)
        {
            InvalFilterID();
            m_plazlfo.Add(new AzLogFilterOperation(ot, null));
        }

        /* R E M O V E */
        /*----------------------------------------------------------------------------
        	%%Function: Remove
        	%%Qualified: AzLog.AzLogFilter.Remove
        	%%Contact: rlittle
        	
            Remove a particular operation
        ----------------------------------------------------------------------------*/
        public void Remove(int i)
        {
            InvalFilterID();
            m_plazlfo.RemoveAt(i);
        }

        public List<AzLogFilterOperation> Operations => m_plazlfo;

        /* I N V A L  F I L T E R  I  D */
        /*----------------------------------------------------------------------------
        	%%Function: InvalFilterID
        	%%Qualified: AzLog.AzLogFilter.InvalFilterID
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void InvalFilterID()
        {
            m_idFilter = Guid.NewGuid();        
        }
        #endregion

        /* F  E V A L U A T E */
        /*----------------------------------------------------------------------------
        	%%Function: FEvaluate
        	%%Qualified: AzLog.AzLogFilter.FEvaluate
        	%%Contact: rlittle
        	
            Evaluate the item against the filter, return true if it matches
        ----------------------------------------------------------------------------*/
        public bool FEvaluate(ILogFilterItem ilf)
        {
            // evaluate the list using a stack...
            List<bool> plfStack = new List<bool>();

            for (int i = 0; i < m_plazlfo.Count; i++)
                {
                AzLogFilterOperation azlfo = m_plazlfo[i];

                switch (azlfo.Op)
                    {
                    case AzLogFilterOperation.OperationType.Value:
                        plfStack.Add(azlfo.Condition.FEvaluate(ilf));
                        break;
                    case AzLogFilterOperation.OperationType.And:
                        {
                        bool f1 = plfStack[plfStack.Count - 1];
                        bool f2 = plfStack[plfStack.Count - 2];
                        plfStack.RemoveRange(plfStack.Count - 2, 2);
                        plfStack.Add(f1 && f2);
                        break;
                        }
                    case AzLogFilterOperation.OperationType.Or:
                        {
                        bool f1 = plfStack[plfStack.Count - 1];
                        bool f2 = plfStack[plfStack.Count - 2];
                        plfStack.RemoveRange(plfStack.Count - 2, 2);
                        plfStack.Add(f1 || f2);
                        break;
                        }
                    }
                }
            if (plfStack.Count > 1)
                throw new Exception("expression did not reduce");

            return plfStack[0];
        }

        #region // Unit Tests

        // ============================================================================
        // M O C K
        // ============================================================================
        private class TestAzLogFilterValueMock : ILogFilterItem
        {
            public DateTime m_dttmStart;
            public DateTime m_dttmEnd;
            public DateTime m_dttmRow;
            public Dictionary<AzLogEntry.LogColumn, string> m_mpColumnValue;


            public object OGetValue(AzLogFilterValue.ValueType vt, AzLogFilterValue.DataSource ds, AzLogEntry.LogColumn lc)
            {
                switch (ds)
                    {
                    case AzLogFilterValue.DataSource.Column:
                        return m_mpColumnValue[lc];
                    case AzLogFilterValue.DataSource.DttmStart:
                        return m_dttmStart;
                    case AzLogFilterValue.DataSource.DttmEnd:
                        return m_dttmEnd;
                    case AzLogFilterValue.DataSource.DttmRow:
                        return m_dttmRow;
                    }
                return null;
            }

            public TestAzLogFilterValueMock() {}
        }

        [Test]
        public static void TestAzLogFilterIdChange()
        {
            AzLogFilter azlf = new AzLogFilter();
            Guid idSave = azlf.ID;

            Assert.AreEqual(idSave, azlf.ID);
            azlf.Add(null);
            Assert.AreNotEqual(idSave, azlf.ID);
            idSave = azlf.ID;
            azlf.Add(null);
            Assert.AreNotEqual(idSave, azlf.ID);
        }

        [Test]
        public static void TestAzLogFilterSingle()
        {
            AzLogFilter azlf = new AzLogFilter();
            TestAzLogFilterValueMock mock = new TestAzLogFilterValueMock();
            mock.m_mpColumnValue = new Dictionary<AzLogEntry.LogColumn, string>();
            mock.m_mpColumnValue.Add(AzLogEntry.LogColumn.AppName, "TestValue");

            azlf.Add(new AzLogFilterCondition(AzLogFilterValue.ValueType.String, AzLogFilterValue.DataSource.Column, AzLogEntry.LogColumn.AppName, AzLogFilterCondition.CmpOp.Eq, "TestValue"));
            Assert.AreEqual(true, azlf.FEvaluate(mock));
        }

        [TestCase("5/5/1995 2:25", AzLogFilterCondition.CmpOp.Gte, "5/5/1995 2:00", AzLogFilterCondition.CmpOp.Lt, "5/5/1995 4:00", true)]
        [TestCase("5/5/1995 2:25", AzLogFilterCondition.CmpOp.Gte, "5/5/1995 3:00", AzLogFilterCondition.CmpOp.Lt, "5/5/1995 4:00", false)]
        [TestCase("5/5/1995 2:25", AzLogFilterCondition.CmpOp.Gte, "5/5/1995 2:25", AzLogFilterCondition.CmpOp.Lt, "5/5/1995 4:00", true)]
        [TestCase("5/5/1995 4:00", AzLogFilterCondition.CmpOp.Gte, "5/5/1995 2:25", AzLogFilterCondition.CmpOp.Lt, "5/5/1995 4:00", false)]
        [TestCase("5/5/1995 3:59:59", AzLogFilterCondition.CmpOp.Gte, "5/5/1995 2:25", AzLogFilterCondition.CmpOp.Lt, "5/5/1995 4:00", true)]
        [Test]
        public static void TestAzLogFilterDateRange(string sDttmRow, AzLogFilterCondition.CmpOp cmpop1, String sCmp1, AzLogFilterCondition.CmpOp cmpop2, string sCmp2,
            bool fExpected)
        {
            AzLogFilter azlf = new AzLogFilter();
            TestAzLogFilterValueMock mock = new TestAzLogFilterValueMock();
            mock.m_dttmRow = DateTime.Parse(sDttmRow);

            azlf.Add(new AzLogFilterCondition(AzLogFilterValue.ValueType.DateTime, AzLogFilterValue.DataSource.DttmRow, AzLogEntry.LogColumn.Nil, cmpop1, DateTime.Parse(sCmp1)));
            azlf.Add(new AzLogFilterCondition(AzLogFilterValue.ValueType.DateTime, AzLogFilterValue.DataSource.DttmRow, AzLogEntry.LogColumn.Nil, cmpop2, DateTime.Parse(sCmp2)));
            azlf.Add(AzLogFilterOperation.OperationType.And);

            Assert.AreEqual(fExpected, azlf.FEvaluate(mock));
        }
        #endregion

        // ============================================================================
        // A Z  L O G  F I L T E R  V A L U E
        //
        // We compare values. A value could be static, or it might fetch data from
        // the ILogFilterItem
        // ============================================================================
        public class AzLogFilterValue
        {
            public enum ValueType
            {
                String,
                DateTime
            }

            public enum DataSource
            {
                Static,
                Column,
                DttmRow,
                DttmStart, // may never be used
                DttmEnd // may never be used
            }

            private object m_oValue;
            private DataSource m_ds;
            private AzLogEntry.LogColumn m_lc;
            private ValueType m_vt;

            public DataSource Source => m_ds;
            public AzLogEntry.LogColumn DataColumn => m_lc;
            public string SValue => (string) m_oValue;
            
            /* O  V A L U E */
            /*----------------------------------------------------------------------------
	            %%Function: OValue
	            %%Contact: rlittle
	
                WARNING: Be VERY CAREFUL using this -- this gives you DIRECT ACCESS to the
                internals of the filter, and lets you (almost requires that you) bypass
                all the view rebuilding/invalidation.  THIS IS GOOD if you are just
                doing somethig like extending the range of the view
            ----------------------------------------------------------------------------*/
            public object OValue
            {
                get { return m_oValue; }
                set { m_oValue = value; }
            }

            /* S  D E S C R I B E */
            /*----------------------------------------------------------------------------
            	%%Function: SDescribe
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.SDescribe
            	%%Contact: rlittle
            	
                return a string describing this value
            ----------------------------------------------------------------------------*/
            public string SDescribe()
            {
                if (m_ds == DataSource.DttmRow)
                    return "Item[Date/Time]";

                if (m_ds == DataSource.Static)
                    {
                    if (m_vt == ValueType.DateTime)
                        return string.Format("Date({0})", ((DateTime) m_oValue).ToString("MM/dd/yy HH:mm"));
                    else
                        {
                        return string.Format("String(\"{0}\")", (String) m_oValue);
                        }
                    }

                return string.Format("Item[{0}]", AzLogViewSettings.GetColumnName(m_lc));
            }

            /* C L O N E */
            /*----------------------------------------------------------------------------
            	%%Function: Clone
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.Clone
            	%%Contact: rlittle
            	
                Return a deep clone of this filter value
            ----------------------------------------------------------------------------*/
            public AzLogFilterValue Clone()
            {
                AzLogFilterValue azlfvNew = new AzLogFilterValue();

                if (m_ds == DataSource.Static)
                    {
                    if (m_vt == ValueType.String)
                        azlfvNew.m_oValue = string.Copy((string) m_oValue); // don't use Clone() -- that just returns a reference to the original string
                    else if (m_vt == ValueType.DateTime)
                        azlfvNew.m_oValue = ((DateTime) m_oValue);
                    }

                azlfvNew.m_ds = m_ds;
                azlfvNew.m_lc = m_lc;
                azlfvNew.m_vt = m_vt;

                return azlfvNew;
            }
            public AzLogFilterValue() {} // for unit test only
            /* A Z  L O G  F I L T E R  V A L U E */
            /*----------------------------------------------------------------------------
            	%%Function: AzLogFilterValue
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.AzLogFilterValue
            	%%Contact: rlittle
            	
                Create a value that is backed by the data source
            ----------------------------------------------------------------------------*/
            public AzLogFilterValue(ValueType vt, DataSource ds, AzLogEntry.LogColumn lc)
            {
                m_vt = vt;
                m_ds = ds;
                m_lc = lc;
            }

            /* A Z  L O G  F I L T E R  V A L U E */
            /*----------------------------------------------------------------------------
            	%%Function: AzLogFilterValue
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.AzLogFilterValue
            	%%Contact: rlittle
            	
                Create a static string value
            ----------------------------------------------------------------------------*/
            public AzLogFilterValue(string sValue)
            {
                m_vt = ValueType.String;
                m_ds = DataSource.Static;

                m_oValue = sValue;
            }

            /* A Z  L O G  F I L T E R  V A L U E */
            /*----------------------------------------------------------------------------
            	%%Function: AzLogFilterValue
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.AzLogFilterValue
            	%%Contact: rlittle
            	
                Create a static DateTime value
            ----------------------------------------------------------------------------*/
            public AzLogFilterValue(DateTime dttm)
            {
                m_vt = ValueType.DateTime;
                m_ds = DataSource.Static;

                m_oValue = dttm;
            }

            /* D T T M */
            /*----------------------------------------------------------------------------
            	%%Function: Dttm
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.Dttm
            	%%Contact: rlittle
            	
                Return the DateTime (either static, or fetched from datasource)
            ----------------------------------------------------------------------------*/
            public DateTime Dttm(ILogFilterItem ilf)
            {
                object o = m_ds == DataSource.Static ? m_oValue : ilf.OGetValue(m_vt, m_ds, m_lc);

                if (m_vt != ValueType.DateTime)
                    throw new Exception("type mismatch");

                return (DateTime) o;
            }

            /* T O  S T R I N G */
            /*----------------------------------------------------------------------------
            	%%Function: String
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.String
            	%%Contact: rlittle
            	
                Return the string (either static or fetched from datasource)
            ----------------------------------------------------------------------------*/
            public string String(ILogFilterItem ilf)
            {
                object o = m_ds == DataSource.Static ? m_oValue : ilf.OGetValue(m_vt, m_ds, m_lc);

                if (m_vt != ValueType.String)
                    throw new Exception("type mismatch");
                return (string) o;
            }

            /* C M P  O P  G E N E R I C  F R O M  C M P  O P */
            /*----------------------------------------------------------------------------
            	%%Function: CmpOpGenericFromCmpOp
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.CmpOpGenericFromCmpOp
            	%%Contact: rlittle
            	
                get the generic compare operation (deals with case insensitive string
                variants by filling in fNoCase and returning the appropriate cmpop)
            ----------------------------------------------------------------------------*/
            private static AzLogFilterCondition.CmpOp CmpOpGenericFromCmpOp(AzLogFilterCondition.CmpOp cmpOp, out bool fNoCase)
            {
                fNoCase = (int) cmpOp >= (int) AzLogFilterCondition.CmpOp.SCaseInsensitiveFirst;

                if (fNoCase)
                    cmpOp = (AzLogFilterCondition.CmpOp) ((int) cmpOp - (int) AzLogFilterCondition.CmpOp.SCaseInsensitiveFirst);

                return cmpOp;
            }

            /* F  E V A L U A T E */
            /*----------------------------------------------------------------------------
            	%%Function: FEvaluate
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterValue.FEvaluate
            	%%Contact: rlittle
            	
            ----------------------------------------------------------------------------*/
            public bool FEvaluate(AzLogFilterCondition.CmpOp cmpOp, AzLogFilterValue azlfvRHS, ILogFilterItem ilf)
            {
                int nCmp;
                bool fNoCase;

                cmpOp = CmpOpGenericFromCmpOp(cmpOp, out fNoCase);

                if (m_vt != azlfvRHS.m_vt)
                    throw new Exception("cannot evaluate dissimilar value types");

                if (m_vt == ValueType.String)
                    nCmp = System.String.Compare(this.String(ilf), azlfvRHS.String(ilf), fNoCase);
                else if (m_vt == ValueType.DateTime)
                    nCmp = DateTime.Compare(this.Dttm(ilf), azlfvRHS.Dttm(ilf));
                else
                    nCmp = 0;

                switch (cmpOp)
                    {
                    case AzLogFilterCondition.CmpOp.Eq:
                        return nCmp == 0;
                    case AzLogFilterCondition.CmpOp.SEq:
                        return nCmp == 0;
                    case AzLogFilterCondition.CmpOp.Ne:
                        return nCmp != 0;
                    case AzLogFilterCondition.CmpOp.SNe:
                        return nCmp == 0;
                    case AzLogFilterCondition.CmpOp.Gt:
                        return nCmp > 0;
                    case AzLogFilterCondition.CmpOp.Gte:
                        return nCmp >= 0;
                    case AzLogFilterCondition.CmpOp.Lt:
                        return nCmp < 0;
                    case AzLogFilterCondition.CmpOp.Lte:
                        return nCmp <= 0;
                    }

                return false;
            }

            #region // Unit Tests

            [TestCase(AzLogFilterCondition.CmpOp.Eq, AzLogFilterCondition.CmpOp.Eq, false)]
            [TestCase(AzLogFilterCondition.CmpOp.Ne, AzLogFilterCondition.CmpOp.Ne, false)]
            [TestCase(AzLogFilterCondition.CmpOp.Gt, AzLogFilterCondition.CmpOp.Gt, false)]
            [TestCase(AzLogFilterCondition.CmpOp.Gte, AzLogFilterCondition.CmpOp.Gte, false)]
            [TestCase(AzLogFilterCondition.CmpOp.Lt, AzLogFilterCondition.CmpOp.Lt, false)]
            [TestCase(AzLogFilterCondition.CmpOp.Lte, AzLogFilterCondition.CmpOp.Lte, false)]
            [TestCase(AzLogFilterCondition.CmpOp.SEq, AzLogFilterCondition.CmpOp.Eq, true)]
            [TestCase(AzLogFilterCondition.CmpOp.SNe, AzLogFilterCondition.CmpOp.Ne, true)]
            [TestCase(AzLogFilterCondition.CmpOp.SGt, AzLogFilterCondition.CmpOp.Gt, true)]
            [TestCase(AzLogFilterCondition.CmpOp.SGte, AzLogFilterCondition.CmpOp.Gte, true)]
            [TestCase(AzLogFilterCondition.CmpOp.SLt, AzLogFilterCondition.CmpOp.Lt, true)]
            [TestCase(AzLogFilterCondition.CmpOp.SLte, AzLogFilterCondition.CmpOp.Lte, true)]
            [Test]
            public static void TestCmpOpGenericFromCmpOp(AzLogFilterCondition.CmpOp cmpopIn, AzLogFilterCondition.CmpOp cmpopExpected, bool fNoCaseExpected)
            {
                bool fNoCase;

                Assert.AreEqual(cmpopExpected, AzLogFilterValue.CmpOpGenericFromCmpOp(cmpopIn, out fNoCase));
                Assert.AreEqual(fNoCaseExpected, fNoCase);
            }

            [TestCase("abc", "abc", AzLogFilterCondition.CmpOp.Eq, true)]
            [TestCase("abc", "abc", AzLogFilterCondition.CmpOp.Lte, true)]
            [TestCase("abc", "abc", AzLogFilterCondition.CmpOp.Gte, true)]
            [TestCase("abc", "abc", AzLogFilterCondition.CmpOp.Ne, false)]
            [TestCase("abc", "abc", AzLogFilterCondition.CmpOp.Lt, false)]
            [TestCase("abc", "abc", AzLogFilterCondition.CmpOp.Gt, false)]
            [TestCase("abc", "abcd", AzLogFilterCondition.CmpOp.Eq, false)]
            [TestCase("abc", "abcd", AzLogFilterCondition.CmpOp.Lt, true)]
            [TestCase("abc", "abcd", AzLogFilterCondition.CmpOp.Lte, true)]
            [TestCase("abcd", "abc", AzLogFilterCondition.CmpOp.Lt, false)]
            [TestCase("abcd", "abc", AzLogFilterCondition.CmpOp.Lte, false)]
            [TestCase("abcd", "abc", AzLogFilterCondition.CmpOp.Gt, true)]
            [TestCase("abcd", "abc", AzLogFilterCondition.CmpOp.Gte, true)]
            [TestCase("A", "A", AzLogFilterCondition.CmpOp.Eq, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.Eq, false)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.SEq, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.Ne, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.SNe, false)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.Lte, false)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.SLte, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.Gte, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.SGte, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.Lt, false)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.SLt, false)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.Gt, true)]
            [TestCase("A", "a", AzLogFilterCondition.CmpOp.SGt, false)]
            [TestCase("a", "A", AzLogFilterCondition.CmpOp.Lt, true)]
            [TestCase("a", "A", AzLogFilterCondition.CmpOp.SLt, false)]
            [TestCase("a", "A", AzLogFilterCondition.CmpOp.Gt, false)]
            [TestCase("a", "A", AzLogFilterCondition.CmpOp.SGt, false)]
            [Test]
            public static void TestStringTypes(string sLHS, string sRHS, AzLogFilterCondition.CmpOp cmpop, bool fExpected)
            {
                AzLogFilterValue azlfvLHS = new AzLogFilterValue(sLHS);
                AzLogFilterValue azlfvRHS = new AzLogFilterValue(sRHS);

                Assert.AreEqual(fExpected, azlfvLHS.FEvaluate(cmpop, azlfvRHS, null));
            }

            [TestCase("5/5/1995", "5/5/1995", AzLogFilterCondition.CmpOp.Eq, true)]
            [TestCase("5/5/1995", "5/5/1995", AzLogFilterCondition.CmpOp.Ne, false)]
            [TestCase("5/5/1995", "5/5/1995", AzLogFilterCondition.CmpOp.Gt, false)]
            [TestCase("5/5/1995", "5/5/1995", AzLogFilterCondition.CmpOp.Gte, true)]
            [TestCase("5/5/1995", "5/5/1995", AzLogFilterCondition.CmpOp.Lt, false)]
            [TestCase("5/5/1995", "5/5/1995", AzLogFilterCondition.CmpOp.Lte, true)]
            [TestCase("5/5/1995", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Eq, false)]
            [TestCase("5/5/1995", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Ne, true)]
            [TestCase("5/5/1995", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Gt, false)]
            [TestCase("5/5/1995", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Gte, false)]
            [TestCase("5/5/1995", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Lt, true)]
            [TestCase("5/5/1995", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Lte, true)]
            [TestCase("5/5/1995 2:00", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Eq, false)]
            [TestCase("5/5/1995 2:00", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Ne, true)]
            [TestCase("5/5/1995 2:00", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Gt, true)]
            [TestCase("5/5/1995 2:00", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Gte, true)]
            [TestCase("5/5/1995 2:00", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Lt, false)]
            [TestCase("5/5/1995 2:00", "5/5/1995 1:00", AzLogFilterCondition.CmpOp.Lte, false)]
            [Test]
            public static void TestDateTimeTypes(string sDttmLHS, string sDttmRHS, AzLogFilterCondition.CmpOp cmpop,
                bool fExpected)
            {
                AzLogFilterValue azlfvLHS = new AzLogFilterValue(DateTime.Parse(sDttmLHS));
                AzLogFilterValue azlfvRHS = new AzLogFilterValue(DateTime.Parse(sDttmRHS));

                Assert.AreEqual(fExpected, azlfvLHS.FEvaluate(cmpop, azlfvRHS, null));

            }

            [Test]
            public static void TestDataSourceDttm()
            {
                TestAzLogFilterValueMock mock = new TestAzLogFilterValueMock();
                mock.m_dttmStart = DateTime.Parse("5/5/1995");
                mock.m_dttmEnd = DateTime.Parse("5/5/1995 3:00");
                mock.m_dttmRow = DateTime.Parse("5/5/1995 1:30");

                AzLogFilterValue azlfvLHS = new AzLogFilterValue(ValueType.DateTime, DataSource.DttmStart, AzLogEntry.LogColumn.Nil);

                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995")), mock));
                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/4/1995 19:00")), mock));
                Assert.AreEqual(false, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 1:00")), mock));
                Assert.AreEqual(false, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 3:00")), mock));
                Assert.AreEqual(false, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 4:00")), mock));

                azlfvLHS = new AzLogFilterValue(ValueType.DateTime, DataSource.DttmEnd, AzLogEntry.LogColumn.Nil);

                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995")), mock));
                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/4/1995 19:00")), mock));
                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 1:00")), mock));
                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 3:00")), mock));
                Assert.AreEqual(false, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 4:00")), mock));

                azlfvLHS = new AzLogFilterValue(ValueType.DateTime, DataSource.DttmRow, AzLogEntry.LogColumn.Nil);

                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995")), mock));
                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/4/1995 19:00")), mock));
                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 1:00")), mock));
                Assert.AreEqual(false, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 3:00")), mock));
                Assert.AreEqual(false, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue(DateTime.Parse("5/5/1995 4:00")), mock));
            }

            [Test]
            public static void TestDataSourceColumn()
            {
                TestAzLogFilterValueMock mock = new TestAzLogFilterValueMock();
                mock.m_mpColumnValue = new Dictionary<AzLogEntry.LogColumn, string>();
                mock.m_mpColumnValue.Add(AzLogEntry.LogColumn.AppName, "TestValue");

                AzLogFilterValue azlfvLHS = new AzLogFilterValue(ValueType.String, DataSource.Column, AzLogEntry.LogColumn.AppName);

                Assert.AreEqual(true, azlfvLHS.FEvaluate(AzLogFilterCondition.CmpOp.Gte, new AzLogFilterValue("TestValue"), mock));
            }

            #endregion
        }

        // ============================================================================
        // I  L O G  F I L T E R  I T E M
        //
        // This is how we connect datasources to FilterValue's.  This interface is
        // provided and allows us to fetch column or data data from the item
        // ============================================================================
        public interface ILogFilterItem
        {
            object OGetValue(AzLogFilterValue.ValueType vt, AzLogFilterValue.DataSource ds, AzLogEntry.LogColumn lc);
        }

        // ============================================================================
        // A Z  L O G  F I L T E R  C O N D I T I O N
        //
        // This is a single match condition. it takes two values (LHS, RHS) and a
        // comparison operator.  LHS is typically variable (datasource) and RHS is
        // typically static
        // ============================================================================
        public class AzLogFilterCondition
        {
            public enum CmpOp
            {
                Gt = 0,
                Gte = 1,
                Lt = 2,
                Lte = 3,
                Eq = 4,
                Ne = 5,
                SCaseInsensitiveFirst = 6,
                SGt = SCaseInsensitiveFirst + Gt,
                SGte = SCaseInsensitiveFirst + Gte,
                SLt = SCaseInsensitiveFirst + Lt,
                SLte = SCaseInsensitiveFirst + Lte,
                SEq = SCaseInsensitiveFirst + Eq,
                SNe = SCaseInsensitiveFirst + Ne
            }

            private CmpOp m_cmpop;
            private AzLogFilterValue m_azlfvLHS;
            private AzLogFilterValue m_azlfvRHS;

            public CmpOp Comparison => m_cmpop;

            public AzLogFilterValue LHS => m_azlfvLHS;
            public AzLogFilterValue RHS => m_azlfvRHS;

            #region Construction/Deconstruction

            /* _  I N I T */
            /*----------------------------------------------------------------------------
            	%%Function: _Init
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition._Init
            	%%Contact: rlittle
            	
            ----------------------------------------------------------------------------*/
            private void _Init(AzLogFilterValue.ValueType vt, AzLogFilterValue.DataSource dsLeft, AzLogEntry.LogColumn lc, CmpOp cmpop, string sValueRight)
            {
                m_azlfvLHS = new AzLogFilterValue(vt, dsLeft, lc);
                m_azlfvRHS = new AzLogFilterValue(sValueRight);
                m_cmpop = cmpop;
            }

            /* _  I N I T */
            /*----------------------------------------------------------------------------
            	%%Function: _Init
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition._Init
            	%%Contact: rlittle
            	
            ----------------------------------------------------------------------------*/
            private void _Init(AzLogFilterValue.ValueType vt, AzLogFilterValue.DataSource dsLeft, AzLogEntry.LogColumn lc, CmpOp cmpop, DateTime dttmValueRight)
            {
                m_azlfvLHS = new AzLogFilterValue(vt, dsLeft, lc);
                m_azlfvRHS = new AzLogFilterValue(dttmValueRight);
                m_cmpop = cmpop;
            }

            /* A Z  L O G  F I L T E R  C O N D I T I O N */
            /*----------------------------------------------------------------------------
            	%%Function: AzLogFilterCondition
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition.AzLogFilterCondition
            	%%Contact: rlittle
            	
            ----------------------------------------------------------------------------*/
            public AzLogFilterCondition(AzLogFilterValue.ValueType vt, AzLogFilterValue.DataSource dsLeft, AzLogEntry.LogColumn lc, CmpOp cmpop, string sValueRight)
            {
                _Init(vt, dsLeft, lc, cmpop, sValueRight);
            }

            /* A Z  L O G  F I L T E R  C O N D I T I O N */
            /*----------------------------------------------------------------------------
            	%%Function: AzLogFilterCondition
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition.AzLogFilterCondition
            	%%Contact: rlittle
            	
            ----------------------------------------------------------------------------*/
            public AzLogFilterCondition(AzLogFilterValue.ValueType vt, AzLogFilterValue.DataSource dsLeft, AzLogEntry.LogColumn lc, CmpOp cmpop, DateTime dttmValueRight)
            {
                _Init(vt, dsLeft, lc, cmpop, dttmValueRight);
            }

            public AzLogFilterCondition() { }
            /* C L O N E */
            /*----------------------------------------------------------------------------
            	%%Function: Clone
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition.Clone
            	%%Contact: rlittle
            	
                Return a deep clone of this filter condition
            ----------------------------------------------------------------------------*/
            public AzLogFilterCondition Clone()
            {
                AzLogFilterCondition azlfcNew = new AzLogFilterCondition();

                azlfcNew.m_azlfvLHS = m_azlfvLHS.Clone();
                azlfcNew.m_azlfvRHS = m_azlfvRHS.Clone();
                azlfcNew.m_cmpop = m_cmpop;

                return azlfcNew;
            }
            #endregion

            /* S  D E S C R I B E */
            /*----------------------------------------------------------------------------
            	%%Function: SDescribe
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition.SDescribe
            	%%Contact: rlittle
            	
                Return a human readable version of the condition
            ----------------------------------------------------------------------------*/
            public string SDescribe()
            {
                string sOp = "";
                switch (m_cmpop)
                    {
                    case CmpOp.Gt:
                        sOp = ">";
                        break;
                    case CmpOp.Gte:
                        sOp = ">=";
                        break;
                    case CmpOp.Lt:
                        sOp = "<";
                        break;
                    case CmpOp.Lte:
                        sOp = "<=";
                        break;
                    case CmpOp.Eq:
                        sOp = "==";
                        break;
                    case CmpOp.Ne:
                        sOp = "!=";
                        break;
                    case CmpOp.SGt:
                        sOp = ":>";
                        break;
                    case CmpOp.SGte:
                        sOp = ":>=";
                        break;
                    case CmpOp.SLt:
                        sOp = ":<";
                        break;
                    case CmpOp.SLte:
                        sOp = ":<=";
                        break;
                    case CmpOp.SEq:
                        sOp = ":==";
                        break;
                    case CmpOp.SNe:
                        sOp = ":!=";
                        break;
                    }
                return String.Format("{0} {1} {2}", m_azlfvLHS.SDescribe(), sOp, m_azlfvRHS.SDescribe());
            }

            /* F  E V A L U A T E */
            /*----------------------------------------------------------------------------
            	%%Function: FEvaluate
            	%%Qualified: AzLog.AzLogFilter.AzLogFilterCondition.FEvaluate
            	%%Contact: rlittle
            	
                Actually evaluate this condition, using the given interface to the item
                to bind datasources
            ----------------------------------------------------------------------------*/
            public bool FEvaluate(ILogFilterItem ilf)
            {
                return m_azlfvLHS.FEvaluate(m_cmpop, m_azlfvRHS, ilf);
            }
        }
    }
}