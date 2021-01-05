using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAzure.Storage.Table;
using TCore.PostfixText;

namespace AzLog
{
    // we're going to store a lot of these, so let's make them smaller, more convenient that AzLogEntryEntitys
    // (also we don't want to be bound strcitly to the azure log format)
    public class AzLogEntry : PostfixText.IValueClient
    {
        private string m_sPartition;
        private Guid m_guidRowKey;
        private Int64 m_nEventTickCount;
        private string m_sAppName;
        private string m_sLevel;
        private int m_nEventID;
        private int m_nInstanceID;
        private int m_nPid;
        private int m_nTid;
        private string m_sMessage;
        private string[] m_rgsMessageParts;
        private DateTime m_dttmUlsTimestamp;
        private string m_sUlsArea;
        private string m_sUlsCategory;
        private string m_sUlsCorrelation;
        private string m_sUlsEventID;

        public enum LogColumn : int
        {
            Nil = -1,
            Partition = 0,
            RowKey = 1,
            EventTickCount = 2,
            AppName = 3,
            Level = 4,
            EventID = 5,
            InstanceID = 6,
            Pid = 7,
            Tid = 8,
            UlsTimestamp = 9,
            UlsArea = 10,
            UlsCategory = 11,
            UlsCorrelation = 12,
            UlsEventID = 13,
            Message = 14,
            Message0 = 15,
            Message1 = 16,
            Message2 = 17,
            Message3 = 18,
            Message4 = 19,
            Message5 = 20,
            Message6 = 21,
            Message7 = 22,
            Message8 = 23,
            Message9 = 24,
            Last = Message9
        };

        private ListViewItem m_lvi; // this is valid if the requested generation matches the generation we have cached...

        private int m_nGeneration;
        // this is going to pose a problem when multiple windows start at the same nGeneration -- we are likely to collide with multiple windows open, which means that they can
        // have separate views but they might have the same nGeneration (so we will be caching lviItem's from one view and thinking they are valid for other views)
        // the way to solve this *might* be to start each nGeneration with a random number, increasing the chances that we will not collide.
        // REVIEW: this might already be solved...

        #region Accessors

        public string Partition
        {
            get => m_sPartition;
            set => m_sPartition = value;
        }

        public Guid RowKey
        {
            get => m_guidRowKey;
            set => m_guidRowKey = value;
        }

        public Int64 EventTickCount
        {
            get => m_nEventTickCount;
            set => m_nEventTickCount = value;
        }

        public string ApplicationName
        {
            get => m_sAppName;
            set => m_sAppName = value;
        }

        public string Level
        {
            get => m_sLevel;
            set => m_sLevel = value;
        }

        public int EventId
        {
            get => m_nEventID;
            set => m_nEventID = value;
        }

        public string InstanceId
        {
            get => m_nInstanceID.ToString("X8");
            set => m_nInstanceID = int.Parse(value, NumberStyles.AllowHexSpecifier);
        }

        public int Pid
        {
            get => m_nPid;
            set => m_nPid = value;
        }

        public int Tid
        {
            get => m_nTid;
            set => m_nTid = value;
        }

        public string Message
        {
            get => m_sMessage;
            set => m_sMessage = value;
        }

        public DateTime UlsTimestamp
        {
            get => m_dttmUlsTimestamp;
            set => m_dttmUlsTimestamp = value;
        }

        public string UlsArea
        {
            get => m_sUlsArea;
            set => m_sUlsArea = value;
        }

        public string UlsCategory
        {
            get => m_sUlsCategory;
            set => m_sUlsCategory = value;
        }

        public string UlsCorrelation
        {
            get => m_sUlsCorrelation;
            set => m_sUlsCorrelation = value;
        }

