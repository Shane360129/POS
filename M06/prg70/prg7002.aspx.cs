using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg7002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE isDel='N' ORDER BY yearMonth DESC";
                DataTable chkoutTbl = getTbl.table("WP", SqlComm);

                string minDate = chkoutTbl.Rows.Count == 0 ? DateTime.Now.ToString("yyyyMMdd") : $"{StartYM}",
                    maxDate = chkoutTbl.Rows.Count == 0 ? DateTime.Now.ToString("yyyyMMdd") : $"{chkoutTbl.Rows[0]["yearMonth"]}";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate"));

                sDate = sDate == "" ? chkoutTbl.Rows.Count == 0 ? "" : $"{chkoutTbl.Rows[0]["yearMonth"].ToString().Substring(0,4)}/{chkoutTbl.Rows[0]["yearMonth"].ToString().Substring(4, 2)}/{chkoutTbl.Rows[0]["yearMonth"].ToString().Substring(6, 2)}" : sDate;
                eDate = eDate == "" ? sDate : eDate;

                Label2.Text = $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>" +
                $"<input type='hidden' id='minDate' value='{minDate}'>" +
                $"<input type='hidden' id='maxDate' value='{maxDate}'>" +
                //$"<input type='hidden' id='sDate' value='{sDate}'>" +
                //$"<input type='hidden' id='eDate' value='{eDate}'>" +
                "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 報表條件</div>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            $"<span class='input-title'>日期區間</span>" +
                            $"<input type='text' class='form-control open-datepicker' id='sDate' name='sDate' size='10' style='margin-right:5px;' value='{sDate}' />至" +
                            //$"<input type='text' class='form-control open-datepicker' id='eDate' name='eDate' size='10' readonly style='margin-left:5px;' value='{eDate}' />" +
                            $"<span style='margin-left:10px;font-weight:bold;font-size:18px;color:#E90080;'>{(chkoutTbl.Rows.Count == 0 ? "<i class='fas fa-exclamation-circle' style='margin-right:3px;'></i>請先結帳" : "")}</span>" +
                        "</div>" +
                    "</div>" +
                    "<div class='page-submit'><button class='btn-admin btn-submit' id='btn-submit' style='margin-left:30px;'>確認送出</button></div>" +
                "</div>" +
                //"<table class='list-main inStk-list-main' style='border-spacing:1px;'>" +
                //    "<tr class='list-title'>" +
                //        "<td style='text-align:left'>項目</td><td>上月結存數</td><td>本月進貨數</td><td>進貨成本</td><td>銷售數</td><td>銷售金額</td><td>庫存數</td><td>庫存成本</td><td>銷售成本</td><td>銷售利潤</td>" +
                //    "</tr>";
                //    if (chkYM == "N")
                //    {
                //        Label2.Text += $"<tr><td colspan='10' class='empty-data'>尚未結帳！</td></tr>";
                //    }
                //    else
                //    {

                //        SqlComm = $"SELECT pNo, pNameS, SUM(amtTotal) AS amtTotal, SUM(qty) AS qtyTotal FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 6)='{rptrYear}{rptrMonth.PadLeft(2, '0')}' GROUP BY pNo, pNameS";
                //        DataTable istkTbl = getTbl.table("WP", SqlComm);

                //        SqlComm = $"SELECT pNo, pNameS, SUM(amtTotal) AS amtTotal, SUM(qty) AS qtyTotal, SUM(dtlCostStd * qty) AS stdTotal FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 6)='{rptrYear}{rptrMonth.PadLeft(2, '0')}' GROUP BY pNo, pNameS";
                //        DataTable ostkTbl = getTbl.table("WP", SqlComm);

                //        SqlComm = $"SELECT * FROM WP_vProduct WHERE isSale NOT IN ('3') ORDER BY PNo";      //isSale:3停止進銷貨
                //        DataTable pdTbl = getTbl.table("WP", SqlComm);

                //        SqlComm = $"SELECT SUM(amtCargo) AS amtCargo, SUM(amtCoupon) AS amtCoupon FROM WP_OutStock WHERE OutStkId IN (SELECT OutStkId FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 6)='{rptrYear}{rptrMonth.PadLeft(2, '0')}')";
                //        DataTable ostkAmtTbl = getTbl.table("WP", SqlComm);

                //        SqlComm = $"SELECT * FROM WP_vInStock WHERE (SUBSTRING(InStkId, 1, 6) <= '{rptrYear}{rptrMonth.PadLeft(2, '0')}') AND (dtlSn NOT IN (SELECT InStkDtlSn FROM WP_StkTrace WHERE QtyLeft = 0)) AND (isDel = 'N') AND (dtlIsDel = 'N')";
                //        DataTable iStkLeftTbl = getTbl.table("WP", SqlComm);

                //        SqlComm = $"SELECT * FROM WP_vStkTrace WHERE (SUBSTRING(InStkId, 1, 6) <= '{rptrYear}{rptrMonth.PadLeft(2, '0')}') AND Kind<>'D'";
                //        DataTable stkTrcTbl = getTbl.table("WP", SqlComm);



                //        string preYear = rptrMonth == "1" ? (int.Parse(rptrYear) - 1).ToString() : rptrYear,
                //        preMonth = rptrMonth == "1" ? "12" : (int.Parse(rptrMonth) - 1).ToString();
                //        SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE YearMonth='{preYear}{preMonth.PadLeft(2, '0')}' AND isDel='N'";
                //        DataTable pstkTbl = getTbl.table("WP", SqlComm);

                //        if (istkTbl.Rows.Count == 0 && ostkTbl.Rows.Count == 0)
                //        {
                //            Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                //        }
                //        else
                //        {
                //            string preQty, iQty, iAmt, oQty, oAmt, initlQty, initlCost;
                //            DataRow[] pstkRows, istkRows, ostkRows, stkTrcRows;
                //            double stkLeftAmt, oStkAmt;
                //            foreach (DataRow row in pdTbl.Rows)
                //            {
                //                initlQty = $"{row["qtyInitial"]}";
                //                initlCost = $"{row["costInitial"]}";

                //                pstkRows = pstkTbl.Select($"pNo='{row["pNo"]}'");
                //                istkRows = istkTbl.Select($"pNo='{row["pNo"]}'");
                //                ostkRows = ostkTbl.Select($"pNo='{row["pNo"]}'");
                //                preQty = pstkRows.Length == 0 ? "0" : $"{pstkRows[0]["qty"]}";
                //                iQty = istkRows.Length == 0 ? "0" : $"{istkRows[0]["qtyTotal"]}";
                //                iAmt = istkRows.Length == 0 ? "0" : $"{toDouble.Numer(double.Parse($"{istkRows[0]["amtTotal"]}"), 2)}";
                //                oQty = ostkRows.Length == 0 ? "0" : $"{ostkRows[0]["qtyTotal"]}";
                //                oAmt = ostkRows.Length == 0 ? "0" : $"{ostkRows[0]["amtTotal"]:#0.}";
                //                if (!(preQty == "0" && iQty == "0" && oQty == "0"))
                //                {
                //                    Label2.Text += "<tr class='tr-row'>" +
                //                        $"<td style='text-align:left'>{row["pNameS"]}</td>" +
                //                        $"<td class='preQty' data-val='{preQty}' data-cost='{initlCost}'>{preQty}</td>" +
                //                        $"<td class='iQty' data-val='{iQty}'>{iQty}</td>" +
                //                        $"<td class='iAmt' data-val='{iAmt}'>{iAmt}</td>" +
                //                        $"<td class='oQty' data-val='{oQty}'>{oQty}</td>" +
                //                        $"<td class='oAmt' data-val='{oAmt}'>{oAmt}</td>" +
                //                        $"<td class='stkQty'></td>";

                //                        stkLeftAmt = 0;     //庫存成本
                //                        oStkAmt = 0;        //銷售成本
                //                        stkTrcRows = stkTrcTbl.Select($"pNo='{row["pNo"]}' AND InStkDtlSn='0'", "sn DESC");
                //                        stkLeftAmt += double.Parse($"{row["costInitial"]}") * (stkTrcRows.Length == 0 ? int.Parse($"{row["qtyInitial"]}") : int.Parse($"{stkTrcRows[0]["QtyLeft"]}"));  //計算是否期初數量

                //                        foreach (DataRow rowLeft in iStkLeftTbl.Select($"pNo='{row["pNo"]}'"))
                //                        {
                //                            stkTrcRows = stkTrcTbl.Select($"InStkDtlSn='{rowLeft["dtlSn"]}'", "sn DESC");
                //                            stkLeftAmt += double.Parse($"{rowLeft["dtlAmt"]}") * int.Parse(stkTrcRows.Length == 0 ? $"{rowLeft["qty"]}" : $"{stkTrcRows[0]["QtyLeft"]}");
                //                        }
                //                        foreach (DataRow rowTrc in stkTrcTbl.Select($"pNo='{row["pNo"]}'"))
                //                        {
                //                            oStkAmt += int.Parse($"{rowTrc["Qty"]}") * ($"{rowTrc["InStkId"]}" == "0" ? double.Parse($"{row["costInitial"]}") : double.Parse($"{rowTrc["IStkAmt"]}"));
                //                        }
                //                        Label2.Text += $"<td class='stkCost' data-val='{toDouble.Numer(stkLeftAmt, 2)}'>{toDouble.Numer(stkLeftAmt, 2)}</td>" +
                //                        $"<td class='oCost' data-val='{toDouble.Numer(oStkAmt, 2)}'>{toDouble.Numer(oStkAmt, 2)}</td>" +
                //                        "<td class='oProfit'></td>" +
                //                    "</tr>";
                //                }
                //            }

                //            string amtCargo = $"{ostkAmtTbl.Rows[0]["amtCargo"]}" == "" ? "0" : $"{ostkAmtTbl.Rows[0]["amtCargo"]}",
                //                amtCoupon = $"{ostkAmtTbl.Rows[0]["amtCoupon"]}" == "" ? "0" : $"{ostkAmtTbl.Rows[0]["amtCoupon"]}";
                //            Label2.Text += "<tr class='total-row'>" +
                //                "<td>合計</td>" +
                //                $"<td class='preQty-total'></td>" +
                //                $"<td class='iQty-total'></td>" +
                //                $"<td class='iAmt-total'></td>" +
                //                $"<td class='oQty-total'></td>" +
                //                $"<td class='oAmt-total'></td>" +
                //                $"<td class='stkQty-total'></td>" +
                //                $"<td class='stkCost-total'></td>" +
                //                $"<td class='oCost-total'></td>" +
                //                $"<td class='oProfit-total'></td>" +
                //            "</tr>" +
                //            $"<tr style='display:none'><td colspan='9'>運費</td><td id='amtCargo' data-val='{amtCargo}'>{amtCargo}</td></tr>" +
                //            $"<tr style='display:none'><td colspan='9'>禮券</td><td id='amtCoupon' data-val='{amtCoupon}'>{amtCoupon}</td></tr>" +
                //            $"<tr style='display:none'><td colspan='9'>利潤總計</td><td id='oProfit-final'></td></tr>" +
                //            "";
                //        }
                //    }
                //Label2.Text += "</table>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080'>列　印</button></div>";
                
            }
        }
    }
}