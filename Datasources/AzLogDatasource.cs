using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TCore.Settings;
using TCore.XmlSettings;

namespace AzLog
{
    public enum DatasourceType
    {
        AzureTable,
        AzureBlob,
        TextFile,
        Unknown
    };

    public interface IAzLogDatasource
    {
        string ToString();
        string GetName();
        void SetName(string sName);
        int GetDatasourceIndex();
        void SetDatasourceIndex(int i);
        DatasourceType GetSourceType();
        void SetSourceType(DatasourceType dt);
        void OpenContainer(AzLogModel azlm, string sName);

        void Save(Collection collection);
        bool FOpen(AzLogModel azlm, string sRegRoot);
        bool FLoad(AzLogModel azlm, Collection.FileDescription file);
        Task<bool> FetchPartitionForDateAsync(AzLogModel azlm, DateTime dttm);
        bool FGetMinMacDateTime(AzLogModel azlm, out DateTime dttmMin, out DateTime dttmMax);
        void Close();
    }

    public class AzLogDatasourceSupport
    {
	    public static Collection CreateCollection()
	    {
            return Collection.CreateCollection("Datasource", ".ds.xml", "AzLog\\Datasources");
        }
	    
        public static DatasourceType TypeFromString(string s)
        {
            if (s == "AzureTableStorage")
                return DatasourceType.AzureTable;
            if (s == "AzureBlobStorage")
                return DatasourceType.AzureBlob;
            if (s == "TextFile")
                return DatasourceType.TextFile;

            return DatasourceType.Unknown;
        }

        public static string TypeToString(DatasourceType st)
        {
            switch (st)
                {
                case DatasourceType.AzureTable:
                    return "AzureTableStorage";
                case DatasourceType.AzureBlob:
                    return "AzureBlobStorage";
                case DatasourceType.TextFile:
                    return "TextFile";
                default:
                    throw new Exception("illegal storage type");
                }
        }

        class DatasourceSniffer
		{
			public DatasourceType Type { get; set; }

			static public void SetType(DatasourceSniffer sniffer, string sType, RepeatContext<DatasourceSniffer>.RepeatItemContext repeatItem)
			{
                sniffer.Type = TypeFromString(sType);
			}
			
			public DatasourceSniffer()
			{
				Type = DatasourceType.Unknown;
			}
		}

        public const string s_namespace = "http://www.thetasoft.com/schemas/AzLog/datasource/2020";
        
        public static IAzLogDatasource LoadDatasource(AzLogModel azlm, Collection.FileDescription file)
        {
	        // general xml description -- just enough to get the type
	        XmlDescription<DatasourceSniffer> snifferDescription = XmlDescriptionBuilder<DatasourceSniffer>
		        .Build(s_namespace, "Datasource")
		        .AddAttribute("type", null, DatasourceSniffer.SetType)
		        .TerminateAfterReadingAttributes();

	        DatasourceSniffer sniffer = new DatasourceSniffer();

            using (ReadFile<DatasourceSniffer> readFile = Collection.CreateSettingsReadFile<DatasourceSniffer>(file))
	        {
		        readFile.DeSerialize(snifferDescription, sniffer);
	        }

            if (sniffer.Type == DatasourceType.Unknown)
	            throw new Exception("Unknown datasource type in load");

            // now delegate to the appropriate load
            switch (sniffer.Type)
            {
	            case DatasourceType.TextFile:
		            return AzLogFile.LoadFileDatasource(null, file);
	            case DatasourceType.AzureTable:
		            return AzLogAzureTable.LoadAzureDatasource(null, file);
	            case DatasourceType.AzureBlob:
		            return AzLogAzureBlob.LoadAzureDatasource(null, file);
	            default:
		            throw new Exception("unknown datasourcetype");
            }
        }
    }
}