        public string UlsEventID
        {
            get => m_sUlsEventID;
            set => m_sUlsEventID = value;
        }
        public string Message0 => m_rgsMessageParts.Length <= 0 ? null : m_rgsMessageParts[0];
        public string Message1 => m_rgsMessageParts.Length <= 1 ? null : m_rgsMessageParts[1];
        public string Message2 => m_rgsMessageParts.Length <= 2 ? null : m_rgsMessageParts[2];
        public string Message3 => m_rgsMessageParts.Length <= 3 ? null : m_rgsMessageParts[3];
        public string Message4 => m_rgsMessageParts.Length <= 4 ? null : m_rgsMessageParts[4];
        public string Message5 => m_rgsMessageParts.Length <= 5 ? null : m_rgsMessageParts[5];
        public string Message6 => m_rgsMessageParts.Length <= 6 ? null : m_rgsMessageParts[6];
        public string Message7 => m_rgsMessageParts.Length <= 7 ? null : m_rgsMessageParts[7];
        public string Message8 => m_rgsMessageParts.Length <= 8 ? null : m_rgsMessageParts[8];
        public string Message9 => m_rgsMessageParts.Length <= 9 ? null : m_rgsMessageParts[9];

        public Value.ValueType GetFieldValueType(string field)
        {
	        if (field == "DATE_ROW")
		        return Value.ValueType.DateTime;

	        AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	        switch (lc)
	        {
		        case LogColumn.UlsTimestamp:
			        return Value.ValueType.DateTime;
		        case LogColumn.EventTickCount:
		        case LogColumn.EventID:
		        case LogColumn.InstanceID:
		        case LogColumn.Pid:
		        case LogColumn.Tid:
			        return Value.ValueType.Number;
		        case LogColumn.Partition:
		        case LogColumn.RowKey:
		        case LogColumn.AppName:
		        case LogColumn.Level:
		        case LogColumn.UlsCategory:
		        case LogColumn.UlsArea:
		        case LogColumn.UlsCorrelation:
		        case LogColumn.UlsEventID:
		        case LogColumn.Message:
		        case LogColumn.Message0:
		        case LogColumn.Message1:
		        case LogColumn.Message2:
		        case LogColumn.Message3:
		        case LogColumn.Message4:
		        case LogColumn.Message5:
		        case LogColumn.Message6:
		        case LogColumn.Message7:
		        case LogColumn.Message8:
		        case LogColumn.Message9:
			        return Value.ValueType.String;
	        }

	        return Value.ValueType.Field;
        }

	    public string GetStringFromField(string field)
        {
	        if (field == "DATE_ROW")
		        return AzLogModel.DttmFromPartition(Partition).ToString("G");

	        AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	        return GetColumn(lc);
        }

        public int? GetNumberFromField(string field)
        {
	        if (field == "DATE_START" || field == "DATE_END" || field == "DATE_ROW")
		        throw new Exception("no number version of builtin date columns");

	        AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	        return Int32.Parse(GetColumn(lc));
        }

        public DateTime? GetDateTimeFromField(string field)
        {
	        if (field == "DATE_ROW")
		        return AzLogModel.DttmFromPartition(Partition);

	        AzLogEntry.LogColumn lc = AzLogEntry.GetColumnIndexByName(field);

	        return DateTime.Parse(GetColumn(lc));
        }

        private static string[] s_rgColumnNames = new string[]
        {
	        "Partition",
            "RowKey",
	        "EventTickCount",
	        "AppName",
	        "Level",
	        "EventID",
	        "InstanceID",
	        "Pid",
	        "Tid",
	        "UlsTimestamp",
	        "UlsArea",
	        "UlsCategory",
	        "UlsCorrelation",
	        "UlsEventID",
	        "Message",
	        "Message0",
	        "Message1",
	        "Message2",
	        "Message3",
	        "Message4",
	        "Message5",
	        "Message6",
	        "Message7",
	        "Message8",
	        "Message9",
        };

