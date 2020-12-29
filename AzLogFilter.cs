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
using TCore.PostfixText;

namespace AzLog
{
    public class AzLogFilter
    {
        // ============================================================================
        // A Z  L O G  F I L T E R  O P E R A T I O N
        // ============================================================================

        // WE STORE THIS LIST in POSTFIX
        // (operation)
        // or 
        // (operation) (operation) (and)

        private PostfixText m_pfFilter;

        public List<Clause.Item> FilterItems => m_pfFilter.Clause.Items;
        
        private Guid m_idFilter;
        
        // this changes every time we change the filter conditions. this allows us to store just the id for data parts to know what (if any) filters were applied

        public Guid ID => m_idFilter;

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        
        public string[] ToStrings()
        {
	        return m_pfFilter.ToStrings();
        }

        public override string ToString()
        {
	        return m_pfFilter.ToString();
        }
        
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
	        m_pfFilter = new PostfixText();
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

            azlfNew.m_pfFilter = m_pfFilter.Clone();
            azlfNew.Start = Start;
            azlfNew.End = End;
            return azlfNew;
        }

        public static AzLogFilter CreateFromLines(AzLogFilter azlfBased, IEnumerable<string> rgs)
        {
	        AzLogFilter azlf = new AzLogFilter();
	        
	        azlf.m_pfFilter = PostfixText.CreateFromParserClient(new StringArrayParserClient(rgs));

	        if (azlfBased != null)
	        {
		        azlf.Start = azlfBased.Start;
		        azlf.End = azlfBased.End;
	        }

            return azlf;
        }

