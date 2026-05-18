using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using static Global;

public partial class prg7005 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7005";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate"));

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
                "</div>";
                if (sDate != "" && sDate != "")
                {
                    //sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                    //sDate = int.Parse(DateTime.Parse(sDate).ToString("yyyyMMdd")) < int.Parse(StartYM) ? (StartYM.Substring(0, 4) + "/" + StartYM.Substring(4, 2) + "/" + StartYM.Substring(6, 2)) : sDate;
                    //eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                    string SqlComm = $"SELECT * FROM WP_OutStock WHERE isDel='N' AND SUBSTRING(OutStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}'  ORDER BY OutStkId";
                    DataTable oStkTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT * FROM WP_OutStockDtl WHERE isDel='N' AND SUBSTRING(OutStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' ORDER BY OutStkId";
                    DataTable oStkDtlTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT * FROM WP_OutStockDtlCb WHERE isDel='N' AND SUBSTRING(OutStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' ORDER BY OutStkId";
                    DataTable oStkDtlCbTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT * FROM WP_vStkTraceUnion WHERE Kind<>'D' AND SUBSTRING(OutStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' ORDER BY OutStkId";
                    DataTable stkTrcTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT * FROM WP_Product WHERE pNo IN (SELECT pNo FROM WP_vStkTraceUnion WHERE Kind<>'D' AND SUBSTRING(OutStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}')";
                    DataTable pdTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT * FROM WP_Member WHERE sn IN (SELECT memSn FROM WP_OutStock WHERE isDel='N' AND SUBSTRING(OutStkId, 1, 8) BETWEEN '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}') ORDER BY memId";
                    DataTable memTbl = getTbl.table("WP", SqlComm);

                    int pageNo = 1;
                    string overStr = "";
                    List<string> overArr = new List<string>();
                    Label2.Text += $"<div class='rptr-main'>" +
                        "<div class='rptr-title'>銷退貨簡要表</div>" +
                        "<div class='rptr-data'>" +
                            $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                        "</div>" +
                        "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                                "<td class='align-l'>單據日期</td><td>合計金額</td><td>本張利潤</td><td class='align-l'>客戶簡稱</td><td>已收金額</td><td>未清金額</td><td>總計金額</td><td>本張成本</td><td>利潤率%</td>" +
                            "</tr>";
                            if (oStkTbl.Rows.Count == 0)
                            {
                                Label2.Text += $"<tr><td colspan='8' class='empty-data'>查無資料！</td></tr>";
                            }
                            else
                            {
                                if (stkTrcTbl.Rows.Count == 0)
                                {
                                    Label2.Text += $"<tr><td colspan='8' class='empty-data'>尚未有結帳資料！</td></tr>";
                                }
                                else
                                {
                                    ThreeDot threeDot = new ThreeDot();
                                    int recNo = 0, discnt, discntDtl, oAmt;
                                    double iAmt, oProfit, pFitPercent;
                                    string oAmtIn, oAmtLeft;
                                    DataRow[] stkTrcRows, pdRows, oStkDtlRows, memRows;
                                    foreach (DataRow row in oStkTbl.Select("outType<>'2'", "OutStkId"))
                                    {
                                        recNo++;
                                        if (((recNo) % 45) == 1 && recNo > 1)
                                        {
                                            pageNo++;

                                            Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                            "<div class='rptr-title'>銷退貨簡要表</div>" +
                                            "<div class='rptr-data'>" +
                                                $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                                $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                            "</div>" +
                                            "<table class='list-main' style='border-spacing:1px;'>" +
                                                "<tr class='list-title'>" +
                                                    "<td class='align-l'>單據日期</td><td>合計金額</td><td>本張利潤</td><td class='align-l'>客戶簡稱</td><td>已收金額</td><td>未清金額</td><td>總計金額</td><td>本張成本</td><td>利潤率%</td>" +
                                                "</tr>";
                                        }
                                        stkTrcRows = stkTrcTbl.Select($"OutStkId='{row["OutStkId"]}'", "pNo ASC");
                                        iAmt = 0;   //進貨成本
                                        discnt = int.Parse($"{row["discount"]:#0}") + int.Parse($"{row["discountShare"]:#0}");  //全折
                                        discntDtl = 0;  //單折
                                        string preDtlSn = "";
                                        foreach (DataRow trcRow in stkTrcRows)
                                        {
                                            pdRows = pdTbl.Select($"pNo='{trcRow["pNo"]}'");
                                            if($"{trcRow["InStkId"]}" == "over" && $"{pdRows[0]["isUpdStock"]}" == "Y" && !overArr.Exists(x => x==$"{trcRow["pNo"]}"))     //超賣商品
                                            {
                                                overArr.Insert(0, $"{trcRow["pNo"]}");
                                                overStr += $"{(overStr == "" ? "" : "、")}{trcRow["pNameS"]}";
                                            }

                                            #region 有庫存調整則不顯示超賣
                                            if ($"{row["memSn"]}"=="-1" && overArr.Exists(x => x == $"{trcRow["pNo"]}"))
                                            {
                                                overArr.RemoveAll(r => r == $"{trcRow["pNo"]}");
                                                overStr = $"、{overStr}、".Replace($"、{trcRow["pNameS"]}、", "、");
                                                overStr = overStr == "、" ? "" : overStr.Substring(1, overStr.Length - 1);
                                            }
                                            #endregion

                                            iAmt += ($"{trcRow["InStkId"]}" == "0")
                                                ? (double.Parse($"{trcRow["costInitial"]}") * double.Parse($"{trcRow["Qty"]}"))
                                                : ($"{trcRow["InStkId"]}" == "over")
                                                    ? 0
                                                    : (double.Parse($"{trcRow["IStkAmt"]}") * double.Parse($"{trcRow["Qty"]}"));
                                            
                                            oStkDtlRows = $"{trcRow["OutStkDtlSn"]}".IndexOf("cb") < 0
                                                ? oStkDtlTbl.Select($"OutStkId='{row["OutStkId"]}' AND sn={trcRow["OutStkDtlSn"]}")
                                                : oStkDtlCbTbl.Select($"OutStkId='{row["OutStkId"]}' AND sn={($"{trcRow["OutStkDtlSn"]}".Replace("cb", ""))}");
                                            if (preDtlSn != $"{trcRow["OutStkDtlSn"]}")
                                            {
                                                discntDtl += int.Parse($"{oStkDtlRows[0]["discount"]:#0}") + int.Parse($"{oStkDtlRows[0]["discountShare"]:#0}");
                                                preDtlSn = $"{trcRow["OutStkDtlSn"]}";
                                            }
                                        }
                                        discnt += discntDtl;
                                        oAmt = int.Parse($"{row["amount"]:#0}");
                                        oProfit = Math.Round(oAmt - iAmt);
                                        memRows = memTbl.Select($"sn={row["memSn"]}");
                                        oAmtIn = $"{row["outType"]}" == "2" ? $"{oAmt}" : $"{row["outType"]}" == "0" ? "" : $"{(oAmt - int.Parse($"{row["outLeft"]:#0}"))}";
                                        oAmtLeft = $"{row["outType"]}" == "2" ? "" : $"{row["outLeft"]:#0}";
                                        pFitPercent = oAmt == 0 ? 0 : (oProfit / oAmt * 100);   //利潤率
                                        Label2.Text += $"<tr class='amt-row'>" +
                                            $"<td class='align-l'><a href='/prg40/prg4002Upd.aspx?sn={row["sn"]}'><i class='far fa-hand-point-up oStk-link' style='margin-right:3px;'></i>{row["OutStkDate"]:yyyy/MM/dd}</a></td>" +
                                            $"<td class='odAmt' data-val='{(oAmt + discnt)}'>{threeDot.To3Dot($"{(oAmt + discnt)}")}</td>" +
                                            $"<td class='oProfit' data-val='{oProfit}'>{threeDot.To3Dot($"{oProfit}")}</td>" +
                                            $"<td class='align-l'>{(memRows.Length == 0 ? "" : $"{memRows[0]["sn"]}" == "0" ? "" : $"{memRows[0]["MemName"]}")}</td>" +
                                            $"<td class='oAmtIn' data-val='{(oAmtIn=="" ? "0" : oAmtIn)}'>{(oAmtIn == "" ? "" : threeDot.To3Dot(oAmtIn))}</td>" +
                                            $"<td class='oAmtLeft' data-val='{(oAmtLeft == "" ? "0" : oAmtLeft)}'>{(oAmtLeft == "" ? "" : threeDot.To3Dot(oAmtLeft))}</td>" +
                                            $"<td class='oAmt' data-val='{oAmt}'>{threeDot.To3Dot($"{oAmt}")}</td>" +
                                            $"<td class='iAmt' data-val='{Math.Round(iAmt)}'>{threeDot.To3Dot($"{Math.Round(iAmt)}")}</td>" +
                                            $"<td class='pFitPercent' data-val='{toDouble.Numer(pFitPercent, pointQty)}'>{threeDot.To3Dot(pFitPercent.ToString(pointRule))}</td>" +
                                        "</tr>";
                                    }
                                    foreach (DataRow row in oStkTbl.Select("outType='2'", "OutStkId"))
                                    {
                                        recNo++;
                                        if (((recNo) % 45) == 1 && recNo > 1)
                                        {
                                            pageNo++;

                                            Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                            "<div class='rptr-title'>銷退貨簡要表</div>" +
                                            "<div class='rptr-data'>" +
                                                $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                                $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                            "</div>" +
                                            "<table class='list-main' style='border-spacing:1px;'>" +
                                                "<tr class='list-title'>" +
                                                    "<td class='align-l'>單據日期</td><td>合計金額</td><td>本張利潤</td><td class='align-l'>客戶簡稱</td><td>已收金額</td><td>未清金額</td><td>總計金額</td><td>本張成本</td><td>利潤率%</td>" +
                                                "</tr>";
                                        }
                                        stkTrcRows = stkTrcTbl.Select($"OutStkId='{row["OutStkId"]}'", "pNo ASC");
                                        iAmt = 0;   //進貨成本
                                        discnt = int.Parse($"{row["discount"]:#0}") + int.Parse($"{row["discountShare"]:#0}");  //全折
                                        discntDtl = 0;  //單折
                                        string preDtlSn = "";
                                        foreach (DataRow trcRow in stkTrcRows)
                                        {
                                            pdRows = pdTbl.Select($"pNo='{trcRow["pNo"]}'");
                                            if($"{trcRow["InStkId"]}" == "over" && $"{pdRows[0]["isUpdStock"]}" == "Y" && !overArr.Exists(x => x==$"{trcRow["pNo"]}"))     //超賣商品
                                            {
                                                overArr.Insert(0, $"{trcRow["pNo"]}");
                                                overStr += $"{(overStr == "" ? "" : "、")}{trcRow["pNameS"]}";
                                            }
                                            if($"{row["memSn"]}"=="-1" && overArr.Exists(x => x == $"{trcRow["pNo"]}"))
                                            {
                                                overArr.RemoveAll(r => r == $"{trcRow["pNo"]}");
                                                overStr = $"、{overStr}、".Replace($"、{trcRow["pNameS"]}、", "、");
                                                overStr = overStr == "、" ? "" : overStr.Substring(1, overStr.Length - 1);
                                            }

                                            iAmt += ($"{trcRow["InStkId"]}" == "0")
                                                ? (double.Parse($"{trcRow["costInitial"]}") * double.Parse($"{trcRow["Qty"]}"))
                                                : ($"{trcRow["InStkId"]}" == "over")
                                                    ? 0
                                                    : (double.Parse($"{trcRow["IStkAmt"]}") * double.Parse($"{trcRow["Qty"]}"));
                                            
                                            oStkDtlRows = $"{trcRow["OutStkDtlSn"]}".IndexOf("cb") < 0
                                                ? oStkDtlTbl.Select($"OutStkId='{row["OutStkId"]}' AND sn={trcRow["OutStkDtlSn"]}")
                                                : oStkDtlCbTbl.Select($"OutStkId='{row["OutStkId"]}' AND sn={($"{trcRow["OutStkDtlSn"]}".Replace("cb", ""))}");
                                            if (preDtlSn != $"{trcRow["OutStkDtlSn"]}")
                                            {
                                                discntDtl += int.Parse($"{oStkDtlRows[0]["discount"]:#0}") + int.Parse($"{oStkDtlRows[0]["discountShare"]:#0}");
                                                preDtlSn = $"{trcRow["OutStkDtlSn"]}";
                                            }
                                        }
                                        discnt += discntDtl;
                                        oAmt = int.Parse($"{row["amount"]:#0}");
                                        oProfit = Math.Round(oAmt - iAmt);
                                        memRows = memTbl.Select($"sn={row["memSn"]}");
                                        oAmtIn = $"{row["outType"]}" == "2" ? $"{oAmt}" : $"{row["outType"]}" == "0" ? "" : $"{(oAmt - int.Parse($"{row["outLeft"]:#0}"))}";
                                        oAmtLeft = $"{row["outType"]}" == "2" ? "" : $"{row["outLeft"]:#0}";
                                        pFitPercent = oAmt == 0 ? 0 : (oProfit / oAmt * 100);
                                        Label2.Text += $"<tr class='amt-row'>" +
                                            $"<td class='align-l'><a href='/prg40/prg4002Upd.aspx?sn={row["sn"]}'><i class='far fa-hand-point-up oStk-link' style='margin-right:3px;'></i>{row["OutStkDate"]:yyyy/MM/dd}</a></td>" +
                                            $"<td class='odAmt' data-val='{(oAmt + discnt)}'>{threeDot.To3Dot($"{(oAmt + discnt)}")}</td>" +
                                            $"<td class='oProfit' data-val='{oProfit}'>{threeDot.To3Dot($"{oProfit}")}</td>" +
                                            $"<td class='align-l'>{(memRows.Length == 0 ? "" : $"{memRows[0]["sn"]}" == "0" ? "" : $"{memRows[0]["MemName"]}")}</td>" +
                                            $"<td class='oAmtIn' data-val='{(oAmtIn=="" ? "0" : oAmtIn)}'>{(oAmtIn == "" ? "" : threeDot.To3Dot(oAmtIn))}</td>" +
                                            $"<td class='oAmtLeft' data-val='{(oAmtLeft == "" ? "0" : oAmtLeft)}'>{(oAmtLeft == "" ? "" : threeDot.To3Dot(oAmtLeft))}</td>" +
                                            $"<td class='oAmt' data-val='{oAmt}'>{threeDot.To3Dot($"{oAmt}")}</td>" +
                                            $"<td class='iAmt' data-val='{Math.Round(iAmt)}'>{threeDot.To3Dot($"{Math.Round(iAmt)}")}</td>" +
                                            $"<td class='pFitPercent' data-val='{toDouble.Numer(pFitPercent, pointQty)}'>{threeDot.To3Dot(pFitPercent.ToString(pointRule))}</td>" +
                                        "</tr>";
                                    }
                                    Label2.Text += $"<tr class='rptr-total'>" +
                                        $"<td class='align-l'>合計</td>" +
                                        $"<td class='odAmt_total'></td>" +
                                        $"<td class='oProfit_total'></td>" +
                                        $"<td></td>" +
                                        $"<td class='oAmtIn_total'></td>" +
                                        $"<td class='oAmtLeft_total'></td>" +
                                        $"<td class='oAmt_total'></td>" +
                                        $"<td class='iAmt_total'></td>" +
                                        $"<td class='pFitPercent_total'></td>" +
                                    "</tr>";
                                }
                            }
                            DataRow[] oStkcoffee = stkTrcTbl.Select("pNo IN (251,252)");
                            string oStkcoffeeHTML = "", pName1 = "", pName2 = "";
                            int qty1 = 0, qty2 = 0;
                            if (oStkcoffee.Length > 0)
                            {
                                foreach (DataRow row in oStkcoffee) {
                                    pName1 = pName1=="" ? $"{row["pNo"]}" == "251" ? $"{row["pNameS"]}" : "" : pName1;
                                    pName2 = pName2=="" ? $"{row["pNo"]}" == "252" ? $"{row["pNameS"]}" : "" : pName2;
                                    qty1 += $"{row["pNo"]}" == "251" ? int.Parse($"{row["qty"]}") : 0;
                                    qty2 += $"{row["pNo"]}" == "252" ? int.Parse($"{row["qty"]}") : 0;
                                }
                            }
                            oStkcoffeeHTML = qty1 == 0 ? "" : $"<div class='rprt-memo'>{pName1}︰數量={qty1}</div>";
                            oStkcoffeeHTML += qty2 == 0 ? "" : $"<div class='rprt-memo'>{pName2}︰數量={qty2}</div>";
                        Label2.Text += "</table>" +
                        $"{oStkcoffeeHTML}" +
                        $"{(overArr.Count == 0 ? "" : $"<div style='color:#f00' class='rprt-memo'>超賣商品︰{overStr}</div>")}" +
                        $"<input type='hidden' id='page-total' value='{pageNo}' />" +
                    "</div>" +
                    "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印簡要表</button></div>";
                }
            }
        }
    }


    /// <summary>
    /// 計算付款方式總額傳回JSON
    /// </summary>
    /// <param name="_oStkTbl"></param>出貨DataTable
    /// <returns></returns>
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