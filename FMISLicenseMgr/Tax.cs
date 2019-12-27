using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMISLicenseMgr
{
    public class Tax
    {
        public double ComputeTaxAmount(double Amount, int TaxId, string TaxType, bool IsInclusive, double TaxPercentage) {
            double RetVal = 0;

            double VATAmount = 0;
            double WTAXAmount = 0;

            if (TaxType == "VAT")
            {
                if (IsInclusive)
                {
                    VATAmount = Math.Round((Amount / (1 + (TaxPercentage / 100))) * (TaxPercentage / 100), 4);
                }
                else
                {
                    VATAmount = Math.Round(Amount * (TaxPercentage / 100), 4);
                }

                RetVal = VATAmount;
            }
            else if (TaxType == "WTAX")
            {
                if (IsInclusive)
                {
                    WTAXAmount = Math.Round((Amount / (1 + (TaxPercentage / 100))) * (TaxPercentage / 100), 4);
                }
                else
                {
                    WTAXAmount = Math.Round(Amount * (TaxPercentage / 100), 4);
                }
                RetVal = WTAXAmount;
            }
            else {
                RetVal = 0;
            }           
            return RetVal;
        }
    }
}
