using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.WebControls;
using static Global;

public partial class prg8003 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "8003";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                string SqlComm, sYMD, uYMD,
                    sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate"));

                SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE isDel='N' ORDER BY YearMonth DESC";      //已結帳日期
                DataTable chkoutYMTbl = getTbl.table("WP", SqlComm);

                if (chkoutYMTbl.Rows.Count == 0)
                    sYMD = StartYM;
                else
                {
                    uYMD = $"{chkoutYMTbl.Rows[0]["YearMonth"]}";
                    sYMD = DateTime.Parse($"{uYMD.Substring(0, 4)}-{uYMD.Substring(4, 2)}-{uYMD.Substring(6, 2)}").AddDays(1).ToString("yyyyMMdd");
                }

                Label2.Text = $"<input type='hidden' id='sYMD' value='{sYMD}'>" +
                $"<input type='hidden' id='StartYM' value='{StartYM}' />" +
                $"<input type='hidden' id='pvType' value='{pvType}' />" +
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
                        "</div>" +
                    "</div>" +
                    "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>" +
                "<button class='btn-submit' id='btn-add'><i class='fas fa-check-circle'></i> 新增調整商品</button>" +
                "<div id='add-main' style='display:none;'>" +
                    "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 新增調整商品</div>" +
                    "<div class='edit-zone'>" +
                        "<span class='input-title'>日期</span>" +
                        "<div class='del-txt-group' style='margin-right:20px;'>" +
                            "<input type='text' class='form-control del-txt-input' readonly id='stkDate' name='stkDate' data-alt='MI_日期' style='width:105px;' />" +
                            "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                        "</div>" +
                        "<span class='input-title'>商品</span>" +
                        "<div class='del-txt-group' style='margin-right:20px;'>" +
                            "<input type='text' class='form-control del-txt-input' placeholder='請輸入商品名稱或條碼' data-alt='MI_商品' maxlength='42' style='width: 266px;' id='pd-filter' />" +
                            "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "<input type='hidden' id='act-pNo' name='pNo' />" +
                        "</div>" +
                        "<span class='input-title'>庫存數</span>" +
                        "<input type='text' name='qty' maxlength='5' class='form-control pd-stk align-r chk-input' data-func='number' data-alt='MI_庫存' style='width:70px;margin-right:20px;' />" +
                        "<span class='input-title'>成本</span>" +
                        "<input type='text' name='cost' maxlength='10' class='form-control pd-cost align-r chk-input' data-func='real_number' data-alt='MI_成本' style='width:103px;margin-right:20px;' />" +
                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                        "<button class='btn-submit edit-btn' data-id='submit' id='btn-add-submit'><i class='fas fa-check-circle'></i>確定</button>" +
                    "</div>" +
                "</div>";

                if (sDate != "" && eDate != "")
                {
                    SqlComm = $"SELECT * FROM WP_vStkUpd WHERE isDel='N' AND (stkDate BETWEEN '{sDate}' AND '{eDate}') " +
                       $"ORDER BY stkDate DESC, timeUpdate, pNo";
                    DataTable updTbl = getTbl.table("WP", SqlComm);

                    //調整前數量
                    SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND (InStkDate BETWEEN '{sDate}' AND '{eDate}') and pvSn = '-1' " +
                        $"ORDER BY InStkId, pNo";
                    DataTable InTbl = getTbl.table("WP", SqlComm);
                    SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND (OutStkDate BETWEEN '{sDate}' AND '{eDate}') and memSn = '-1' " +
                        $"ORDER BY OutStkId, pNo";
                    DataTable outTbl = getTbl.table("WP", SqlComm);


                    Label2.Text += "<div id='rptr-main'>" +
                        "<div class='rptr-title'>貨品倉庫庫存報表</div>" +
                        "<div class='rptr-data'>" +
                            $"<div><div style='float:left;'>日期區間︰{sDate} 至 {eDate}</div></div>" +
                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                        "</div>" +

                        "<table class='list-main inStk-list-main' style='border-spacing:1px;margin-top:20px;'>" +
                        "<tr class='list-title'><td>調整日期</td><td>商品條碼</td><td>商品簡稱</td><td style='text-align:right'>調整前數量</td><td style='text-align:right'>調整後數量</td><td style='text-align:right'>調整後成本</td></tr>";

                    if (updTbl.Rows.Count == 0)
                        Label2.Text += "<tr><td colspan='5' class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無庫存調整資料！</td></tr>";
                    else
                    {
                        TableName();
                        DataRow[] istkRows, ostkRows;
                        int INqty, OUTqty, qty;
                        foreach (DataRow row in updTbl.Rows)
                        {
                            istkRows = InTbl.Select($"pNo='{row["pNo"]}'");
                            ostkRows = outTbl.Select($"pNo='{row["pNo"]}'");
                            INqty = 0;
                            if (istkRows.Length > 0)
                            {
                                for (int i = 0; i < istkRows.Length; i++)
                                {
                                    INqty += int.Parse(istkRows[i]["qty"].ToString());
                                }
                            }
                            OUTqty = 0;
                            if (ostkRows.Length > 0)
                            {
                                for (int i = 0; i < ostkRows.Length; i++)
                                {
                                    OUTqty += int.Parse(ostkRows[i]["qty"].ToString());
                                }
                            }
                            qty = int.Parse(row["qty"].ToString()) - INqty + OUTqty;

                            Label2.Text += "<tr>" +
                                    $"<td>{row["stkDate"]:yyyy/MM/dd}</td>" +
                                    $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}</td>" +
                                    $"<td>{row["pNameS"]}</td>" +
                                    $"<td style='text-align:right;'>{qty}</td>" +
                                    $"<td style='text-align:right;'>{row["qty"]}</td>" +
                                    $"<td style='text-align:right;'>{row["cost"]}</td>" +
                                "</tr>";
                            Input($"{row["stkDate"]:yyyy/MM/dd}","'" + $"{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}",
                                $"{row["pNameS"]}", $"{qty}",$"{row["qty"]}",$"{row["cost"]}");
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
    public static List<Lable_8003> dataList;
    #region 欄位設定
    public class Lable_8003
    {
        public string Date { get; set; } //調整日期	
        public string Bcode { get; set; } //商品條碼	
        public string Name { get; set; } //商品簡稱	
        public string LastQty { get; set; } //調整前數量	
        public string NowQry { get; set; } //調整後數量	
        public string Cost { get; set; } //調整後成本
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_8003>();
        dataList.Add(new Lable_8003()
        {
            Date = "調整日期",
            Bcode = "商品條碼",
            Name = "商品簡稱",
            LastQty = "調整前數量",
            NowQry = "調整後數量",
            Cost = "調整後成本"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string d, string b, string na, string lq, string nq, string c)
    {
        dataList.Add(new Lable_8003()
        {
            Date = d,
            Bcode = b,
            Name = na,
            LastQty = lq,
            NowQry = nq,
            Cost = c,
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = " 8003庫存調整";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Date;
            worksheet.Cell(j, 2).Value = dataList[j - 1].Bcode;
            worksheet.Cell(j, 3).Value = dataList[j - 1].Name;
            worksheet.Cell(j, 4).Value = dataList[j - 1].LastQty;
            worksheet.Cell(j, 5).Value = dataList[j - 1].NowQry;
            worksheet.Cell(j, 6).Value = dataList[j - 1].Cost;
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