using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using static Global;

public partial class prg7011 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7011";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                TableName();
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                cookies.Clear("sDate"); cookies.Clear("eDate");
                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    pKSId = WebUtility.UrlDecode(cookies.Read("pKSId"));


                sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                sDate = int.Parse(DateTime.Parse(sDate).ToString("yyyyMMdd")) < int.Parse(StartYM) ? (StartYM.Substring(0, 4) + "/" + StartYM.Substring(4, 2) + "/" + StartYM.Substring(6, 2)) : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;


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
                    "<div class='search-sub'>" +
                        "<span class='input-title'>類別</span>" +
                        "<select class='add-pKSId' name='pKSId' id='pKSId' data-alt='MI_商品分類'>" +
                            "<option value='0'>請選擇</option>"+
                            "<option value='1'>全部</option>";
                string SqlComm = $"SELECT * FROM WP_vPdKind ORDER BY pKSId";
                DataTable pvTbl = getTbl.table("WP", SqlComm);
                foreach (DataRow row in pvTbl.Rows)
                {
                    Label2.Text += $"<option value='{row["pKSId"]}'>{row["pKSId"]} {row["pKSName"]}</option>";
                }
                Label2.Text += "</select>" +
                    "</div>" +
                    "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>";

                if (sDate != "" && eDate != "" && pKSId != "0")
                {
                    int pageNo = 1;

                    if (pKSId == "1")
                        SqlComm = $"SELECT * FROM WP_vPdKindProd WHERE isSale NOT IN ('3') ORDER BY PNo";      //isSale:3停止進銷貨
                    else
                        SqlComm = $"SELECT * FROM WP_vPdKindProd WHERE isSale NOT IN ('3') and pKSId='{pKSId}' ORDER BY PNo";      //isSale:3停止進銷貨
                    DataTable pdTbl = getTbl.table("WP", SqlComm);

                    if (pdTbl.Rows.Count == 0)
                    {
                        Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                    }
                    else
                    {

                        ThreeDot threeDot = new ThreeDot();
                        int recNo = 0;
                        int Beginning_qty, Finally_Qty, inQty_all, outQty_all, in_inQty, in_outQty, out_outQty, out_inQty;
                        double in_inCost, in_outCost, out_outCost, out_inCost, Beginning_Cost, Finally_Cost;
                        double costAvg, stkLeftAmt, iCost, preCost, oStkAmt, oProfit;

                        Label2.Text +=
                         "<div class='rptr-main'>" +
                             "<div class='rptr-title'>門市營業彙總表</div>" +
                             "<div class='rptr-data'>" +
                                 $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                 $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                             "</div>" +
                             "<table class='list-main' style='border-spacing:1px;'>" +
                                 "<tr class='list-title'>" +
                                     "<td class='align-l'>項次</td><td class='align-l'>商品編號及名稱</td><td>期初庫存</td><td>期初金額</td><td>進貨數量</td><td>進貨金額</td>" +
                                     "<td>退貨數量</td><td>退貨金額</td><td>銷貨數量</td><td>銷貨成本</td><td>銷退數量</td><td>銷退金額</td>" +
                                     //"<td>盤損</td><td>盤盈</td>" + //目前先不用
                                     "<td>期末庫存</td><td>期末金額</td><td>銷貨金額</td><td>銷貨利潤</td>" +
                                 "</tr>";

                        foreach (DataRow row in pdTbl.Rows)
                        {

                            recNo++;
                            if (((recNo) % 45) == 1 && recNo > 1)
                            {
                                pageNo++;

                                Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                "<div class='rptr-title'>門市營業彙總表</div>" +
                                "<div class='rptr-data'>" +
                                    $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                    $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                "</div>" +
                                "<table class='list-main' style='border-spacing:1px;'>" +
                                    "<tr class='list-title'>" +
                                        "<td class='align-l'>項次</td><td class='align-l'>商品編號及名稱</td><td>期初庫存</td><td>期初金額</td><td>進貨數量</td><td>進貨金額</td>" +
                                        "<td>退貨數量</td><td>退貨金額</td><td>銷貨數量</td><td>銷貨成本</td><td>銷退數量</td><td>銷退金額</td>" +
                                        //"<td>盤損</td><td>盤盈</td>" + //目前先不用
                                        "<td>期末庫存</td><td>期末金額</td><td>銷貨金額</td><td>銷貨利潤</td>" +
                                    "</tr>";
                            }

                            SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock" +
                                $" WHERE isDel='N' AND dtlIsDel='N' " +
                                $"AND SUBSTRING(InStkId, 1, 8) < '{sDate.Replace("/", "")}' and pNo='{row["pNo"]}' " +
                                $"group by pNo,pBarcode,pName ORDER BY pNo";
                            DataTable inskBeginTbl = getTbl.table("WP", SqlComm);//期初進貨數

                            //SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' and qty > 0 and pvSn<>'-1' group by pNo,pBarcode,pName ORDER BY pNo";
                            SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock " +
                                $"WHERE isDel='N' AND dtlIsDel='N' " +
                                $"AND SUBSTRING(InStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' " +
                                $"and qty > 0 and pNo='{row["pNo"]}' " +
                                $"group by pNo,pBarcode,pName ORDER BY pNo";
                            DataTable inskTbl = getTbl.table("WP", SqlComm);//進貨數，進貨金額

                            //SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' and qty < 0 and pvSn<>'-1' group by pNo,pBarcode,pName ORDER BY pNo";
                            SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock " +
                                $"WHERE isDel='N' AND dtlIsDel='N' " +
                                $"AND SUBSTRING(InStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' " +
                                $"and qty < 0 and pNo='{row["pNo"]}' " +
                                $"group by pNo,pBarcode,pName ORDER BY pNo";
                            DataTable inskReturnTbl = getTbl.table("WP", SqlComm);//進退數，進退金額

                            //SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' and  pvSn='-1' group by pNo,pBarcode,pName ORDER BY pNo";
                            //DataTable diskLossTbl = getTbl.table("WP", SqlComm);//盤損

                            SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vOutStock " +
                                $"WHERE isDel='N' AND dtlIsDel='N' " +
                                $"AND SUBSTRING(OutStkId, 1, 8) < '{sDate.Replace("/", "")}' and pNo='{row["pNo"]}' " +
                                $"group by pNo,pBarcode,pName ORDER BY pNo";
                            DataTable outskBeginTbl = getTbl.table("WP", SqlComm);//期初銷貨數

                            //SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' and qty > 0 and memSn<>'-1' group by pNo,pBarcode,pName ORDER BY pNo";
                            SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vOutStock " +
                                $"WHERE isDel='N' AND dtlIsDel='N' " +
                                $"AND SUBSTRING(OutStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' " +
                                $"and qty > 0 and pNo='{row["pNo"]}' " +
                                $"group by pNo,pBarcode,pName ORDER BY pNo";
                            DataTable outskTbl = getTbl.table("WP", SqlComm);//銷貨數，銷貨金額


                            //SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' and qty < 0 and memSn<>'-1' group by pNo,pBarcode,pName ORDER BY pNo";
                            SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vOutStock " +
                                $"WHERE isDel='N' AND dtlIsDel='N' " +
                                $"AND SUBSTRING(OutStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' " +
                                $"and qty < 0  and pNo='{row["pNo"]}' " +
                                $"group by pNo,pBarcode,pName ORDER BY pNo";
                            DataTable outskReturnTbl = getTbl.table("WP", SqlComm);//銷退數，銷退金額

                            SqlComm = $"SELECT pNo, pNameS, SUM(oStkCost * Qty) AS costTotal, SUM(Qty) AS qtyTotal FROM WP_vStkTraceUnion " +
                                $"WHERE Kind<>'D' AND SUBSTRING(OutStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' " +
                                $"and qty > 0 and pNo='{row["pNo"]}' " +
                                $"GROUP BY pNo, pNameS";
                            DataTable oCostTbl = getTbl.table("WP", SqlComm);    //銷售成本, 銷售數

                            //SqlComm = $"SELECT pNo,pBarcode,pName,SUM(qty) AS sumQty, SUM(amtTotal) AS sumAmt FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 8) between '{sDate.Replace("/", "")}' AND '{eDate.Replace("/", "")}' and memSn='-1' group by pNo,pBarcode,pName ORDER BY pNo";
                            //DataTable surpluTbl = getTbl.table("WP", SqlComm);//盤盈


                            inQty_all = inskBeginTbl.Rows.Count == 0 ? 0 : int.Parse($"{inskBeginTbl.Rows[0]["sumQty"]}");
                            outQty_all = outskBeginTbl.Rows.Count == 0 ? 0 : int.Parse($"{outskBeginTbl.Rows[0]["sumQty"]}");
                            Beginning_qty = int.Parse($"{row["qtyInitial"]}") + inQty_all - outQty_all; //期初庫存數
                            in_inQty = inskTbl.Rows.Count == 0 ? 0 : int.Parse($"{inskTbl.Rows[0]["sumQty"]}");//進貨數
                            in_outQty = inskReturnTbl.Rows.Count == 0 ? 0 : int.Parse($"{inskReturnTbl.Rows[0]["sumQty"]}");//進退數
                            out_outQty = outskTbl.Rows.Count == 0 ? 0 : int.Parse($"{outskTbl.Rows[0]["sumQty"]}");//銷貨數
                            out_inQty = outskReturnTbl.Rows.Count == 0 ? 0 : int.Parse($"{outskReturnTbl.Rows[0]["sumQty"]}");//銷退數

                            //in_inCost = inskTbl.Rows.Count == 0 ? 0 : double.Parse($"{inskTbl.Rows[0]["sumAmt"]:#0.00}");//進貨金額
                            //in_outCost = inskReturnTbl.Rows.Count == 0 ? 0 : double.Parse($"{inskReturnTbl.Rows[0]["sumAmt"]:#0.00}");//進退金額
                            //out_inCost = outskReturnTbl.Rows.Count == 0 ? 0 : double.Parse($"{outskReturnTbl.Rows[0]["sumAmt"]:#0.00}");//銷退金額
                            
                            oStkAmt = outskTbl.Rows.Count == 0 ? 0 : double.Parse($"{outskTbl.Rows[0]["sumAmt"]:#0.00}");//銷貨金額
                            costAvg = double.Parse($"{row["costAvg"]:#0.00}");

                            Beginning_Cost = Beginning_qty * costAvg;
                            in_inCost = in_inQty * costAvg;//進貨金額
                            in_outCost = in_outQty * costAvg;//進退金額
                            out_outCost = out_outQty * costAvg;//銷貨金額
                            out_inCost = out_inQty * costAvg;//銷退金額

                            //iCost = in_inQty == 0 ? 0 : (in_inCost / in_inQty);//進貨成本(單)
                            Finally_Qty = $"{row["isUpdStock"]}" == "N" ? 0 : Beginning_qty + in_inQty - out_outQty - in_outQty + out_inQty;//期末數

                            //SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE YearMonth='{sDate.Replace("/", "").Substring(0,6)}' AND isDel='N' and pNo='{row["pNo"]}'";
                            //DataTable preStkTbl = getTbl.table("WP", SqlComm);
                            //preCost = preStkTbl.Rows.Count == 0 ? 0 : double.Parse($"{preStkTbl.Rows[0]["cost"]}");
                            //stkLeftAmt = 0;     //庫存成本
                            //if (Finally_Qty > 0) //庫存數
                            //{
                            //    stkLeftAmt = (Beginning_qty >= out_outQty) //Beginning_qty:上月結存數 / out_outQty:銷售數
                            //        ? ((Beginning_qty - out_outQty) * preCost) + in_inCost
                            //        : Finally_Qty * iCost;
                            //}
                            //oStkAmt = 0;        //銷售成本
                            //if (out_outQty > 0)
                            //{
                            //    if (oCostTbl.Rows.Count > 0)
                            //    {
                            //        oStkAmt = double.Parse($"{oCostTbl.Rows[0]["costTotal"]}");
                            //    }
                            //}
                            //Finally_Cost = stkLeftAmt+ in_inCost- in_outCost+ oStkAmt- out_inCost;
                            //oProfit = out_outCost - oStkAmt;

                            Finally_Cost = Beginning_Cost + in_inCost - in_outCost - out_outCost + out_inCost;
                            oProfit = oStkAmt - out_outCost;

                            Label2.Text += $"<tr class='amt-row'>" +
                                $"<td class='align-l'>{recNo}</td>" +
                                $"<td class='align-l'>{row["pBarcode"]}-{row["pName"]}</td>" +
                                $"<td class='Beginning_qty' data-val='{Beginning_qty}'>{threeDot.To3Dot(Beginning_qty.ToString())}</td>" +
                                $"<td class='Beginning_Cost' data-val='{Beginning_Cost}'>{threeDot.To3Dot(Beginning_Cost.ToString("0.00"))}</td>" +

                                $"<td class='in_inQty' data-val='{in_inQty}'>{threeDot.To3Dot(in_inQty.ToString())}</td>" +
                                $"<td class='in_inCost' data-val='{in_inCost}'>{threeDot.To3Dot(in_inCost.ToString("0.00"))}</td>" +
                                $"<td class='in_outQty' data-val='{in_outQty}'>{threeDot.To3Dot(in_outQty.ToString())}</td>" +
                                $"<td class='in_outCost' data-val='{in_outCost}'>{threeDot.To3Dot(in_outCost.ToString("0.00"))}</td>" +

                                $"<td class='out_outQty' data-val='{out_outQty}'>{threeDot.To3Dot(out_outQty.ToString())}</td>" +
                                $"<td class='out_outCost' data-val='{out_outCost}'>{threeDot.To3Dot(out_outCost.ToString("0.00"))}</td>" +
                                $"<td class='out_inQty' data-val='{out_inQty}'>{threeDot.To3Dot(out_inQty.ToString())}</td>" +
                                $"<td class='out_inCost' data-val='{out_inCost}'>{threeDot.To3Dot(out_inCost.ToString("0.00"))}</td>" +

                                $"<td class='Finally_Qty' data-val='{Finally_Qty}'>{threeDot.To3Dot(Finally_Qty.ToString())}</td>" +
                                $"<td class='Finally_Cost' data-val='{Finally_Cost}'>{threeDot.To3Dot(Finally_Cost.ToString("0.00"))}</td>" +
                                $"<td class='oStkAmt' data-val='{oStkAmt}'>{threeDot.To3Dot(oStkAmt.ToString())}</td>" +
                                $"<td class='oProfit' data-val='{oProfit}'>{threeDot.To3Dot(oProfit.ToString())}</td>" +
                            //$"<td class='amt_in' data-val='0'>0</td>" +
                            //$"<td class='qty_sum' data-val='{qty_total}'>{threeDot.To3Dot(qty_total)}</td>" +
                            //$"<td class='amt_sum' data-val='{amt_total}'>{threeDot.To3Dot(amt_total)}</td>" +
                            "</tr>";

                            Input(recNo.ToString(),$"{row["pBarcode"]}-{row["pName"]}", Beginning_qty.ToString(), Beginning_Cost.ToString(),
                                in_inQty.ToString(), in_inCost.ToString(), in_outQty.ToString(), in_outCost.ToString(),
                                out_outQty.ToString(), out_outCost.ToString(), out_inQty.ToString(), out_inCost.ToString(),
                                Finally_Qty.ToString(), Finally_Cost.ToString(), oStkAmt.ToString(), oProfit.ToString());
                        }
                        Label2.Text += $"<tr class='rptr-total'>" +
                            $"<td colspan='2'>合計︰</td>" +
                            $"<td class='Beginning_qty_total'></td>" +
                            $"<td class='Beginning_Cost_total'></td>" +
                            $"<td class='in_inQty_total'></td>" +
                            $"<td class='in_inCost_total'></td>" +
                            $"<td class='in_outQty_total'></td>" +
                            $"<td class='in_outCost_total'></td>" +
                            $"<td class='out_outQty_total'></td>" +
                            $"<td class='out_outCost_total'></td>" +
                            $"<td class='out_inQty_total'></td>" +
                            $"<td class='out_inCost_total'></td>" +
                            $"<td class='Finally_Qty_total'></td>" +
                            $"<td class='Finally_Cost_total'></td>" +
                            $"<td class='oStkAmt_total'></td>" +
                            $"<td class='oProfit_total'></td>" +
                        "</tr>";
                    }
                    Label2.Text += "</table>" +
                    "<div class='stamp' style='margin-top:50px;'><span>主辦︰　　　主任︰　　　會計︰　　　秘書︰　　　</div>" +
                    $"<input type='hidden' id='page-total' value='{pageNo}' />" +
                "</div>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印統計表</button>" + 
                "<input type = 'button' value = 'Excel下載' style='color:#080' onclick='DownloadFile()' />" +
                "</div>";
                }
                else if(pKSId == "0")
                {
                    Label2.Text += $"<tr><td colspan='10' class='empty-data'>請選擇類別！</td></tr>";
                }
            }
        }
    }
    public static List<Lable_7011> dataList;
    #region 欄位設定
    public class Lable_7011
    {
        public string Id { get; set; } //項次
        public string PdName { get; set; } //商品編號及名稱
        public string PrQty { get; set; } //期初庫存
        public string PrAmount { get; set; } //期初金額
        public string In_InQty { get; set; } //進貨數量
        public string In_InAmount { get; set; } //進貨金額
        public string In_OutQty { get; set; } //退貨數量
        public string In_OutAmount { get; set; } //退貨金額
        public string Out_InQty { get; set; } //銷貨數量
        public string Out_InAmount { get; set; } //銷貨成本
        public string Out_OutQty { get; set; } //銷退數量
        public string Out_OutAmount { get; set; } //銷退金額
        public string FinQty { get; set; } //期末庫存
        public string FinAmount { get; set; } //期末金額
        public string Cost { get; set; } //銷貨金額
        public string profit { get; set; } //銷貨利潤
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_7011>();
        dataList.Add(new Lable_7011()
        {
            Id = "項次",
            PdName = "商品編號及名稱",
            PrQty = "期初庫存",
            PrAmount = "期初金額",
            In_InQty = "進貨數量",
            In_InAmount = "進貨金額",
            In_OutQty = "退貨數量",
            In_OutAmount = "退貨金額",
            Out_InQty = "銷貨數量",
            Out_InAmount = "銷貨成本",
            Out_OutQty = "銷退數量",
            Out_OutAmount = "銷退金額",
            FinQty = "期末庫存",
            FinAmount = "期末金額",
            Cost = "銷貨金額",
            profit = "銷貨利潤"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string i, string name, string pq, string pa, 
        string iiq, string iia, string ioq, string ioa,
        string oiq,string oia, string ooq, string ooa,
        string fq, string fa,string co,string p)
    {
        dataList.Add(new Lable_7011()
        {
            Id = i,
            PdName = name,
            PrQty = pq,
            PrAmount = pa,
            In_InQty = iiq,
            In_InAmount = iia,
            In_OutQty = ioq,
            In_OutAmount = ioa,
            Out_InQty = oiq,
            Out_InAmount = oia,
            Out_OutQty = ooq,
            Out_OutAmount = ooa,
            FinQty = fq,
            FinAmount = fa,
            Cost = co,
            profit = p
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "7011門市營業彙總表";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Id;
            worksheet.Cell(j, 2).Value = dataList[j - 1].PdName;
            worksheet.Cell(j, 3).Value = dataList[j - 1].PrQty;
            worksheet.Cell(j, 4).Value = dataList[j - 1].PrAmount;
            worksheet.Cell(j, 5).Value = dataList[j - 1].In_InQty;
            worksheet.Cell(j, 6).Value = dataList[j - 1].In_InAmount;
            worksheet.Cell(j, 7).Value = dataList[j - 1].In_OutQty;
            worksheet.Cell(j, 8).Value = dataList[j - 1].In_OutAmount;
            worksheet.Cell(j, 9).Value = dataList[j - 1].Out_InQty;
            worksheet.Cell(j, 10).Value = dataList[j - 1].Out_InAmount;
            worksheet.Cell(j, 11).Value = dataList[j - 1].Out_OutQty;
            worksheet.Cell(j, 12).Value = dataList[j - 1].Out_OutAmount;
            worksheet.Cell(j, 13).Value = dataList[j - 1].FinQty;
            worksheet.Cell(j, 14).Value = dataList[j - 1].FinAmount;
            worksheet.Cell(j, 15).Value = dataList[j - 1].Cost;
            worksheet.Cell(j, 16).Value = dataList[j - 1].profit;

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