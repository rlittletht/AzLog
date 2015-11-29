using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using NUnit.Framework;
using TCore.Settings;

namespace AzLog
{
    public interface ILogClient
    {
        void SetDefaultView(string s);
    }

    public partial class AzLog : Form, ILogClient
    {

        private Settings.SettingsElt[] _rgsteeApp =
            {
                new Settings.SettingsElt("DefaultView", Settings.Type.Str, "", ""),
                new Settings.SettingsElt("DefaultCollection", Settings.Type.Str, "", ""),
            };

        private AzLogCollection m_azlc;
        private string m_sDefaultView;
        private string m_sDefaultCollection;

        private AzLogModel m_azlm;
        private Settings m_ste;

        private static string s_sRegRoot = "Software\\Thetasoft\\AzLog";
        private static string s_sRegRootCollections = "Software\\Thetasoft\\AzLog";

        #region Initialization

        public AzLog()
        {
            InitializeComponent();

            m_ste = new Settings(_rgsteeApp, "Software\\Thetasoft\\AzLog", "App");
            m_ste.Load();
            m_sDefaultView = m_ste.SValue("DefaultView");
            m_sDefaultCollection = m_ste.SValue("DefaultCollection");

            PopulateCollections();
            PopulateDatasources();
        }

        /* P O P U L A T E  C O L L E C T I O N S */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateCollections
        	%%Qualified: AzLog.AzLog.PopulateCollections
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void PopulateCollections()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(String.Format("{0}\\Collections", s_sRegRoot));

            if (rk != null)
                {
                string[] rgs = rk.GetSubKeyNames();

                foreach (string s in rgs)
                    {
                    m_cbxCollections.Items.Add(s);
                    if (String.Compare(s, m_sDefaultCollection, true) == 0)
                        m_cbxCollections.SelectedIndex = m_cbxCollections.Items.Count - 1;
                    }
                rk.Close();
                }
        }