        public static AzLogFilter CreateFromLine(AzLogFilter azlfBased, string line)
        {
	        AzLogFilter azlf = new AzLogFilter();

	        azlf.m_pfFilter = PostfixText.CreateFromParserClient(new StringParserClient(line));

	        if (azlfBased != null)
	        {
		        azlf.Start = azlfBased.Start;
		        azlf.End = azlfBased.End;
	        }

	        return azlf;
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
        public void Add(Expression expression)
        {
            InvalFilterID();
            m_pfFilter.AddExpression(expression);
        }

        /* A D D */
        /*----------------------------------------------------------------------------
        	%%Function: Add
        	%%Qualified: AzLog.AzLogFilter.Add
        	%%Contact: rlittle
        	
            add a boolean log filter operation
        ----------------------------------------------------------------------------*/
        public void Add(PostfixOperator op)
        {
            InvalFilterID();
            m_pfFilter.AddOperator(op);
        }

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

        class ValueContext : PostfixText.IValueClient
        {
	        private PostfixText.IValueClient m_delegate;
	        private DateTime m_dttmStart;
	        private DateTime m_dttmEnd;

	        public ValueContext(DateTime dttmStart, DateTime dttmEnd, PostfixText.IValueClient del)
	        {
		        m_dttmEnd = dttmEnd;
		        m_dttmStart = dttmStart;
		        m_delegate = del;
	        }
	        
			public string GetStringFromField(string field)
			{
				if (field == "DATE_START")
					return m_dttmStart.ToString("G");
				if (field == "DATE_END")
					return m_dttmEnd.ToString("G");

				return m_delegate.GetStringFromField(field);
			}

			public int? GetNumberFromField(string field)
			{
				return m_delegate.GetNumberFromField(field);
			}

			public DateTime? GetDateTimeFromField(string field)
			{
				if (field == "DATE_START")
					return m_dttmStart;
				if (field == "DATE_END")
					return m_dttmEnd;
				
				return m_delegate.GetDateTimeFromField(field);
			}

			public Value.ValueType GetFieldValueType(string field)
			{
				if (field == "DATE_START" || field == "DATE_END")
					return Value.ValueType.DateTime;

				return m_delegate.GetFieldValueType(field);
			}
		}
        
		/* F  E V A L U A T E */
		/*----------------------------------------------------------------------------
        	%%Function: FEvaluate
        	%%Qualified: AzLog.AzLogFilter.FEvaluate
        	%%Contact: rlittle
        	
            Evaluate the item against the filter, return true if it matches
        ----------------------------------------------------------------------------*/
		public bool FEvaluate(PostfixText.IValueClient client)
        {
	        return m_pfFilter.FEvaluate(new ValueContext(Start, End, client));
        }

		public static Value CreateValueForColumn(AzLogEntry.LogColumn col)
		{
			return Value.CreateForField(col.ToString());
		}

		#region // Unit Tests

        // ============================================================================
        // M O C K
        // ============================================================================
        private class TestAzLogFilterValueMock : PostfixText.IValueClient
        {
            public DateTime m_dttmStart;
            public DateTime m_dttmEnd;
            public DateTime m_dttmRow;
            public Dictionary<AzLogEntry.LogColumn, string> m_mpColumnValue;

            public TestAzLogFilterValueMock() {}
            
            public string GetStringFromField(string field)
            {
	            if (field == "DATE_START")
		            return m_dttmStart.ToString("G");
	            if (field == "DATE_END")
		            return m_dttmEnd.ToString("G");
	            if (field == "DATE_ROW")
		            return m_dttmRow.ToString("G");
	            
	            AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	            return m_mpColumnValue[lc];
            }

            public int? GetNumberFromField(string field)
            {
	            if (field == "DATE_START" || field == "DATE_END" || field == "DATE_ROW")
		            throw new Exception("no number version of builtin date columns");

	            AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	            return Int32.Parse(m_mpColumnValue[lc]);
            }

            public DateTime? GetDateTimeFromField(string field)
            {
	            if (field == "DATE_START")
		            return m_dttmStart;
	            if (field == "DATE_END")
		            return m_dttmEnd;
	            if (field == "DATE_ROW")
		            return m_dttmRow;

	            AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	            return DateTime.Parse(m_mpColumnValue[lc]);
            }

			public Value.ValueType GetFieldValueType(string field)
			{
				if (field == "DATE_START" || field == "DATE_END" || field == "DATE_ROW")
					return Value.ValueType.DateTime;

				return Value.ValueType.Field;
			}

		}

		[Test]
        public static void TestAzLogFilterIdChange()
        {
            AzLogFilter azlf = new AzLogFilter();
            Guid idSave = azlf.ID;

            Assert.AreEqual(idSave, azlf.ID);
            azlf.Add(new PostfixOperator(PostfixOperator.Op.And));
            Assert.AreNotEqual(idSave, azlf.ID);
            idSave = azlf.ID;
            azlf.Add(new PostfixOperator(PostfixOperator.Op.And));
            Assert.AreNotEqual(idSave, azlf.ID);
        }

        [Test]
        public static void TestAzLogFilterSingle()
        {
	        AzLogFilter azlf = new AzLogFilter();
	        TestAzLogFilterValueMock mock = new TestAzLogFilterValueMock();
	        mock.m_mpColumnValue = new Dictionary<AzLogEntry.LogColumn, string>();
	        mock.m_mpColumnValue.Add(AzLogEntry.LogColumn.AppName, "TestValue");
	        azlf.Add(Expression.Create(CreateValueForColumn(AzLogEntry.LogColumn.AppName), Value.Create("TestValue"), new ComparisonOperator(ComparisonOperator.Op.Eq)));
	        Assert.AreEqual(true, azlf.FEvaluate(mock));
        }

        [TestCase("5/5/1995 2:25",    ComparisonOperator.Op.Gte, "5/5/1995 2:00", ComparisonOperator.Op.Lt, "5/5/1995 4:00", true)]
        [TestCase("5/5/1995 2:25",    ComparisonOperator.Op.Gte, "5/5/1995 3:00", ComparisonOperator.Op.Lt, "5/5/1995 4:00", false)]
        [TestCase("5/5/1995 2:25",    ComparisonOperator.Op.Gte, "5/5/1995 2:25", ComparisonOperator.Op.Lt, "5/5/1995 4:00", true)]
        [TestCase("5/5/1995 4:00",    ComparisonOperator.Op.Gte, "5/5/1995 2:25", ComparisonOperator.Op.Lt, "5/5/1995 4:00", false)]
        [TestCase("5/5/1995 3:59:59", ComparisonOperator.Op.Gte, "5/5/1995 2:25", ComparisonOperator.Op.Lt, "5/5/1995 4:00", true)]
        [Test]
        public static void TestAzLogFilterDateRange(string sDttmRow, ComparisonOperator.Op cmpop1, String sCmp1, ComparisonOperator.Op cmpop2, string sCmp2, bool fExpected)
        {
            AzLogFilter azlf = new AzLogFilter();
            TestAzLogFilterValueMock mock = new TestAzLogFilterValueMock();
            mock.m_dttmRow = DateTime.Parse(sDttmRow);

            azlf.Add(Expression.Create(Value.CreateForField("DATE_ROW"), Value.Create(DateTime.Parse(sCmp1)), new ComparisonOperator(cmpop1)));
            azlf.Add(Expression.Create(Value.CreateForField("DATE_ROW"), Value.Create(DateTime.Parse(sCmp2)), new ComparisonOperator(cmpop2)));
            azlf.Add(new PostfixOperator(PostfixOperator.Op.And));

            Assert.AreEqual(fExpected, azlf.FEvaluate(mock));
        }
        #endregion
    }
}