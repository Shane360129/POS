using System;
using System.Data;
using static Global;

public partial class prg6001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "6001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                //Stock stock = new Stock();
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";


                string SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' ORDER BY OutStkId, sn, dtlSn";
                DataTable outStkTbl = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT * FROM WP_StkTrace WHERE Kind NOT IN ('D') ORDER BY OutStkId DESC";
                DataTable OStkTrace = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE isDel='N' ORDER BY YearMonth DESC";      //已結帳日期
                DataTable chkoutYMTbl = getTbl.table("WP", SqlComm);
                string sYMD = "", sYear = "", sMonth = "", sDay = "";
                string chkoutYM, uYMD = "", uYear = "", uMonth = "", uDay = "";

                if (chkoutYMTbl.Rows.Count == 0)
                {
                    sYMD = StartYM;
                    chkoutYM = "N";
                }
                else
                {
                    uYMD = $"{chkoutYMTbl.Rows[0]["YearMonth"]}";
                    sYMD = DateTime.Parse($"{uYMD.Substring(0, 4)}-{uYMD.Substring(4, 2)}-{uYMD.Substring(6, 2)}").AddDays(1).ToString("yyyyMMdd");

                    uYear = uYMD.Substring(0, 4);
                    uMonth = uYMD.Substring(4, 2);
                    uDay = uYMD.Substring(6, 2);
                    chkoutYM = "Y";
                }
                sYear = sYMD.Substring(0, 4);
                sMonth = sYMD.Substring(4, 2);
                sDay = sYMD.Substring(6, 2);

                Label2.Text += $"<input type='hidden' id='sYMD' value='{sYMD}'>" +
                $"<input type='hidden' id='uYMD' value='{uYMD}'>" +
                "<div class='chk-main'>" +
                    "<div style='margin-bottom:20px;'>" +
                        $"<input type='radio' style='margin-left:0px;' class='chkoutYN' name='chkoutYN' id='chkoutY' checked value='Y' /><label for='chkoutY'>日結作業</label>" +
                        $"{(chkoutYM == "Y" ? $"<span style='display:flex;align-items:center;margin-left:15px;'><input type='radio' style='margin-left:0;' class='chkoutYN' name='chkoutYN' id='chkoutN' value='N' /><label for='chkoutN' style='color:#f00;'>取消日結</label></span>" : "")}" +
                    "</div>" +
                    "<div>" +
                        "<div class='chkoutY-main'>" +
                            $"<span><i class='fas fa-caret-square-right'></i> 結帳年月︰ {sYear} 年 {sMonth} 月 {sDay} 日</span>" +
                            "<button class='btn-admin btn-submit' id='btn-submit' style='margin-left:10px;'>確認送出</button>" +
                        "</div>" +
                        "<div class='chkoutN-main' style='display:none;padding:2px;'>" +
                            $"<span><i class='fas fa-caret-square-right'></i> 取消年月︰ {uYear} 年 {uMonth} 月 {uDay} 日</span>" +
                            "<button class='btn-admin btn-submit' id='btn-unSubmit'>確認送出</button>" +
                        "</div>" +
                        "<div id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</div>" +
                    "</div>" +
                "</div>" + "<div id='result'></div>";

            }
        }
    }
}