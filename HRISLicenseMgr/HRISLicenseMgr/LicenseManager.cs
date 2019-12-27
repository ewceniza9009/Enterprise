using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Access = Microsoft.Office.Interop.Access;
using FMISDao = Microsoft.Office.Interop.Access.Dao;

using Microsoft.VisualBasic;
using System.Reflection;

using System.Windows.Forms;
using System.Drawing;

using System.Management;
using System.IO;

namespace HRISLicenseMgr
{
    public class hris_license
    {
        //Color Reference
        //White = 16777215
        //Gray = 5855577
        //Black = 0

        private string licStatus = "Unlicensed Software";
        private const int licenseTrialDays = 0;

        private string getPassKey()
        {
            return "hris18";
        }

        private int GetCurrentUserId(Access.Application FMIS)
        {
            int DefaultUserId = 0;
            DefaultUserId = 0;

            Access.Dao.Database currDB = FMIS.CurrentDb();
            Access.Dao.Recordset rs = currDB.OpenRecordset("SELECT CurrentUserId FROM SysCurrent", Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

            if (rs.RecordCount > 0)
            {
                rs.MoveFirst();
                DefaultUserId = rs.Fields["CurrentUserId"].Value;
            }

            return DefaultUserId;
        }

        private int GetFormId(Access.Application FMIS, string strFormName)
        {
            int DefaultFormId = 0;
            DefaultFormId = 0;

            Access.Dao.Database currDB = FMIS.CurrentDb();
            Access.Dao.Recordset rs = currDB.OpenRecordset("SELECT Id FROM SysForm WHERE FormName='" + strFormName + "'", Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

            if (rs.RecordCount > 0)
            {
                rs.MoveFirst();
                DefaultFormId = rs.Fields["Id"].Value;
            }

            return DefaultFormId;
        }

        public bool OpenForm(Access.Application FMIS, Access.Form FrmActiveForm, int UserId, bool CheckRecordLocked)
        {
            bool RetVal = false;

            try
            {
                string strCriteria = "";
                int countRec = 0;

                string rawCode = "";
                string licenseCode = "";
                bool licensedSoftware = false;

                rawCode = GetWorkstationCode();
                licenseCode = GetLicenseCode();

                if (Decryption(licenseCode) == rawCode)
                {
                    licensedSoftware = true;
                }
                else if (licenseCode.ToUpper() == "hris_trial")
                {
                    var trialExpirationDate = GetDateStartTrial().Date.AddDays(licenseTrialDays + GetTrialExt());

                    if (trialExpirationDate > DateTime.Now.Date)
                    {
                        licensedSoftware = true;
                        licStatus = LicenseStatus(licenseCode.ToUpper());
                        GetLicenseStatus();
                    }
                    else if (DateTime.Now.Date < GetDateStartTrial().Date)
                    {
                        MessageBox.Show("Bad date, Please contact our support team to resolve this issue", "hris_license", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    else
                    {
                        licensedSoftware = false;
                        MessageBox.Show("hris_trial period expired, Please contact our support team to resolve this issue", "hris_license", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                if (licensedSoftware)
                {
                    strCriteria = "UserId=" + UserId + " AND FormId=" + GetFormId(FMIS, FrmActiveForm.Name);

                    Access.Dao.Database currDB = FMIS.CurrentDb();
                    Access.Dao.Recordset rs = currDB.OpenRecordset("SELECT Id FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                    if (rs.RecordCount > 0)
                    {
                        countRec++;
                    }

                    if (countRec == 0)
                    {
                        RetVal = false;
                    }
                    else
                    {
                        RetVal = true;
                    }

                    licStatus = LicenseStatus(licenseCode.ToUpper());
                    GetLicenseStatus();

                    rs.Close();
                    rs = null;
                }
                else
                {
                    MessageBox.Show("No hris_license.", "Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }

            return RetVal;
        }

        public bool OpenReport(Access.Application FMIS, string ReportName, Access.AcView view, int pageType)
        {
            bool RetVal = false;

            string strCriteria = "";
            int UserId = 0;

            strCriteria = "UserId=" + UserId + " AND FormId=" + GetFormId(FMIS, ReportName);

            FMISDao.Recordset rs = FMIS.CurrentDb().OpenRecordset("SELECT Id FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

            if (rs.RecordCount == 0)
            {
                MessageBox.Show("No rights", "Security", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (view == Access.AcView.acViewPreview)
                {
                    FMIS.DoCmd.OpenReport(ReportName, Access.AcView.acViewPreview);
                }
                else if (view == Access.AcView.acViewReport)
                {
                    FMIS.DoCmd.OpenReport(ReportName, Access.AcView.acViewReport);
                }
                else
                {
                    FMIS.DoCmd.OpenReport(ReportName);
                }

                RetVal = true;
            }

            return RetVal;
        }

        public void LockForm(Access.Application FMIS, Access.Form FrmActiveForm, bool BolLocked, int UserId, bool CheckRecordLocked)
        {
            string strCriteria = "";
            FMISDao.Recordset rs = null;

            bool CanAdd = false;
            bool CanEdit = false;
            bool CanDelete = false;
            bool CanLock = false;
            bool CanUnlock = false;
            bool CanPrint = false;

            CheckRecordLocked = false;

            UserId = GetCurrentUserId(FMIS);

            if (OpenForm(FMIS, FrmActiveForm, UserId, CheckRecordLocked))
            {
                strCriteria = "UserId=" + UserId + " AND FormId=" + GetFormId(FMIS, FrmActiveForm.Name);

                Access.Dao.Database currDB = FMIS.CurrentDb();
                rs = currDB.OpenRecordset("SELECT * FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);


                if (rs.RecordCount > 0)
                {
                    rs.MoveFirst();

                    CanAdd = rs.Fields["CanAdd"].Value;
                    CanEdit = rs.Fields["CanEdit"].Value;
                    CanDelete = rs.Fields["CanDelete"].Value;
                    CanLock = rs.Fields["CanLock"].Value;
                    CanUnlock = rs.Fields["CanUnlock"].Value;
                    CanPrint = rs.Fields["CanPrint"].Value;

                }
                rs.Close();
                rs = null;
            }


            if (FrmActiveForm.Name.Substring(0, 3).ToUpper() == "REP")
            {
                try
                {
                    foreach (Access.Control objControl in FrmActiveForm.Controls)
                    {
                        if (objControl.Name.Substring(0, 3).ToUpper() == "CMD")
                        {
                            Access.CommandButton cmdControl = ((Access.CommandButton)objControl);
                            cmdControl.Enabled = true;

                            switch (cmdControl.Name)
                            {
                                case "cmdPrint":
                                    if (CanPrint == false) cmdControl.Enabled = false; break;
                                case "cmdPreview":
                                    if (CanPrint == false) cmdControl.Enabled = false; break;
                                case "cmdView":
                                    if (CanPrint == false) cmdControl.Enabled = false; break;
                            }
                        }
                    }
                }

                catch
                {
                    FrmActiveForm.Form.Section["FormHeader"].BackColor = 15921906;
                    FrmActiveForm.Form.Section["Detail"].BackColor = 10921638;

                    string appDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    foreach (Access.Control control in FrmActiveForm.Section["FormHeader"].Controls)
                    {
                        try
                        {
                            if (Information.TypeName(control).ToUpper() == "IMAGECLASS")
                            {
                                Access.Image FormIcons = (Access.Image)control;
                                string strImageName = FrmActiveForm.Name.Substring("SYS".Length, (FrmActiveForm.Name.Length) - "SYS".Length);

                                if (strImageName.ToUpper().Contains("DETAIL"))
                                {
                                    strImageName = strImageName.Substring(0, (strImageName.Length - "DETAIL".Length));
                                }
                                else if (FrmActiveForm.Name.Substring(0, 3).ToUpper() == "REP")
                                {
                                    strImageName = "Print";
                                }

                                FormIcons.Picture = appDataPath + "\\icons\\" + strImageName + ".png";
                            }

                            if (Information.TypeName(control).ToUpper() == "COMMANDBUTTONCLASS")
                            {
                                Access.CommandButton CommandIcons = (Access.CommandButton)control;

                                CommandIcons.Picture = appDataPath + "\\icons\\command\\" + control.Name + ".png";
                                CommandIcons.PictureCaptionArrangement = Access.AcPictureCaptionArrangement.acGeneral;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message + "\nStackTrack: " + ex.StackTrace);
                        }
                    }

                    foreach (Access.Control ctl in FrmActiveForm.Section["Detail"].Controls)
                    {
                        if (Information.TypeName(ctl).ToUpper() == "LABELCLASS")
                        {
                            Access.Label label = (Access.Label)ctl;
                            label.BackColor = 15921906;
                        }
                    }

                    foreach (Access.Control control in FrmActiveForm.Section["FormFooter"].Controls)
                    {
                        if (control.Name == "Image1")
                        {
                            Access.Image Image1 = (Access.Image)control;
                            Image1.Picture = appDataPath + "\\etechLogo.png";

                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (Access.Control objControl in FrmActiveForm.Controls)
                {
                    if (objControl.Name.Substring(((objControl.Name.Length) - 3), 3).ToUpper() == "Mem".ToUpper())
                    {
                        if (Information.TypeName(objControl).ToUpper() == "TEXTBOXCLASS")
                        {
                            Access.TextBox inputControl = ((Access.TextBox)objControl);
                            inputControl.Locked = BolLocked;

                            if (BolLocked)
                            {
                                inputControl.ForeColor = 16777215;
                                inputControl.BackColor = 5855577;
                            }
                            else
                            {
                                inputControl.ForeColor = 0;
                                inputControl.BackColor = 16777215;
                            }
                        }
                        else if (Information.TypeName(objControl).ToUpper() == "LABELCLASS")
                        {
                            Access.Label inputControl = ((Access.Label)objControl);

                            
                            if (BolLocked)
                            {
                                inputControl.ForeColor = 0; //16777215;
                                inputControl.BackColor = 5855577;  //5855577;
                            }
                            else
                            {
                                inputControl.ForeColor = 0;
                                inputControl.BackColor = 16777215;
                            }
                        }
                        else if (Information.TypeName(objControl).ToUpper() == "CHECKBOXCLASS")
                        {
                            Access.CheckBox inputControl = ((Access.CheckBox)objControl);
                            inputControl.Locked = BolLocked;
                        }
                        else if (Information.TypeName(objControl).ToUpper() == "COMBOBOXCLASS")
                        {
                            Access.ComboBox inputControl = ((Access.ComboBox)objControl);
                            inputControl.Locked = BolLocked;

                            if (BolLocked)
                            {
                                inputControl.ForeColor = 16777215;
                                inputControl.BackColor = 5855577;
                            }
                            else
                            {
                                inputControl.ForeColor = 0;
                                inputControl.BackColor = 16777215;
                            }
                        }
                    }
                    else if (objControl.Name.Substring(0, 3).ToUpper() == "CMD")
                    {
                        Access.CommandButton cmdControl = ((Access.CommandButton)objControl);
                        cmdControl.Enabled = true;

                        cmdControl.BackColor = 10921638;
                        cmdControl.ForeColor = 0;

                        switch (cmdControl.Name)
                        {
                            case "cmdAdd":
                                if (CanAdd == false) cmdControl.Enabled = false; break;
                            case "cmdEdit":
                                if (CanEdit == false) cmdControl.Enabled = false; break;
                            case "cmdDelete":
                                if (CanDelete == false) cmdControl.Enabled = false; break;
                            case "cmdLock":
                                if (CanLock == false) cmdControl.Enabled = false; break;
                            case "cmdUnlock":
                                if (CanUnlock == false) cmdControl.Enabled = false; break;
                            case "cmdPrint":
                                if (CanPrint == false) cmdControl.Enabled = false; break;
                            case "cmdPreview":
                                if (CanPrint == false) cmdControl.Enabled = false; break;
                            case "cmdView":
                                if (CanPrint == false) cmdControl.Enabled = false; break;
                        }
                    }
                }

                FrmActiveForm.Form.Section["FormHeader"].BackColor = 15921906;
                FrmActiveForm.Form.Section["Detail"].BackColor = 10921638;

                string appDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                foreach (Access.Control control in FrmActiveForm.Section["FormHeader"].Controls)
                {
                    try
                    {
                        //if (Information.TypeName(control).ToUpper() == "IMAGECLASS")
                        //{
                        //    Access.Image FormIcons = (Access.Image)control;
                        //    string strImageName = FrmActiveForm.Name.Substring("SYS".Length, (FrmActiveForm.Name.Length) - "SYS".Length);

                        //    if (strImageName.ToUpper().Contains("DETAIL"))
                        //    {
                        //        strImageName = strImageName.Substring(0, (strImageName.Length - "DETAIL".Length));
                        //    }
                        //    else if (FrmActiveForm.Name.Substring(0, 3).ToUpper() == "REP")
                        //    {
                        //        strImageName = "Print";
                        //    }

                        //    FormIcons.Picture = appDataPath + "\\icons\\" + strImageName + ".png";
                        //}

                        if (Information.TypeName(control).ToUpper() == "COMMANDBUTTONCLASS")
                        {
                            Access.CommandButton CommandIcons = (Access.CommandButton)control;

                            CommandIcons.Picture = appDataPath + "\\icons\\command\\" + control.Name + ".png";
                            CommandIcons.PictureCaptionArrangement = Access.AcPictureCaptionArrangement.acGeneral;
                        }

                        if (Information.TypeName(control).ToUpper() == "LABELCLASS")
                        {
                            Access.Label label = (Access.Label) control;

                            label.ForeColor = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message + "\nStackTrack: " + ex.StackTrace);
                    }
                }

                foreach (Access.Control control in FrmActiveForm.Section["FormFooter"].Controls)
                {
                    if (control.Name == "Image1")
                    {
                        Access.Image FormIcons = (Access.Image)control;

                        FormIcons.Picture = appDataPath + "\\etechLogo.png";
                        break;
                    }
                }
            }
        }

        public void LockSubForm(Access.Application FMIS, Access.SubForm FrmActiveSubForm, bool BolLocked, int Level, string strCriteria)
        {
            Access.Dao.Database currDB = FMIS.CurrentDb();
            Access.Dao.Recordset rs = null;

            try
            {
                foreach (Access.Control objControl in FrmActiveSubForm.Controls)
                {
                    string ControlType = "";
                    ControlType = Information.TypeName(objControl).ToUpper();

                    string appDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                    foreach (Access.Control control in FrmActiveSubForm.Form.Section["Detail"].Controls)
                    {

                        if (Information.TypeName(control).ToUpper() == "COMMANDBUTTONCLASS" &&
                            (control.Name.ToUpper() == "CMDEDIT" || control.Name.ToUpper() == "CMDDELETE")
                            )
                        {
                            try
                            {
                                Access.CommandButton CommandIcons = (Access.CommandButton)control;

                                CommandIcons.Picture = appDataPath + "\\icons\\command\\" + control.Name + ".png";
                                CommandIcons.PictureCaptionArrangement = Access.AcPictureCaptionArrangement.acGeneral;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }

                    if (ControlType == "TEXTBOXCLASS" || ControlType == "LABELCLASS" || ControlType == "CHECKBOXCLASS" || ControlType == "COMBOBOXCLASS" || ControlType == "COMMANDBUTTONCLASS")
                    {
                        if (objControl.Name.Substring(((objControl.Name.Length) - 3)).ToUpper() == "MEM")
                        {
                            if (Information.TypeName(objControl).ToUpper() == "TEXTBOXCLASS")
                            {
                                Access.TextBox inputControl = ((Access.TextBox)objControl);
                                inputControl.Locked = BolLocked;

                                if (BolLocked)
                                {
                                    inputControl.ForeColor = 16777215;
                                    inputControl.BackColor = 5855577;
                                }
                                else
                                {
                                    inputControl.ForeColor = 0;
                                    inputControl.BackColor = 16777215;
                                }
                            }
                            else if (Information.TypeName(objControl).ToUpper() == "LABELCLASS")
                            {
                                Access.Label inputControl = ((Access.Label)objControl);

                                if (BolLocked)
                                {
                                    inputControl.ForeColor = 16777215;
                                    inputControl.BackColor = 5855577;
                                }
                                else
                                {
                                    inputControl.ForeColor = 0;
                                    inputControl.BackColor = 16777215;
                                }
                            }
                            else if (Information.TypeName(objControl).ToUpper() == "CHECKBOXCLASS")
                            {
                                Access.CheckBox inputControl = ((Access.CheckBox)objControl);
                                inputControl.Locked = BolLocked;
                            }
                            else if (Information.TypeName(objControl).ToUpper() == "COMBOBOXCLASS")
                            {
                                Access.ComboBox inputControl = ((Access.ComboBox)objControl);
                                inputControl.Locked = BolLocked;

                                if (BolLocked)
                                {
                                    inputControl.ForeColor = 16777215;
                                    inputControl.BackColor = 5855577;
                                }
                                else
                                {
                                    inputControl.ForeColor = 0;
                                    inputControl.BackColor = 16777215;
                                }
                            }
                        }
                        else if (objControl.Name.Substring(0, 3).ToUpper() == "CMD")
                        {
                            Access.CommandButton cmdControl = (Access.CommandButton)objControl;

                            cmdControl.BackColor = 10921638;
                            cmdControl.ForeColor = 0;

                            switch (objControl.Name)
                            {
                                case "cmdAdd":
                                    bool CanAdd = true;

                                    rs = currDB.OpenRecordset("SELECT canAdd FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanAdd = rs.Fields["canAdd"].Value;
                                    }

                                    if (!CanAdd)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    rs = null;
                                    break;
                                case "cmdEdit":
                                    bool CanEdit = true;

                                    rs = currDB.OpenRecordset("SELECT canEdit FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanEdit = rs.Fields["canEdit"].Value;
                                    }

                                    if (!CanEdit)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    rs = null;
                                    break;
                                case "cmdDelete":
                                    bool CanDelete = true;

                                    rs = currDB.OpenRecordset("SELECT canDelete FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanDelete = rs.Fields["canDelete"].Value;
                                    }

                                    if (!CanDelete)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    rs = null;
                                    break;
                                case "cmdLock":
                                    bool CanLock = true;

                                    rs = currDB.OpenRecordset("SELECT canLock FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);
                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanLock = rs.Fields["canLock"].Value;
                                    }

                                    if (!CanLock)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    rs = null;
                                    break;
                                case "cmdUnlock":
                                    bool CanUnlock = true;

                                    rs = currDB.OpenRecordset("SELECT canUnlock FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanUnlock = rs.Fields["canUnlock"].Value;
                                    }

                                    if (!CanUnlock)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    rs = null;
                                    break;
                                case "cmdPrint":
                                    bool CanPrint = true;

                                    rs = currDB.OpenRecordset("SELECT canPrint FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanPrint = rs.Fields["canPrint"].Value;
                                    }

                                    if (!CanPrint)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    rs = null;
                                    break;
                                case "cmdPreview":
                                    CanPrint = true;

                                    rs = currDB.OpenRecordset("SELECT canPrint FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanPrint = rs.Fields["canPrint"].Value;
                                    }

                                    if (!CanPrint)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    break;
                                case "cmdView":
                                    CanPrint = true;

                                    rs = currDB.OpenRecordset("SELECT canPrint FROM MstUserForm WHERE " + strCriteria, Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                                    if (rs.RecordCount > 0)
                                    {
                                        rs.MoveFirst();
                                        CanPrint = rs.Fields["canPrint"].Value;
                                    }

                                    if (!CanPrint)
                                    {
                                        cmdControl.Enabled = false;
                                    }
                                    rs.Close();
                                    break;
                            }
                        }
                    }
                }

                if (BolLocked)
                {
                    FrmActiveSubForm.Form.Section["Detail"].BackColor = 7893851;
                }
                else
                {
                    FrmActiveSubForm.Form.Section["Detail"].BackColor = 16777215;
                }

                FrmActiveSubForm.Form.Section["FormHeader"].BackColor = 15921906;
                FrmActiveSubForm.Form.Section["FormFooter"].BackColor = 5855577;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public string GetWorkstationCode()
        {
            string SerialNumber = "";
            ManagementObjectSearcher searcher = null;
            try
            {
                searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");

                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    if (wmi_HD["SerialNumber"] != null)
                    {
                        SerialNumber = wmi_HD["SerialNumber"].ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return SerialNumber.Trim();
        }

        public string Decryption(string DataIn)
        {
            string strDataOut = "";

            string CodeKey = getPassKey();

            int intXOrValue1 = 0;
            int intXOrValue2 = 0;

            try
            {
                for (int lonDataPtr = 0; lonDataPtr < (DataIn.Length / 2); lonDataPtr++)
                {
                    string strXOrValue1 = ("&H" + DataIn.Substring((2 * lonDataPtr), 2));
                    int intStartSubStrCodeKey = (lonDataPtr % CodeKey.Length) + 1;

                    intXOrValue1 = (int)Conversion.Val(strXOrValue1);
                    intXOrValue2 = Strings.Asc(CodeKey.Substring(intStartSubStrCodeKey == CodeKey.Length ? 0 : intStartSubStrCodeKey, 1));

                    strDataOut = strDataOut + (char)(intXOrValue1 ^ intXOrValue2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.StackTrace);
            }

            return strDataOut;
        }

        private string Encryption(string CodeKey, string DataIn)
        {
            string strDataOut = "";

            int temp = 0;
            string tempString = "";

            int intXOrValue1 = 0;
            int intXOrValue2 = 0;

            try
            {
                for (int lonDataPtr = 0; lonDataPtr < DataIn.Length; lonDataPtr++)
                {
                    intXOrValue1 = Strings.Asc(DataIn.Substring(lonDataPtr, 1));

                    int intStartSubStrCodeKey = (lonDataPtr % CodeKey.Length) + 1;

                    intXOrValue2 = Strings.Asc(CodeKey.Substring(intStartSubStrCodeKey == CodeKey.Length ? 0 : intStartSubStrCodeKey, 1));

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

            return strDataOut;
        }

        public void hris_trial()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string fullFolderPath = Path.Combine(appDataPath, "eTech");
                string txtPath = Path.Combine(fullFolderPath, "hris_trial.lic");

                if (!System.IO.File.Exists(txtPath))
                {
                    Directory.CreateDirectory(fullFolderPath);

                    string EncryptedDateString = Cryptor.EncryptString(DateTime.Now.Date.ToShortDateString(), getPassKey());

                    using (StreamWriter writer = new StreamWriter(txtPath, true))
                    {
                        writer.WriteLine(EncryptedDateString);
                        writer.WriteLine("Ext15");
                        writer.WriteLine("Please don't change the two lines above this text unless you know what your doing.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }

        public string GetLicenseCode()
        {
            string RetVal = "NA";

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullFolderPath = Path.Combine(appDataPath, "eTech");
            string txtPathLicensed = Path.Combine(fullFolderPath, "hris_license.lic");

            if (System.IO.File.Exists(txtPathLicensed))
            {
                string[] lines = System.IO.File.ReadAllLines(txtPathLicensed);
                RetVal = lines[0] ?? "NA";
            }

            return RetVal;
        }

        public void SaveLicenseCode(string hris_license)
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullFolderPath = Path.Combine(appDataPath, "eTech");
            string txtPath = Path.Combine(fullFolderPath, "hris_license.lic");

            if (!System.IO.File.Exists(txtPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }
            using (StreamWriter writer = new StreamWriter(txtPath, false))
            {
                writer.WriteLine(hris_license);
                writer.WriteLine("Please don't change the line above this text unless you know what your doing.");
            }
        }

        private DateTime GetDateStartTrial()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullFolderPath = Path.Combine(appDataPath, "eTech");
            string txtPath = Path.Combine(fullFolderPath, "hris_trial.lic");

            string[] lines = System.IO.File.ReadAllLines(txtPath);

            var trialDate = Convert.ToDateTime(Cryptor.DecryptString(lines[0], getPassKey()));

            return trialDate;
        }

        private int GetTrialExt()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullFolderPath = Path.Combine(appDataPath, "eTech");
            string txtPath = Path.Combine(fullFolderPath, "hris_trial.lic");

            string[] lines = System.IO.File.ReadAllLines(txtPath);

            var ext = TrialExtensions.trialExt[lines[1]];

            return ext;
        }

        public string LicenseStatus(string lStatus)
        {
            string RetVal = "";
            if (lStatus.ToUpper() == "hris_trial")
            {
                DateTime trialDate = GetDateStartTrial().AddDays(licenseTrialDays + GetTrialExt());

                RetVal = "Expiry: " + trialDate.ToShortDateString() + ", " + Math.Round(trialDate.Subtract(DateTime.Now).TotalDays).ToString() + " day(s) left.";
            }
            else
            {
                RetVal = "Licensed Software";
            }

            return RetVal;
        }

        public string GetLicenseStatus()
        {
            return licStatus;
        }

        public void UnlicenseSoftware()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullFolderPath = Path.Combine(appDataPath, "eTech");

            string txtPathTrial = Path.Combine(fullFolderPath, "hris_trial.lic");
            string txtPathLicense = Path.Combine(fullFolderPath, "hris_license.lic");

            File.Delete(txtPathTrial);
            File.Delete(txtPathLicense);
        }
    }
}