        /* P O P U L A T E  D A T A S O U R C E S */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateDatasources
        	%%Qualified: AzLog.AzLog.PopulateDatasources
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void PopulateDatasources()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey(String.Format("{0}\\Datasources", s_sRegRoot));
            if (rk != null)
                {
                string[] rgs = rk.GetSubKeyNames();

                foreach (string s in rgs)
                    {
                    // make sure the datasource is valid before we add it -- do this by just loading its
                    // info from the registry (load doesn't connect to the datasource...)
                    IAzLogDatasource iazlds = AzLogDatasourceSupport.LoadDatasource(null, s_sRegRoot, s);
                    if (iazlds != null)
                        m_lbAvailableDatasources.Items.Add(iazlds);
                    }
                rk.Close();
                }
        }
        #endregion

        /* S E T  D E F A U L T  V I E W */
        /*----------------------------------------------------------------------------
        	%%Function: SetDefaultView
        	%%Qualified: AzLog.AzLog.SetDefaultView
        	%%Contact: rlittle
        	
            The default view is stored on the collection
        ----------------------------------------------------------------------------*/
        public void SetDefaultView(string sView)
        {
            if (m_azlc != null)
                m_azlc.SetDefaultView(sView);
        }

        /* S E T  D E F A U L T  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: SetDefaultCollection
        	%%Qualified: AzLog.AzLog.SetDefaultCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        void SetDefaultCollection(string sCollection)
        {
            m_ste.SetSValue("DefaultCollection", sCollection);
            m_ste.Save();
        }

        /* C R E A T E  V I E W  F O R  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: CreateViewForCollection
        	%%Qualified: AzLog.AzLog.CreateViewForCollection
        	%%Contact: rlittle
        	
            Create a window with the initial query parameters from this screen
        ----------------------------------------------------------------------------*/
        private void CreateViewForCollection(object sender, EventArgs e)
        {
            if (m_azlm == null)
                {
                m_azlm = new AzLogModel();

                // load all of our datasources and attach them
                foreach (IAzLogDatasource iazlds in m_azlc.Sources)
                    {
                    if (!iazlds.FOpen(null, s_sRegRoot))
                        throw new Exception(String.Format("couldn't open datasource {0}", iazlds.ToString()));

                    m_azlm.AttachDatasource((AzLogAzure) iazlds);
                    }
            }

            // figure out the timespan being requested
            DateTime dttmMin, dttmMac;

            AzLogModel.FillMinMacFromStartEnd(m_ebStart.Text, m_ebEnd.Text, out dttmMin, out dttmMac);

            AzLogFilter azlf = new AzLogFilter();

            // create a basic filter based on the range they asked for
            azlf.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.DateTime, AzLogFilter.AzLogFilterValue.DataSource.DttmRow, AzLogEntry.LogColumn.Nil, 
                                                          AzLogFilter.AzLogFilterCondition.CmpOp.Gte, dttmMin));
            azlf.Add(new AzLogFilter.AzLogFilterCondition(AzLogFilter.AzLogFilterValue.ValueType.DateTime, AzLogFilter.AzLogFilterValue.DataSource.DttmRow, AzLogEntry.LogColumn.Nil, 
                                                          AzLogFilter.AzLogFilterCondition.CmpOp.Lt, dttmMac));
            azlf.Add(AzLogFilter.AzLogFilterOperation.OperationType.And);

            AzLogWindow azlw = AzLogWindow.CreateNewWindow(m_azlm, m_sDefaultView, azlf, this);

            azlw.Show();

            m_azlm.AddView(azlw.View);

            // we don't know what partition we're going to find this data in, so launch a query 
            // from the first partition for this date range
            //m_azlm.FFetchPartitionsForDateRange(dttmMin, nHourMin, dttmMac, nHourMac);
            m_azlm.FFetchPartitionsForDateRange(dttmMin, dttmMac);
        }


        // a collection is a set of datasources.
        private void DoCreateCollection(object sender, EventArgs e)
        {
            string sName;
            if (TCore.UI.InputBox.ShowInputBox("Collection name", out sName))
                {
                m_cbxCollections.Items.Add(sName);
                m_cbxCollections.SelectedIndex = m_cbxCollections.Items.Count - 1;
                m_azlc = new AzLogCollection(sName);
                SyncUIToCollection();
                }
        }

        /* S Y N C  U  I  T O  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: SyncUIToCollection
        	%%Qualified: AzLog.AzLog.SyncUIToCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void SyncUIToCollection()
        {
            m_lbCollectionSources.BeginUpdate();

            m_lbCollectionSources.Items.Clear();

            foreach (IAzLogDatasource iazlds in m_azlc.Sources)
                {
                m_lbCollectionSources.Items.Add(iazlds);
                }
            m_lbCollectionSources.EndUpdate();
        }


        /* D O  S A V E  C O L L E C T I O N */
        /*----------------------------------------------------------------------------
        	%%Function: DoSaveCollection
        	%%Qualified: AzLog.AzLog.DoSaveCollection
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoSaveCollection(object sender, EventArgs e)
        {
            // each collection is just a list of datasources and any other data we want to remember
            if (m_azlc != null)
                m_azlc.Save(s_sRegRoot);
        }

        /* D O  A D D  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: DoAddDatasource
        	%%Qualified: AzLog.AzLog.DoAddDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoAddDatasource(object sender, EventArgs e)
        {
            IAzLogDatasource iazlds = AzAddDatasource_Azure.CreateDatasource("Software\\Thetasoft\\AzLog");

            if (iazlds != null)
                {
                iazlds.Save("Software\\Thetasoft\\AzLog");
                m_lbAvailableDatasources.Items.Add(iazlds);
                }
        }

        /* D O  C O L L E C T I O N  C H A N G E D */
        /*----------------------------------------------------------------------------
        	%%Function: DoCollectionChanged
        	%%Qualified: AzLog.AzLog.DoCollectionChanged
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoCollectionChanged(object sender, EventArgs e)
        {
            string sCollection = (string) m_cbxCollections.SelectedItem;

            m_azlc = AzLogCollection.LoadCollection(s_sRegRoot, sCollection);
            SyncUIToCollection();
            SetDefaultCollection(sCollection);
        }

        /* D O  I N C L U D E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: DoIncludeDatasource
        	%%Qualified: AzLog.AzLog.DoIncludeDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoIncludeDatasource(object sender, EventArgs e)
        {
            if (m_azlc == null)
                return;

            IAzLogDatasource iazlds = (IAzLogDatasource) m_lbAvailableDatasources.SelectedItem;

            if (iazlds != null)
                {
                if (m_azlc.FAddDatasource(iazlds))
                    {
                    m_lbCollectionSources.Items.Add(iazlds);

                    // TODO: If there's a model, then we should add it to the model, and cause a query 
                    // to happen against this datasource!
                    }
                }
        }

        /* D O  R E M O V E  D A T A S O U R C E */
        /*----------------------------------------------------------------------------
        	%%Function: DoRemoveDatasource
        	%%Qualified: AzLog.AzLog.DoRemoveDatasource
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoRemoveDatasource(object sender, EventArgs e)
        {
            if (m_azlc == null)
                return;

            IAzLogDatasource iazlds = (IAzLogDatasource)m_lbCollectionSources.SelectedItem;

            if (iazlds != null)
            {
                m_lbCollectionSources.Items.Remove(iazlds);
                m_azlc.RemoveDatasource(iazlds);
                // to happen against this datasource!
            }
        }

        /* R E N D E R  H E A D I N G  L I N E */
        /*----------------------------------------------------------------------------
        	%%Function: RenderHeadingLine
        	%%Qualified: AzLog.AzLog.RenderHeadingLine
        	%%Contact: rlittle
        	
            UI support to paint a separator line
        ----------------------------------------------------------------------------*/
        private void RenderHeadingLine(object sender, PaintEventArgs e)
        {
            TCore.UI.RenderSupp.RenderHeadingLine(sender, e);
        }
    }
}


