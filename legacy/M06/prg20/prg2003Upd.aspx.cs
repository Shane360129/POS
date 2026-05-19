using System;
using System.Data;
using System.Text.RegularExpressions;
using static Global;

public partial class prg2003Upd : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2003";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 會員修改</div>";

                string sn = Request.QueryString["sn"];

                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷商品分類編號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg20/prg2003.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vMember WHERE sn='{sn}'";
                    DataTable memTbl = getTbl.table("WP", SqlComm);
                    if (memTbl.Rows.Count == 0) { Response.Redirect("/prg20/prg2003.aspx"); }
                    else
                    {
                        DataRow row0 = memTbl.Rows[0];
                        Label2.Text = $"<div class='page-main mem-upd-main'>" +
                        $"<input type='hidden' name='sn' value='{sn}' />" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>會員分類</div>";
                            SqlComm = $"SELECT * FROM WP_MemKind WHERE memKExist ='Y' ORDER BY memKId";
                            DataTable pvTbl = getTbl.table("WP", SqlComm);
                            Label2.Text += $"<select name='memKId' class='form-control' dat-alt='MI_會員分類' style='margin-right:94px;'>";
                                foreach (DataRow row in pvTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["memKId"]}' {(row["memKId"].ToString() == row0["memKId"].ToString() ? "selected" : "")}>{row["memKId"]} {row["memKName"]}</option>";
                                }
                            Label2.Text += "</select>" +
                            "<div class='input-text'>" +
                                "<span class='input-tag'>狀態︰</span>" +
                                "<select name='isStop' class='form-control' style='margin-right:115px;'>" +
                                    $"<option value='N' {(row0["isStop"].ToString() == "N" ? "selected" : "")}>正常往來</option>" +
                                    $"<option value='Y' {(row0["isStop"].ToString() == "Y" ? "selected" : "")}>停止往來</option></select>" +
                                "<span class='input-tag'>預設售價︰</span>" +
                                "<select name='priceKind' class='form-control'>" +
                                    $"<option value='1' {(row0["priceKind"].ToString() == "1" ? "selected" : "")}>標準售價</option>" +
                                    $"<option value='2' {(row0["priceKind"].ToString() == "2" ? "selected" : "")}>最低應售價</option>" +
                                    $"<option value='3' {(row0["priceKind"].ToString() == "3" ? "selected" : "")}>會員價</option>" +
                                    $"<option value='4' {(row0["priceKind"].ToString() == "4" ? "selected" : "")}>大批價</option>" +
                                "</select>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row'>" +
                            "<div class='input-title'>會員資料</div>" +
                            "<div class='input-text'>" +
                                "<div style='display:flex;align-items:center;'>" +
                                    "<span class='input-tag'>會員編號︰</span>" +
                                    $"<input type='text' id='memId' name='memId' class='form-control' data-alt='MI_會員編號' maxlength='10' style='width:109px;margin-right:32px;' value='{row0["memId"]}' />" +
                                    "<span class='input-tag'>姓名︰</span>" +
                                    $"<input type='text' name='memName' class='form-control' data-alt='MI_會員姓名' maxlength='50' style='width:260px;margin-right:171px;' value='{row0["memName"]}' />" +
                                "</div>" +
                                "<div style='display: flex; align-items: center;margin-top:10px;'>" +
                                    "<span class='input-tag'>性別︰</span>" +
                                    $"<input type='radio' name='gender' id='genderF' class='form-control' value='F' {(row0["gender"].ToString() == "F" ? "checked" : "")} style='margin-left:2px;' /><label for='genderF'>女</label>" +
                                    $"<input type='radio' name='gender' id='genderM' class='form-control' value='M' {(row0["gender"].ToString() == "M" ? "checked" : "")} /><label for='genderM' style='margin-right:77px;'>男</label>" +
                                    $"<span class='input-tag'>生日︰</span><input type='text' class='form-control open-datepicker' name='birthday' size='10' readonly style='margin-right:93px;' value='{($"{row0["birthday"]:yyyy/MM/dd}" == "1911/01/01" ? "" : $"{row0["birthday"]:yyyy/MM/dd}")}' />" +
                                    "<span class='input-tag'>身份證字號︰</span>" +
                                    $"<input type='text' name='idNo' maxlength='10' class='form-control' style='width:118px;' value='{row0["idNo"]}' />" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>聯絡資訊</div>" +
                            "<div class='input-text'>" +
                                "<div>" +
                                    "<span class='input-tag'>電話︰</span>" +
                                    $"<input type='text' name='memTel' maxlength='40' class='form-control' style='width:259px;margin-right:63px;' value='{row0["memTel"]}' />" +
                                    "<span class='input-tag'>手機︰</span>" +
                                    $"<input type='text' name='memMobil' maxlength='40' class='form-control' style='width:259px;' value='{row0["memMobil"]}' />" +
                                "</div>" +
                                "<div style='display:flex;align-items:center;margin-top:10px;'>" +
                                    "<span class='input-tag'>傳真︰</span>" +
                                    $"<input type='text' name='memFax' maxlength='40' class='form-control' style='width:259px;margin-right:53px;' value='{row0["memFax"]}' />" +
                                    "<span class='input-tag'>Email︰</span>" +
                                    $"<input type='text' name='memEmail' maxlength='50' class='form-control' style='width:259px;' value='{row0["memEmail"]}' />" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>通訊地址</div>" +
                            "<div class='input-text'>" +
                                "<div class='city-change'>" +
                                    $"<select class='form-control city-id' name='memCityId' data-id='{row0["memCityId"]}' style='margin-right:4px;'></select>" +
                                    $"<select class='form-control zone-id' name='memZoneId' data-id='{row0["memZoneId"]}' style='margin-right:4px;width:110px;'></select>" +
                                    $"<input type='text' name='memAddr' maxlength='150' class='form-control' style='width:458px;' value='{row0["memAddr"]}' />" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>送貨地址</div>" +
                            "<div class='input-text'>" +
                                "<div style='height:33px;display:flex;align-items:center;'>" +
                                    "<span class='input-tag'>同通訊地址︰</span>" +
                                    $"<input type='radio' name='dittoMemAddr' id='dittoMemAddrY' class='form-control dittoMemAddr' value='Y' {(row0["dittoMemAddr"].ToString() == "Y" ? "checked" : "")} checked style='margin-left:2px;'/><label for='dittoMemAddrY'>是</label>" +
                                    $"<input type='radio' name='dittoMemAddr' id='dittoMemAddrN' class='form-control dittoMemAddr' value='N' {(row0["dittoMemAddr"].ToString() == "N" ? "checked" : "")} /><label for='dittoMemAddrN'>否</label>" +
                                "</div>" +
                                "<div class='city-change' id='ord-addr-main' style='margin-top:8px;display:none;'>" +
                                    $"<select class='city-id' name='ordCityId' data-id='{row0["ordCityId"]}' style='margin-right:4px;'></select>" +
                                    $"<select class='zone-id' name='ordZoneId' data-id='{row0["ordZoneId"]}' style='margin-right:4px;width:110px;'></select>" +
                                    $"<input type='text' name='ordAddr' maxlength='150' class='form-control' style='width:458px;' value='{row0["ordAddr"]}' />" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>往來銀行</div>" +
                            "<div class='input-text'>" +
                                "<div>" +
                                    "<span class='input-tag'>銀行代號︰</span>" +
                                    $"<input type='text' name='bankId' maxlength='20' class='form-control' style='width: 100px; margin-right: 35px;' value='{row0["bankId"]}' />" +
                                    "<span class='input-tag'>銀行名稱︰</span>" +
                                    $"<input type='text' name='bankName' maxlength='50' class='form-control' style='width: 381px; margin-right: 67px;' value='{row0["bankName"]}' />" +
                                "</div>" +
                                "<div style='display:flex;align-items:center;margin-top:10px;'>" +
                                    "<span class='input-tag'>銀行帳號︰</span>" +
                                    $"<input type='text' name='bankAccount' maxlength='50' class='form-control' style='width:180px;margin-right:27px;' value='{row0["bankAccount"]}' />" +
                                    "<span class='input-tag'>戶　　名︰</span>" +
                                    $"<input type='text' name='bankAcctName' maxlength='80' class='form-control' style='width:309px;margin-right:67px;' value='{row0["bankAcctName"]}' />" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>備　　註</div>" +
                            "<div class='input-text'>" +
                                $"<textarea name='memo' maxlength='500' class='form-control' rows='2' style='width:675px;height:60px;'>{row0["memo"]}</textarea>" +
                            "</div>" +
                        "</div>" +
                    "</div>";
                    }
                }
            }
        }
    }
}