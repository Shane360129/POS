using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg7004 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7004";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate"));

                sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                sDate = int.Parse(DateTime.Parse(sDate).ToString("yyyyMMdd")) < int.Parse(StartYM) ? (StartYM.Substring(0, 4) + "/" + StartYM.Substring(4, 2) + "/" + StartYM.Substring(6, 2)) : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                string SqlComm = $"SELECT * FROM WP_Provider WHERE isStop='N' ORDER BY pvId";
                DataTable pvTbl = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT * FROM WP_InStock WHERE isDel='N' AND SUBSTRING(InStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' ORDER BY InStkId";
                DataTable iStkTbl = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT pvSn, SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' AND (qty>=0) GROUP BY pvSn ORDER BY pvSn";
                DataTable GrpTbl_P = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT pvSn, SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' AND (qty<0) GROUP BY pvSn ORDER BY pvSn";
                DataTable GrpTbl_M = getTbl.table("WP", SqlComm);

                int pageNo = 1;
                Label2.Text = $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>" +
                $"<input type='hidden' id='startYM' value='{StartYM}'>" +
                "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 報表條件</div>" +
                "<div class='search-main'>" +
                    "<div class='search-sub'>" +
                        "<span class='input-title'>查詢區間</span>" +
                        $"<input type='text' class='form-control del-txt-input open-datepicker' readonly id='sDate' value='{sDate}' />" +
                        "<span style='margin:0 5px;'>至</span>" +
                        $"<input type='text' class='form-control del-txt-input open-datepicker' readonly id='eDate' value='{eDate}' />" +
                    "</div>" +
                    "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>" +
                "<div class='rptr-main'>" +
                    "<div class='rptr-title'>廠商進退貨統計排行</div>" +
                    "<div class='rptr-data'>" +
                        $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                        $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                    "</div>" +
                    "<table class='list-main' style='border-spacing:1px;'>" +
                        "<tr class='list-title'>" +
                            "<td class='align-l'>排行</td><td class='align-l'>廠商編號</td><td class='align-l'>廠商名稱</td><td>進貨數量</td><td>進貨金額</td><td>退貨數量</td><td>退貨金額</td><td>已收金額</td><td>合計數量</td><td>合計金額</td>" +
                        "</tr>";
                        if (iStkTbl.Rows.Count == 0)
                        {
                            Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                        }
                        else
                        {
                            ThreeDot threeDot = new ThreeDot();
                            int recNo = 0;
                            foreach (DataRow row in pvTbl.Rows)
                            {
                                DataRow[] pvRow_P = GrpTbl_P.Select($"pvSn='{row["sn"]}'"),
                                    pvRow_M = GrpTbl_M.Select($"pvSn='{row["sn"]}'");
                                if (pvRow_P.Length > 0 || pvRow_M.Length > 0)
                                {
                                    recNo++;
                                    if (((recNo) % 45) == 1 && recNo > 1) {
                                        pageNo++;
                                      
                                        Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                        "<div class='rptr-title'>廠商進退貨統計排行</div>" +
                                        "<div class='rptr-data'>" +
                                            $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                        "</div>" +
                                        "<table class='list-main' style='border-spacing:1px;'>" +
                                            "<tr class='list-title'>" +
                                                "<td class='align-l'>排行</td><td class='align-l'>廠商編號</td><td class='align-l'>廠商名稱</td><td>進貨數量</td><td>進貨金額</td><td>退貨數量</td><td>退貨金額</td><td>已收金額</td><td>合計數量</td><td>合計金額</td>" +
                                            "</tr>";
                                    }
                                    string qty_p = pvRow_P.Length == 0 ? "0" : $"{pvRow_P[0]["sumQty"]}",
                                        amt_p = pvRow_P.Length == 0 ? "0" : $"{pvRow_P[0]["sumAmt"]:#0}",
                                        qty_m = pvRow_M.Length == 0 ? "0" : $"{pvRow_M[0]["sumQty"]}",
                                        amt_m = pvRow_M.Length == 0 ? "0" : $"{pvRow_M[0]["sumAmt"]:#0}",
                                        qty_total = (int.Parse(qty_p)+ int.Parse(qty_m)).ToString(),
                                        amt_total = (int.Parse(amt_p) + int.Parse(amt_m)).ToString();
                                    Label2.Text += $"<tr class='amt-row'>" +
                                        $"<td class='align-l'>{recNo.ToString().PadLeft(4,'0')}</td>" +
                                        $"<td class='align-l'>{row["pvId"]}</td>" +
                                        $"<td class='align-l'>{row["pvName"]}</td>" +
                                        $"<td class='qty_p' data-val='{qty_p}'>{threeDot.To3Dot(qty_p)}</td>" +
                                        $"<td class='amt_p' data-val='{amt_p}'>{threeDot.To3Dot(amt_p)}</td>" +
                                        $"<td class='qty_m' data-val='{qty_m}'>{threeDot.To3Dot(qty_m)}</td>" +
                                        $"<td class='amt_m' data-val='{amt_m}'>{threeDot.To3Dot(amt_m)}</td>" +
                                        $"<td class='amt_in' data-val='0'>0</td>" +
                                        $"<td class='qty_sum' data-val='{qty_total}'>{threeDot.To3Dot(qty_total)}</td>" +
                                        $"<td class='amt_sum' data-val='{amt_total}'>{threeDot.To3Dot(amt_total)}</td>" +
                                    "</tr>";
                                }
                            }
                            Label2.Text += $"<tr class='rptr-total'>" +
                                $"<td colspan='3'>合計︰</td>" +
                                $"<td class='qty_p_total'></td>" +
                                $"<td class='amt_p_total'></td>" +
                                $"<td class='qty_m_total'></td>" +
                                $"<td class='amt_m_total'></td>" +
                                $"<td class='amt_in_total'></td>" +
                                $"<td class='qty_sum_total'></td>" +
                                $"<td class='amt_sum_total'></td>" +
                            "</tr>";
                        }
                    Label2.Text += "</table>" +
                    $"<input type='hidden' id='page-total' value='{pageNo}' />" +
                "</div>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印統計表</button></div>";

            }
        }
    }

    private string CalcPayList(DataRow[] _oStkTbl)
    {
        int pay01 = 0, pay02 = 0, pay03 = 0, pay04 = 0, pay05 = 0, pay06 = 0, pay07 = 0, pay08 = 0;
        JArray payJSON;
        foreach (DataRow row in _oStkTbl)
        {
            payJSON = JArray.Parse($"{row["payList"]}");
            foreach (var pay in payJSON)
            {
                switch ($"{pay["PAYID"]}")
                {
                    case "01":
                        pay01 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "02":
                        pay02 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "03":
                        pay03 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "04":
                        pay04 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "05":
                        pay05 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "06":
                        pay06 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "07":
                        pay07 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "08":
                        pay08 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                }
            }
        }
        return $"[{{\"pay01\":\"{pay01}\",\"pay02\":\"{pay02}\",\"pay03\":\"{pay03}\",\"pay04\":\"{pay04}\",\"pay05\":\"{pay05}\",\"pay06\":\"{pay06}\",\"pay07\":\"{pay07}\",\"pay08\":\"{pay08}\"}}]";
    }
}