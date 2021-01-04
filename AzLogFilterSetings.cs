using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TCore.XmlSettings;

// Handle loading and saving content and color filters
namespace AzLog
{
    public class AzLogFilterSettings
    {
        public string Name { get; set; }
        public bool IsDirty { get; set; }
        public string LogContentFilter { get; set; }
        public List<AzColorFilterSettings> ColorFilters { get; set; }
        public override string ToString() => Name;

        private ColorFilterColors m_colorFilterColors;
        
        /*----------------------------------------------------------------------------
			%%Function:CreateCollection
			%%Qualified:AzLog.AzLogFilterSettings.CreateCollection
        ----------------------------------------------------------------------------*/
        public static Collection CreateCollection()
        {
	        return Collection.CreateCollection("Filters", ".fx.xml", "AzLog\\Filters");
        }

        /*----------------------------------------------------------------------------
			%%Function:AzLogFilterSettings
			%%Qualified:AzLog.AzLogFilterSettings.AzLogFilterSettings
        ----------------------------------------------------------------------------*/
        public AzLogFilterSettings(ColorFilterColors colors)
        {
	        m_colorFilterColors = colors;
        }

		/*----------------------------------------------------------------------------
			%%Function:AzLogFilterSettings
			%%Qualified:AzLog.AzLogFilterSettings.AzLogFilterSettings

			We want to extract the filter and the colorfilter from AzLogView.
        ----------------------------------------------------------------------------*/
		public AzLogFilterSettings(string name, AzLogWindow window, ColorFilterColors colors)
        {
            Name = name;
            UpdateFromWindow(window);
            m_colorFilterColors = colors;
        }

        /*----------------------------------------------------------------------------
			%%Function:UpdateFromWindow
			%%Qualified:AzLog.AzLogFilterSettings.UpdateFromWindow

			Update from the content and color filters on the given window
        ----------------------------------------------------------------------------*/
		public void UpdateFromWindow(AzLogWindow window)
        {
	        LogContentFilter = window.View.Filter.ToString();
	        ColorFilters = new List<AzColorFilterSettings>();
	        
	        foreach (AzColorFilter colorFilter in window.View.ColorFilters)
	        {
		        AzColorFilterSettings colorFilterSettings = new AzColorFilterSettings();
		        colorFilterSettings.MatchCondition = colorFilter.Filter.ToString();
		        colorFilterSettings.BackColor = colorFilter.BackColor;
		        colorFilterSettings.ForeColor = colorFilter.ForeColor;

		        ColorFilters.Add(colorFilterSettings);
	        }
        }

		#region Settings I/O
        static XmlDescription<AzLogFilterSettings> CreateXmlDescriptor()
        {
	        return XmlDescriptionBuilder<AzLogFilterSettings>
		        .Build("http://www.thetasoft.com/schemas/AzLog/filters/2020", "Collection")
		        .DiscardAttributesWithNoSetter()
		        .DiscardUnknownAttributes()
		        .AddChildElement("Filter")
		        .AddChildElement("Name", GetName, SetName)
		        .AddElement("LogFilter", GetContentFilter, SetContentFilter)
		        .AddElement("ColorFilters")
		        .AddChildElement("ColorFilter")
		        .SetRepeating(CreateRepeatingColorFilter, AreRemainingColorFilters, CommitRepeatColorFilters)
		        .AddChildElement("ColorMatchCondition", GetColorMatch, SetColorMatch)
		        .AddElement("BackColor", GetBackColor, SetBackColor)
		        .AddElement("ForeColor", GetForeColor, SetForeColor);
        }