        private static Dictionary<string, LogColumn> s_mpColumnNames =
            new Dictionary<string, LogColumn>
            {
                {"Nil", LogColumn.Nil},
                {"Partition", LogColumn.Partition},
                {"RowKey", LogColumn.RowKey},
                {"EventTickCount", LogColumn.EventTickCount},
                {"AppName", LogColumn.AppName},
                {"Level", LogColumn.Level},
                {"EventID", LogColumn.EventID},
                {"InstanceID", LogColumn.InstanceID},
                {"Pid", LogColumn.Pid},
                {"Tid", LogColumn.Tid},
                {"UlsArea", LogColumn.UlsArea },
                {"UlsCategory",LogColumn.UlsCategory },
                {"UlsCorrelation",LogColumn.UlsCorrelation },
                {"UlsTimestamp",LogColumn.UlsTimestamp },
                {"UlsEventID", LogColumn.UlsEventID },
                {"Message", LogColumn.Message},
                {"Message0", LogColumn.Message0},
                {"Message1", LogColumn.Message1},
                {"Message2", LogColumn.Message2},
                {"Message3", LogColumn.Message3},
                {"Message4", LogColumn.Message4},
                {"Message5", LogColumn.Message5},
                {"Message6", LogColumn.Message6},
                {"Message7", LogColumn.Message7},
                {"Message8", LogColumn.Message8},
                {"Message9", LogColumn.Message9},
            };
                
        public static LogColumn GetColumnIndexByName(string sColumn)
        {
            return s_mpColumnNames[sColumn];
        }

        public static string GetColumnBuiltinNameByIndex(LogColumn lc)
        {
	        return s_rgColumnNames[(int) lc];
        }        
        public string GetColumn(LogColumn lc)
        {
            switch (lc)
                {
                case LogColumn.Partition:
                    return Partition;
                case LogColumn.RowKey:
                    return RowKey.ToString();
                case LogColumn.EventTickCount:
                    return m_nEventTickCount.ToString();
                case LogColumn.AppName:
                    return m_sAppName;
                case LogColumn.Level:
                    return m_sLevel;
                case LogColumn.EventID:
                    return m_nEventID.ToString();
                case LogColumn.InstanceID:
                    return m_nInstanceID.ToString();
                case LogColumn.Pid:
                    return m_nPid.ToString();
                case LogColumn.Tid:
                    return m_nTid.ToString();
                case LogColumn.UlsTimestamp:
                    return m_dttmUlsTimestamp.ToString();
                case LogColumn.UlsCategory:
                    return m_sUlsCategory;
                case LogColumn.UlsArea:
                    return m_sUlsArea;
                case LogColumn.UlsCorrelation:
                    return m_sUlsCorrelation;
                case LogColumn.UlsEventID:
                    return m_sUlsEventID;
                case LogColumn.Message:
                    return m_sMessage;
                case LogColumn.Message0:
                    return Message0;
                case LogColumn.Message1:
                    return Message1;
                case LogColumn.Message2:
                    return Message2;
                case LogColumn.Message3:
                    return Message3;
                case LogColumn.Message4:
                    return Message4;
                case LogColumn.Message5:
                    return Message5;
                case LogColumn.Message6:
                    return Message6;
                case LogColumn.Message7:
                    return Message7;
                case LogColumn.Message8:
                    return Message8;
                case LogColumn.Message9:
                    return Message9;
                default:
                    return "";
                }
        }

