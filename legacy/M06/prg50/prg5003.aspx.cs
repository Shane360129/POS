using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using static Global;

public partial class prg5003 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "5003";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                TableName();
                Stock stock = new Stock();
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>";

                string SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND outType<>'2' ORDER BY memId, OutStkId, sn, dtlSn";
                DataTable outStkTbl = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT * FROM WP_Member WHERE isStop='N' AND (sn IN (80, 81)) ORDER BY sn";
                DataTable memTbl = getTbl.table("WP", SqlComm);
                Label2.Text = "<div style='border:2px #000 solid;border-radius:3px;padding:20px 14px;'>" +
                    "<div>" +
                        "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 銷帳日期︰</span>" +
                        $"<input type='text' class='form-control open-datepicker' id='acctInDate' name='acctInDate' size='10' readonly  style='background-color:#fff;margin-right:65px;' value='{DateTime.Today.ToString("yyyy/MM/dd")}' />" +
                    "</div>" +
                    "<div class='acctIn-main'>" +
                        "<input type='radio' class='rd-acctIn' id='acctInA' name='radioAcctIn' value='A' checked /><label for='acctInA'>依訂單</label>" +
                        "<input type='radio' class='rd-acctIn' id='acctInB' name='radioAcctIn' value='B' /><label for='acctInB'>依金額</label>" +
                        "<div class='kind-B mem-select'>" +
                            "<select id='mem'>";
                foreach (DataRow row in memTbl.Rows)
                {
                    Label2.Text += $"<option value='{row["sn"]}'>{row["memName"]}</option>";
                }
                Label2.Text += "</select>" +
                "<span style='margin-left:10px;'>銷帳金額︰</span><input type='text' size='6' class='chk-input' data-func='number' id='mem-amt' />" +
            "</div>" +
        "</div>" +
    "</div>" +

    "<div class='list-main' id='outStk-list-main' style='margin-top:20px;'>" +
        "<div class='list-title' style='display:flex;'>" +
            "<div style='display:flex;align-items:center;'><input type='checkbox' class='ckbox kind-A' id='all-ckbox'>會員</div><div>銷貨單號</div><div>銷貨日期</div><div style='text-align:center'>銷帳狀態</div><div style='text-align:right'>全折</div><div style='text-align:right'>未結金額</div><div>商品</div><div style='text-align:right'>含稅單價</div><div style='text-align:center'>稅別</div><div style='text-align:right'>數量</div><div style='text-align:right'>單折</div><div style='text-align:right'>小計</div>" +
        "</div>";
                string empty = "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無應收帳款銷帳交易！</div>";
                if (outStkTbl.Rows.Count == 0)
                {
                    Label2.Text += empty;
                }
                else
                {
                    DataTable dt;
                    DataRow[] dtRows = outStkTbl.Select("memSn NOT IN (80, 81)", "MemSn ASC");
                    dt = dtRows.Length == 0 ? null : dtRows.GroupBy(r => r.Field<int>("memSn")).Select(g => g.OrderBy(r => r["memSn"]).First()).CopyToDataTable();
                    Label2.Text += $"<div class='kind-main kind-A'>{(dtRows.Length == 0 ? empty : createHTML(dt, outStkTbl, "A"))}</div>";

                    dtRows = outStkTbl.Select("memSn IN (80, 81)", "MemSn ASC");
                    dt = dtRows.Length == 0 ? null : dtRows.GroupBy(r => r.Field<int>("memSn")).Select(g => g.OrderBy(r => r["memSn"]).First()).CopyToDataTable();
                    Label2.Text += $"<div class='kind-main kind-B'>{(dtRows.Length == 0 ? empty : createHTML(dt, outStkTbl, "B"))}</div>";
                }
                Label2.Text += "</div>" +
                "<div style='padding:5px;border-top:2px #444 solid;margin-top:20px;display:flex;'>" +
                    "<div style='width:53.7%;text-align:right;padding:5px;' class='amt-show'>總計︰<span id='outStk-total'></span><span class='amt-show' id='outStk-chk-total'></span></div>" +
                    "<div style='width:45%;text-align:right;margin-top:15px;'>" +
                    "<a href='prg5003PRN.aspx' id='btn-print' target='_blank'>" +
                    "<button style='color:#080'>銷貨單列印</button></a>" +
                    "<input type = 'button' value = 'Excel下載' style='color:#080' onclick='DownloadFile()' /></div>" +
                "</div>";
            }
        }
    }

    private string createHTML(DataTable _dt, DataTable _outStkTbl, string _kind)
    {
        Stock stock = new Stock();
        string HTML = "";
        foreach (DataRow row in _dt.Rows)
        {
            HTML += $"<div class='node-main node-mem mem-{row["memSn"]}'>" +
                $"<div>{(_kind == "A" ? "<input type='checkbox' class='ckbox node-ckbox' data-name='node-checkbox' />" : "")} {row["memName"]} <span class='kind-A'>未結︰<span class='amt-show amt-total'></span><span class='amt-show amt-chk-total'></span></div>" +
                $"<div class='mem-dtl'>";
            string preId = "";
            int discnt = 0, dtlDiscnt;
            foreach (DataRow rowMem in _outStkTbl.Select($"memId='{row["memId"]}'", "outStkId ASC, pNo ASC"))
            {
                if (preId != rowMem["sn"].ToString())
                {
                    discnt = (-1) * (int.Parse($"{rowMem["discount"]:#0}") + int.Parse($"{rowMem["discountShare"]:#0}"));
                    HTML += $"{(preId == "" ? "" : "</div></div>")}<div class='mem-dtl-row'>" +
                        $"<div>{(_kind == "A" ? $"<input type='checkbox' class='ckbox prg-ckbox' name='node-checkbox' data-id='{rowMem["sn"]}'>" : "")} {rowMem["OutStkId"]}</div>" +
                        $"<div>{rowMem["OutStkDate"]:yyyy/MM/dd}</div>" +
                        $"<div style='text-align:center;'>{stock.OutTypeName($"{rowMem["OutType"]}")}</div>" +
                        $"<div style='text-align:right;' class='discnt' data-val='{discnt}'>{discnt}</div>" +
                        $"<div style='text-align:right;' class='outLeft-amt' data-val='{rowMem["outLeft"]:#0}'>{rowMem["outLeft"]:#0}</div>" +
                        "<div class='id-dtl'>";
                    preId = rowMem["sn"].ToString();
                }

                dtlDiscnt = (-1) * (int.Parse($"{rowMem["dtlDiscnt"]:#0}") + int.Parse($"{rowMem["dtlDiscntShare"]:#0}"));
                HTML += "<div class='id-dtl-row'>" +
                    $"<div>{(rowMem["pBarcode"].ToString() == "" ? rowMem["pCode"].ToString() : rowMem["pBarcode"].ToString())}-{rowMem["pNameS"]}</div>" +
                    $"<div style='text-align:right'>{rowMem["dtlAmt"]:#0}</div>" +
                    $"<div style='text-align:center'>{(rowMem["dtlIsTax"].ToString() == "Y" ? "應稅" : "免稅")}</div>" +
                    $"<div style='text-align:right'>{rowMem["qty"]}</div>" +
                    $"<div style='text-align:right' class='dtlDiscnt' data-val='{dtlDiscnt}'>{dtlDiscnt}</div>" +
                    $"<div style='text-align:right' class='dtlAmtTotal'>{int.Parse($"{rowMem["amtTotal"]:#0}") + dtlDiscnt}</div>" +
                "</div>";


                if (_kind == "A")
                {
                    var ta = int.Parse($"{rowMem["amtTotal"]:#0}") + dtlDiscnt;
                    Input(row["memName"].ToString(), "'"+rowMem["OutStkId"].ToString(), $"{rowMem["OutStkDate"]:yyyy/MM/dd}", rowMem["OutType"].ToString(), discnt.ToString(), $"{rowMem["outLeft"]:#0}",
                    rowMem["pBarcode"].ToString() + "-" + rowMem["pNameS"], $"{rowMem["dtlAmt"]:#0}", (rowMem["dtlIsTax"].ToString() == "Y" ? "應稅" : "免稅"), rowMem["qty"].ToString(),
                    dtlDiscnt.ToString(), ta.ToString());
                }
            }
            HTML += $"{(preId == "" ? "" : "</div></div>")}" +
        "</div>" +
    "</div>";
        }
        return HTML;
    }

    public static List<Lable_5003> dataList;
    #region 欄位設定
    public class Lable_5003
    {
        public string Number { get; set; } //會員
        public string Id { get; set; } //銷貨單號
        public string Day { get; set; } //銷貨日期
        public string State { get; set; } //銷貨狀態
        public string FullDiscount { get; set; } //全折
        public string Commodity { get; set; } //商品
        public string Nopaymon { get; set; } //商品
        public string Tax { get; set; } //含稅單價
        public string TaxCategory { get; set; } //稅別
        public string Amount { get; set; } //數量
        public string SingleDiscount { get; set; } //單折
        public string Subtotal { get; set; } //小計
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_5003>();
        dataList.Add(new Lable_5003()
        {
            Number = "會員",
            Id = "銷貨單號",
            Day = "銷貨日期",
            State = "銷帳狀態",
            FullDiscount = "全折",
            Commodity = "商品",
            Nopaymon = "未結金額",
            Tax = "含稅單價",
            TaxCategory = "稅別",
            Amount = "數量",
            SingleDiscount = "單折",
            Subtotal = "小計",
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string num, string data, string mem, string state, string fd, string comm, string no, string tax, string taxc, string amo, string sd, string sub)
    {
        dataList.Add(new Lable_5003()
        {
            Number = num,
            Id = data,
            Day = mem,
            State = state,
            FullDiscount = fd,
            Commodity = comm,
            Nopaymon = no,
            Tax = tax,
            TaxCategory = taxc,
            Amount = amo,
            SingleDiscount = sd,
            Subtotal = sub,
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "5003應收帳款銷帳";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Number;
            worksheet.Cell(j, 2).Value = dataList[j - 1].Id;
            worksheet.Cell(j, 3).Value = dataList[j - 1].Day;
            worksheet.Cell(j, 4).Value = dataList[j - 1].State;
            worksheet.Cell(j, 5).Value = dataList[j - 1].FullDiscount;
            worksheet.Cell(j, 6).Value = dataList[j - 1].Commodity;
            worksheet.Cell(j, 7).Value = dataList[j - 1].Nopaymon;
            worksheet.Cell(j, 8).Value = dataList[j - 1].Tax;
            worksheet.Cell(j, 9).Value = dataList[j - 1].TaxCategory;
            worksheet.Cell(j, 10).Value = dataList[j - 1].Amount;
            worksheet.Cell(j, 11).Value = dataList[j - 1].SingleDiscount;
            worksheet.Cell(j, 12).Value = dataList[j - 1].Subtotal;
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