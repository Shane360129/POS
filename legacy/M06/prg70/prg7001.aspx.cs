using ClosedXML.Excel;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using static Global;

public partial class prg7001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                TableName();
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                string rptrYM = WebUtility.UrlDecode(cookies.Read("rptrYM")), 
                    pKSId = WebUtility.UrlDecode(cookies.Read("pKSId")),
                rptrYear = "",
                    rptrMonth = "",
                    sYear = StartYM.Substring(0, 4),
                    sMonth = StartYM.Substring(4, 2);
                Regex reg1 = new Regex(@"^[\d]+${6}");      //判斷商品分類編號只能0-9所組成
                WsString wsString = new WsString();


                rptrYM = reg1.IsMatch(rptrYM)
                    ? wsString.IsDate($"{rptrYM.Substring(0, 4)}/{rptrYM.Substring(4, 2)}/01") ? rptrYM : ""
                    : "";

                if (rptrYM == "") { cookies.Clear("rptrYM"); }
                else
                {
                    rptrYM = int.Parse(rptrYM) < int.Parse(StartYM.Substring(0, 6)) ? StartYM.Substring(0, 6) : rptrYM;
                    rptrYear = rptrYM.Substring(0, 4);
                    rptrMonth = rptrYM.Substring(4, 2);
                }

                Label2.Text = $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>" +
                $"<input type='hidden' id='startYM' value='{StartYM}'>" +
                $"<input type='hidden' id='rptrYM' value='{rptrYM}'>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            $"<span class='input-title'>報表年月</span>" +
                            "<select style='margin-right:3px;' id='rptr-year'><option value=''>請選擇</option>";
                                for (int i = int.Parse(StartYM.Substring(0, 4)); i <= int.Parse($"{DateTime.Now:yyyy}"); i++) { Label2.Text += $"<option value='{i}' {(rptrYM == "" ? "" : i == int.Parse(rptrYM.Substring(0, 4)) ? "selected" : "")}>{i}</option>"; };
                            Label2.Text += "</select>年" +
                            "<select style='margin-left:10px;margin-right:3px;' id='rptr-month'></select>月" +
                        "</div>" +
                    "</div>" +
                    "<div class='search-sub'>" +
                        "<span class='input-title'>類別</span>" +
                        "<select class='add-pKSId' name='pKSId' id='pKSId' data-alt='MI_商品分類'>" +
                            //"<option value='0'>請選擇</option>" +
                            "<option value='0'>全部</option>";
                string pk_SQL = $"SELECT * FROM WP_vPdKind ORDER BY pKSId";
                DataTable pvTbl = getTbl.table("WP", pk_SQL);
                foreach (DataRow row in pvTbl.Rows)
                {
                    Label2.Text += $"<option value='{row["pKSId"]}'>{row["pKSId"]} {row["pKSName"]}</option>";
                }
                Label2.Text += "</select></div>" +
                    "<div class='page-submit'>" +
                        "<span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span>" +
                        "<button class='btn-admin btn-submit' id='btn-submit' style='margin-left:30px;'>確認送出</button>" +
                    "</div>" +
                "</div>";

                if (rptrYM != "")
                {
                    string SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE isDel='N' ORDER BY yearMonth DESC";
                    DataTable chkoutTbl = getTbl.table("WP", SqlComm);
                    string chkYM = chkoutTbl.Rows.Count != 0 ? int.Parse($"{rptrYM}") <= int.Parse($"{chkoutTbl.Rows[0]["yearMonth"]}") ? "Y" : "N" : "N";     //判斷報表日期是否已結帳

                    string tblTitle = "<table class='list-main inStk-list-main' style='border-spacing:1px;'>" +
                        "<tr class='list-title'>" +
                            "<td rowspan='2' style='text-align:left'>項目</td><td rowspan='2'>商品</td><td colspan='2' style='text-align:center'>上月</td>" +
                            "<td colspan='10' style='text-align:center'>本月</td>" +
                       "</tr>" +
                       "<tr class='list-title l-title2'>" +
                            "<td class='pre-month'>結存數</td><td>庫存成本(總)</td><td>進貨數</td><td>進貨成本</td><td>銷售數</td><td>銷售金額</td>" +
                            "<td>庫存數</td><td>庫存成本(單)</td><td>庫存成本(總)</td><td>銷售成本</td><td>銷售利潤</td><td>利潤率%</td>" +
                        "</tr>";
                    Label2.Text += $"<input type='hidden' id='chkYM' value='{chkYM}'>";
                    if (chkYM == "N")
                    {
                        Label2.Text += $"<tr><td colspan='12' class='empty-data'>尚未結帳！</td></tr>";
                    }
                    else
                    {
                        //2023年8月31日後，成本皆用平均成本
                        var StandardDay = DateTime.Parse("2023/08/31");
                        string rptrYM_ = rptrYM == "" ? "" : rptrYM.Substring(0, 4) + "/" + rptrYM.Substring(4, 2) + "/01";
                        var day = DateTime.Parse(rptrYM_);
                        int result = DateTime.Compare(day, StandardDay);


                        SqlComm = $"SELECT pNo, pNameS, SUM(amtTotal) AS amtTotal, SUM(qty) AS qtyTotal FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 6)='{rptrYM}' GROUP BY pNo, pNameS";
                        DataTable istkTbl = getTbl.table("WP", SqlComm);    //進貨成本, 進貨數

                        SqlComm = $"SELECT pNo, pNameS, SUM(amtTotal-(dtlDiscnt + dtlDiscntShare)) AS amtTotal, SUM(qty) AS qtyTotal FROM WP_vOutStockUnion " +
                            $"WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 6)='{rptrYM}' GROUP BY pNo, pNameS";
                        DataTable ostkTbl = getTbl.table("WP", SqlComm);    //銷售金額, 銷售數

                        SqlComm = $"SELECT pNo, pNameS, SUM(oStkCost * Qty) AS costTotal, SUM(Qty) AS qtyTotal FROM WP_vStkTraceUnion WHERE Kind<>'D' AND SUBSTRING(OutStkId, 1, 6)='{rptrYM}' GROUP BY pNo, pNameS";
                        DataTable oCostTbl = getTbl.table("WP", SqlComm);    //銷售成本, 銷售數

                        SqlComm = $"SELECT SUM(discount+discountShare) AS discntTotal FROM WP_OutStock WHERE isDel='N' AND SUBSTRING(OutStkId, 1, 6)='{rptrYM}'";
                        DataTable ostkDiscntTbl = getTbl.table("WP", SqlComm);  //全折金額

                        //2023-08-31後的期初
                        SqlComm = $"select * from WP_pdStopLoss where isDel ='N' and Date < '{rptrYM_}' order by pNo";
                        DataTable proInTbl = getTbl.table("WP", SqlComm);

                        //SqlComm = $"SELECT * FROM WP_vProduct WHERE isSale NOT IN ('3') ORDER BY PNo";      //isSale:3停止進銷貨
                        var SQl_pdprod = "";
                        if (pKSId == "0")
                            SQl_pdprod = $"SELECT * FROM WP_vPdKindProd WHERE isSale NOT IN ('3') ORDER BY PNo";      //isSale:3停止進銷貨
                        else
                            SQl_pdprod = $"SELECT * FROM WP_vPdKindProd WHERE isSale NOT IN ('3') and pKSId='{pKSId}' ORDER BY PNo";      //isSale:3停止進銷貨
                        DataTable pdTbl = getTbl.table("WP", SQl_pdprod);

                        SqlComm = $"SELECT SUM(amtCargo) AS amtCargo, SUM(amtCoupon) AS amtCoupon FROM WP_OutStock WHERE OutStkId IN (SELECT OutStkId FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 6)='{rptrYM}')";
                        DataTable ostkAmtTbl = getTbl.table("WP", SqlComm);


                        string preYear = int.Parse(rptrMonth) == 1 ? (int.Parse(rptrYear) - 1).ToString() : rptrYear,
                        preMonth = int.Parse(rptrMonth) == 1 ? "12" : (int.Parse(rptrMonth) - 1).ToString();
                        var Month = int.Parse(rptrMonth) == 1 ? "12" : (int.Parse(rptrMonth)).ToString();

                        SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE YearMonth='{preYear}{preMonth.PadLeft(2, '0')}' AND isDel='N'";
                        DataTable preStkTbl = getTbl.table("WP", SqlComm);

                        //SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE YearMonth='{preYear}{Month.PadLeft(2, '0')}' AND isDel='N'";
                        //DataTable preStk_Tbl = getTbl.table("WP", SqlComm);

                        var pkname = pKSId == "0" ? "" : pdTbl.Rows[0]["pKSName"].ToString();

                        if (istkTbl.Rows.Count == 0 && ostkTbl.Rows.Count == 0)
                        {
                            Label2.Text += $"<tr><td colspan='11' class='empty-data'>查無資料！</td></tr>";
                        }
                        else
                        {
                            SqlComm = $"SELECT pNo FROM WP_pdStkQtyYM WHERE YearMonth = '{rptrYM}'";
                            DataTable thisStkTbl = getTbl.table("WP", SqlComm);   //本月庫存

                            ThreeDot threeDot = new ThreeDot();
                            DataRow[] pstkRows, istkRows, ostkRows, oCostRows, proInitialRows, pstk_Rows, thisStkRows = null;
                            double iAmt, iCost, stkLeftAmt, oStkAmt, oProfit, preCost, initlCost, pFitPercent, pdStkLAmt, avgCost,Cost,oCost, proInitialCost, pre_Cost, preCostTotal;
                            int recNo = 1, preQty, iQty, oQty, oAmt, stkQty, oStkQty, initlQty,InOutQty;


                            var pk = pkname == "0" ? "<div></div>" : $"<div style='width:50%;display:inline-block;'>類別 : {pKSId}-{pkname}</div>";
                            
                            string rptrTitle = $"<div class='rptr-title'>水里鄉農會 供銷部</div>" +
                                $"<div class='rptr-title'>{rptrYear}年{rptrMonth}月進銷存月報</div>" +
                                "<div style='font-size:14px;width:100%;'>" + pk + "</div>"+
                                "<div style='font-size:14px;width:100%;'>" +
                                    $"<div style='width:50%;display:inline-block;'>頁次︰pageNow / <span class='page-total'></span></div><div style='width:50%;text-align:right;display:inline-block;'>列印日期︰{DateTime.Now.ToString("yyyy/MM/dd")}</div>" +
                                "</div>";
                            int pageNow = 1;
                            foreach (DataRow row in pdTbl.Rows)
                            {
                                if (thisStkTbl.Rows.Count > 0) { thisStkRows = thisStkTbl.Select($"pNo='{row["pNo"]}'"); }  //庫存檔
                               
                                initlQty = int.Parse($"{row["qtyInitial"]}");
                                initlCost = double.Parse($"{row["costInitial"]}");
                                avgCost = double.Parse ($"{row["costAvg"]}");

                                int pNo = int.Parse ($"{row["pNo"]}");

                                proInitialRows = proInTbl.Select($"pNo='{row["pNo"]}'");

                                if (pNo == 1861) { 
                                    Console.WriteLine (123); };
                                pstkRows = preStkTbl.Select($"pNo='{row["pNo"]}'");
                                istkRows = istkTbl.Select($"pNo='{row["pNo"]}'");
                                ostkRows = ostkTbl.Select($"pNo='{row["pNo"]}'");

                                pstk_Rows = preStkTbl.Select($"pNo='{row["pNo"]}'");

                                proInitialCost = proInitialRows.Length == 0 ? 0 : toDouble.Numer(double.Parse($"{proInitialRows[0]["costAvg"]}"), pointQty); 

                                preQty = pstkRows.Length == 0 ? 0 : int.Parse($"{pstkRows[0]["qty"]}");
                                preCost = pstkRows.Length == 0 ? 0 : double.Parse ($"{pstkRows[0]["cost"]}");
                                preCostTotal = preQty * preCost;

                                pre_Cost = pstk_Rows.Length == 0 ? 0 : double.Parse($"{pstk_Rows[0]["cost"]}");

                                iQty = istkRows.Length == 0 ? 0 : int.Parse($"{istkRows[0]["qtyTotal"]}");
                                iAmt = istkRows.Length == 0 ? 0 : toDouble.Numer(double.Parse($"{istkRows[0]["amtTotal"]}"), pointQty);
                                iCost = iQty == 0 ? 0 : (iAmt / iQty);
                                oQty = ostkRows.Length == 0 ? 0 : int.Parse($"{ostkRows[0]["qtyTotal"]}");
                                oAmt = ostkRows.Length == 0 ? 0 : int.Parse($"{ostkRows[0]["amtTotal"]:#0.}");
                                stkQty = $"{row["isUpdStock"]}" == "N" ? 0 : preQty + iQty - oQty;

                                InOutQty = $"{row["isUpdStock"]}" == "N" ? 0 :  iQty - oQty;

                                if (!(preQty == 0 && iQty == 0 && oQty == 0))
                                {
                                    if ((recNo % 75) == 1)
                                    {
                                        Label2.Text += $"{(recNo == 1 ? "<div id='rptr-main'>" : $"</table><p style='page-break-after:always'></p>")}{rptrTitle.Replace("pageNow", $"{pageNow}")}{tblTitle}";
                                        pageNow++;
                                    }

                                    Label2.Text += $"<tr class='tr-row' data-updstk='{row["isUpdStock"]}'>" +
                                        $"<td>{recNo}</td>" +
                                        $"<td class='pNo' data-val='{pNo} style='text-align:left;white-space:nowrap;'><span style='margin-right:5px'>{row["pBarcode"]}</span>{row["pNameS"]}</td>" +
                                        $"<td class='preQty' data-val='{preQty}' data-cost='{preCost}'>{threeDot.To3Dot($"{preQty}")}</td>" +
                                        $"<td class='preCost' data-val='{toDouble.Numer (preCostTotal, pointQty)}' data-cost='{preCostTotal}'>{threeDot.To3Dot ($"{preCostTotal}")}</td>" +
                                        $"<td class='iQty' data-val='{iQty}'>{threeDot.To3Dot($"{iQty}")}</td>" +
                                        $"<td class='iAmt' data-val='{iAmt}'>{threeDot.To3Dot($"{iAmt}")}</td>" +
                                        $"<td class='oQty' data-val='{oQty}'>{threeDot.To3Dot($"{oQty}")}</td>" +
                                        $"<td class='oAmt' data-val='{oAmt}'>{threeDot.To3Dot($"{oAmt}")}</td>" +
                                        $"<td class='stkQty' data-val='{stkQty}'>{threeDot.To3Dot($"{stkQty}")}</td>";
                        
                                    stkLeftAmt = 0;     //庫存成本
                                    double preQtyPlusIQty = preQty + iQty; //期初庫存數+本期進貨數
                                    double Cost2;

                                    Cost2 = preQtyPlusIQty != 0 ?(preCostTotal + iAmt) / preQtyPlusIQty : 0; //(期初庫存成本+本期進貨)/(期初庫存數+本期進貨數)
                                    Cost2 = Math.Round (Cost2, 2);
                                   
                                    oStkAmt = 0;        //銷售成本
                                    oStkQty = 0;

                                    oCost = result <= 0 ? oStkAmt : Cost2 * oQty ;  // 根據 result 的值選擇使用 oStkAmt 或 (Cost2 * oQty) 作為銷售成本
                                    oCost = stkQty == 0 ? (preCostTotal + iAmt) : oCost; // 如果庫存數為 0，則將 oCost 設為 (期初庫存成本 + 本期進貨)
                                    double com =  preCostTotal + iAmt - oCost;
                                    oProfit = oAmt - oCost;
                                    pdStkLAmt = stkQty == 0 ? 0 : Cost2; // 如果庫存數為 0，則 pdStkLAmt 為 0，否則為 Cost2
                                    pFitPercent = oAmt == 0 ? 0 : (oProfit / oAmt * 100); // 如果 oAmt 為 0，則 pFitPercent 為 0，否則為 (oProfit / oAmt * 100)
                                    stkLeftAmt = stkQty > 0 ? pdStkLAmt* stkQty : 0; // 如果庫存數大於 0，則計算庫存成本

                                    if (iAmt < 0)
                                        {
                                        oCost = 0;
                                        }
                                        Label2.Text += 
                                        $"<td class='pdStkLAmt' data-val='{toDouble.Numer(pdStkLAmt, pointQty)}'>{threeDot.To3Dot($"{pdStkLAmt.ToString(pointRule)}")}</td>" +
                                        $"<td class='stkCost' data-val='{toDouble.Numer(stkLeftAmt, pointQty)}'>{threeDot.To3Dot($"{toDouble.Numer(stkLeftAmt, pointQty).ToString(pointRule)}")}</td>" +
                                        $"<td class='oCost' data-val='{toDouble.Numer(oCost, pointQty)}'>{threeDot.To3Dot($"{toDouble.Numer(oCost, pointQty).ToString(pointRule)}")}</td>" +
                                        $"<td class='oProfit' data-val='{toDouble.Numer(oProfit, pointQty)}'>{threeDot.To3Dot($"{toDouble.Numer(oProfit, pointQty).ToString(pointRule)}")}</td>" +
                                        $"<td class='pFitPercent' data-val='{toDouble.Numer(pFitPercent, pointQty)}'>{threeDot.To3Dot(pFitPercent.ToString(pointRule))}</td>" +
                                    "</tr>";
                                    #region 更新庫存檔
                                    string checkYM = DateTime.Now.ToString ("yyyyMM");
                                    if (rptrYM == checkYM)
                                        {
                                        if (thisStkRows == null || thisStkRows.Length == 0)
                                            {
                                            SqlComm = $"INSERT INTO WP_pdStkQtyYM (pNo, qty, cost, YearMonth) VALUES ('{pNo}', '{stkQty}', '{(stkLeftAmt == 0 ? 0 : toDouble.Numer (stkLeftAmt / stkQty, 2))}', '{rptrYM}')";
                                            getTbl.updTbl ("WP", SqlComm);
                                            }
                                        else { 
                                        SqlComm = $"UPDATE WP_pdStkQtyYM SET qty='{stkQty}', cost='{(stkLeftAmt == 0 ? 0 : toDouble.Numer (stkLeftAmt / stkQty, 2))}', " +
                                            $"isDel='N', timeUpdate=getdate() WHERE pNo='{row["pNo"]}' AND YearMonth='{rptrYM}'";
                                            getTbl.updTbl ("WP", SqlComm);
                                            }
                                        }

                                        //SqlComm = (thisStkRows == null || thisStkRows.Length == 0)
                                        //   ? $"INSERT INTO WP_pdStkQtyYM (pNo, qty, cost, YearMonth) VALUES ('{row["pNo"]}', '{stkQty}', '{(stkLeftAmt == 0 ? 0 : toDouble.Numer ((stkLeftAmt / stkQty), 2))}', '{rptrYM}')"
                                        //   : $"UPDATE WP_pdStkQtyYM SET qty='{stkQty}', cost='{(stkLeftAmt == 0 ? 0 : toDouble.Numer ((stkLeftAmt / stkQty), 2))}', isDel='N', timeUpdate=getdate() WHERE pNo='{row["pNo"]}' AND YearMonth='{rptrYM}'";
                                        //getTbl.updTbl ("WP", SqlComm);

                                        #endregion

                                        // EXCEL檔案下載
                                        Input (recNo.ToString(), row["pBarcode"].ToString() + "-" + row["pNameS"].ToString(), preQty.ToString(), preCostTotal.ToString(),
                                        iQty.ToString(), iAmt.ToString(), oQty.ToString(), oAmt.ToString(), stkQty.ToString(),
                                        pdStkLAmt.ToString(pointRule), toDouble.Numer(stkLeftAmt, pointQty).ToString(pointRule),
                                        toDouble.Numer(oCost, pointQty).ToString(pointRule), toDouble.Numer(oProfit, pointQty).ToString(pointRule), pFitPercent.ToString(pointRule));
                                    recNo++;

                                }
                                else
                                {
                                    #region 更新庫存檔
                                    if (!(thisStkRows == null || thisStkRows.Length == 0))
                                    {
                                        SqlComm = $"UPDATE WP_pdStkQtyYM SET qty='0', cost='0', isDel='N', timeUpdate=getdate() WHERE pNo='{row["pNo"]}' AND YearMonth='{rptrYM}'";
                                        getTbl.updTbl ("WP", SqlComm);
                                        }
                                    #endregion
                                }
                            }

                            string amtCargo = $"{ostkAmtTbl.Rows[0]["amtCargo"]}" == "" ? "0" : $"{ostkAmtTbl.Rows[0]["amtCargo"]}",
                                amtCoupon = $"{ostkAmtTbl.Rows[0]["amtCoupon"]}" == "" ? "0" : $"{ostkAmtTbl.Rows[0]["amtCoupon"]}",
                                discntTotal = $"{ostkDiscntTbl.Rows[0]["discntTotal"]:#0}";
                            Label2.Text += "<tr class='total-row'>" +
                                "<td colspan='2'>合計</td>" +
                                $"<td class='preQty-total'></td>" +
                                $"<td class='preCost-total'></td>" +
                                $"<td class='iQty-total'></td>" +
                                $"<td class='iAmt-total'></td>" +
                                $"<td class='oQty-total'></td>" +
                                $"<td class='oAmt-total' data-discntVal='{discntTotal}'></td>" +
                                $"<td class='stkQty-total'></td>" +
                                $"<td></td>" +
                                $"<td class='stkCost-total'></td>" +
                                $"<td class='oCost-total'></td>" +
                                $"<td class='oProfit-total'></td>" +
                                $"<td></td>" +
                            "</tr>" +
                            $"<tr style='display:none'><td colspan='9'>運費</td><td id='amtCargo' data-val='{amtCargo}'>{amtCargo}</td></tr>" +
                            $"<tr style='display:none'><td colspan='9'>禮券</td><td id='amtCoupon' data-val='{amtCoupon}'>{amtCoupon}</td></tr>" +
                            $"<tr style='display:none'><td colspan='9'>利潤總計</td><td id='oProfit-final'></td></tr>" +
                            $"<input type='hidden' id='page-total' value='{pageNow-1}' />";
                        }
                    }
                    Label2.Text += "</table></div>" +
                    "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印進銷存月報</button>" +
                    "<input type = 'button' value = 'Excel下載' style='color:#080' onclick='DownloadFile()' />" +
                    "</div>";
                }
            }
        }
    }
    public static List<Lable_7001> dataList;
    #region 欄位設定
    public class Lable_7001
    {
        public string Number { get; set; } //項目
        public string Name { get; set; } //商品名稱
        public string LastQty { get; set; } //上月結存數
        public string LastCost { get; set; } //上月結存數
        public string PurchaseQty { get; set; } //進貨數
        public string PurchaseCost { get; set; } //進貨成本
        public string SalesQty { get; set; } //銷售數量
        public string SalesAmount { get; set; } //銷售金額
        public string QtyNow { get; set; } //庫存數
        public string InventorySingleCost { get; set; } //庫存成本(單)
        public string InventoryTotalCost { get; set; } //庫存成本(總)
        public string SalesCost { get; set; } //銷售成本
        public string SalesProfit { get; set; } //銷售利潤 
        public string ProfitMargin { get; set; } //利潤 
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_7001>();
        dataList.Add(new Lable_7001()
        {
            Number = "項目",
            Name = "商品名稱",
            LastQty = "上月結存數",
            LastCost = "上月庫存成本",
            PurchaseQty = "進貨數",
            PurchaseCost = "進貨成本",
            SalesQty = "銷售數量",
            SalesAmount = "銷售金額",
            QtyNow = "庫存數",
            InventorySingleCost = "庫存成本(單)",
            InventoryTotalCost = "庫存成本(總)",
            SalesCost = "銷售成本",
            SalesProfit = "銷售利潤",
            ProfitMargin = "利潤"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string num, string name, string lq, string lc, string pq, string pc, string sq, string sa, string qn, string isc, string itc, string sc, string sp, string pm)
    {
        dataList.Add(new Lable_7001()
        {
            Number = num,
            Name = name,
            LastQty = lq,
            LastCost = lc,
            PurchaseQty = pq,
            PurchaseCost = pc,
            SalesQty = sq,
            SalesAmount = sa,
            QtyNow = qn,
            InventorySingleCost = isc,
            InventoryTotalCost = itc,
            SalesCost = sc,
            SalesProfit = sp,
            ProfitMargin = pm
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "7001進銷存月報表";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Number;
            worksheet.Cell(j, 2).Value = dataList[j - 1].Name;
            worksheet.Cell (j, 3).Value = dataList[j - 1].LastQty;
            worksheet.Cell (j, 4).Value = dataList[j - 1].LastCost;
            worksheet.Cell(j, 5).Value = dataList[j - 1].PurchaseQty;
            worksheet.Cell(j, 6).Value = dataList[j - 1].PurchaseCost;
            worksheet.Cell(j, 7).Value = dataList[j - 1].SalesQty;
            worksheet.Cell(j, 8).Value = dataList[j - 1].SalesAmount;
            worksheet.Cell(j, 9).Value = dataList[j - 1].QtyNow;
            worksheet.Cell(j, 10).Value = dataList[j - 1].InventorySingleCost;
            worksheet.Cell(j, 11).Value = dataList[j - 1].InventoryTotalCost;
            worksheet.Cell(j, 12).Value = dataList[j - 1].SalesCost;
            worksheet.Cell(j, 13).Value = dataList[j - 1].SalesProfit;
            worksheet.Cell(j, 14).Value = dataList[j - 1].ProfitMargin;
        }
        worksheet.Columns().AdjustToContents();//自動調整欄位寬度

        var fileName = WorksheetsName + ".xlsx";//儲存名稱

        var pathdownload = HttpContext.Current.Request.MapPath("~/Files/");
        //儲存
        workbook.SaveAs(pathdownload + fileName);
        //return pathdownload + fileName;
        //Read the File as Byte Array.
        byte[] bytes = File.ReadAllBytes(pathdownload + fileName);

        //Convert File to Base64 string and send to Client.
        return Convert.ToBase64String(bytes, 0, bytes.Length);
    }
    #endregion
}