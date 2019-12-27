using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRISLicenseMgr
{
    static class Program
    {
        static void Main()
        {
            hris_license lic = new hris_license();

            string x = lic.GetLicenseCode();
        }
    }
}
