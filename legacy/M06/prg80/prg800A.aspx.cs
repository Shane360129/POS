using System;
using System.Data;
using System.Linq;
using System.Net;
using static Global;

public partial class prg800A : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "800A";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                //Stock stock = new Stock();
                //DataRow prgRow0 = emp.PrgTbl.Rows[0];
                //Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                //$"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                //string sYear = "", sMonth = "";

                //string SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' ORDER BY OutStkId, sn, dtlSn";
                //DataTable outStkTbl = getTbl.table("S20", SqlComm);

                //SqlComm = $"SELECT * FROM WP_StkTrace WHERE Kind NOT IN ('D') ORDER BY OutStkId DESC";
                //DataTable OStkTrace = getTbl.table("S20", SqlComm);
                //string lastChkout = OStkTrace.Rows.Count == 0 ? "0" : OStkTrace.Rows[0]["OutStkId"].ToString().Substring(0, 6);
                //if (lastChkout == "0")
                //{
                //    if (outStkTbl.Rows.Count == 0)
                //    {
                //        sYear = DateTime.Now.AddMonths(-1).ToString("yyyy");
                //        sMonth = DateTime.Now.AddMonths(-1).ToString("MM");
                //    }
                //    else
                //    {
                //        sYear = outStkTbl.Rows[0]["OutStkId"].ToString().Substring(0, 4);
                //        sMonth = outStkTbl.Rows[0]["OutStkId"].ToString().Substring(4, 2);
                //    }
                //}
                //else
                //{
                //    sYear = lastChkout.Substring(0, 4);
                //    sMonth = lastChkout.Substring(4, 2);

                //    sYear = sMonth == "12" ? (int.Parse(sYear) + 1).ToString() : sYear;
                //    sMonth = sMonth == "12" ? "01" : (int.Parse(sMonth) +1).ToString().PadLeft(2, '0');
                //}

                Label2.Text = $"" +
                //$"<input type='hidden' id='chkout-sYear' value='{sYear}'>" +
                //$"<input type='hidden' id='chkout-sMonth' value='{sMonth}'>" +
                "<div style='border:2px #000 solid;border-radius:3px;padding:20px 14px;'>" +
                    "<div>" +
                        //$"<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 結帳年月︰ {sYear} 年 {sMonth} 月</span>" +
                        $"<span style='font-weight:bold;font-size:18px;'>庫存試算1</span>" +
                        "<button class='' id='btn-submit' style='margin-left:10px;'>確認送出</button>" +
                    "</div>" +
                "</div>" +
                "<div id='result'></div>";
                    
            }
        }
    }
}