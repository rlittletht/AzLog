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

        void Save(string sRegRoot);
        bool FOpen(AzLogModel azlm, string sRegRoot);
        bool FLoad(AzLogModel azlm, string sRegRoot, string sName);
        Task<bool> FetchPartitionForDateAsync(AzLogModel azlm, DateTime dttm);
        void Close();
    }

    public class AzLogDatasourceSupport
    {
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

        /* L O A D  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: LoadDatasource
        	%%Qualified: AzLog.AzLogDatasourceSupport.LoadDatasource
        	%%Contact: rlittle
        	
            Load the named datasource. This handles figuring out the datasource
            type and loading the correct type. Regurns an interface to the datasource
        ----------------------------------------------------------------------------*/
        public static IAzLogDatasource LoadDatasource(AzLogModel azlm, string sRegRoot, string sName)
        {
            // first, figure out what type of datasource this is
            Settings.SettingsElt[] _rgsteeDatasource =
                {
                    new Settings.SettingsElt("Type", Settings.Type.Str, "", ""),
                };

            string sKey = String.Format("{0}\\Datasources\\{1}", sRegRoot, sName);

            // save everything we need to be able to recreate ourselves
            Settings ste = new Settings(_rgsteeDatasource, sKey, "ds");

            ste.Load();
            string sType = ste.SValue("Type");
            DatasourceType dt;

            dt = TypeFromString(sType);
            switch (dt)
                {
                case DatasourceType.TextFile:
                    return AzLogFile.LoadFileDatasource(null, sRegRoot, sName);
                case DatasourceType.AzureTable:
                    return AzLogAzureTable.LoadAzureDatasource(null, sRegRoot, sName);
                case DatasourceType.AzureBlob:
                    return AzLogAzureBlob.LoadAzureDatasource(null, sRegRoot, sName);
                default:
                    throw new Exception("unknown datasourcetype");
                }
        }
    }
}