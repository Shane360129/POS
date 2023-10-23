using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Web;
using static Global;
using DataTable = System.Data.DataTable;

public partial class prg7010 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7010";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string
                    //sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate"));

                //sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                //sDate = int.Parse(DateTime.Parse(sDate).ToString("yyyyMMdd")) < int.Parse(StartYM) ? (StartYM.Substring(0, 4) + "/" + StartYM.Substring(4, 2) + "/" + StartYM.Substring(6, 2)) : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                if (eDate != "")
                {

                    var StandardDay = DateTime.Parse("2023/08/31");
                    var day = DateTime.Parse(eDate);
                    int result = DateTime.Compare(day, StandardDay);
                    string SqlComm;
                    if (result <= 0)
                        SqlComm = $"select pNo,pName,pBarcode,pUName,costStd,qtyInitial,costInitial,costAvg from WP_vProduct order by pNo";
                    else
                        SqlComm = $"select pNo,pName,pBarcode,pUName,costStd,qtyInitial,costInitial,costAvg from WP_vProduct where isSale <>3 and isUpdStock ='Y' order by pNo";

                    //string SqlComm = $"select pNo,pName,pBarcode,pUName,costStd,qtyInitial,costInitial,costAvg from WP_vProduct order by pNo";
                    DataTable proTbl = getTbl.table("WP", SqlComm);

                    //2023-08-31後的期初
                    SqlComm = $"select * from WP_pdStopLoss where isDel ='N' and Date < '{eDate}' order by pNo";
                    DataTable proInTbl = getTbl.table("WP", SqlComm);

                    SqlComm = "select* from WP_prophase order by pNo";
                    DataTable prophTbl = getTbl.table("WP", SqlComm);

                    int pageNo = 1;
                    Label2.Text = $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>" +
                    $"<input type='hidden' id='startYM' value='{StartYM}'>" +
                    "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 報表條件</div>" +
                    "<div class='search-main'>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>查詢區間</span>" +
                            $"<input type='text' class='form-control del-txt-input open-datepicker' readonly id='eDate' value='{eDate}' />" +
                        "</div>" +
                        "<div class='page-submit'><span id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</span><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                    "</div>" +
                    "<div class='rptr-main'>" +
                        "<div class='rptr-title'>貨品倉庫庫存報表</div>" +
                        "<div class='rptr-data'>" +
                            $"<div><div style='float:left;'>截止日期︰ {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                            $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                        "</div>" +
                        "<table class='list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'>" +
                            "<td class='align-l'>貨品編號</td><td class='align-l'>貨品名稱</td><td class='align-l'>單位</td><td>倉庫名稱</td><td>倉庫數量</td><td>倉庫小計</td><td>庫存總量</td><td>庫存總值</td>";
                    Label2.Text += result <= 0 ? "<td>標準成本</td>" : "<td>平均成本</td>";
                    Label2.Text += "</tr>";
                    if (proTbl.Rows.Count == 0)
                    {
                        Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                    }
                    else
                    {
                        TableName();

                        ThreeDot threeDot = new ThreeDot();
                        ToDouble twoDot = new ToDouble();
                        int recNo = 0;
                        DataRow[] prophRows, proInitialRows;
                        int qtyInitial;
                        string costStd, costOld;
                        foreach (DataRow row in proTbl.Rows)
                        {
                            SqlComm = $"select pNo,pName,SUM(qty) as qty from WP_vOutStock " +
                                //$"where OutStkDate <= '{eDate} 23:29:59' "+
                                $"{(result <= 0  ? $"where OutStkDate <= '{eDate} 23:29:59' " : $"where OutStkDate between '2023/09/01' and '{eDate} 23:29:59' ")} " +
                                $"and pNo = '{row["pNo"]}' " +
                                $"and isDel='N' AND dtlIsDel='N' " +
                                $"group by pNo,pName order by pNo";
                            DataTable outTbl = getTbl.table("WP", SqlComm);
                            SqlComm = $"select pNo,pName,SUM(qty) as qty from WP_vInStock " +
                                //$"where InStkDate <= '{eDate} 23:29:59'" +
                                $"{(result <= 0 ? $"where InStkDate <= '{eDate} 23:29:59' " : $"where InStkDate between '2023/09/01' and '{eDate} 23:29:59' ")} " +
                                $"and pNo = '{row["pNo"]}' " +
                                $"and isDel='N' AND dtlIsDel='N' " +
                                $"group by pNo,pName order by pNo";
                            DataTable inTbl = getTbl.table("WP", SqlComm);

                            SqlComm = $"select pNo,pName,SUM(qty) as qty from WP_vOutStockCb " +
                                //$"where OutStkDate <= '{eDate} 23:29:59' " +
                                $"{(result <= 0 ? $"where OutStkDate <= '{eDate} 23:29:59' " : $"where OutStkDate between '2023/09/01' and '{eDate} 23:29:59' ")} " +
                                $"and pNo = '{row["pNo"]}' " +
                                $"and isDel='N' AND dtlIsDel='N' " +
                                $"group by pNo,pName order by pNo";
                            DataTable outcobTbl = getTbl.table("WP", SqlComm);

                            recNo++;
                            if (((recNo) % 45) == 1 && recNo > 1)
                            {
                                pageNo++;

                                Label2.Text += "</table><p style='page-break-after:always'></p>" +
                                "<div class='rptr-title'>貨品倉庫庫存報表</div>" +
                                "<div class='rptr-data'>" +
                                    $"<div><div style='float:left;'>截止日期︰ {eDate}</div><div style='float:right;'>頁次︰{pageNo} / <span class='page-total'></span></div></div>" +
                                    $"<div style='clear:both;text-align:right;width:100%;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                "</div>" +
                                "<table class='list-main' style='border-spacing:1px;'>" +
                                    "<tr class='list-title'>" + 
                                    "<td class='align-l'>貨品編號</td><td class='align-l'>貨品名稱</td><td class='align-l'>單位</td><td>倉庫名稱</td><td>倉庫數量</td><td>倉庫小計</td><td>庫存總量</td><td>庫存總值</td>";
                                
                                Label2.Text += result <= 0 ? "<td>標準成本</td>" : "<td>平均成本</td>";
                                Label2.Text += "</tr>";
                            }
                            

                            int outqty = outTbl.Rows.Count == 0 ? 0 : int.Parse(outTbl.Rows[0]["qty"].ToString());
                            int inqty = inTbl.Rows.Count == 0 ? 0 : int.Parse(inTbl.Rows[0]["qty"].ToString());
                            int outcobqty = outcobTbl.Rows.Count == 0 ? 0 : int.Parse(outcobTbl.Rows[0]["qty"].ToString());
                            outcobqty = result <= 0 ? 0 : outcobqty;

                            //string costStd = $"{row["costStd"]:0.00}" ;


                            if (result <= 0)
                            {
                                prophRows = prophTbl.Select($"pNo='{row["pNo"]}'");
                                costStd = prophRows.Length == 0 ? "0.00" : $"{prophRows[0]["costStd"]:0.00}";
                                qtyInitial = int.Parse(row["qtyInitial"].ToString());
                                costOld = costStd;

                            }
                            else
                            {
                                proInitialRows = proInTbl.Select($"pNo='{row["pNo"]}'");
                                //costStd = $"{row["costAvg"]:0.0000}";
                                if (proInitialRows.Length == 0)
                                {
                                    costStd = $"{row["costAvg"]:0.0000}";
                                    qtyInitial = int.Parse(row["qtyInitial"].ToString());
                                    costOld = $"{row["costAvg"]:0.0000}";
                                }
                                else
                                {
                                    costStd = $"{row["costAvg"]:0.0000}";//$"{proInitialRows[0]["costAvg"]:0.0000}";
                                    qtyInitial = int.Parse(proInitialRows[0]["qty"].ToString());
                                    costOld = $"{proInitialRows[0]["costAvg"]:0.0000}";

                                }


                            }

                            string qty = (qtyInitial + inqty - outqty - outcobqty).ToString();
                            string qty_ = (inqty - outqty - outcobqty).ToString();

                            string cost;
                            //cost = qty != null ? ((int.Parse(qty)) * (double.Parse(costStd))).ToString("0.0000") : "0";
                            cost = qty != null ? ((qtyInitial * (double.Parse(costOld))) + ((int.Parse(qty_)) * (double.Parse(costStd)))).ToString("0.0000") : "0";


                            Label2.Text += $"<tr class='amt-row'>" +
                                $"<td class='align-l'>{row["pBarcode"]}</td>" +
                                $"<td class='align-l'>{row["pName"]}</td>" +
                                $"<td class='align-l'>{row["pUName"]}</td>" +
                                $"<td>總倉</td>" +
                                $"<td class='qty_p' data-val='{qty}'>{threeDot.To3Dot(qty)}</td>" +
                                $"<td class='amt_p' data-val='{cost}'>{threeDot.To3Dot(cost)}</td>" +
                                $"<td class='qty_m' data-val='{qty}'>{threeDot.To3Dot(qty)}</td>" +
                                $"<td class='amt_m' data-val='{cost}'>{threeDot.To3Dot(cost)}</td>" +
                                $"<td>{threeDot.To3Dot(costStd)}</td>" +
                            "</tr>";

                            Input("'" + row["pBarcode"].ToString(), row["pName"].ToString(), row["pUName"].ToString(), "總倉", qty.ToString(),
                                cost.ToString(), qty.ToString(), cost.ToString(), costStd.ToString(), row["pNo"].ToString());
                        }
                        Label2.Text += $"<tr class='rptr-total'>" +
                            $"<td colspan='4'>合計︰</td>" +
                            $"<td class='qty_p_total'></td>" +
                            $"<td class='amt_p_total'></td>" +
                            $"<td class='qty_m_total'></td>" +
                            $"<td class='amt_m_total'></td>" +
                            $"<td></td>" +
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
            }
        }
    }
    public static List<Lable_7010> dataList;
    #region 欄位設定
    public class Lable_7010
    {
        public string pdId { get; set; } //貨品編號
        public string PdName { get; set; } //貨品名稱
        public string PUName { get; set; } //單位
        public string PName { get; set; } //倉庫名稱
        public string Qty { get; set; } //倉庫數量
        public string Amount { get; set; } //倉庫小計
        public string ToQty { get; set; } //庫存總量
        public string ToAmount { get; set; } //庫存總值
        public string Cost { get; set; } //最後成本
        public string Pno { get; set; } //最後成本

    }
    #endregion
    #region 欄位表頭設定
    public void TableName()
    {
        dataList = new List<Lable_7010>();
        dataList.Add(new Lable_7010()
        {
            pdId = "貨品編號",
            PdName = "貨品名稱",
            PUName = "單位",
            PName = "倉庫名稱",
            Qty = "倉庫數量",
            Amount = "倉庫小計",
            ToQty = "庫存總量",
            ToAmount = "庫存總值",
            Cost = "成本",
            Pno = "Pno"
        }); ;
    }
    #endregion
    #region 欄位表身寫入
    public void Input(string num, string name, string lq, string pq, string pc, string sq, string tq, string ta, string sa,string pno)
    {
        dataList.Add(new Lable_7010()
        {
            pdId = num,
            PdName = name,
            PUName = lq,
            PName = pq,
            Qty = pc,
            Amount = sq,
            ToQty = tq,
            ToAmount = ta,
            Cost = sa,
            Pno = pno
        });
    }
    #endregion
    #region 存為Excel檔
    [System.Web.Services.WebMethod]
    public static string SaveExcel()
    {
        // 建立活頁簿
        IXLWorkbook workbook = new XLWorkbook();

        var WorksheetsName = "7010貨品倉庫庫存報表";
        var datatime = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        // 建立工作表
        IXLWorksheet worksheet = workbook.Worksheets.Add(WorksheetsName);

        for (int j = 1; j <= dataList.Count; j++)
        {
            worksheet.Cell(j, 1).Value = dataList[j - 1].pdId;
            worksheet.Cell(j, 2).Value = dataList[j - 1].PdName;
            worksheet.Cell(j, 3).Value = dataList[j - 1].PUName;
            worksheet.Cell(j, 4).Value = dataList[j - 1].PName;
            worksheet.Cell(j, 5).Value = dataList[j - 1].Qty;
            worksheet.Cell(j, 6).Value = dataList[j - 1].Amount;
            worksheet.Cell(j, 7).Value = dataList[j - 1].ToQty;
            worksheet.Cell(j, 8).Value = dataList[j - 1].ToAmount;
            worksheet.Cell(j, 9).Value = dataList[j - 1].Cost;
            worksheet.Cell(j, 10).Value = dataList[j - 1].Pno;

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