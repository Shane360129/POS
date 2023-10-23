using System;
using System.Data;
using System.Linq;
using System.Net;
using static Global;

public partial class prg3002PRN : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        Stock stock = new Stock();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "3002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    pvSn = cookies.Read("pvSn"),
                    reciptNo = cookies.Read("reciptNo"),
                    pName = WebUtility.UrlDecode(cookies.Read("pName")),
                    pNo = cookies.Read("pNo"),
                    isTax = cookies.Read("isTax"),
                    payType = cookies.Read("payType"),
                    InStkId = cookies.Read("InStkId");
                sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                string SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND " +
                    $"(InStkDate BETWEEN '{sDate}' AND '{eDate}') " +
                    $"{(pvSn == "" ? "" : $"AND pvSn='{pvSn}'")} " +
                    $"{(reciptNo == "" ? "" : $"AND reciptNo='{reciptNo}'")} " +
                    $"{(pNo == "" ? pName == "" ? "" : $"AND (pNameS like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')" : $"AND (pNo='{pNo}')")} " +
                    $"{(isTax == "" ? "" : $"AND isTax='{isTax}'")} " +
                    $"{(payType == "" ? "" : $"AND payType='{payType}'")} " +
                    $"{(InStkId == "" ? "" : $"AND InStkId='{InStkId}'")} " +
                    $"ORDER BY pvId, InStkDate, InStkId, sn, dtlSn";
                DataTable inStkTbl = getTbl.table("WP", SqlComm);
                if (inStkTbl.Rows.Count == 0)
                {
                    Label2.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無進貨交易！</div>";
                }
                else
                {
                    SqlComm = $"SELECT * FROM WP_vProvider";
                    DataTable pvTbl = getTbl.table("WP", SqlComm);

                    ThreeDot threeDot = new ThreeDot();
                    string preInStkId = "", preAmtNoneTax = "", preTax = "", preAmt = "";
                    int pageNo = 0, recNo = 0, printNo = 0, preMarginTop = 0, prtLine = 0, prePrtLine = 0;
                    foreach (DataRow row in inStkTbl.Rows)
                    {
                        if (preInStkId != $"{row["InStkId"]}") {
                            pageNo++;
                            var temp = (from dr in inStkTbl.Select($"InStkId='{row["InStkId"]}'").AsEnumerable()
                                        group dr by new { InStkId = dr["InStkId"] }
                                        into g
                                        select new
                                        {
                                            StkId = g.Key.InStkId,
                                            Count = g.Count()
                                        }).ToArray();
                            prtLine = temp[0].Count;
                            DataRow[] pvRows = pvTbl.Select($"sn='{row["pvSn"]}'");
                            if(preInStkId != "")
                            {
                                    Label2.Text += $"<tr class='inStk-tbl-footer'><td colspan='7' style='padding-right:2px;'>採購淨額︰{preAmtNoneTax}　採購稅額(應)︰{preTax}　合計︰{preAmt}</td><td colspan='2'></td></tr>" +
                                "</table>" +
                                "<div class='inStk-tbl-reptr'><span class='rptr-cell'>審　核︰</span><span class='rptr-cell'>經　辧︰</span><span class='rptr-cell'>會　計︰</span><span class='rptr-cell'>倉　管︰</span><span class='rptr-cell'>簽　收︰</span></div>";
                                printNo++;
                                if (printNo == 2 || prtLine > 15 || prePrtLine > 15)
                                {
                                    Label2.Text += $"<p style='page-break-after:always'></p>";
                                    printNo = 0;
                                    preMarginTop = 0;
                                }
                                else
                                {
                                    preMarginTop = ((16 - recNo) * 22) + 50;
                                }
                            }
                            prePrtLine = prtLine;
                            Label2.Text += $"<div class='inStk inStk-title' style='margin-top:{preMarginTop}px'>" +
                                "<div style='display:flex;align-items:flex-end;'>" +
                                    $"<div>廠商名稱︰{pvRows[0]["pvId"]}-{pvRows[0]["pvName"]}</div>" +
                                    "<div class='rptr-title' style='font-family:標楷體!important;'>商品採購單</div>" +
                                    $"<div>單號︰{row["InStkId"]}　頁次︰{pageNo}/<span class='page-total'></span></div>" +
                                "</div>" +
                                "<div>" +
                                    $"<div style='width:66%;'>廠商地址︰{($"{pvRows[0]["pvZoneId"]}" == "0" ? "" : $"{pvRows[0]["pvZoneId"]}")}{($"{pvRows[0]["pvCityId"]}" == "0" ? "" : $"{pvRows[0]["pvCity"]}-")}{($"{pvRows[0]["pvZoneId"]}" == "0" ? "" : $"{pvRows[0]["pvZone"]}")}{pvRows[0]["pvAddr"]}</div>" +
                                    $"<div style='text-align:right;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                "</div>" +
                                "<div>" +
                                    $"<div>廠商電話︰{pvRows[0]["pvTel"]}</div>" +
                                    $"<div>統一編號︰{pvRows[0]["taxId"]}</div>" +
                                    $"<div>採購日期︰{row["InStkDate"]:yyyy/MM/dd}</div>" +
                                "</div>" +
                                $"<div style='overflow:hidden'>備註︰{pvRows[0]["memo"]}</div>" +
                            "</div>" +
                            "<table class='inStk inStk-Tbl'>" +
                                "<tr class='inStk-tbl-title'><td>序</td><td>商品編號</td><td>商品名稱</td><td>單位</td><td>數量</td><td>單價</td><td>小計</td><td>售價</td><td>備註</td></tr>";
                            recNo = 0;
                        };
                        recNo++;
                        double dtlAmt = toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), pointQty);

                        Label2.Text += "<tr>" +
                            $"<td>{recNo}</td>" +
                            $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}</td>" +
                            $"<td>{row["pName"]}</td>" +
                            $"<td>{row["pUName"]}</td>" +
                            $"<td>{threeDot.To3Dot($"{row["qty"]}")}</td>" +
                            $"<td>{threeDot.To3Dot($"{dtlAmt.ToString(pointRule)}")}</td>" +
                            $"<td>{threeDot.To3Dot($"{row["amtTotal"]:#0}")}</td>" +
                            $"<td>{threeDot.To3Dot($"{row["priceStd"]:#0}")}</td>" +
                            $"<td>{row["memo"]}</td>" +
                        "</tr>";
                        preAmtNoneTax = $"{threeDot.To3Dot(toDouble.Numer(double.Parse($"{row["amtNoneTax"]}"), pointQty).ToString())}";
                        preTax = $"{threeDot.To3Dot(toDouble.Numer(double.Parse($"{row["tax"]}"), pointQty).ToString())}";
                        preAmt = $"{threeDot.To3Dot($"{row["amount"]:#0}")}";
                        preInStkId = $"{row["InStkId"]}";
                    }
                    if (preInStkId != "")
                    {
                        Label2.Text += $"<tr class='inStk-tbl-footer'><td colspan='7' style='padding-right:2px;'>採購淨額︰{threeDot.To3Dot($"{preAmtNoneTax}")}　採購稅額(應)︰{threeDot.To3Dot($"{preTax}")}　合計︰{threeDot.To3Dot($"{preAmt}")}</td><td colspan='2'></td></tr>" +
                        "</table>" +
                        "<div class='inStk-tbl-reptr'><span class='rptr-cell'>審　核︰</span><span class='rptr-cell'>經　辧︰</span><span class='rptr-cell'>會　計︰</span><span class='rptr-cell'>倉　管︰</span><span class='rptr-cell'>簽　收︰</span></div>";
                    }
                    Label2.Text += $"<input type='hidden' id='pageTotal' value='{pageNo}'>";
                }
            }
        }
    }
}