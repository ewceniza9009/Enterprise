using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;

namespace LicenseGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateKey_Click(object sender, RoutedEventArgs e)
        {
            if (this.WorkstationCode.Text != "" && this.CodeKey.Text != "")
            {
                string strDataOut = "";

                int temp = 0;
                string tempString = "";

                int intXOrValue1 = 0;
                int intXOrValue2 = 0;

                try
                {
                    for (int lonDataPtr = 0; lonDataPtr < this.WorkstationCode.Text.Length; lonDataPtr++)
                    {
                        intXOrValue1 = Strings.Asc(this.WorkstationCode.Text.Substring(lonDataPtr, 1));

                        int intStartSubStrCodeKey = (lonDataPtr % this.CodeKey.Text.Length) + 1;

                        intXOrValue2 = Strings.Asc(this.CodeKey.Text.Substring(intStartSubStrCodeKey == this.CodeKey.Text.Length ? 0 : intStartSubStrCodeKey, 1));

                        temp = (intXOrValue1 ^ intXOrValue2);
                        tempString = Conversion.Hex(temp);

                        if (tempString.Length == 1)
                        {
                            tempString = "0" + tempString;
                        }

                        strDataOut = strDataOut + tempString;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
                }
                this.License.Text = strDataOut;
            }
            else {
                MessageBox.Show("Please fill up Workstation Code and Key properly", "License", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
