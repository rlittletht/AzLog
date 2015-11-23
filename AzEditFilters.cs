using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AzLog
{
    public partial class AzEditFilters : Form
    {
        private AzLogFilter m_azlf;

        public AzLogFilter Filters => m_azlf;

        /* P O P U L A T E  L I S T B O X */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateListbox
        	%%Qualified: AzLog.AzEditFilters.PopulateListbox
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        void PopulateListbox()
        {
            foreach (AzLogFilter.AzLogFilterOperation azlfo in m_azlf.Operations)
                {
                m_lbFilters.Items.Add(azlfo.SDescribe());
                }

        }

        /* A Z  E D I T  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: AzEditFilters
        	%%Qualified: AzLog.AzEditFilters.AzEditFilters
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzEditFilters(AzLogFilter azlf)
        {
            m_azlf = azlf;

            InitializeComponent();
            PopulateListbox();
        }

        /* F  E D I T  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: EditFilters
        	%%Qualified: AzLog.AzEditFilters.EditFilters
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogFilter EditFilters(AzLogFilter azlf)
        {
            AzEditFilters azef = new AzEditFilters(azlf.Clone());
            
            return azef.ShowDialog() == System.Windows.Forms.DialogResult.OK ? azef.Filters : null;
        }

        /* D O  D E L E T E  I T E M */
        /*----------------------------------------------------------------------------
        	%%Function: DoDeleteItem
        	%%Qualified: AzLog.AzEditFilters.DoDeleteItem
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        private void DoDeleteItem(object sender, EventArgs e)
        {
            int[] rgn = new int[m_lbFilters.SelectedIndices.Count];

            m_lbFilters.SelectedIndices.CopyTo(rgn, 0);

            for (int i = rgn.Length - 1; i >= 0; i--)
                {
                m_azlf.Remove(rgn[i]);
                }
            m_lbFilters.Items.Clear();
            PopulateListbox();
        }
    }
}
