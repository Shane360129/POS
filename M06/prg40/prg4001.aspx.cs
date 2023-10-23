using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using static Global;

public partial class prg4001 : System.Web.UI.Page
{

    protected string ProviderNamesJson { get; set; }

    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "4001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";

                Stock stock = new Stock();
                Label2.Text = $"<input type='hidden' id='minDate' value='{stock.MinDate()}' />" +
                "<div class='outStk-input-main'>" +
                    "<div>" +
                        "<div class='input-sub'>" +
                            "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 銷貨日期︰</span>" +
                            $"<input type='text' class='form-control open-datepicker' id='OutStkDate' name='OutStkDate' size='10' readonly  style='background-color:#fff;margin-right:65px;' value='{DateTime.Today.ToString("yyyy/MM/dd")}' />" +
                        "</div>" +
                        "<div class='input-sub'>" +
                            "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 會　員︰</span>";

                
                string SqlComm = $"SELECT * FROM WP_Member WHERE isStop='N' ORDER BY sn";
                DataTable memTbl = getTbl.table ("WP", SqlComm);
                List<object> providerNames = new List<object> ();

                foreach (DataRow row in memTbl.Rows)
                    {
                    providerNames.Add (new
                        {
                        label = row["memId"] + "．" + row["memName"],
                        value = row["sn"],
                        //sn = row["sn"]
                        });
                    }

                ProviderNamesJson = JsonConvert.SerializeObject (providerNames);

                //輸入廠商
                Label2.Text += $"<input type='text' class='form-control' id='memFilter' name='memFilter' placeholder='請輸入廠商名稱' style='margin-right:65px;' />";
                Label2.Text += $"<input type='hidden' class='form-control' id='memSn' name='memSn' />";

                Label2.Text += "</div>" +
                        "<div class='input-sub'>" +
                            "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 發票號碼︰</span>" +
                            $"<input type='text' class='form-control' id='reciptNo' name='reciptNo' size='20' maxlength='50' />" +
                        "</div>" +
                    "</div>" +
                    "<div>" +
                        "<div class='input-sub'>" +
                            "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 付款方式︰</span>" +
                            $"{new PayKind().HTML2("")}" +
                        "</div>" +
                    "</div>" +
                "</div>";
            }
        }
    }
}