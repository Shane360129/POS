using System;
using System.Data;
using System.Linq;
using System.Net;
using static Global;

public partial class prg4002PRN : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        Stock stock = new Stock();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "4002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    memSn = cookies.Read("memSn"),
                    outStkId = cookies.Read("outStkId"),
                    pName = WebUtility.UrlDecode(cookies.Read("pName")),
                    reciptNo = cookies.Read("reciptNo"),
                    pNo = cookies.Read("pNo"),
                    isTax = cookies.Read("isTax"),
                    outType = cookies.Read("outType");

                string SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND " +
                    $"(OutStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') " +
                    $"{(memSn == "" ? "" : $"AND memSn='{memSn}'")} " +
                    $"{(outStkId == "" ? "" : $"AND OutStkId='{outStkId}'")} " +
                    $"{(pNo == "" ? pName == "" ? "" : $"AND (pNameS like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')" : $"AND (pNo='{pNo}')")} " +
                    $"{(reciptNo == "" ? "" : $"AND (reciptNo like'%{reciptNo}%')")} " +
                    $"{(isTax == "" ? "" : $"AND isTax='{isTax}'")} " +
                    $"{(outType == "" ? "" : $"AND outType='{outType}'")} " +
                    $"ORDER BY OutStkId, sn, dtlSn";
                DataTable outStkTbl = getTbl.table("WP", SqlComm);

                //string SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND outType<>'2' ORDER BY memId, OutStkId, sn, dtlSn";
                //DataTable outStkTbl = getTbl.table("WP", SqlComm);
                if (outStkTbl.Rows.Count == 0)
                {
                    Label2.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無應收銷貨單！</div>";
                }
                else
                {
                    SqlComm = $"SELECT * FROM WP_vMember";
                    DataTable memTbl = getTbl.table("WP", SqlComm);

                    ThreeDot threeDot = new ThreeDot();
                    string preOutStkId = "", preAmtNoneTax = "", preTax = "", preAmt = "";
                    int pageNo = 0, recNo = 0, printNo = 0, preMarginTop = 0, prtLine = 0, prePrtLine = 0, preDiscnt = 0, preDiscntDtl = 0;
                    foreach (DataRow row in outStkTbl.Rows)
                    {
                        if (preOutStkId != $"{row["OutStkId"]}") {
                            pageNo++;
                            var temp = (from dr in outStkTbl.Select($"OutStkId='{row["OutStkId"]}'").AsEnumerable()
                                        group dr by new { OutStkId = dr["OutStkId"] }
                                        into g
                                        select new
                                        {
                                            StkId = g.Key.OutStkId,
                                            Count = g.Count()
                                        }).ToArray();
                            prtLine = temp[0].Count;
                            DataRow[] memRows = memTbl.Select($"sn='{row["memSn"]}'");
                            if(preOutStkId != "")
                            {
                                    Label2.Text += $"<tr class='inStk-tbl-footer'><td colspan='8' style='padding-right:2px;'>全單折扣︰{preDiscnt}　單項折扣︰{preDiscntDtl}　合計︰{preAmt}</td><td colspan='2'></td></tr>" +
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
                                    $"<div>客戶名稱︰{memRows[0]["memId"]}-{memRows[0]["memName"]}</div>" +
                                    "<div class='rptr-title' style='font-family:標楷體!important;'>應收款銷貨單</div>" +
                                    $"<div>單號︰{row["OutStkId"]}　頁次︰{pageNo}/<span class='page-total'></span></div>" +
                                "</div>" +
                                "<div>" +
                                    $"<div style='width:66%;'>客戶地址︰{($"{memRows[0]["memZoneId"]}" == "0" ? "" : $"{memRows[0]["memZoneId"]}")}{($"{memRows[0]["memCityId"]}" == "0" ? "" : $"{memRows[0]["memCityName"]}-")}{($"{memRows[0]["memZoneId"]}" == "0" ? "" : $"{memRows[0]["memZoneName"]}")}{memRows[0]["memAddr"]}</div>" +
                                    $"<div style='text-align:right;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                                "</div>" +
                                "<div>" +
                                    $"<div>客戶電話︰{memRows[0]["memAddr"]}</div>" +
                                    $"<div>統一編號︰{memRows[0]["idNo"]}</div>" +
                                    $"<div>銷售日期︰{row["OutStkDate"]:yyyy/MM/dd}</div>" +
                                "</div>" +
                                $"<div style='overflow:hidden'>備註︰{memRows[0]["memo"]}</div>" +
                            "</div>" +
                            "<table class='inStk inStk-Tbl'>" +
                                "<tr class='inStk-tbl-title'><td>序</td><td>商品編號</td><td>商品名稱</td><td>單位</td><td>數量</td><td>單價</td><td>單折</td><td>小計</td><td>備註</td></tr>";
                            recNo = 0;
                        };
                        recNo++;
                        //double dtlAmt = toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), 2);
                        double dtlAmt = toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), 2);
                        preDiscntDtl = (-1) * (int.Parse($"{row["dtlDiscnt"]:#0}") + int.Parse($"{row["dtlDiscntShare"]:#0}"));

                        Label2.Text += "<tr>" +
                            $"<td>{recNo}</td>" +
                            $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}</td>" +
                            $"<td>{row["pName"]}</td>" +
                            $"<td>{row["pUName"]}</td>" +
                            $"<td>{threeDot.To3Dot($"{row["qty"]}")}</td>" +
                            $"<td>{threeDot.To3Dot($"{row["dtlAmt"]:#0}")}</td>" +
                            $"<td>{threeDot.To3Dot($"{preDiscntDtl}")}</td>" +
                            $"<td>{threeDot.To3Dot($"{(int.Parse($"{row["amtTotal"]:#0}") + preDiscntDtl)}")}</td>" +
                            $"<td>{row["memo"]}</td>" +
                        "</tr>";
                        preAmtNoneTax = $"{threeDot.To3Dot(toDouble.Numer(double.Parse($"{row["amtNoneTax"]}"), pointQty).ToString())}";
                        preTax = $"{threeDot.To3Dot(toDouble.Numer(double.Parse($"{row["tax"]}"), pointQty).ToString())}";
                        preAmt = $"{threeDot.To3Dot($"{row["amount"]:#0}")}";
                        preOutStkId = $"{row["OutStkId"]}";
                    }
                    if (preOutStkId != "")
                    {
                        //Label2.Text += $"<tr class='inStk-tbl-footer'><td colspan='7' style='padding-right:2px;'>銷售淨額︰{threeDot.To3Dot($"{preAmtNoneTax}")}　銷售稅額(應)︰{threeDot.To3Dot($"{preTax}")}　合計︰{threeDot.To3Dot($"{preAmt}")}</td><td colspan='2'></td></tr>" +
                        Label2.Text += $"<tr class='inStk-tbl-footer'><td colspan='8' style='padding-right:2px;'>全單折扣︰{preDiscnt}　單項折扣︰{preDiscntDtl}　合計︰{preAmt}</td><td colspan='2'></td></tr>" +
                        "</table>" +
                        "<div class='inStk-tbl-reptr'><span class='rptr-cell'>審　核︰</span><span class='rptr-cell'>經　辧︰</span><span class='rptr-cell'>會　計︰</span><span class='rptr-cell'>倉　管︰</span><span class='rptr-cell'>簽　收︰</span></div>";
                    }
                    Label2.Text += $"<input type='hidden' id='pageTotal' value='{pageNo}'>";
                }
            }
        }
    }
}