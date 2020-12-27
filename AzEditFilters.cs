using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCore.PostfixText;

namespace AzLog
{
    public partial class AzEditFilters : Form
    {
        private AzLogFilter m_azlf;
        private List<AzColorFilter> m_colorFilters;
        
        public AzLogFilter Filters => m_azlf;
        public List<AzColorFilter> ColorFilters => m_colorFilters;
        
        /* P O P U L A T E  L I S T B O X */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateListbox
        	%%Qualified: AzLog.AzEditFilters.PopulateListbox
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        void PopulateFilters()
        {
	        m_ebFilter.Lines = m_azlf.ToStrings();

	        foreach (AzColorFilter filter in ColorFilters)
	        {
		        ListViewItem lvi = new ListViewItem();
		        lvi.Tag = filter;

		        lvi.SubItems[0].Text = filter.Filter.ToString();
                lvi.SubItems.Add(filter.BackColor.ToString().Substring(6));
		        lvi.SubItems.Add(filter.ForeColor.ToString().Substring(6));

		        m_lvColorFilters.Items.Add(lvi);
	        }
        }

        /* A Z  E D I T  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: AzEditFilters
        	%%Qualified: AzLog.AzEditFilters.AzEditFilters
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public AzEditFilters(AzLogFilter azlf, List<AzColorFilter> colorFilters)
        {
            m_azlf = azlf;
            m_colorFilters = colorFilters;
            
            InitializeComponent();
            PopulateFilters();
        }

        /* F  E D I T  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: EditFilters
        	%%Qualified: AzLog.AzEditFilters.EditFilters
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogFilter EditFilters(AzLogFilter azlf, List<AzColorFilter> colorFilters)
        {
            AzEditFilters azef = new AzEditFilters(azlf, colorFilters);

            if (azef.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
	            // parse the filters
	            AzLogFilter azlfNew = AzLogFilter.CreateFromLines(azlf, azef.m_ebFilter.Lines);

	            return azlfNew;
            }

            return null;
        }
    }
}
