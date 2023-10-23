using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Web;
using static Global;

public partial class prg7012 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7012";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    ChkDate = WebUtility.UrlDecode(cookies.Read("ChkDate")) ;

                WsString wsString = new WsString();
                ChkDate = ChkDate != "" ? wsString.IsDate(ChkDate) ? DateTime.Parse(ChkDate).ToString("yyyy/MM/dd") : "" : "";


                string SqlComm = $"SELECT * FROM WP_StkChk order by ChkDate desc";
                DataTable CHkTbl = getTbl.table("WP", SqlComm);

                Label2.Text = $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>" +
                $"<input type='hidden' id='startYM' value='{StartYM}'>" +
                "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 報表條件</div>" +
                "<div class='search-main'>" +
                    "<div class='search-sub'>" +
                        "<span class='input-title'>報表區間</span>" +
                            "<select id='ChkDate' class='form-control'>";
                foreach (DataRow row in CHkTbl.Rows)
                {
                    Label2.Text += $"<option value='{row["ChkDate"]}' {(ChkDate == row["ChkDate"].ToString() ? ChkDate : "")}>{DateTime.Parse(row["ChkDate"].ToString()).ToString("yyyy-MM-dd")}</option>";
                }
                Label2.Text += "</select></div>" +
                    "<div class='page-submit'>" +
                        "<span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span>" +
                        "<button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button>" +
                    "</div>" +
                "</div>";
                if (ChkDate != "" )
                {
                    SqlComm = $"SELECT * FROM WP_vProduct WHERE isSale IN ('0', '1', '2') AND isUpdStock='Y' ORDER BY pNo";
                    DataTable pdTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT ChkDate FROM WP_StkChk　" +
                        $"where ChkDate = '{ChkDate}'" +
                        $"order by ChkDate desc";
                    DataTable daTbl = getTbl.table("WP", SqlComm);                  
                    
                    if (daTbl.Rows.Count == 0)
                    {
                        Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                    }
                    else
                    {
                        TableName();
                        var Date = DateTime.Parse(daTbl.Rows[0]["ChkDate"].ToString()).ToString("yyyy-MM-dd");
                        var strDate = DateTime.Parse(daTbl.Rows[0]["ChkDate"].ToString()).ToString("yyyyMMdd");

                        int pageNo = 1;
                        Label2.Text += "<div class='rptr-main'>" +
                        "<div class='rptr-title'>門市盤點明細表</div>" +
                        "<div class='rptr-data'>" +
                            $"<div><div style='float:left;'>日期︰{Date}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                        "</div>" +
                        "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                                "<td style='text-align:left'>盤點日期</td><td style='text-align:left'>商品條碼</td><td style='text-align:left'>商品名稱</td><td>帳面數量</td><td>盤點數量</td><td>盤差數量</td><td>商品成本</td><td>盤差金額</td>" +
                            "</tr>";

                        ThreeDot threeDot = new ThreeDot();
                        int recNo = 0;
                        string isShow = "Y";
                        double QtyCost , difCost, difTotal = 0;
                        int txtQty, tblQty , difQty, difQty_total = 0;

                        //string chks = DateTime.Parse(sDate).ToString("yyyyMMdd") + "0001";//DateTime.Now.ToString("yyyyMMdd0001");
                        //string chke = DateTime.Parse(eDate).ToString("yyyyMMdd") + "0001";//DateTime.Now.ToString("yyyyMMdd0001");

                        string chk = strDate + "0001";

                        //int txt_allQty = 0, tbl_allQty = 0;
                        //double txt_allQtyCost = 0, tbl_allQtyCost = 0;

                        foreach (DataRow row in pdTbl.Rows)
                        {
                            SqlComm = $"SELECT * FROM WP_StkChkDtl where ChkId ='{chk}' and pNo ='{row["pNo"]}' and isDel='N' order by sn desc";
                            DataTable stkTbl = getTbl.table("WP", SqlComm);
                            if (((recNo) % 40) == 1 && recNo > 1 && isShow == "Y")
                            {
                                pageNo++;

                                Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                "<div class='rptr-title'>門市盤點明細表</div>" +
                                "<div class='rptr-data'>" +
                                    $"<div><div style='float:left;'>日期區間︰{Date} </div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                    $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                "</div>" +
                                "<table class='list-main' style='border-spacing:1px;'>" +
                                    "<tr class='list-title'>" +
                                        "<td style='text-align:left'>盤點日期</td><td style='text-align:left'>商品條碼</td><td style='text-align:left'>商品名稱</td><td>帳面數量</td><td>盤點數量</td><td>盤差數量</td><td>商品平均成本</td><td>盤差金額</td>" +
                                    "</tr>";
                            }

                            txtQty = stkTbl.Rows.Count == 0 ? 0 : int.Parse($"{stkTbl.Rows[0]["RealQty"]}");
                            tblQty = stkTbl.Rows.Count == 0 ? 0 : int.Parse($"{stkTbl.Rows[0]["DataQty"]}");

                            QtyCost = double.Parse($"{row["costAvg"]:0.00}");

                            difQty = tblQty - txtQty;
                            difCost = difQty * QtyCost;
                            if (txtQty != tblQty)
                            {
                                Label2.Text += "<tr class='tr-row'>" +
                                    $"<td style='text-align:left'>{Date}</td>" +
                                    $"<td style='text-align:left'>{row["pBarcode"]}</td>" +
                                    $"<td style='text-align:left'>{row["pNameS"]}</td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(tblQty.ToString())}</td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(txtQty.ToString())}</td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(difQty.ToString())}</td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(QtyCost.ToString())}</td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(difCost.ToString())}</td>" +
                                "</tr>";
                                isShow = "Y";
                                recNo++;
                            }
                            else
                            {
                                isShow = "N";
                            }
                            Input(Date , "'" + row["pBarcode"].ToString(), row["pNameS"].ToString(), tblQty.ToString(), txtQty.ToString(),
                                difQty.ToString(), QtyCost.ToString(), difCost.ToString());

                            difQty_total += difQty;
                            difTotal += difCost;
                            //txt_allQty += txtQty;
                            //tbl_allQty += tblQty;
                            //txt_allQtyCost += txtQty * QtyCost;
                            //tbl_allQtyCost += tblQty * QtyCost;
                        }
                        //var difference_qty = tbl_allQty - txt_allQty;
                        //var difference_cost = tbl_allQtyCost - txt_allQtyCost;

                        Label2.Text += "<tr class='tr-row'>" +
                                    $"<td style='text-align:left'>合計</td>" +
                                    $"<td style='text-align:left'></td>" +
                                    $"<td style='text-align:left'></td>" +
                                    $"<td style='text-align:right'></td>" +
                                    $"<td style='text-align:right'></td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(difQty_total.ToString())}</td>" +
                                    $"<td style='text-align:right'></td>" +
                                    $"<td style='text-align:right'>{threeDot.To3Dot(difTotal.ToString())}</td>" +
                                "</tr></table>"+
                                "<div class='stamp' style='margin-top:50px;'><span>主辦︰　　　主任︰　　　會計︰　　　秘書︰　　　</div>" +
                            $"</div>" +
                            $"<input type='hidden' id='page-total' value='{(pageNo)}' />" +
                        "</div></div>";

                        //Label2.Text += $"</table>" +
                        //    "<hr style='border-color:#0808FF;border-width:5px;'>" +
                        //    $"<div style='text-align:left;margin-top:30px;'>" +
                        //    $"電腦庫存合計 : {threeDot.To3Dot(tbl_allQty.ToString())}&nbsp;&nbsp; &nbsp; &nbsp; &nbsp; " +
                        //    $"盤點庫存合計 : {threeDot.To3Dot(txt_allQty.ToString())}&nbsp;&nbsp; &nbsp; &nbsp; &nbsp; " +
                        //    $"總庫存差額 : {threeDot.To3Dot(difference_qty.ToString())}&nbsp;&nbsp; &nbsp; &nbsp; &nbsp; " +
                        //    $"</div>" +
                        //    $"<div style='text-align:left;margin-top:30px;'>" +
                        //    $"整間店的總電腦庫存金額 : {threeDot.To3Dot(tbl_allQtyCost.ToString())}元 &nbsp;&nbsp; &nbsp;" +
                        //    $"當日盤點的總庫存金額 : {threeDot.To3Dot(txt_allQtyCost.ToString())}元 &nbsp;&nbsp; &nbsp;" +
                        //    $"總盤點庫存成本減掉總電腦庫存成本 : {threeDot.To3Dot(difference_cost.ToString("0.00"))}元 &nbsp;&nbsp; &nbsp;" +
                        //    $"</div>" +
                        //    "<hr style='border-color:#0808FF;border-width:5px;'>" +
                        //    "<div class='stamp' style='margin-top:50px;'><span>總幹事︰　　　秘書︰　　　會計主任︰　　　供銷主任︰　　　經辦︰　　　會計︰　　　倉管︰</div>" +
                        //    $"</div>" +
                        //    $"<input type='hidden' id='page-total' value='{(pageNo)}' />" +
                        //"</div></div>";
                    }
                    Label2.Text += "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印統計表</button>" +
                    "<input type = 'button' value = 'Excel下載' style='color:#080' onclick='DownloadFile()' />" +
                    "</div>";
                }
            }
        }
    }
    public static List<Lable_7012> dataList;
    #region 欄位設定
    public class Lable_7012
    {
        public string Date { get; set; } //盤點日期
        public string pdId { get; set; } //商品條碼
        public string PdName { get; set; } //商品名稱
        public string PCQty { get; set; } //帳面數量
        public string TxtQty { get; set; } //盤點數量
        public string DifQty { get; set; } //盤差數量
        public string Cost { get; set; } //商品平均成本
        public string difCost { get; set; } //盤差金額
    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_7012>();
        dataList.Add(new Lable_7012()
        {
            Date = "盤點日期",
            pdId = "商品條碼",
            PdName = "商品名稱",
            PCQty = "帳面數量",
            TxtQty = "盤點數量",
            DifQty = "盤差數量",
            Cost = "商品平均成本",
            difCost = "盤差金額"
        });
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string num, string name, string lq, string pq, string pc, string sq, string tq, string ta)
    {
        dataList.Add(new Lable_7012()
        {
            Date = num,
            pdId = name,
            PdName = lq,
            PCQty = pq,
            TxtQty = pc,
            DifQty = sq,
            Cost = tq,
            difCost = ta
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "7012門市盤點明細表";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].Date;
            worksheet.Cell(j, 2).Value = dataList[j - 1].pdId;
            worksheet.Cell(j, 3).Value = dataList[j - 1].PdName;
            worksheet.Cell(j, 4).Value = dataList[j - 1].PCQty;
            worksheet.Cell(j, 5).Value = dataList[j - 1].TxtQty;
            worksheet.Cell(j, 6).Value = dataList[j - 1].DifQty;
            worksheet.Cell(j, 7).Value = dataList[j - 1].Cost;
            worksheet.Cell(j, 8).Value = dataList[j - 1].difCost;
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