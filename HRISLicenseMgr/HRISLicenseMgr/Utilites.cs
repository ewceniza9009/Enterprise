using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HRISLicenseMgr
{
    public class Utilities
    {
        public bool CompareString(string String1, string String2) {
            bool RetVal = false;
            string[] String1Words = null;
            string[] String2Words = null;

            int i = 0;
            int ii = 0;

            String1Words = String1.Split(' ');
            String2Words = String2.Split(' ');

            i = 0;
            ii = 0;

            foreach(var s in String1Words){
                i = i + 1;
            }

            foreach (var s in String2Words) {
                bool ReturnResult = false;
                foreach (string ss in String1Words) {
                    if (s == ss) {
                        ReturnResult = true;
                        return true;
                    }
                }
                if (ReturnResult) {
                    ii = ii + 1;
                }
            }

            if (i == ii)
            {
                RetVal = true;
            }
            else {
                RetVal = false;
            }         
            return RetVal;
        }

        public string NtoW(double N) {
            return EnglishInvoke(N);
        }

        private string EnglishInvoke(double N) {
            string RetVal = "";

            const double Thousand = 1000;
            const double Million = Thousand * Thousand;
            const double Billion = Thousand * Million;
            const double Trillion = Billion * Billion;

            string Buf = "";
            double Frac = 0;
            int AtleastOne = 0;

            if (N == 0) {
                RetVal = "Zero";
            }

            if (N < 0)
            {
                Buf = "negative ";
            }
            else {
                Buf = "";
            }

            Frac = Math.Abs(N - Math.Truncate(N));

            if (N < 0 || Frac != 0) {
                N = Math.Abs(Math.Truncate(N));
            }

            AtleastOne = N >= 1 ? 1 : 0;

            if (N >= Trillion) {
                Buf = Buf + EnglishDigitGroup(int.Parse(Math.Truncate(N / Trillion).ToString())) + " trillion";
                N = N - int.Parse(Math.Truncate(N / Trillion).ToString()) * Trillion;
                if (N >= 1) {
                    Buf = Buf + " ";
                }
            }

            if (N >= Billion)
            {
                Buf = Buf + EnglishDigitGroup(int.Parse(Math.Truncate(N / Billion).ToString())) + " billion";
                N = N - int.Parse(Math.Truncate(N / Billion).ToString()) * Billion;
                if (N >= 1)
                {
                    Buf = Buf + " ";
                }
            }

            if (N >= Million)
            {
                Buf = Buf + EnglishDigitGroup(int.Parse(Math.Truncate(N / Million).ToString())) + " million";
                N = N - int.Parse(Math.Truncate(N / Million).ToString()) * Million;
                if (N >= 1)
                {
                    Buf = Buf + " ";
                }
            }

            if (N >= Thousand)
            {
                Buf = Buf + EnglishDigitGroup(int.Parse(Math.Truncate(N / Thousand).ToString())) + " thousand";
                N = N - int.Parse(Math.Truncate(N / Thousand).ToString()) * Thousand;
                if (N >= 1)
                {
                    Buf = Buf + " ";
                }
            }

            if (N >= 1) {
                Buf = Buf + EnglishDigitGroup(int.Parse(N.ToString()));
            }

            if (Frac == 0) {
                Buf = Buf + " pesos";
            }
            else if (int.Parse(Math.Truncate((Frac * 100)).ToString()) == (Frac * 100))
            {
                if (AtleastOne == 1)
                {
                    Buf = Buf + " pesos and ";
                }
                Buf = Buf + String.Format("{0:00}", (Frac * 100)) + "/100";
            }
            else {
                if (AtleastOne == 1) {
                    Buf = Buf + " pesos and ";
                }
                Buf = Buf + String.Format("{0:0000}", (Frac * 10000)) + "/10000";
            }

            RetVal = (Buf + " only ").Trim().ToUpper();

            return RetVal;
        }

        private string EnglishDigitGroup(int N)
        {
            string RetVal = "";

            const string Hundred = " hundred";

            const string One = "one";
            const string Two = "two";
            const string Three = "three";
            const string Four = "four";
            const string Five = "five";
            const string Six = "six";
            const string Seven = "seven";
            const string Eight = "eight";
            const string Nine = "nine";

            string Buf = "";

            bool Flag = false;

            //Do Hundreds
            switch (N / 100)
            { 
                case 0: Buf = ""; Flag = false; break;
                case 1: Buf = One + Hundred; Flag = true; break;
                case 2: Buf = Two + Hundred; Flag = true; break;
                case 3: Buf = Three + Hundred; Flag = true; break;
                case 4: Buf = Four + Hundred; Flag = true; break;
                case 5: Buf = Five + Hundred; Flag = true; break;
                case 6: Buf = Six + Hundred; Flag = true; break;
                case 7: Buf = Seven + Hundred; Flag = true; break;
                case 8: Buf = Eight + Hundred; Flag = true; break;
                case 9: Buf = Nine + Hundred; Flag = true; break;
            }

            if (Flag != false) N = N % 100;

            if (N > 0)
            {
                if (Flag != false)
                {
                    Buf = Buf + " ";
                }
            }
            else
            {
                RetVal = Buf;
                return "";
            }

            //Do Tens (except teens)
            switch (N / 10)
            {
                case 0: Flag = false; break;
                case 1: Flag = false; break;
                case 2: Buf = Buf + "twenty"; Flag = true; break;
                case 3: Buf = Buf + "thirty"; Flag = true; break;
                case 4: Buf = Buf + "forty"; Flag = true; break;
                case 5: Buf = Buf + "fifty"; Flag = true; break;
                case 6: Buf = Buf + "sixty"; Flag = true; break;
                case 7: Buf = Buf + "seventy"; Flag = true; break;
                case 8: Buf = Buf + "eighty"; Flag = true; break;
                case 9: Buf = Buf + "ninety"; Flag = true; break;
            }

            if (Flag != false) N = N % 10;

            if (N > 0)
            {
                if (Flag != false)
                {
                    Buf = Buf + "-";
                }
            }
            else
            {
                RetVal = Buf;
                return "";
            }

            switch (N)
            {
                case 0: break;
                case 1: Buf = Buf + One; break;
                case 2: Buf = Buf + Two; break;
                case 3: Buf = Buf + Three; break;
                case 4: Buf = Buf + Four; break;
                case 5: Buf = Buf + Five; break;
                case 6: Buf = Buf + Six; break;
                case 7: Buf = Buf + Seven; break;
                case 8: Buf = Buf + Eight; break;
                case 9: Buf = Buf + Nine; break;
                case 10: Buf = Buf + "ten"; break;
                case 11: Buf = Buf + "eleven"; break;
                case 12: Buf = Buf + "twelve"; break;
                case 13: Buf = Buf + "thirteen"; break;
                case 14: Buf = Buf + "fourteen"; break;
                case 15: Buf = Buf + "fifteen"; break;
                case 16: Buf = Buf + "sixteen"; break;
                case 17: Buf = Buf + "seventeen"; break;
                case 18: Buf = Buf + "eighteen"; break;
                case 19: Buf = Buf + "nineteen"; break;
            }

            RetVal = Buf;

            return RetVal;
        }

    }
}