        public void SetColumn(LogColumn lc, string s)
        {
            if (s == null)
                return;

            switch (lc)
                {
                case LogColumn.Partition:
                    Partition = s;
                    break;
                case LogColumn.RowKey:
                    RowKey = Guid.Parse(s);
                    break;
                case LogColumn.EventTickCount:
                    m_nEventTickCount = Int64.Parse(s);
                    break;
                case LogColumn.AppName:
                    m_sAppName = s;
                    break;
                case LogColumn.Level:
                    m_sLevel = s;
                    break;
                case LogColumn.EventID:
                    m_nEventID = int.Parse(s);
                    break;
                case LogColumn.InstanceID:
                    m_nInstanceID = int.Parse(s);
                    break;
                case LogColumn.Pid:
                    m_nPid = int.Parse(s);
                    break;
                case LogColumn.Tid:
                    if (s.StartsWith("0x"))
                        m_nTid = int.Parse(s.Substring(2), NumberStyles.AllowHexSpecifier | NumberStyles.HexNumber);
                    else
                        m_nTid = int.Parse(s);
                    break;
                case LogColumn.UlsTimestamp:
                    m_dttmUlsTimestamp = DateTime.Parse(s);
                    break;
                case LogColumn.UlsCategory:
                    m_sUlsCategory = s;
                    break;
                case LogColumn.UlsArea:
                    m_sUlsArea = s;
                    break;
                case LogColumn.UlsCorrelation:
                    m_sUlsCorrelation = s;
                    break;
                case LogColumn.UlsEventID:
                    m_sUlsEventID = s;
                    break;
                case LogColumn.Message:
                    m_sMessage = s;
                    break;
                case LogColumn.Message0:
                    m_rgsMessageParts[0] = s;
                    break;
                case LogColumn.Message1:
                    m_rgsMessageParts[1] = s;
                    break;
                case LogColumn.Message2:
                    m_rgsMessageParts[2] = s;
                    break;
                case LogColumn.Message3:
                    m_rgsMessageParts[3] = s;
                    break;
                case LogColumn.Message4:
                    m_rgsMessageParts[4] = s;
                    break;
                case LogColumn.Message5:
                    m_rgsMessageParts[5] = s;
                    break;
                case LogColumn.Message6:
                    m_rgsMessageParts[6] = s;
                    break;
                case LogColumn.Message7:
                    m_rgsMessageParts[7] = s;
                    break;
                case LogColumn.Message8:
                    m_rgsMessageParts[8] = s;
                    break;
                case LogColumn.Message9:
                    m_rgsMessageParts[9] = s;
                    break;
                }
        }
        /* L V I  F E T C H */
        /*----------------------------------------------------------------------------
        	%%Function: LviFetch
        	%%Qualified: AzLog.AzLogEntry.LviFetch
        	%%Contact: rlittle
        	
            Fetch a list view item appropriate for the given LogView.

            We only cache one lvi at a time right now, which means that if two
            windows decide to fetch the same lvi, we are going to be missing this
            cache every single time because we will be flip-flopping between two
            generations

            TODO: Fix this by having more than one cache LVI (to support multiple
            windows)
        ----------------------------------------------------------------------------*/
        public ListViewItem LviFetch(int nGeneration, AzLogViewSettings azlvs, AzLogView view)
        {
            if (m_lvi == null || m_nGeneration != nGeneration)
                {
                m_lvi = new ListViewItem();
                if (view.FGetColorForItem(this, out Color backColor, out Color foreColor))
                {
                    m_lvi.BackColor = backColor;
                    m_lvi.ForeColor = foreColor;
                }

                for (int i = 0; i < azlvs.ColumnCount(); i++)
                    {
                    m_lvi.SubItems.Add(new ListViewItem.ListViewSubItem());
                    m_lvi.SubItems[i].Text = GetColumn(azlvs.Column(i).DataColumn);
                    m_lvi.Tag = this;
                    }
                m_nGeneration = nGeneration;
                }
            return m_lvi;
        }
        #endregion

        #region Construct/Destruct

        public AzLogEntry() {}

        /* C R E A T E */
        /*----------------------------------------------------------------------------
        	%%Function: Create
        	%%Qualified: AzLog.AzLogEntry.Create
        	%%Contact: rlittle
        	
            Create a log entry from its constituent parts. This will parse
            sMessage into all its subparts too (split by \t).

            FUTURE: If this gets too slow, we can make this "opt in" to parse the
            message
        ----------------------------------------------------------------------------*/
        public static AzLogEntry Create(string sPartition, Guid guidRowKey, Int64 nEventTickCount, string sAppName,
            string sLevel, int nEventId, string sInstanceId, int nPid, int nTid, string sMessage)
        {
            AzLogEntry azle = new AzLogEntry();
            azle.Partition = sPartition;
            azle.RowKey = guidRowKey;
            azle.EventTickCount = nEventTickCount;
            azle.ApplicationName = sAppName;
            azle.Level = sLevel;
            azle.EventId = nEventId;
            azle.InstanceId = sInstanceId;
            azle.Pid = nPid;
            azle.Tid = nTid;
            azle.Message = sMessage;
            azle.m_rgsMessageParts = sMessage.Split('\t');

            return azle;
        }

