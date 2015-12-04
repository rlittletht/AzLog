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
    public interface IAzLogDatasource
    {
        string ToString();
        string GetName();
        void SetName(string sName);
        int GetDatasourceIndex();
        void SetDatasourceIndex(int i);

        void Save(string sRegRoot);
        bool FOpen(AzLogModel azlm, string sRegRoot);
        bool FLoad(AzLogModel azlm, string sRegRoot, string sName);
        Task<bool> FetchPartitionForDateAsync(AzLogModel azlm, DateTime dttm);
        void Close();
    }

    public class AzLogDatasourceSupport
    {
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
            if (string.Compare(sType, "AzureTableStorage") == 0)
                return AzLogAzure.LoadAzureDatasource(null, sRegRoot, sName);
            if (string.Compare(sType, "TextFile") == 0)
                return AzLogFile.LoadFileDatasource(null, sRegRoot, sName);
            return null;
        }
    }
}