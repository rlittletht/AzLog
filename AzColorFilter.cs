// ============================================================================
// A Z  C O L O R  F I L T E R
// ============================================================================

// AzColorFilter - maps a filter to a color (or set of colors) to apply to the
// view row
using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using TCore.PostfixText;

namespace AzLog
{
    public class AzColorFilter
    {
        public AzLogFilter Filter { get; set; }
        public Color BackColor { get; set; }
        public Color ForeColor { get; set; }

        public bool Marked { get; set; }
        
        public AzColorFilter()
        {
            Filter = new AzLogFilter();
            BackColor = Color.White;
            ForeColor = Color.Black;
        }

        public AzColorFilter(AzLogFilter filter, Color colorBack, Color colorFore)
        {
            Filter = filter;
            BackColor = colorBack;
            ForeColor = colorFore;
        }

        public bool Matches(PostfixText.IValueClient client)
        {
            return Filter.FEvaluate(client);
        }
    }
}