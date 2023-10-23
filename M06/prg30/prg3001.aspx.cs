using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using static Global;

public partial class prg3001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "3001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";

                Stock stock = new Stock();

                Label2.Text = $"<input type='hidden' id='minDate' value='{stock.MinDate()}' />" +
                $"<input type='hidden' id='pvType' value='{pvType}' />" +
                "<div class='input-main'>" +
                    "<div class='input-sub'>" +
                        "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 進貨日期︰</span>" +
                        $"<input type='text' class='form-control open-datepicker' id='InStkDate' name='InStkDate' size='10' readonly  style='background-color:#fff;margin-right:65px;' value='{DateTime.Today:yyyy/MM/dd}' />" +
                    "</div>" +
                    "<div class='input-sub' style='display:flex;align-items:center'>" +
                        "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 廠　　商︰</span>";
                        if (pvType == "1") {    //下拉選單
                                string SqlComm = $"SELECT * FROM WP_Provider WHERE isStop='N' ORDER BY pvId";
                                DataTable pvTbl = getTbl.table("WP", SqlComm);
                                Label2.Text += "<select id='pvSn' name='pvSn' class='form-control' data-alt='MI_廠商' style='margin-right:65px;'><option value='0'>請選擇</option>";
                                foreach (DataRow row in pvTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["sn"]}'>{row["pvId"]}-{row["pvNameS"]}</option>";
                                }
                            Label2.Text += "</select>";
                        }
                        else    //autocomplete
                        {
                            Label2.Text += "<div class='del-txt-group' style='margin-right:65px;'>" +
                                "<input type='text' class='form-control del-txt-input' placeholder='請輸入廠商名稱或代號' maxlength='42' style='width:266px;' id='pv-filter'  />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                                "<input type='hidden' name='pvSn' id='act-pvSn' />" +
                            "</div>";
                        }
                    Label2.Text +="</div>" +
                    "<div class='input-sub'>" +
                        "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 發票號碼︰</span>" +
                        $"<input type='text' class='form-control' id='reciptNo' name='reciptNo' size='20' maxlength='50' />" +
                    "</div>"+
                "</div>";
            }
        }
    }
}