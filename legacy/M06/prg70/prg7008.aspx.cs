using ClosedXML.Excel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Web;
using static Global;

public partial class prg7008 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7008";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                TableName();
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate"));
                string outType = cookies.Read("outType");

                sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                sDate = int.Parse(DateTime.Parse(sDate).ToString("yyyyMMdd")) < int.Parse(StartYM) ? (StartYM.Substring(0, 4) + "/" + StartYM.Substring(4, 2) + "/" + StartYM.Substring(6, 2)) : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                if (sDate != "" && eDate != "")
                {
                    string name = "";
                    string SqlComm;

                    if (outType == "0")
                    {
                        SqlComm = $"SELECT InStkId,InStkDate,pvId,pvName,amount,payLeft ,(amount-payLeft) as pay " +
                       $"FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' " +
                       $"AND (InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') " +
                       $"AND pvId <> 'stkUpd' " +
                       $"AND payType IN (2) " +
                       $"group by pvId,pvName,InStkId,InStkDate,amount,payLeft,(amount-payLeft) " +
                       $"ORDER BY InStkDate,InStkId,pvId ";
                        name = "------已付";
                    }
                    else if (outType == "1")
                    {
                        SqlComm = $"SELECT InStkId,InStkDate,pvId,pvName,amount,payLeft ,(amount-payLeft) as pay " +
                           $"FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' " +
                           $"AND (InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') " +
                           $"AND pvId <> 'stkUpd' " +
                           $"AND payType IN (0,1) " +
                           $"group by pvId,pvName,InStkId,InStkDate,amount,payLeft,(amount-payLeft) " +
                           $"ORDER BY InStkDate,InStkId,pvId ";
                        name = "------未付";
                    }
                    else
                     SqlComm = $"SELECT InStkId,InStkDate,pvId,pvName,amount,payLeft ,(amount-payLeft) as pay " +
                        $"FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' " +
                        $"AND (InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') " +
                        $"AND pvId <> 'stkUpd' " +
                        $"group by pvId,pvName,InStkId,InStkDate,amount,payLeft,(amount-payLeft) " +
                        $"ORDER BY InStkDate,InStkId,pvId ";
                    
                    DataTable inStkTbl = getTbl.table("WP", SqlComm);

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
                         "<div class='search-sub'>" +
                            "<span class='input-title'>類別</span>" +
                                "<select id='outType' name='outType' class='form-control' data-alt='MI_次類別編號'>" +
                                    "<option value=''>全部</option>" +
                                    $"<option value='0' {(outType == "0" ? "selected" : "")}>已付</option>" +
                                    $"<option value='1' {(outType == "1" ? "selected" : "")}>未付</option>" +
                                "</select>" +
                        "</div>" +
                        "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                    "</div>";

                    Label2.Text += "<div class='rptr-main'>" +
                        $"<div class='rptr-title'>進貨統計表{name}</div>" +
                        "<div class='rptr-data'>" +
                            $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                        "</div>" +
                        "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                                        "<td class='align-l'>進貨單號</td><td class='align-l'>進貨日期</td><td class='align-l'>廠商代號</td><td class='align-l'>廠商名稱</td><td>應付金額</td><td>已結金額</td><td>未結金額</td>" +
                            "</tr>";
                    if (inStkTbl.Rows.Count == 0)
                    {
                        Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                    }
                    else
                    {
                        ThreeDot threeDot = new ThreeDot();
                        int recNo = 0;
                        foreach (DataRow row in inStkTbl.Rows)
                        {

                            recNo++;
                            if (((recNo) % 45) == 1 && recNo > 1)
                            {
                                pageNo++;

                                Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                $"<div class='rptr-title'>進貨統計表{name}</div>" +
                                "<div class='rptr-data'>" +
                                    $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                    $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                "</div>" +
                                "<table class='list-main' style='border-spacing:1px;'>" +
                                    "<tr class='list-title'>" +
                                        "<td class='align-l'>進貨單號</td><td class='align-l'>進貨日期</td><td class='align-l'>廠商代號</td><td class='align-l'>廠商名稱</td><td>應付金額</td><td>已結金額</td><td>未結金額</td>" +
                                    "</tr>";
                            }
                            string amount = $"{row["amount"]:#0}";
                            string Notpay = $"{row["payLeft"]:#0}";
                            string pay = $"{row["pay"]:#0}";

                            Label2.Text += $"<tr class='amt-row'>" +
                                $"<td class='align-l'>{row["InStkId"]}</td>" +
                                $"<td class='align-l'>{row["InStkDate"]:yyyy/MM/dd}</td>" +
                                $"<td class='align-l'>{row["pvId"]}</td>" +
                                $"<td class='align-l'>{row["pvName"]}</td>" +
                                $"<td class='qty_p' data-val='{amount}'>{threeDot.To3Dot(amount)}</td>" +
                                $"<td class='amt_p' data-val='{pay}'>{threeDot.To3Dot(pay)}</td>" +
                                $"<td class='qty_m' data-val='{Notpay}'>{threeDot.To3Dot(Notpay)}</td>" +
                            "</tr>";

                            Input("'"+row["InStkId"].ToString(),$"{row["InStkDate"]:yyyy/MM/dd}",$"{row["pvId"]}", $"{row["pvName"]}",
                                amount.ToString(), pay.ToString(),Notpay.ToString());
                        }
                        Label2.Text += $"<tr class='rptr-total'>" +
                            $"<td colspan='4'>合計︰</td>" +
                            $"<td class='qty_p_total'></td>" +
                            $"<td class='amt_p_total'></td>" +
                            $"<td class='qty_m_total'></td>" +
                        "</tr>";
                        }
                    SqlComm = $"SELECT sum(amtTotal) as Total, sum(qty) as qtyTotal,pNameS FROM WP_vInStock WHERE pNameS like'酒瓶' AND InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59'group by pNameS";
                    DataTable w = getTbl.table ("WP", SqlComm);
                    DataRow[] wRows;
                    wRows = w.Select ();
                    string qtyTotal = ""; double total = 0;
                    foreach (DataRow row in w.Rows)
                        {
                        string pNameS = row["pNameS"].ToString ();
                        qtyTotal = row["qtyTotal"].ToString () == "" ? "0" : row["qtyTotal"].ToString ();
                        total = Convert.ToDouble (row["Total"]);
                        // 現在你可以使用 'pNameS' 和 'total' 變數進行後續的操作
                        // 例如，將它們添加到你的 Label2.Text 中
                        }
                    qtyTotal = string.IsNullOrEmpty (qtyTotal) ? "0" : qtyTotal;
                    Label2.Text += "</table>" +
                         //$"<div class='stamp' style='margin-top:50px;'>'{}'"+
                         $"酒瓶退貨數：{qtyTotal}, 總計：{total:#0.00}<br>"+
                    "<div class='stamp' style='margin-top:50px;'><span>主辦︰　　　主任︰　　　會計︰　　　秘書︰　　　</div>" +
                    $"<input type='hidden' id='page-total' value='{pageNo}' />" +
                "</div>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印統計表</button>" +
                "<input type = 'button' value = 'Excel下載' style='color:#080' onclick='DownloadFile()' />" +
                "</div>";

                }
            }
        }
    }
    public static List<Lable_7008> dataList;
    #region 欄位設定
    public class Lable_7008
    {
        public string InstkId { get; set; } //進貨單號
        public string Date { get; set; } //進貨日期
        public string PvId { get; set; } //廠商代號
        public string PvName { get; set; } //廠商名稱
        public string Amount { get; set; } //應付金額
        public string Pay { get; set; } //已結金額
        public string Notpay { get; set; } //未結金額
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_7008>();
        dataList.Add(new Lable_7008()
        {
            InstkId = "進貨單號",
            Date = "進貨日期",
            PvId = "廠商代號",
            PvName = "廠商名稱",
            Amount = "應付金額",
            Pay = "已結金額",
            Notpay = "未結金額"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string num, string name, string lq, string pq, string pc, string sq, string sa)
    {
        dataList.Add(new Lable_7008()
        {
            InstkId = num,
            Date = name,
            PvId = lq,
            PvName = pq,
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

        var WorksheetsName = "7008進貨統計表";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].InstkId;
            worksheet.Cell(j, 2).Value = dataList[j - 1].Date;
            worksheet.Cell(j, 3).Value = dataList[j - 1].PvId;
            worksheet.Cell(j, 4).Value = dataList[j - 1].PvName;
            worksheet.Cell(j, 5).Value = dataList[j - 1].Amount;
            worksheet.Cell(j, 6).Value = dataList[j - 1].Pay;
            worksheet.Cell(j, 7).Value = dataList[j - 1].Notpay;
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