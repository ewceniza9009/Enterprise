using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FMISLicenseMgr
{
    public class FinancialStatement
    {
        public double NormalBalance(double DebitAmount, double CreditAmount, int AccountCategoryId) {
            double RetVal = 0;

            switch (AccountCategoryId)
            {
                case 1:  //Asset
                    RetVal = DebitAmount - CreditAmount;
                    break;
                case 2:  //Liability
                    RetVal = CreditAmount - DebitAmount;
                    break;
                case 4:  //Equity
                    RetVal = CreditAmount - DebitAmount;
                    break;
                case 5:  //Income
                    RetVal = CreditAmount - DebitAmount;
                    break;
                case 6:  //'Expense
                    RetVal = DebitAmount - CreditAmount;
                    break;
                default:
                    RetVal = 0;
                    break;
            }
            return RetVal;
        }
    }
}
