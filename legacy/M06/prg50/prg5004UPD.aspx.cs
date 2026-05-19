using System;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using static Global;

public partial class prg5004UPD : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        Stock stock = new Stock();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "5004";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                string sn = Request.QueryString["sn"];
                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷銷帳序號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg50/prg5004.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vAcctIn WHERE isDel='N' AND dtlIsDel='N' AND sn={sn}";
                    DataTable acctInTbl = getTbl.table("WP", SqlComm);
                    if (acctInTbl.Rows.Count == 0) { Response.Redirect("/prg50/prg5004.aspx"); }
                    else
                    {
                        ToDouble toDouble = new ToDouble();

                        DataRow prgRow0 = emp.PrgTbl.Rows[0];
                        Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                        $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 銷帳單內容</div>";

                        DataRow row0 = acctInTbl.Rows[0];
                        Label2.Text = $"<input type='hidden' id='sn' value='{sn}'>" +
                        "<div style='border:1px #000 solid;border-radius:3px;padding:10px 0;'>" +
                            "<div style='margin-bottom:10px;'>" +
                                $"<span style='font-weight:bold;font-size:18px;margin-left:1%;margin-right:45px'><i class='fas fa-caret-square-right'></i> 銷帳單號︰{row0["acctInId"]}</span>" +
                                $"<span style='font-weight:bold;font-size:18px;margin-left:1%;margin-right:45px'><i class='fas fa-caret-square-right'></i> 銷帳日期︰{row0["acctInDate"]:yyyy/MM/dd}</span>" +
                                "<button class='btn-admin btn-del' id='btn-acctIn-del'><i class='fas fa-trash'></i> 刪除整筆</button>" +
                            "</div>" +
                        "</div>" +
                        "<table class='list-main inStk-list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'><td>銷帳單號</td><td>銷帳日期</td><td style='text-align:right;'>銷帳金額</td><td>會員</td><td>銷貨單號</td><td style='text-align:right'>全折</td><td>商品</td><td style='text-align:right'>單價</td><td style='text-align:right'>數量</td><td style='text-align:right'>單折</td><td style='text-align:right'>小計</td></tr>";
                            ThreeDot threeDot = new ThreeDot();
                            string preSn = "", sn_dataId = "", preMemSn = "", memSn_dataId = "", preDtlSn = "", dtlSn_dataId = "";
                            int snSubTotal = 0, snTotal = 0, discnt = 0, dtlDiscnt = 0, discntTotal = 0, dtlDiscntTotal = 0;
                            foreach (DataRow row in acctInTbl.Rows)
                            {

                                sn_dataId = preSn != $"{row["sn"]}" ? $"{row["sn"]}" : "";
                                memSn_dataId = sn_dataId == "" ? preMemSn != $"{row["memSn"]}" ? $"{row["memSn"]}" : "" : $"{row["memSn"]}";
                                dtlSn_dataId = memSn_dataId == "" ? preDtlSn != $"{row["dtlSn"]}" ? $"{row["dtlSn"]}" : "" : $"{row["dtlSn"]}";
                                discnt = (-1) * (int.Parse($"{row["discount"]:#0}") + int.Parse($"{row["discountShare"]:#0}"));
                                discnt = memSn_dataId == "" ? preDtlSn != $"{row["dtlSn"]}" ? discnt : 0 : discnt;
                                discntTotal += discnt;      //全折
                                

                                dtlDiscnt = (-1) * (int.Parse($"{row["dtlDiscnt"]:#0}") + int.Parse($"{row["dtlDiscntShare"]:#0}"));        //單折
                                dtlDiscntTotal += dtlDiscnt;
                                snSubTotal = int.Parse($"{row["oStkDtlAmtTotal"]:#0}") + dtlDiscnt;
                                snTotal += snSubTotal;      //總計
                                Label2.Text += "<tr>" +
                                    $"<td class='row-main' data-id='{sn_dataId}'>{row["acctInId"]}</div>" +
                                    $"<td class='row-main' data-id='{sn_dataId}'>{row["acctInDate"]:yyyy/MM/dd}</div>" +
                                    $"<td class='row-main' data-id='{sn_dataId}' style='text-align:right;'>{threeDot.To3Dot($"{row["amount"]:#0}")}</div>" +
                                    $"<td class='row-main' data-id='{memSn_dataId}'>{row["memId"]}-{row["memName"]}</td>" +
                                    $"<td class='row-main' data-id='{dtlSn_dataId}'>{row["OutStkId"]}</td>" +
                                    $"<td class='row-main' data-id='{dtlSn_dataId}'>{discnt}</td>" +
                                    $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}-{row["pNameS"]}</td>" +
                                    $"<td style='text-align:right'>{row["oStkDtlAmt"]:#0}</td>" +
                                    $"<td style='text-align:right'>{row["oStkDtlQty"]}</td>" +
                                    $"<td style='text-align:right' class='dtl-discnt' data-val='{dtlDiscnt}'>{dtlDiscnt}</td>" +
                                    $"<td style='text-align:right' class='oStkDtlAmtTotal'>{threeDot.To3Dot($"{snSubTotal}")}</td>" +
                                "</tr>";
                                preSn = $"{row["sn"]}";
                                preMemSn = $"{row["memSn"]}";
                                preDtlSn = $"{row["dtlSn"]}";
                            }
                            Label2.Text += "<tr class='foot-total'>" +
                                $"<td colspan='10'>共計︰" +
                                    "<span id='total-discnt-row' style='margin-right:0px;'>(" +
                                        $"<span style='text-align:right;' id='total-discnt' data-val='{discntTotal}'>{threeDot.To3Dot($"{discntTotal}")}[全折] / </span>" +
                                        $"<span style='text-align:right;' id='total-dtlDiscnt' data-val='{dtlDiscntTotal}'>{threeDot.To3Dot($"{dtlDiscntTotal}")}[單折]</span>" +
                                    ")</span>" +
                                "</td>" +
                                $"<td>{threeDot.To3Dot($"{snTotal}")}</td>" +
                            "</tr>" +
                        "</table>";
                    }
                }
            }
        }
    }
}