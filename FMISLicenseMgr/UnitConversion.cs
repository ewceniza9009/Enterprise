using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMISLicenseMgr
{
    public class UnitConversion
    {
        public double ConvertToBaseQuantity(double Quantity, double Divisor) {
            return Quantity / Divisor;
        }

        public double ConvertToBaseCost(double Amount, double VATAmount, bool IsInclusive, double BaseQuantity) {

            double RetVal = 0;

            if (BaseQuantity == 0)
            {
                RetVal = 0;
            }
            else
            {
                if (IsInclusive)
                {
                    RetVal = Math.Round((Amount - VATAmount) / BaseQuantity, 4);
                }
                else
                {
                    RetVal = Math.Round(Amount / BaseQuantity, 4);
                }
            }    
            return RetVal;
        }
    }
}
