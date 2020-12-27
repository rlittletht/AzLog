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

        public AzLogFilter Filters => m_azlf;

        /* P O P U L A T E  L I S T B O X */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateListbox
        	%%Qualified: AzLog.AzEditFilters.PopulateListbox
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        void PopulateFilters()
        {
	        m_ebFilter.Lines = m_azlf.Describe();
#if OLD
	        foreach (AzLogFilter.AzLogFilterOperation azlfo in m_azlf.Operations)
	        {
		        m_lbFilters.Items.Add(azlfo.SDescribe());
	        }
#endif

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
            PopulateFilters();
        }

        /* F  E D I T  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: EditFilters
        	%%Qualified: AzLog.AzEditFilters.EditFilters
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public static AzLogFilter EditFilters(AzLogFilter azlf)
        {
            AzEditFilters azef = new AzEditFilters(azlf);

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