        /* I N I T  M E S S A G E  P A R T S */
        /*----------------------------------------------------------------------------
        	%%Function: InitMessageParts
        	%%Qualified: AzLog.AzLogEntry.InitMessageParts
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void InitMessageParts(int c)
        {
            m_rgsMessageParts = new string[c];
        }

        /* C R E A T E */
        /*----------------------------------------------------------------------------
        	%%Function: Create
        	%%Qualified: AzLog.AzLogEntry.Create
        	%%Contact: rlittle
        	
            Create the log entry from the Azure entity.
        ----------------------------------------------------------------------------*/
        public static AzLogEntry Create(AzLogEntryEntity azlee)
        {
            return AzLogEntry.Create(azlee.PartitionKey, Guid.Parse(azlee.RowKey), azlee.EventTickCount,
                                     azlee.ApplicationName, azlee.Level, azlee.EventId, azlee.InstanceId, azlee.Pid,
                                     azlee.Tid, azlee.Message);
        }
        #endregion

        public bool FMatchSearch(string search, AzLogViewSettings azlvs)
        {
	        for (int i = 0; i < azlvs.ColumnCount(); i++)
	        {
		        string s = GetColumn(azlvs.Column(i).DataColumn);

		        if (s != null)
		        {
			        if (s.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) >= 0)
				        return true;
		        }
            }

	        return false;
        }
    }

    // We need an uber collection that supports virtualized set of AzLogEntries. Preferably partitioned. 
    // Supporting filtered and sorted views that doesn't change the underlying collection
    // we also need to know the ranges of data that are "complete", "pending", or "partial"
    // The we will use PartitionKey + RowKey as the master key for uniqueness (to allow filling of partial data)
    //
    // for now we will partition the data by day + hour (using the partitions)
    public class AzLogEntries
    {
        // this collection must only grow and only append. all views depend on that.
        private List<AzLogEntry> m_plale;
        private AzLogParts m_azlps;

        // the underlying length is the same regardless of the number of parts -- its just the number
        // of log entries
        public int Length => m_plale.Count;

        public AzLogEntries()
        {
            m_plale = new List<AzLogEntry>();
            m_azlps = new AzLogParts();

        }
        public AzLogEntry Item(int i)
        {
            return m_plale[i];
        }

        public AzLogPartState GetPartState(DateTime dttm)
        {
            AzLogPartState azlps;

            lock (this)
                {
                azlps = m_azlps.GetPartState(dttm);
                }

            return azlps;
        }

        public void UpdatePart(DateTime dttmMin, DateTime dttmMac, Int32 grfDatasource, AzLogPartState azpls)
        {
            lock (this)
                {
                m_azlps.SetPartState(dttmMin, dttmMac, grfDatasource, azpls);
                }
        }

        public void AddLogEntry(AzLogEntry azle)
        {
            lock (this)
                {
                m_plale.Add(azle);
                }
        }

        public void AddSegment(AzLogEntry[] rgazle, int cazle, out int iFirst, out int iLast)
        {
            lock (this)
                {
                iFirst = m_plale.Count;

                for (int i = 0; i < cazle; i++)
                    m_plale.Add(rgazle[i]);

                iLast = m_plale.Count;
                }
        }

        public void AddSegment(TableQuerySegment<AzLogEntryEntity> qsazle, out int iFirst, out int iLast)
        {
            lock (this)
                {
                iFirst = m_plale.Count;
                foreach (AzLogEntryEntity azlee in qsazle.Results)
                    {
                    m_plale.Add(AzLogEntry.Create(azlee));
                    }
                iLast = m_plale.Count;
                }
        }

    }

    // ============================================================================
    // C O M P A R E  L O G  E N T R Y  T I C K  C O U N T
    // ============================================================================
    internal class CompareLogEntryTickCount : IComparer<int>
    {
        private AzLogModel m_azlm;

        public CompareLogEntryTickCount(AzLogModel azlm)
        {
            m_azlm = azlm;
        }

        public int Compare(int iLeft, int iRight)
        {
            long n = (m_azlm.LogEntry(iLeft).EventTickCount - m_azlm.LogEntry(iRight).EventTickCount);

            if (n > 0)
                return 1;
            if (n == 0)
                return 0;

            return -1;
        }
    }
}