using System;
using System.Data;
using System.Text.RegularExpressions;
using static Global;

public partial class prg2004Upd : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2004";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 帳號資料修改</div>";

                string sn = Request.QueryString["sn"];

                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷商品分類編號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg20/prg2004.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vEmployee WHERE empGrpId NOT IN ('admin','0000') AND sn='{sn}'";
                    DataTable empTbl = getTbl.table("WP", SqlComm);
                    if (empTbl.Rows.Count == 0) { Response.Redirect("/prg20/prg2004.aspx"); }
                    else
                    {
                        DataRow row0 = empTbl.Rows[0];
                        Label2.Text = $"<div class='page-main emp-upd-main'>" +
                            $"<input type='hidden' name='sn' value='{sn}' />" +
                            "<div class='input-row'>" +
                                "<div class='input-title'>登入帳號</div>" +
                                "<div class='input-text' style='display:flex;align-items:center;'>" +
                                    $"<input type='text' name='empId' class='form-control' data-alt='MI_登入帳號' maxlength='10' style='width:112px;margin-left:4px; margin-right:35px;' value='{row0["empId"]}' />" +
                                    "<div style='display:flex; align-items:center;'>" +
                                        "<span class='input-tag'>停用︰</span>" +
                                        $"<input type='radio' name='isStop' id='isStopN' class='form-control' value='N' {(row0["isStop"].ToString() == "N" ? "checked" : "")} style='margin-left: 2px;' /><label for='isStopN'>否</label>" +
                                        $"<input type='radio' name='isStop' id='isStopY' class='form-control' value='Y' {(row0["isStop"].ToString() == "Y" ? "checked" : "")} /><label for='isStopY'>是</label>" +
                                    "</div>" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style=''>" +
                                "<div class='input-title'>帳號名稱</div>" +
                                "<div class='input-text'>" +
                                    $"<input type='text' name='empName' class='form-control' data-alt='MI_帳號名稱' maxlength='50' style='width:260px;margin-left:4px;margin-right:74px;' value='{row0["empName"]}' />" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:center;'>" +
                                "<div class='input-title'>管理人員</div>" +
                                "<div class='input-text' style='display:flex;align-items:center;'>" +
                                    $"<input type='radio' name='isAdmin' id='isAdminN' class='form-control' value='N' {(row0["isAdmin"].ToString() == "N" ? "checked" : "")} style='margin-left: 2px;' /><label for='isAdminN'>否</label>" +
                                    $"<input type='radio' name='isAdmin' id='isAdminY' class='form-control' value='Y' {(row0["isAdmin"].ToString() == "Y" ? "checked" : "")} /><label for='isAdminY'>是</label>" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items: flex-start;'>" +
                                "<div class='input-title'>群　　組</div>" +
                                "<div class='input-text'>" +
                                    "<select name='empGrpId' class='form-control' dat-alt='MI_員工群組'>";
                                        SqlComm = $"SELECT * FROM WP_EmpGrp WHERE isStop ='N' AND empGrpId NOT IN ('admin','0000') ORDER BY empGrpId";
                                        DataTable empGrpTbl = getTbl.table("WP", SqlComm);
                                        foreach (DataRow row in empGrpTbl.Rows)
                                        {
                                            Label2.Text += $"<option value='{row["empGrpId"]}' {(row0["empGrpId"].ToString() == row["empGrpId"].ToString() ? "selected" : "")} >{row["empGrpId"]} {row["empGrpName"]}</option>";
                                        }
                                    Label2.Text += "</select>" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items: flex-start;'>" +
                                "<div class='input-title'>備　　註</div>" +
                                "<div class='input-text'>" +
                                    $"<textarea name='memo' maxlength='500' class='form-control' rows='2' style='width: 688px;'>{row0["memo"]}</textarea>" +
                                "</div>" +
                            "</div>" +
                        "</div>";
                    }
                }
            }
        }
    }
}