        static string GetName(AzLogFilterSettings model, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => model.Name;
        static void SetName(AzLogFilterSettings model, string value, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => model.Name = value;
        static string GetContentFilter(AzLogFilterSettings model, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => model.LogContentFilter;
        static void SetContentFilter(AzLogFilterSettings model, string value, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => model.LogContentFilter = value;

        static string GetColorMatch(AzLogFilterSettings model, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => ((AzColorFilterSettings)repeatItem.RepeatKey).MatchCondition;
        static void SetColorMatch(AzLogFilterSettings model, string value, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => ((AzColorFilterSettings)repeatItem.RepeatKey).MatchCondition = value;

        static string GetBackColor(AzLogFilterSettings model, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => model.m_colorFilterColors.GetColorName(((AzColorFilterSettings)repeatItem.RepeatKey).BackColor);
        static string GetForeColor(AzLogFilterSettings model, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem) => model.m_colorFilterColors.GetColorName(((AzColorFilterSettings)repeatItem.RepeatKey).ForeColor);

        /*----------------------------------------------------------------------------
			%%Function:SetBackColor
			%%Qualified:AzLog.AzLogFilterSettings.SetBackColor
        ----------------------------------------------------------------------------*/
        static void SetBackColor(AzLogFilterSettings model, string value, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem)
        {
	        Color color;
	        if (!model.m_colorFilterColors.FGetColor(value, out color))
		        MessageBox.Show($"invalid color value {value}. defaulting to black");
	        
	        ((AzColorFilterSettings) repeatItem.RepeatKey).BackColor = color;
        }

        /*----------------------------------------------------------------------------
			%%Function:SetForeColor
			%%Qualified:AzLog.AzLogFilterSettings.SetForeColor
        ----------------------------------------------------------------------------*/
        static void SetForeColor(AzLogFilterSettings model, string value, RepeatContext<AzLogFilterSettings>.RepeatItemContext repeatItem)
        {
	        Color color;
	        if (!model.m_colorFilterColors.FGetColor(value, out color))
		        MessageBox.Show($"invalid color value {value}. defaulting to black");

	        ((AzColorFilterSettings)repeatItem.RepeatKey).ForeColor = color;
        }

        private IEnumerator<AzColorFilterSettings> m_iteratorForRepeatingColorFilters;
        
        /*----------------------------------------------------------------------------
			%%Function:CreateRepeatingColorFilter
			%%Qualified:AzLog.AzLogFilterSettings.CreateRepeatingColorFilter
        ----------------------------------------------------------------------------*/
        static RepeatContext<AzLogFilterSettings>.RepeatItemContext CreateRepeatingColorFilter(
	        AzLogFilterSettings model,
	        Element<AzLogFilterSettings> element,
	        RepeatContext<AzLogFilterSettings>.RepeatItemContext parent)
        {
	        if (model.ColorFilters != null && model.m_iteratorForRepeatingColorFilters != null)
	        {
		        return new RepeatContext<AzLogFilterSettings>.RepeatItemContext(
			        element,
			        parent,
			        model.m_iteratorForRepeatingColorFilters.Current);
	        }

	        return new RepeatContext<AzLogFilterSettings>.RepeatItemContext(element, parent, new AzColorFilterSettings());
        }

        /*----------------------------------------------------------------------------
			%%Function:AreRemainingColorFilters
			%%Qualified:AzLog.AzLogFilterSettings.AreRemainingColorFilters
        ----------------------------------------------------------------------------*/
        static bool AreRemainingColorFilters(AzLogFilterSettings model, RepeatContext<AzLogFilterSettings>.RepeatItemContext itemContext)
        {
	        if (model.ColorFilters == null)
		        return false;

	        if (model.m_iteratorForRepeatingColorFilters == null)
		        model.m_iteratorForRepeatingColorFilters = model.ColorFilters.GetEnumerator();

	        return model.m_iteratorForRepeatingColorFilters.MoveNext();
        }

        /*----------------------------------------------------------------------------
			%%Function:CommitRepeatColorFilters
			%%Qualified:AzLog.AzLogFilterSettings.CommitRepeatColorFilters
        ----------------------------------------------------------------------------*/
        static void CommitRepeatColorFilters(AzLogFilterSettings settings, RepeatContext<AzLogFilterSettings>.RepeatItemContext itemContext)
        {
	        AzColorFilterSettings viewColumn = ((AzColorFilterSettings)itemContext.RepeatKey);

	        if (settings.ColorFilters == null)
		        settings.ColorFilters = new List<AzColorFilterSettings>();

	        settings.ColorFilters.Add(viewColumn);
        }

        /*----------------------------------------------------------------------------
			%%Function:SaveFilters
			%%Qualified:AzLog.AzLogFilterSettings.SaveFilters
        ----------------------------------------------------------------------------*/
        public static void SaveFilters(string sName, AzLogWindow logWindow)
        {
	        AzLogFilterSettings filterSettings = new AzLogFilterSettings(sName, logWindow, logWindow._ColorFilterColors);
        }

        /*----------------------------------------------------------------------------
			%%Function:Save
			%%Qualified:AzLog.AzLogFilterSettings.Save
        ----------------------------------------------------------------------------*/
        public void Save()
        {
	        Collection collection = CreateCollection();
	        XmlDescription<AzLogFilterSettings> descriptor = CreateXmlDescriptor();

	        m_iteratorForRepeatingColorFilters = null;
	        
	        using (WriteFile<AzLogFilterSettings> writeFile = collection.CreateSettingsWriteFile<AzLogFilterSettings>(Name))
	        {
		        writeFile.SerializeSettings(descriptor, this);
	        }
        }

        /*----------------------------------------------------------------------------
			%%Function:CreateFromFile
			%%Qualified:AzLog.AzLogFilterSettings.CreateFromFile
        ----------------------------------------------------------------------------*/
        public static AzLogFilterSettings CreateFromFile(string sName, ColorFilterColors colors)
        {
	        Collection collection = CreateCollection();
	        XmlDescription<AzLogFilterSettings> descriptor = CreateXmlDescriptor();

	        AzLogFilterSettings settings = new AzLogFilterSettings(colors);
	        settings.Name = sName;
	        
	        using (ReadFile<AzLogFilterSettings> writeFile = ReadFile<AzLogFilterSettings>.CreateSettingsFile(collection.GetFullPathName(sName)))
	        {
		        writeFile.DeSerialize(descriptor, settings);
	        }

	        return settings;
        }
        #endregion
	}
}