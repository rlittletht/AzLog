using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using NUnit.Framework;
using TCore.Settings;

namespace AzLog
{
    public class AzLogFilterSettings
    {
        public string Name { get; set; }
        public bool IsDirty { get; set; }

        public override string ToString() => Name;

        // We want to extract the filter and the colorfilter from AzLogView.
        // However, right now, the filter always starts with the min/max date filter, so we can
        // just easily add to the postfix condition (since we always have a condition).

        // we don't want to save that min/mac compare though -- this is dynamically set
        // during the creation of the view (likewise, when we change the min/max, we don't
        // want to dirty the filter. 
        
        // we will replace the date min/max comparision (which is two lines plus an AND operator)
        // with two constant (true) values.

        // so:

        //      Item[Date/Time] > min
        //      Item[Date/Time< < mac
        //      AND
        // becomes
        //      MinDateAlwaysTrue
        //      MacDateAlwaysTrue
        //      AND
    }
}