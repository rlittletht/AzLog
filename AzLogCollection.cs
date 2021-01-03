using System;
using System.Collections.Generic;
using AzLog;
using Microsoft.Win32;
using TCore.Settings;
using TCore.XmlSettings;

namespace AzLog
{
    internal class AzLogCollection
    {
        private string m_sName;
        private List<IAzLogDatasource> m_pliazld;
        IEnumerator<IAzLogDatasource> m_iteratorDatasourceForWrite;
        
        private string m_sDefaultView;

        public static Collection CreateCollection()
        {
	        return Collection.CreateCollection("Collections", ".cx.xml", "AzLog\\Collections");
        }
        
        /* L O A D  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: LoadCollection
        	%%Qualified: AzLog.AzLogCollection.LoadCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogCollection LoadCollection(string sRegRoot, string sName)
        {
            AzLogCollection azlc = new AzLogCollection(sName);
            azlc.Load();

            return azlc;
        }

        /* A Z  L O G  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: AzLogCollection
        	%%Qualified: AzLog.AzLogCollection.AzLogCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzLogCollection(string sName)
        {
            m_sName = sName;
            m_pliazld = new List<IAzLogDatasource>();
        }

        public List<IAzLogDatasource> Sources => m_pliazld;

        /* S E T  D E F A U L T  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: SetDefaultView
        	%%Qualified: AzLog.AzLogCollection.SetDefaultView
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void SetDefaultView(string sView)
        {
            m_sDefaultView = sView;
        }

        public string DefaultView => m_sDefaultView;

        /* F  A D D  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: FAddDatasource
        	%%Qualified: AzLog.AzLogCollection.FAddDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public bool FAddDatasource(IAzLogDatasource iazlds)
        {
            m_pliazld.Add(iazlds);
            return true;
        }

        /* F  A D D  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: FAddDatasource
        	%%Qualified: AzLog.AzLogCollection.FAddDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public bool FAddDatasource(string fullPath)
        {
            IAzLogDatasource iazlds = AzLogDatasourceSupport.LoadDatasource(null, new Collection.FileDescription(fullPath));
            if (iazlds != null)
                return FAddDatasource(iazlds);

            return false;
        }

        /* R E M O V E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: RemoveDatasource
        	%%Qualified: AzLog.AzLogCollection.RemoveDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void RemoveDatasource(IAzLogDatasource iazlds)
        {
            string sDatasource = iazlds.ToString();

            for (int i = 0; i < m_pliazld.Count; i++)
                {
                if (string.Compare(sDatasource, m_pliazld[i].ToString(), true) == 0)
                    {
                    m_pliazld[i].Close();
                    m_pliazld.RemoveAt(i);
                    return;
                    }
                }
        }


        static XmlDescription<AzLogCollection> CreateXmlDescriptor()
        {
	        return XmlDescriptionBuilder<AzLogCollection>
		        .Build("http://www.thetasoft.com/schemas/AzLog/collections/2020", "Collection")
		        .DiscardAttributesWithNoSetter()
		        .DiscardUnknownAttributes()
		        .AddChildElement("Datasources")
		        .AddChildElement("Datasource", GetDatasourceName, SetDatasourceName)
		        .SetRepeating(AzLogCollection.CreateRepeatingDatasource, AzLogCollection.AreRemainingDatasources, AzLogCollection.CommitRepeatDatasource)
		        .Pop()
		        .AddElement("DefaultView", GetDefaultView, SetDefaultView);
        }

        static string GetDefaultView(AzLogCollection model, RepeatContext<AzLogCollection>.RepeatItemContext repeatItem) => model.m_sDefaultView;
        static void SetDefaultView(AzLogCollection model, string value, RepeatContext<AzLogCollection>.RepeatItemContext repeatItem) => model.m_sDefaultView = value;

        static string GetDatasourceName(AzLogCollection model, RepeatContext<AzLogCollection>.RepeatItemContext repeatItem) => ((IAzLogDatasource)repeatItem.RepeatKey).GetName();
        static void SetDatasourceName(AzLogCollection model, string value, RepeatContext<AzLogCollection>.RepeatItemContext repeatItem) => ((string[]) repeatItem.RepeatKey)[0] = value;

        static RepeatContext<AzLogCollection>.RepeatItemContext CreateRepeatingDatasource(
	        AzLogCollection model,
	        Element<AzLogCollection> element,
	        RepeatContext<AzLogCollection>.RepeatItemContext parent)
        {
	        if (model.m_pliazld != null && model.m_iteratorDatasourceForWrite != null)
	        {
		        // also propagate the name
		        return new RepeatContext<AzLogCollection>.RepeatItemContext(
			        element,
			        parent,
			        model.m_iteratorDatasourceForWrite.Current);
	        }

	        return new RepeatContext<AzLogCollection>.RepeatItemContext(element, parent, new string[1]);
        }

        public static bool AreRemainingDatasources(AzLogCollection model, RepeatContext<AzLogCollection>.RepeatItemContext itemContext)
        {
	        if (model.m_pliazld == null)
		        return false;

	        if (model.m_iteratorDatasourceForWrite == null)
		        model.m_iteratorDatasourceForWrite = model.m_pliazld.GetEnumerator();

	        return model.m_iteratorDatasourceForWrite.MoveNext();
        }

        // for now, we only have a single string, so that's what we'll collect in the item context...
        public static void CommitRepeatDatasource(AzLogCollection settings, RepeatContext<AzLogCollection>.RepeatItemContext itemContext)
        {
	        string[] strRef = ((string[]) itemContext.RepeatKey);

	        if (settings.m_pliazld == null)
		        settings.m_pliazld = new List<IAzLogDatasource>();

	        Collection collectionDatasources = AzLogDatasourceSupport.CreateCollection();

            settings.FAddDatasource(collectionDatasources.GetFullPathName(strRef[0]));
        }

        public void Load()
        {
	        Collection collectionCollections = AzLogCollection.CreateCollection();

	        XmlDescription<AzLogCollection> descriptor = CreateXmlDescriptor();

	        using (ReadFile<AzLogCollection> readFile = ReadFile<AzLogCollection>.CreateSettingsFile(collectionCollections.GetFullPathName(m_sName)))
		        readFile.DeSerialize(descriptor, this);
        }

        public void Save()
        {
	        Collection collectionCollections = AzLogCollection.CreateCollection();

	        XmlDescription<AzLogCollection> descriptor = CreateXmlDescriptor();

	        using (WriteFile<AzLogCollection> writeFile = collectionCollections.CreateSettingsWriteFile<global::AzLog.AzLogCollection>(m_sName))
                writeFile.SerializeSettings(descriptor, this);
        }

    }
}