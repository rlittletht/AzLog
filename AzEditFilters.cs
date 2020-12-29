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

        private void ScaleListViewColumns(ListView listview, SizeF factor)
        {
	        foreach (ColumnHeader column in listview.Columns)
		        column.Width = (int) Math.Round(column.Width * factor.Width);
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
	        base.ScaleControl(factor, specified);
	        ScaleListViewColumns(m_lvColorFilters, factor);
        }

        /* P O P U L A T E  L I S T B O X */
        /*----------------------------------------------------------------------------
        	%%Function: PopulateListbox
        	%%Qualified: AzLog.AzEditFilters.PopulateListbox
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        void PopulateFilters(ColorFilterColors colors)
        {
	        m_ebFilter.Lines = m_azlf.ToStrings();

	        foreach (AzColorFilter filter in ColorFilters)
	        {
		        ListViewItem lvi = new ListViewItem();
		        lvi.Tag = filter;
		        lvi.BackColor = filter.BackColor;
		        lvi.ForeColor = filter.ForeColor;

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
        public AzEditFilters(AzLogFilter azlf, List<AzColorFilter> colorFilters, ColorFilterColors colors)
        {
            m_azlf = azlf;
            m_colorFilters = colorFilters;
            
            InitializeComponent();
            PopulateFilters(colors);
        }

        
        /* F  E D I T  F I L T E R S */
        /*----------------------------------------------------------------------------
        	%%Function: EditFilters
        	%%Qualified: AzLog.AzEditFilters.EditFilters
        	%%Contact: rlittle
        	
			Bring up the edit filters dialog, and if they OK the dialog, return the 
			new filters and color filters recompiled from the dialog.
        ----------------------------------------------------------------------------*/
        public static bool FEditFilters(ref AzLogFilter azlf, ref List<AzColorFilter> colorFilters, ColorFilterColors colors)
        {
            AzEditFilters azef = new AzEditFilters(azlf, colorFilters, colors);

            if (azef.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
	            // parse the filters
	            AzLogFilter azlfNew = AzLogFilter.CreateFromLines(azlf, azef.m_ebFilter.Lines);

	            // and update the color filters
	            
	            List<AzColorFilter> colorFiltersNew = new List<AzColorFilter>();
	            
	            foreach (ListViewItem lvi in azef.m_lvColorFilters.Items)
	            {
		            bool fSucceeded = true;
		            
		            AzColorFilter colorFilter = new AzColorFilter();
		            colorFilter.Filter = AzLogFilter.CreateFromLine(null, lvi.SubItems[0].Text);

		            if (colors.FGetColor(lvi.SubItems[1].Text, out Color color))
			            colorFilter.BackColor = color;
		            else
			            fSucceeded = false;

		            if (colors.FGetColor(lvi.SubItems[2].Text, out color))
			            colorFilter.ForeColor = color;
		            else
			            fSucceeded = false;

		            if (colorFilter.Filter == null || fSucceeded == false)
		            {
			            MessageBox.Show(
				            $"Failed to add color filter: {lvi.SubItems[0].Text}, {lvi.SubItems[1].Text}, {lvi.SubItems[2].Text}. Aborting entire edit.");
			            return false;
		            }
		            colorFiltersNew.Add(colorFilter);
	            }

	            azlf = azlfNew;
	            colorFilters = colorFiltersNew;
	            
	            return true;
            }

            return false;
        }
    }
}
