using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Access = Microsoft.Office.Interop.Access;

using System.Windows.Forms;

namespace FMISLicenseMgr
{
    public class Link
    {
        public bool LinkTables(Access.Application FMIS, Access.Dao.Recordset accRS, Access.Form accForm) {
            bool RetVal = false;

            try
            {
                Access.Dao.TableDef tdf = null;

                accForm.TimerInterval = 0;

                Access.Dao.Recordset rs = accRS;
                Access.Dao.Recordset rsODBC = FMIS.CurrentDb().OpenRecordset("SELECT * FROM SysTableODBCSettings", Access.Dao.RecordsetTypeEnum.dbOpenDynaset, Access.Dao.RecordsetOptionEnum.dbSeeChanges);

                rsODBC.MoveFirst();

                int IntTableNumber = 1;
                int IntNumberOfTables = rs.RecordCount;

                string StrDSNName = rsODBC.Fields["DSNName"].Value;
                string StrDatabase = rsODBC.Fields["Database"].Value;
                string strUser = rsODBC.Fields["User"].Value;
                string StrPassword = rsODBC.Fields["Password"].Value;
                string StrServer = rsODBC.Fields["Server"].Value;
                string StrDescription = rsODBC.Fields["Description"].Value;

                string StrConnectionString = "ODBC;DRIVER=SQL Server;" +
                                             "SERVER=" + StrServer + ";" +
                                             "DATABASE=" + StrDatabase + ";" +
                                             "UID=" + strUser + ";" +
                                             "PWD=" + StrPassword;

                if (rs.RecordCount > 0) rs.MoveFirst();

                int intTraverseEOF = 0;
                while (intTraverseEOF < rs.RecordCount) {
                    if (TableExist(FMIS, rs.Fields["LocalTable"].Value))
                    {
                        FMIS.DoCmd.DeleteObject(Access.AcObjectType.acTable, rs.Fields["LocalTable"].Value);

                        tdf = FMIS.CurrentDb().CreateTableDef(rs.Fields["LocalTable"].Value, Access.Dao.TableDefAttributeEnum.dbAttachSavePWD);

                        tdf.Connect = StrConnectionString;
                        tdf.SourceTableName = rs.Fields["SourceTable"].Value;

                        FMIS.CurrentDb().TableDefs.Append(tdf);
                        FMIS.CurrentDb().TableDefs.Refresh();

                        bool Box1Found = false;
                        bool Box2Found = false;

                        Access.Rectangle Box1 = null;
                        Access.Rectangle Box2 = null;

                        foreach (Access.Control control in accForm.Section["Detail"].Controls)
                        {
                            if (control.Name == "Percentage")
                            {
                                Access.TextBox Percentage = (Access.TextBox)control;

                                double dblPercentageValue = (IntTableNumber / IntNumberOfTables) * 100;
                                string strPercentage = Math.Truncate(dblPercentageValue).ToString().Trim() + "% Complete";

                                Percentage.Value = strPercentage;
                            }

                            if (control.Name == "Box1")
                            {
                                Box1 = (Access.Rectangle)control;
                            }

                            if (control.Name == "Box2")
                            {
                                Box2 = (Access.Rectangle)control;
                            }

                            if (Box1Found && Box2Found)
                            {
                                double dblBoxValue = Box2.Width * (IntTableNumber / IntNumberOfTables);

                                //Box2.Width = (short)Math.Truncate(dblBoxValue);
                            }
                        }

                        //accForm.Repaint();

                        //rs.MoveNext();
                        //IntTableNumber = IntTableNumber + 1;

                        //Application.DoEvents();

                        intTraverseEOF++;
                    }

                    rs.Close();
                    rs = null;

                    FMIS.DoCmd.Close(Access.AcObjectType.acForm, accForm.Name);

                    FMIS.DoCmd.OpenForm("SysLogin");
                }

                RetVal = true;
            }
            catch (Exception ex){
                RetVal = false;

                MessageBox.Show("Error: " + ex.Message + "\nStackTrack: " + ex.StackTrace);
            }

            return RetVal;
        }

        public bool TableExist(Access.Application FMIS, string strTableName) {
            bool ifExist = false;

            FMIS.CurrentDb().TableDefs.Refresh();
            for (int i = 0; i < FMIS.CurrentDb().TableDefs.Count - 1; i++) {
                if (strTableName == FMIS.CurrentDb().TableDefs[i].Name) {
                    ifExist = true;
                    break;
                }
            }

                return ifExist;
        }
    }
}
