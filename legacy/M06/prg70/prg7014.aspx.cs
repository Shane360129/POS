using ClosedXML.Excel;
using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Web;
using static Global;

public partial class prg7014 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
        {
        Employee emp = new Employee ();
        ToDouble toDouble = new ToDouble ();
        if (!emp.IsEmp) { Response.Redirect ("/Login.aspx"); }
        else
            {
            string PrgId = "7014";
            if (!emp.ChkPrg (PrgId)) { Response.Redirect ("/index.aspx"); }
            else
                {
                TableName ();
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode (cookies.Read ("sDate")),
                    eDate = WebUtility.UrlDecode (cookies.Read ("eDate")),
                    dtl = WebUtility.UrlDecode (cookies.Read ("dtl")),
                    pvSn = cookies.Read ("memSn");
                string payType = cookies.Read ("outType");

                sDate = sDate == "" ? DateTime.Now.AddMonths (-1).ToString ("yyyy/MM/01") : sDate;
                sDate = int.Parse (DateTime.Parse (sDate).ToString ("yyyyMMdd")) < int.Parse (StartYM) ? (StartYM.Substring (0, 4) + "/" + StartYM.Substring (4, 2) + "/" + StartYM.Substring (6, 2)) : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString ("yyyy/MM/dd") : eDate;

                if (sDate != "" && eDate != "")
                    {
                    string name = "";
                    string SqlComm;

                    
                    DataTable inStkTbl = new DataTable ();
                    if (dtl == "N")
                        {
                        if (payType == "0")
                            {
                            SqlComm = $"SELECT pvId, pvNameS, SUM(payLeft) AS totalOutLeft, SUM(amount) AS totalAmount, SUM(amount) - SUM(payLeft) as pay  " +
                                $"FROM (SELECT pvId, amount, payLeft, pvNameS,InStkId " +
                                $"FROM WP_vPay  WHERE isDel = 'N' AND payType='2' AND (InStkDate BETWEEN  '{sDate}' AND '{eDate} 23:59:59') AND pvSn NOT IN ('0', '-1') " +
                                $"GROUP BY pvId, InStkId, amount, payLeft, pvNameS) AS SubQuery  " +
                                $"GROUP BY pvId, pvNameS ORDER BY pvId";
                            name = "------已收";
                            inStkTbl = getTbl.table ("WP", SqlComm);
                        }
                        else if (payType == "1")
                            {
                            SqlComm = $"SELECT pvId, pvNameS, SUM(payLeft) AS totalOutLeft, SUM(amount) AS totalAmount, SUM(amount) - SUM(payLeft) as pay  " +
                               $"FROM (SELECT pvId, amount, payLeft, pvNameS,InStkId " +
                               $"FROM WP_vPay  WHERE isDel = 'N' AND payType != '2' AND (InStkDate BETWEEN  '{sDate}' AND '{eDate} 23:59:59') AND pvSn NOT IN ('0', '-1') " +
                               $"GROUP BY pvId, InStkId, amount, payLeft, pvNameS) AS SubQuery  " +
                               $"GROUP BY pvId, pvNameS ORDER BY pvId";
                            name = "------未收";
                            inStkTbl = getTbl.table ("WP", SqlComm);
                            }
                        else
                            {
                            SqlComm = $"SELECT pvId, pvNameS, SUM(payLeft) AS totalOutLeft, SUM(amount) AS totalAmount, SUM(amount) - SUM(payLeft) as pay  " +
                                $"FROM (SELECT pvId, amount, payLeft, pvNameS,InStkId " +
                                $"FROM WP_vPay  WHERE isDel = 'N' AND (InStkDate BETWEEN  '{sDate}' AND '{eDate} 23:59:59') AND pvSn NOT IN ('0', '-1') " +
                                $"GROUP BY pvId, InStkId, amount, payLeft, pvNameS) AS SubQuery  " +
                                $"GROUP BY pvId, pvNameS ORDER BY pvId";
                            inStkTbl = getTbl.table ("WP", SqlComm);
                            }
                        }
                    else
                        {
                        if (payType == "0")
                            {
                            SqlComm = $"SELECT * FROM " +
                                                    $"(SELECT pvId, pvNameS,  MAX (amount) as amount, MAX (payLeft) as payLeft,InStkId FROM WP_vInStock " +
                                                    $"WHERE isDel = 'N' AND dtlIsDel = 'N' AND (InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') AND pvSn<>'-1' " +
                                                    $"{(pvSn == "" ? "" : $"AND pvSn='{pvSn}'")} " +
                                                    $" AND payType ='2' GROUP BY pvId, pvNameS,InStkId) AS SubQuery ORDER BY pvId";
                            inStkTbl = getTbl.table ("WP", SqlComm);
                            name = "------已收";
                            }
                        else if (payType == "1")
                            {
                            SqlComm = $"SELECT * FROM " +
                                                    $"(SELECT pvId, pvNameS,  MAX (amount) as amount, MAX (payLeft) as payLeft,InStkId FROM WP_vInStock " +
                                                    $"WHERE isDel = 'N' AND dtlIsDel = 'N' AND (InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') AND pvSn<>'-1' " +
                                                    $"{(pvSn == "" ? "" : $"AND pvSn='{pvSn}'")} " +
                                                    $" AND payType !='2' GROUP BY pvId, pvNameS,InStkId) AS SubQuery ORDER BY pvId";
                            inStkTbl = getTbl.table ("WP", SqlComm);
                            name = "------未收";
                            }
                        else
                            {
                            SqlComm = $"SELECT * FROM " +
                                                    $"(SELECT pvId, pvNameS,  MAX (amount) as amount, MAX (payLeft) as payLeft,InStkId FROM WP_vInStock " +
                                                    $"WHERE isDel = 'N' AND dtlIsDel = 'N' AND (InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') AND pvSn<>'-1' " +
                                                    $"{(pvSn == "" ? "" : $"AND pvSn='{pvSn}'")} " +
                                                    $"GROUP BY pvId, pvNameS,InStkId) AS SubQuery ORDER BY pvId";
                            inStkTbl = getTbl.table ("WP", SqlComm);
                            }
                        }

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
                            "<span class='input-title' style='margin:0 10px;'>類別</span>" +
                                "<select id='outType' name='outType' class='form-control' data-alt='MI_次類別編號'>" +
                                    "<option value=''>全部</option>" +
                                    $"<option value='0' {(payType == "0" ? "selected" : "")}>已付</option>" +
                                    $"<option value='1' {(payType == "1" ? "selected" : "")}>未付</option>" +
                                "</select>" +
                        "</div>";
                    Label2.Text += "<div class='search-sub'>" +
                         $"<span class='input-title'>明細</span>" +
                           "<select id='dtl' class='form-control'>" +
                           $"<option value='Y'  {(dtl == "Y" ? "selected" : "")}>是</option>" +
                           $"<option value='N' {(dtl == "N" ? "selected" : "")}>否</option>" +
                           "</select>" +
                           "</div>";
                    SqlComm = $"SELECT * FROM WP_Provider WHERE sn<>'0' ORDER BY sn";
                    DataTable memTbl = getTbl.table ("WP", SqlComm);
                    Label2.Text += "<div class='search-sub'>" +
                        "<span class='input-title'>會　　員</span>" +
                        "<select id='memSn' class='form-control'>" +
                            "<option value=''>全部</option>";
                    foreach (DataRow row in memTbl.Rows)
                        {
                        Label2.Text += $"<option value='{row["sn"]}' {(pvSn == row["sn"].ToString () ? "selected" : "")}>{row["pvId"]}-{row["pvName"]}</option>";
                        }
                    Label2.Text += "</select>" +
                        "</div>" +
                            "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                    "</div>" +
                    "<div class='rptr-main'>" +
                        $"<div class='rptr-title'>應付帳款彙總表</div>" +
                        "<div class='rptr-data'>" +
                            $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString ("yyyy/MM/dd")}</div>" +
                        "</div>" ;
                        
                    if (dtl == "N")
                        {
                        if (inStkTbl.Rows.Count == 0)
                            {
                            Label2.Text += "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                            "<td class='align-l'>會員代號</td><td class='align-l'>會員名稱</td><td>應付金額</td><td>已付金額</td><td>未付金額</td>" +
                            "</tr>";
                            Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                            }
                        else
                            {
                            ThreeDot threeDot = new ThreeDot ();
                            int recNo = 0;
                            Label2.Text += "<table class='list-main' style='border-spacing:1px;'>" +
                      "<tr class='list-title'>" +
                          "<td class='align-l'>會員代號</td><td class='align-l'>會員名稱</td><td>應付金額</td><td>已付金額</td><td>未付金額</td>" +
                      "</tr>";
                            foreach (DataRow row in inStkTbl.Rows)
                                {
                                recNo++;
                                if (((recNo) % 45) == 1 && recNo > 1)
                                    {
                                    pageNo++;

                                    Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                    $"<div class='rptr-title'>應付帳款彙總表{name}</div>" +
                                    "<div class='rptr-data'>" +
                                        $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                        $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString ("yyyy/MM/dd")}</div>" +
                                    "</div>" +
                                    "<table class='list-main' style='border-spacing:1px;'>" +
                                        "<tr class='list-title'>" +
                                            "<td class='align-l'>會員代號</td><td class='align-l'>會員名稱</td><td>應付金額</td><td>已付金額</td><td>未付金額</td>" +
                                        "</tr>";
                                    }

                                string Notpay = $"{row["totalOutLeft"]:#0}";
                                string pay = $"{row["pay"]:#0}";
                                decimal totalAmount = Math.Round(Convert.ToDecimal (row["totalAmount"]));
                                decimal pay2 = Math.Round(Convert.ToDecimal (row["pay"]));
                                decimal amount2 = totalAmount - pay2;
                                string amount = amount2.ToString ();

                                    
                                Label2.Text += 
                                $"<tr class='amt-row'>" +
                                    $"<td class='align-l'>{row["pvId"]}</td>" +
                                    $"<td class='align-l'>{row["pvNameS"]}</td>" +
                                    $"<td class='qty_p' data-val='{totalAmount}'>{threeDot.To3Dot (totalAmount.ToString ())}</td>" +
                                    $"<td class='amt_p' data-val='{pay}'>{threeDot.To3Dot (pay)}</td>" +
                                    $"<td class='qty_m' data-val='{amount}'>{threeDot.To3Dot (amount)}</td>" +
                                "</tr>";

                                //Input ($"{row["OutStkId"]}", $"{row["memName"]}", $"{row["memId"]}", $"{row["memName"]}",
                                //        amount.ToString (), pay.ToString (), Notpay.ToString ());
                                }
                            Label2.Text += $"<tr class='rptr-total'>" +
                                $"<td colspan='2'>合計︰</td>" +
                                $"<td class='qty_p_total'></td>" +
                                $"<td class='amt_p_total'></td>" +
                                $"<td class='qty_m_total'></td>" +
                            "</tr>";
                            }
                        }
                    else
                        {
                        if (inStkTbl.Rows.Count == 0)
                            {
                            Label2.Text += "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                            "<td class='align-l'>會員代號</td><td class='align-l'>會員名稱</td><td>應付金額</td><td>已付金額</td><td>未付金額</td>" +
                            "</tr>";
                            Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                            }
                        else
                            {
                            ThreeDot threeDot = new ThreeDot ();
                            int recNo = 0;
                            Label2.Text += "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                                "<td class='align-l'>單號</td><td class='align-l'>會員名稱</td><td>應付金額</td><td>已付金額</td><td>未付金額</td>" +
                            "</tr>";
                            foreach (DataRow row in inStkTbl.Rows)
                                {
                                recNo++;
                                if (((recNo) % 45) == 1 && recNo > 1)
                                    {
                                    pageNo++;

                                    Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                    $"<div class='rptr-title'>應付帳款彙總表{name}</div>" +
                                    "<div class='rptr-data'>" +
                                        $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                        $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString ("yyyy/MM/dd")}</div>" +
                                    "</div>" +
                                    "<table class='list-main' style='border-spacing:1px;'>" +
                                        "<tr class='list-title'>" +
                                            "<td class='align-l'>單號</td><td class='align-l'>會員名稱</td><td>應付金額</td><td>已付金額</td><td>未付金額</td>" +
                                        "</tr>";
                                    }

                                string Notpay = $"{row["payLeft"]:#0}";
                                string pay = $"{row["amount"]:#0}";
                                decimal totalAmount = Math.Round (Convert.ToDecimal (row["amount"]));
                                decimal pay2 = Math.Round (Convert.ToDecimal (row["payLeft"]));
                                decimal amount2 = totalAmount - pay2;
                                string amount = amount2.ToString ();


                                Label2.Text +=
                                $"<tr class='amt-row'>" +
                                    $"<td class='align-l'>{row["InStkId"]}</td>" +
                                    $"<td class='align-l'>{row["pvNameS"]}</td>" +
                                    $"<td class='qty_p' data-val='{totalAmount}'>{threeDot.To3Dot (totalAmount.ToString ())}</td>" +
                                    $"<td class='amt_p' data-val='{amount}'>{threeDot.To3Dot (amount)}</td>" +
                                    $"<td class='qty_m' data-val='{Notpay}'>{threeDot.To3Dot (Notpay)}</td>" +
                                "</tr>";

                                //Input ($"{row["OutStkId"]}",$"{row["memName"]}",$"{row["memId"]}", $"{row["memName"]}",
                                //        amount.ToString (), pay.ToString (), Notpay.ToString ());
                                }
                            Label2.Text += $"<tr class='rptr-total'>" +
                                $"<td colspan='2'>合計︰</td>" +
                                $"<td class='qty_p_total'></td>" +
                                $"<td class='amt_p_total'></td>" +
                                $"<td class='qty_m_total'></td>" +
                            "</tr>";
                            }
                        }
                    Label2.Text += "</table>" +
                    "<div style='margin-top:50px;'>主管︰<span style='padding-left:200px;'></span>經辦︰<span style='padding-left:200px;'></span>製表︰門市人員</div>" +
                    $"<input type='hidden' id='page-total' value='{pageNo}' />" +
                "</div>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印報表</button>" +
                //"<input type = 'button' value = 'Excel下載' style='color:#080;margin-left:5px;' onclick='DownloadFile()' />" +
                "</div>";

                    }
                }
            }
        }
    public static List<Lable_7009> dataList;
    #region 欄位設定
    public class Lable_7009
    {
        public string OutstkId { get; set; } //銷貨單號
        public string Date { get; set; } //銷貨日期
        public string MemId { get; set; } //會員代號
        public string MemName { get; set; } //會員名稱
        public string Amount { get; set; } //應收金額
        public string Pay { get; set; } //已結金額
        public string Notpay { get; set; } //未結金額
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_7009>();
        dataList.Add(new Lable_7009()
        {
            OutstkId = "銷貨單號",
            Date = "銷貨日期",
            MemId = "會員代號",
            MemName = "會員名稱",
            Amount = "應收金額",
            Pay = "已結金額",
            Notpay = "未結金額"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string num,string name,string lq, string pq, string pc, string sq, string sa)
    {
        dataList.Add(new Lable_7009()
        {
            OutstkId = num,
            Date = name,
            MemId = lq,
            MemName = pq,
            Amount = pc,
            Pay = sq,
            Notpay = sa
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "7009銷貨統計表";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
            {
            worksheet.Cell (j, 1).Value = dataList[j - 1].OutstkId;
            worksheet.Cell (j, 2).Value = dataList[j - 1].Date;
            worksheet.Cell (j, 3).Value = dataList[j - 1].MemId;
            worksheet.Cell (j, 4).Value = dataList[j - 1].MemName;
            worksheet.Cell (j, 5).Value = dataList[j - 1].Amount;
            worksheet.Cell (j, 6).Value = dataList[j - 1].Pay;
            worksheet.Cell (j, 7).Value = dataList[j - 1].Notpay;

            if (j != 1)
                {
                worksheet.Cell (j, 1).Style.NumberFormat.Format = "0";
                }
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