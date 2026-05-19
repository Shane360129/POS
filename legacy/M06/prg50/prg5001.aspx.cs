using System;
using System.Data;
using System.Linq;
using static Global;

public partial class prg5001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "5001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                Label2.Text = $"<input type='hidden' id='pvType' value='{pvType}' />" +
                "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 搜尋條件</div>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>銷帳日期</span>" +
                            $"<input type='text' class='form-control open-datepicker' id='acctOutDate' name='acctOutDate' size='10' readonly value='{DateTime.Today.ToString("yyyy/MM/dd")}' />" +
                        "</div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>廠　　商</span>";
                            if (pvType == "1")
                            {    //下拉選單
                                string SqlComm = $"SELECT * FROM WP_Provider ORDER BY pvId";
                                DataTable pvTbl = getTbl.table("WP", SqlComm);
                                Label2.Text += "<select id='pvSn' name='pvSn' class='form-control' data-alt='MI_廠商'><option value='0'>請選擇</option>";
                                foreach (DataRow row in pvTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["sn"]}'>{row["pvId"]}-{row["pvNameS"]}</option>";
                                }
                                Label2.Text += "</select>";
                            }
                            else    //autocomplete
                            {
                                Label2.Text += "<div class='del-txt-group' style='margin-right:65px;display:inline-flex;'>" +
                                    "<input type='text' class='form-control del-txt-input' placeholder='請輸入廠商名稱或代號' maxlength='42' style='width:266px;' id='pv-filter'  />" +
                                    "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                                    "<input type='hidden' name='pvSn' id='act-pvSn' />" +
                                "</div>";
                            }
                        Label2.Text += "</div>" +
                    "</div>" +
                    "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>";
            }
        }
    }
}