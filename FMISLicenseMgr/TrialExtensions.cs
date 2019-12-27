using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMISLicenseMgr
{
    class TrialExtensions
    {
        public static IDictionary<string, int> trialExt = new Dictionary<string, int>() { 
            {"Ext15", 15},
            {"Ext30", 30},
            {"Ext45", 45},
            {"Ext60", 60}
        };
    }
}
