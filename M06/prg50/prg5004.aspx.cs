using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg5004 : System.Web.UI.Page
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
                ToDouble toDouble = new ToDouble();

                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    memSn = cookies.Read("memSn"),
                    acctInId = cookies.Read("acctInId"),
                    OutStkId = cookies.Read("OutStkId"),
                    pName = WebUtility.UrlDecode(cookies.Read("pName")),
                    pNo = cookies.Read("pNo");
                sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                string SqlComm = $"SELECT * FROM WP_vAcctIn WHERE isDel='N' AND dtlIsDel='N' AND " +
                    $"(acctInDate BETWEEN '{sDate}' AND '{eDate}') " +
                    $"{(memSn == "" ? "" : $"AND memSn='{memSn}'")} " +
                    $"{(acctInId == "" ? "" : $"AND acctInId='{acctInId}'")} " +
                    $"{(OutStkId == "" ? "" : $"AND OutStkId='{OutStkId}'")} " +
                    $"{(pNo == "" ? pName == "" ? "" : $"AND (pNameS like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')" : $"AND (pNo='{pNo}')")} " +
                    $"ORDER BY sn, memSn, dtlSn, oStkDtlSn";
                DataTable acctInTbl = getTbl.table("WP", SqlComm);
                Label2.Text = "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 搜尋條件</div>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>查詢區間</span>" +
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
                        "<div class='search-sub' style='margin-right:62px;'>" +
                            "<span class='input-title'>銷帳單號</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input' id='acctInId' maxlength='40' style='width:130px;' value='{acctInId}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                    "<div>";
                        SqlComm = $"SELECT * FROM WP_Member ORDER BY memId";
                        DataTable memTbl = getTbl.table("WP", SqlComm);
                        Label2.Text += "<div class='search-sub'>" +
                            "<span class='input-title'>會　　員</span>" +
                            "<select id='memSn' class='form-control'>" +
                                "<option value=''>全部</option>";
                                foreach (DataRow row in memTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["sn"]}' {(memSn == row["sn"].ToString() ? "selected" : "")}>{row["memId"]}-{row["memName"]}</option>";
                                }
                            Label2.Text += "</select>" +
                        "</div>" +
                        "<div class='search-sub' style='margin-right:45px;'>" +
                            "<span class='input-title'>銷貨單號</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input' id='OutStkId' maxlength='40' style='width:130px;' value='{OutStkId}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                    "<div>" +
                        "<div class='search-sub' style='margin-right:45px;'>" +
                            "<span class='input-title'>商　　品</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input' placeholder='請輸入商品名稱或條碼' maxlength='42' style='width:266px;' id='pd-filter' value='{pName}' />" +
                                $"<input type='hidden' id='act-pNo' value='{pNo}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                    "<div class='page-submit'><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>" +
                "<table class='list-main inStk-list-main' id=rptr-main style='border-spacing:1px;'>" +
                    "<tr class='list-title'>" +
                        "<td>銷帳單號</td><td>銷帳日期</td><td style='text-align:right;'>銷帳總額</td>" +
                        "<td>會員</td>" +
                        "<td>銷貨單號</td><td style='text-align:right'>銷帳金額</td><td style='text-align:right'>全折</td>" +
                        "<td>商品</td><td style='text-align:right'>單價</td><td style='text-align:right'>數量</td><td style='text-align:right'>單折</td><td style='text-align:right'>小計</td>" +
                    "</tr>";
                    if (acctInTbl.Rows.Count == 0)
                    {
                        Label2.Text += "<tr><td colspan='9' class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無 應收帳款銷帳 交易！</div>";
                    }
                    else
                    {
                        ThreeDot threeDot = new ThreeDot();
                        string preSn = "", sn_dataId = "", preMemSn = "", memSn_dataId = "", preDtlSn = "", dtlSn_dataId = "";
                        int pdSubTotal = 0, snSubTotal = 0, snTotal = 0, discnt = 0, dtlDiscnt = 0, discntTotal = 0, dtlDiscntTotal = 0, amt = 0, amtTotal = 0;
                        foreach (DataRow row in acctInTbl.Rows)
                        {
                            sn_dataId = preSn != row["sn"].ToString() ? $"{row["sn"]}" : "";
                            memSn_dataId = sn_dataId == "" ? preMemSn != row["memSn"].ToString() ? $"{row["memSn"]}" : "" : $"{row["memSn"]}";
                            dtlSn_dataId = memSn_dataId == "" ? preDtlSn != row["dtlSn"].ToString() ? $"{row["dtlSn"]}" : "" : $"{row["dtlSn"]}";
                            if ((preSn != $"{row["sn"]}" && preSn != "") || (preMemSn != $"{row["memSn"]}" && preMemSn != ""))
                            {
                                discnt = (-1) * (int.Parse($"{row["discount"]:#0}") + int.Parse($"{row["discountShare"]:#0}"));
                                Label2.Text += $"<tr><td colspan=4 class='row-main' data-id=''></td><td colspan=6 style='text-align:right;'>合計</td><td colspan=2 class='snSubTotal' style='text-align:right;' data-val='{snSubTotal}'>{threeDot.To3Dot($"{snSubTotal}")}</td></tr>";
                                snTotal += snSubTotal;      //總計
                                discntTotal += discnt;      //全折
                                snSubTotal = 0;
                            }
                            dtlDiscnt = (-1) * (int.Parse($"{row["dtlDiscnt"]:#0}") + int.Parse($"{row["dtlDiscntShare"]:#0}"));
                            dtlDiscntTotal += dtlDiscnt;
                            pdSubTotal = int.Parse($"{row["oStkDtlAmtTotal"]:#0}") + dtlDiscnt;
                            snSubTotal += pdSubTotal;
                            amt = int.Parse($"{row["amount"]:#0}");
                            if (preSn != $"{row["sn"]}") { amtTotal += amt; }
                            Label2.Text += "<tr>" +
                                $"<td class='row-main' data-id='{sn_dataId}'><a href='/prg50/prg5004Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["acctInId"]}</a></div>" +
                                $"<td class='row-main' data-id='{sn_dataId}'>{row["acctInDate"]:yyyy/MM/dd}</div>" +
                                $"<td class='row-main' data-id='{sn_dataId}' style='text-align:right;'>{threeDot.To3Dot($"{amt}")}</div>" +
                                $"<td class='row-main' data-id='{memSn_dataId}'>{row["memId"]}-{row["memName"]}</td>" +
                                $"<td class='row-main' data-id='{dtlSn_dataId}'>{row["OutStkId"]}</td>" +
                                $"<td class='row-main' data-id='{dtlSn_dataId}' style='text-align:right;'>{threeDot.To3Dot($"{row["outStkAmtTotal"]:#0}")}</td>" +
                                $"<td class='row-main' data-id='{dtlSn_dataId}' style='text-align:right;'>{discnt}</td>" +
                                $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}-{row["pNameS"]}</td>" +
                                $"<td style='text-align:right'>{threeDot.To3Dot($"{row["oStkDtlAmt"]:#0}")}</td>" +
                                $"<td style='text-align:right'>{threeDot.To3Dot($"{row["oStkDtlQty"]:#0}")}</td>" +
                                $"<td style='text-align:right' class='dtl-discnt' data-val='{dtlDiscnt}'>{dtlDiscnt}</td>" +
                                $"<td style='text-align:right'>{threeDot.To3Dot($"{pdSubTotal}")}</td>" +
                            "</tr>";
                            preSn = row["sn"].ToString();
                            preMemSn = row["memSn"].ToString();
                            preDtlSn = row["dtlSn"].ToString();
                        }
                        snTotal += snSubTotal;
                        Label2.Text += $"<tr><td colspan=4 class='row-main' data-id=''></td><td colspan=6 style='text-align:right;'>合計</td><td colspan=2 class='snSubTotal' style='text-align:right;' data-val='{snSubTotal}'>{threeDot.To3Dot($"{snSubTotal}")}</td></tr>" +
                        "<tr class='foot-total'>" +
                            $"<td colspan=3 style='text-align:right;'>銷帳總計︰{threeDot.To3Dot($"{amtTotal}")}</td>" + 
                            "<td colspan=9>" +
                                //"<span id='total-discnt-row' style='margin-right:0px;'>(" +
                                //    $"<span id='total-discnt' data-val='{discntTotal}'>{threeDot.To3Dot($"{discntTotal}")}[全折] / </span>" +
                                //    $"<span id='total-dtlDiscnt' data-val='{dtlDiscntTotal}'>{threeDot.To3Dot($"{dtlDiscntTotal}")}[單折]</span>" +
                                //")</span>" +
                            "</td>" +
                            //$"<td>{threeDot.To3Dot($"{snTotal}")}</td>" +
                        "</tr>";
                    }
                Label2.Text += "</table>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080;font-weight:bold;'>列印銷帳表</button></div>";
            }
        }
    }
}