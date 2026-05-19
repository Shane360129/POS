using System;
using System.Data;
using System.Text.RegularExpressions;
using static Global;

public partial class prg2002Upd : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 廠商修改</div>";

                string sn = Request.QueryString["sn"];

                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷商品分類編號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg20/prg2002.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vProvider WHERE sn='{sn}'";
                    DataTable pvTbl = getTbl.table("WP", SqlComm);
                    if (pvTbl.Rows.Count == 0) { Response.Redirect("/prg20/prg2002.aspx"); }
                    else
                    {
                        DataRow row0 = pvTbl.Rows[0];
                        Label2.Text = $"<div class='page-main pv-upd-main'>" +
                        $"<input type='hidden' name='sn' value='{sn}' />" +
                        "<div class='input-row'>" +
                            "<div class='input-title'>編　　號</div>" +
                            "<div class='input-text'>" +
                                $"<input type='text' name='pvId' class='form-control' data-alt='MI_廠商編號' maxlength='6' style='width:70px;' value='{row0["pvId"]}' />" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row'>" +
                            "<div class='input-title'>名　　稱</div>" +
                            "<div class='input-text'>" +
                                "<span class='input-tag'>全名︰</span>" +
                                $"<input type='text' name='pvName' class='form-control' data-alt='MI_廠商全名' maxlength='50' style='width:335px;margin-right:27px;' value='{row0["pvName"]}' />" +
                                "<span class='input-tag'>簡稱︰</span>" +
                                $"<input type='text' name='pvNameS' class='form-control' data-alt='MI_廠商簡稱' maxlength='10' style='width:227px;' value='{row0["pvNameS"]}' />" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>分　　類</div>" +
                            "<div class='input-text'>";
                                SqlComm = $"SELECT * FROM WP_PvKind WHERE pvExist ='Y' ORDER BY pvKId";
                                DataTable pvKTbl = getTbl.table("WP", SqlComm);
                                Label2.Text += $"<select name='pvKId' class='form-control' dat-alt='MI_廠商分類' style='margin-right:35px;'>";
                                foreach (DataRow row in pvKTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["pvKId"]}' {(row0["pvKid"].ToString() == row["pvKid"].ToString() ? "selected" : "")}>{row["pvKId"]} {row["pvKName"]}</option>";
                                }
                                Label2.Text += "</select>" +
                                "<span class='input-tag'>狀態︰</span>" +
                                "<select name='isStop' class='form-control'>" +
                                    $"<option value='N' {(row0["isStop"].ToString() == "N" ? "selected" : "")}>正常往來</option>" +
                                    $"<option value='Y' {(row0["isStop"].ToString() == "Y" ? "selected" : "")}>停止往來</option>" +
                                "</select>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>公　　司</div>" +
                            "<div class='input-text'>" +
                                "<div>" +
                                    "<span class='input-tag'>負責人︰</span>" +
                                    $"<input type='text' name='pvBoss' maxlength='50' class='form-control' style='width:230px; margin-right:43px;' value='{row0["pvBoss"]}' />" +
                                    "<span class='input-tag'>電話︰</span>" +
                                    $"<input type='text' name='pvTel' maxlength='50' class='form-control' style='width:300px; margin-right:67px;' value='{row0["pvTel"]}' />" +
                                "</div>" +
                                "<div style='display:flex;align-items:center;margin-top:10px;'>" +
                                    "<span class='input-tag'>傳　真︰</span>" +
                                    $"<input type='text' name='fax' maxlength='50' class='form-control' style='width:230px;margin-right:33px;' value='{row0["fax"]}' />" +
                                    "<span class='input-tag'>Email︰</span>" +
                                    $"<input type='text' name='email' maxlength='50' class='form-control' style='width:300px;margin-right:67px;' value='{row0["email"]}' />" +
                                "</div>" +
                                "<div style='display:flex;align-items:center;margin-top:10px;'>" +
                                    "<span class='input-tag'>地　址︰</span>" +
                                    "<div class='city-change'>" +
                                        $"<select class='form-control city-id' name='pvCityId' data-id='{row0["pvCityId"]}' style='margin-right:4px;'></select>" +
                                        $"<select class='form-control zone-id' name='pvZoneId' data-id='{row0["pvZoneId"]}' style='margin-right:4px;width:111px;'></select>" +
                                        $"<input type='text' name='pvAddr' maxlength='150' class='form-control' style='width:402px;margin-right:67px;' value='{row0["pvAddr"]}' />" +
                                    "</div>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>聯絡人員</div>" +
                            "<div class='input-text'>" +
                                "<div>" +
                                    "<span class='input-tag'>姓　名︰</span>" +
                                    $"<input type='text' name='ctactName' data-alt='MI_聯絡人姓名' maxlength='50' class='form-control' style='width:230px;margin-right:43px;' value='{row0["ctactName"]}' />" +
                                    "<span class='input-tag'>電話︰</span>" +
                                    $"<input type='text' name='ctactTel' data-alt='MI_聯絡人電話' maxlength='50' class='form-control' style='width:300px; margin-right:67px;' value='{row0["ctactTel"]}' />" +
                                "</div>" +
                                "<div style='display:flex;align-items:center;margin-top:10px;'>" +
                                    "<div class='input-tag'>地　址︰</div>" +
                                    "<div class='input-text'>" +
                                        "<div class='city-change'>" +
                                            $"<select class='form-control city-id' name='ctactCityId' data-id='{row0["ctactCityId"]}' style='margin-right:4px;'></select>" +
                                            $"<select class='form-control zone-id' name='ctactZoneId' data-id='{row0["ctactZoneId"]}' style='margin-right:4px;width:111px;'></select>" +
                                            $"<input type='text' name='ctactAddr' maxlength='150' class='form-control' style='width:402px; margin-right:67px;' value='{row0["ctactAddr"]}' />" +
                                        "</div>" +
                                    "</div>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>發　　票</div>" +
                            "<div class='input-text'>" +
                                "<span class='input-tag'>統一編號︰</span>" +
                                $"<input type='text' name='taxId' maxlength='8' class='form-control' style='width:100px;margin-right:43px;' value='{row0["taxId"]}' />" +
                                "<span class='input-tag'>發票抬頭︰</span>" +
                                $"<input type='text' name='invoTitle' maxlength='80' class='form-control' style='width:381px;margin-right:67px;' value='{row0["invoTitle"]}' />" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>往來銀行</div>" +
                            "<div class='input-text'>" +
                                "<div>" +
                                    "<span class='input-tag'>銀行代號︰</span>" +
                                    $"<input type='text' name='bankId' maxlength='20' class='form-control' style='width:100px;margin-right:43px;' value='{row0["bankId"]}' />" +
                                    "<span class='input-tag'>銀行名稱︰</span>" +
                                    $"<input type='text' name='bankName' maxlength='50' class='form-control' style='width:381px;margin-right:67px;' value='{row0["bankName"]}' />" +
                                "</div>" +
                                "<div style='display:flex;align-items:center;margin-top:10px;'>" +
                                    "<span class='input-tag'>銀行帳號︰</span>" +
                                    $"<input type='text' name='bankAccount' maxlength='50' class='form-control' style='width:180px;margin-right:35px;' value='{row0["bankAccount"]}' />" +
                                    "<span class='input-tag'>戶　　名︰</span>" +
                                    $"<input type='text' name='bankAcctName' maxlength='80' class='form-control' style='width: 309px;margin-right:67px;' value='{row0["bankAcctName"]}' />" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='input-row' style='align-items: flex-start;'>" +
                            "<div class='input-title'>備　　註</div>" +
                            "<div class='input-text'>" +
                                $"<textarea name='memo' maxlength='500' class='form-control' rows='2' style='width:684px;height:60px;'>{row0["memo"]}</textarea>" +
                            "</div>" +
                        "</div>" +
                    "</div>";
                    }
                }
            }
        }
    }
}