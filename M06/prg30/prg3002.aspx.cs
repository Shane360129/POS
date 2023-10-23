using System;
using System.Data;
using System.Net;
using static Global;


using System.Collections.Generic;
using ClosedXML.Excel;
using System.Web;
using System.IO;


public partial class prg3002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        TableName();
        Employee emp = new Employee();
        Stock stock = new Stock();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "3002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                ToDouble toDouble = new ToDouble();

                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm,
                    sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    pvSn = cookies.Read("pvSn"),
                    pvFilter = WebUtility.UrlDecode(cookies.Read("pvFilter")),
                    reciptNo = cookies.Read("reciptNo"),
                    pName = WebUtility.UrlDecode(cookies.Read("pName")),
                    pNo = cookies.Read("pNo"),
                    isTax = cookies.Read("isTax"),
                    payType = cookies.Read("payType"),
                    InStkId = cookies.Read("InStkId");

                Label2.Text = $"<input type='hidden' id='pvType' value='{pvType}' />" +
                "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 搜尋條件</div>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>日期區間</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input open-datepicker' readonly id='sDate' value='{sDate}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                            "<span style='margin:0 5px;'>至</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input open-datepicker' readonly id='eDate' value='{eDate}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>";
                Label2.Text += "<div class='search-sub'>" +
                    "<span class='input-title'>廠　　商</span>";
                if (pvType == "1")
                {    //下拉選單
                    SqlComm = $"SELECT * FROM WP_Provider ORDER BY pvId";
                    DataTable pvTbl = getTbl.table("WP", SqlComm);
                    Label2.Text += "<select id='pvSn' class='form-control'>" +
                        "<option value=''>全部</option>";
                    foreach (DataRow row in pvTbl.Rows)
                    {
                        Label2.Text += $"<option value='{row["sn"]}' {(pvSn == row["sn"].ToString() ? "selected" : "")}>{row["pvId"]}-{row["pvName"]}</option>";
                    }
                    Label2.Text += "</select>";
                }
                else    //autocomplete
                {
                    Label2.Text += "<div class='del-txt-group' style='margin-right:65px;'>" +
                        $"<input type='text' class='form-control del-txt-input' placeholder='請輸入廠商名稱或代號' maxlength='42' style='width:266px;' id='pv-filter'  value='{pvFilter}' />" +
                        "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                        $"<input type='hidden' name='pvSn' id='act-pvSn' value='{pvSn}' />" +
                    "</div>";
                }
                Label2.Text += "</div>" +
                "<div class='search-sub'>" +
                    "<span class='input-title'>進貨單號</span>" +
                    $"<input type='text' class='form-control' style='margin-right:45px;' id='InStkId' size='20' maxlength='50' value='{InStkId}' />" +
                "</div>" +
            "</div>" +
            "<div>" +
                "<div class='search-sub'>" +
                    "<span class='input-title'>發票號碼</span>" +
                    $"<input type='text' class='form-control' style='margin-right:104px;' id='reciptNo' size='20' maxlength='50' value='{reciptNo}' />" +
                "</div>" +
                "<div class='search-sub' style='margin-right:155px;'>" +
                    "<span class='input-title'>稅　　別</span>" +
                    "<select id='isTax' class='form-control'>" +
                        "<option value=''>全部</option>" +
                        $"<option value='Y' {(isTax == "Y" ? "selected" : "")}>應稅</option>" +
                        $"<option value='N' {(isTax == "N" ? "selected" : "")}>免稅</option>" +
                    "</select>" +
                "</div>" +
                "<div class='search-sub'>" +
                    "<span class='input-title'>銷帳狀態</span>" +
                    "<select id='payType' class='form-control'>" +
                        "<option value=''>全部</option>" +
                        $"<option value='0' {(payType == "0" ? "selected" : "")}>未結</option>" +
                        $"<option value='1' {(payType == "1" ? "selected" : "")}>未結完</option>" +
                        $"<option value='2' {(payType == "2" ? "selected" : "")}>全結</option>" +
                    "</select>" +
                "</div>" +
            "</div>" +
            "<div>" +
                "<div class='search-sub' style='margin-right:40px;'>" +
                    "<span class='input-title'>商　　品</span>" +
                    "<div class='del-txt-group'>" +
                        $"<input type='text' class='form-control del-txt-input' placeholder='請輸入商品名稱或條碼' maxlength='42' style='width:266px;' id='pd-filter' value='{pName}' />" +
                        $"<input type='hidden' id='act-pNo' value='{pNo}' />" +
                        "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                    "</div>" +
                "</div>" +
            "</div>" +
            "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
        "</div>";

                if (sDate != "" && eDate != "")
                {
                    SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND " +
                       $"(InStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') " +
                       $"{(pvSn == "" ? "" : $"AND pvSn='{pvSn}'")} " +
                       $"{(reciptNo == "" ? "" : $"AND reciptNo='{reciptNo}'")} " +
                       $"{(pNo == "" ? pName == "" ? "" : $"AND (pNameS like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')" : $"AND (pNo='{pNo}')")} " +
                       $"{(isTax == "" ? "" : $"AND isTax='{isTax}'")} " +
                       $"{(payType == "" ? "" : $"AND payType='{payType}'")} " +
                       $"{(InStkId == "" ? "" : $"AND InStkId='{InStkId}'")} " +
                       $"ORDER BY pvId, InStkDate, InStkId, sn, dtlSn";
                    DataTable inStkTbl = getTbl.table("WP", SqlComm);

                    Label2.Text += "<table class='list-main inStk-list-main' style='border-spacing:1px;'>" +
                        "<tr class='list-title'><td>廠商</td><td>進貨日期</td><td>進貨單號</td><td>商品</td><td style='text-align:right'>含稅單價</td><td style='text-align:center'>稅別</td><td style='text-align:right'>數量</td><td style='text-align:right'>小計</td><td style='text-align:center'>銷帳狀態</td></tr>";
                    if (inStkTbl.Rows.Count == 0)
                    {
                        Label2.Text += "<tr><td colspan='9' class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無進貨交易！</td></tr>";
                    }
                    else
                    {
                        string preId = "", main_data_id = "";
                        string preMainId = "", pre2nd = "", pre3th = "", mainId = "", twoId = "", thirdId = "";
                        foreach (DataRow row in inStkTbl.Rows)
                        {
                            if (preMainId != $"{row["pvId"]}")
                            {
                                mainId = $"{row["pvId"]}";
                                twoId = $"{row["InStkDate"]}";
                                thirdId = $"{row["InStkId"]}";
                            }
                            else
                            {
                                mainId = "";
                                if (pre2nd != $"{row["InStkDate"]}")
                                {
                                    twoId = $"{row["InStkDate"]}";
                                    thirdId = $"{row["InStkId"]}";
                                }
                                else
                                {
                                    twoId = $"";
                                    thirdId = pre3th != $"{row["InStkId"]}" ? $"{row["InStkId"]}" : "";
                                }
                            }
                            main_data_id = preId != row["sn"].ToString() ? $"{row["sn"]}" : "";
                            double dtlAmt = toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), 2);

                            Label2.Text += "<tr>" +
                                $"<td class='row-main' data-id='{mainId}'>{row["pvId"]}-{row["pvNameS"]}</td>" +
                                $"<td class='row-main' data-id='{twoId}'>{row["InStkDate"]:yyyy/MM/dd}</div>" +
                                $"<td class='row-main' data-id='{thirdId}'><a href='/prg30/prg3002Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["InStkId"]}</a></div>" +
                                $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}-{row["pNameS"]}</td>" +
                                $"<td style='text-align:right'>{row["dtlAmt"]:#0.00}</td>" +
                                $"<td style='text-align:center'>{(row["dtlIsTax"].ToString() == "Y" ? "應稅" : "免稅")}</td>" +
                                $"<td style='text-align:right' class='dtl-qty'>{row["qty"]}</td>" +
                                $"<td style='text-align:right' class='dtl-amt'>{row["amtTotal"]:#0}</td>" +
                                $"<td style='text-align:center'>{stock.PayTypeName(row["dtlPayType"].ToString())}</td>" +
                            "</tr>";
                            preId = row["sn"].ToString();
                            preMainId = $"{row["pvId"]}";
                            pre2nd = $"{row["InStkDate"]}";
                            pre3th = $"{row["InStkId"]}";

                            #region 存為Excel的欄位資料轉換
                            //廠商
                            var com = row["pvId"].ToString() + " " + row["pvNameS"].ToString();
                            //進貨日期
                            var date = Convert.ToDateTime(row["InStkDate"]).ToString("yyyy/MM/dd");
                            //進貨單號
                            var num = "'" + row["InStkId"].ToString();//在EXCEL中在數值前加入一個單引號(')，數值就會變成文字
                            //商品
                            var Barcode = row["pBarcode"].ToString();//國際條碼
                            if (Barcode == "")
                                Barcode = row["pCode"].ToString();//商品代碼
                            else
                                Barcode = row["pBarcode"].ToString();
                            var comm = Barcode + " - " + row["pNameS"].ToString();
                            //含稅單價
                            var tax = Convert.ToDouble(row["dtlAmt"]).ToString("f2");
                            //稅別
                            var dtlIsTax = row["dtlIsTax"].ToString();
                            if (dtlIsTax == "Y")
                                dtlIsTax = "應稅";
                            else
                                dtlIsTax = "免稅";
                            //數量
                            var amo = row["qty"].ToString();
                            //小計
                            var sub = Convert.ToDouble(row["amtTotal"]).ToString("0");
                            //銷帳狀態
                            var state = row["dtlPayType"].ToString();
                            if (state == "0")
                                state = "未結";
                            else if (state == "1")
                                state = "未結完";
                            else if (state == "2")
                                state = "全結";

                            Input(com, date, num, comm, tax, dtlIsTax, amo, sub, state);
                            #endregion
                        }
                        Label2.Text += "<tr><td colspan='7' style='text-align:right'>總計︰<span id='total-qty'>0</span></td><td style='text-align:right' id='total-amt'></td><td></td></tr>";
                    }
                    //Label2.Text += "</table>" +
                    //"<div class='align-r' style='margin-top:15px;'>" +
                    //"<a href='prg3002PRN.aspx' target='_blank'><button style='color:#080'>採購單列印</button></a>" +
                    //"</div>";
                    //Label2.Text += "</table>" +
                    //"<div class='align-r' style='margin-top:15px;'>" +
                    //"<button class='btn-submit' id='btn_CalPerBonusCount'>Excel檔案下載</button>" +
                    //"</div>";
                    Label2.Text += "</table>" +
                    "<div class='align-r' style='margin-top:15px;'>" +
                    "<a href='prg3002PRN.aspx' target='_blank'><button style='color:#080'>採購單列印</button></a>" +
                    "<input type = 'button' value = 'Excel下載' style='color:#080;margin-left:5px;' onclick='DownloadFile()' />" +
                    "</div>";

                }
            }
        }
    }

    public static List<Lable_3002> dataList;
    #region 欄位設定
    public class Lable_3002
    {
        public string Company { get; set; } //廠商
        public string Date { get; set; } //進貨日期
        public string Number { get; set; } //進貨單號
        public string Commodity { get; set; } //商品
        public string Tax { get; set; } //含稅單價
        public string TaxCategory { get; set; } //稅別
        public string Amount { get; set; } //數量
        public string Subtotal { get; set; } //小計
        public string State { get; set; } //銷帳狀態

    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_3002>();
        dataList.Add(new Lable_3002()
        {
            Company = "廠商",
            Date = " 進貨日期",
            Number = "進貨單號",
            Commodity = "商品",
            Tax = "含稅單價",
            TaxCategory = "稅別",
            Amount = "數量",
            Subtotal = "小計",
            State = "銷帳狀態"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string com, string data, string num, string comm, string tax, string taxcat, string amoun, string sub, string state)
    {
        dataList.Add(new Lable_3002()
        {
            Company = com,
            Date = data,
            Number = num,
            Commodity = comm,
            Tax = tax,
            TaxCategory = taxcat,
            Amount = amoun,
            Subtotal = sub,
            State = state
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "3002 進貨交易查詢";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Company;
            worksheet.Cell(j, 2).Value = dataList[j - 1].Date;
            worksheet.Cell(j, 3).Value = dataList[j - 1].Number;
            worksheet.Cell(j, 4).Value = dataList[j - 1].Commodity;
            worksheet.Cell(j, 5).Value = dataList[j - 1].Tax;
            worksheet.Cell(j, 6).Value = dataList[j - 1].TaxCategory;
            worksheet.Cell(j, 7).Value = dataList[j - 1].Amount;
            worksheet.Cell(j, 8).Value = dataList[j - 1].Subtotal;
            worksheet.Cell(j, 9).Value = dataList[j - 1].State;
            if (j != 1)
            {
                worksheet.Cell(j, 5).Style.NumberFormat.Format = "$ #,##0.00";
                worksheet.Cell(j, 8).Style.NumberFormat.Format = "$ #,##0";
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
    protected void ExportExcel(object sender, EventArgs e)
    {
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "3002 進貨交易查詢";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Company;
            worksheet.Cell(j, 2).Value = dataList[j - 1].Date;
            worksheet.Cell(j, 3).Value = dataList[j - 1].Number;
            worksheet.Cell(j, 4).Value = dataList[j - 1].Commodity;
            worksheet.Cell(j, 5).Value = dataList[j - 1].Tax;
            worksheet.Cell(j, 6).Value = dataList[j - 1].TaxCategory;
            worksheet.Cell(j, 7).Value = dataList[j - 1].Amount;
            worksheet.Cell(j, 8).Value = dataList[j - 1].Subtotal;
            worksheet.Cell(j, 9).Value = dataList[j - 1].State;
            if (j != 1)
            {
                worksheet.Cell(j, 5).Style.NumberFormat.Format = "$ #,##0.00";
                worksheet.Cell(j, 8).Style.NumberFormat.Format = "$ #,##0";
            }
        }
        worksheet.Columns().AdjustToContents();//自動調整欄位寬度

        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.Buffer = true;
        HttpContext.Current.Response.Charset = "";
        HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + WorksheetsName + ".xlsx");
        HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";//application/vnd.openxmlformats-officedocument.spreadsheetml.sheet

        using (MemoryStream MyMemoryStream = new MemoryStream())
        {
            workbook.SaveAs(MyMemoryStream);
            MyMemoryStream.WriteTo(HttpContext.Current.Response.OutputStream);
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();
        }
    }
}