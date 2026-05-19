using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg5002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        Stock stock = new Stock();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "5002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                ToDouble toDouble = new ToDouble();

                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    pvSn = cookies.Read("pvSn"),
                    acctOutId = cookies.Read("acctOutId"),
                    InStkId = cookies.Read("InStkId"),
                    pName = WebUtility.UrlDecode(cookies.Read("pName")),
                    pNo = cookies.Read("pNo"),
                    isTax = cookies.Read("isTax"),
                    dtlPayType = cookies.Read("dtlPayType");
                sDate = sDate == "" ? DateTime.Now.AddMonths(-1).ToString("yyyy/MM/01") : sDate;
                eDate = eDate == "" ? DateTime.Now.ToString("yyyy/MM/dd") : eDate;

                string SqlComm = $"SELECT * FROM WP_vAcctOut WHERE isDel='N' AND dtlIsDel='N' AND " +
                    $"(acctOutDate BETWEEN '{sDate}' AND '{eDate}') " +
                    $"{(pvSn == "" ? "" : $"AND pvSn='{pvSn}'")} " +
                    $"{(acctOutId == "" ? "" : $"AND acctOutId='{acctOutId}'")} " +
                    $"{(InStkId == "" ? "" : $"AND InStkId='{InStkId}'")} " +
                    //$"{(pName == "" ? "" : $"AND (pName like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')")} " +
                    $"{(pNo == "" ? pName == "" ? "" : $"AND (pNameS like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')" : $"AND (pNo='{pNo}')")} " +
                    $"{(isTax == "" ? "" : $"AND isTax='{isTax}'")} " +
                    $"{(dtlPayType == "" ? "" : $"AND dtlPayType='{dtlPayType}'")} " +
                    $"ORDER BY acctOutId, sn, dtlSn, InStkDtlSn";
                DataTable acctOutTbl = getTbl.table("WP", SqlComm);
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
                        "</div>";
                        SqlComm = $"SELECT * FROM WP_Provider ORDER BY pvId";
                        DataTable pvTbl = getTbl.table("WP", SqlComm);
                        Label2.Text += "<div class='search-sub'>" +
                            "<span class='input-title'>廠　　商</span>" +
                            "<select id='pvSn' class='form-control'>" +
                                "<option value=''>全部</option>";
                                foreach (DataRow row in pvTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["sn"]}' {(pvSn == row["sn"].ToString() ? "selected" : "")}>{row["pvId"]}-{row["pvName"]}</option>";
                                }
                            Label2.Text += "</select>" +
                        "</div>" +
                    "</div>" +
                    "<div>" +
                        "<div class='search-sub' style='margin-right:62px;'>" +
                            "<span class='input-title'>銷帳單號</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input' id='acctOutId' maxlength='40' style='width:130px;' value='{acctOutId}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='search-sub' style='margin-right:45px;'>" +
                            "<span class='input-title'>進貨單號</span>" +
                            "<div class='del-txt-group'>" +
                                $"<input type='text' class='form-control del-txt-input' id='InStkId' maxlength='40' style='width:130px;' value='{InStkId}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                    "<div>" +
                        "<div class='search-sub' style='margin-right:45px;'>" +
                            "<span class='input-title'>商　　品</span>" +
                            "<div class='del-txt-group'>" +
                                //$"<input type='text' class='form-control del-txt-input' id='pName' maxlength='40' style='width:130px;' value='{pName}' />" +
                                $"<input type='text' class='form-control del-txt-input' placeholder='請輸入商品名稱或條碼' maxlength='42' style='width:266px;' id='pd-filter' value='{pName}' />" +
                                $"<input type='hidden' id='act-pNo' value='{pNo}' />" +
                                "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                            "</div>" +
                        "</div>" +
                        "<div class='search-sub' style='margin-right:45px;'>" +
                            "<span class='input-title'>稅　　別</span>" +
                            "<select id='isTax' class='form-control'>" +
                                "<option value=''>全部</option>" +
                                $"<option value='Y' {(isTax == "Y" ? "selected" : "")}>應稅</option>" +
                                $"<option value='N' {(isTax == "N" ? "selected" : "")}>免稅</option>" +
                            "</select>" +
                        "</div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>銷貨狀態</span>" +
                            "<select id='dtlPayType' class='form-control'>" +
                                "<option value=''>全部</option>" +
                                $"<option value='0' {(dtlPayType == "0" ? "selected" : "")}>未結</option>" +
                                $"<option value='1' {(dtlPayType == "1" ? "selected" : "")}>未結完</option>" +
                                $"<option value='2' {(dtlPayType == "2" ? "selected" : "")}>全結</option>" +
                            "</select>" +
                        "</div>" +
                    "</div>" +
                    "<div class='page-submit'><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>" +
                "<table class='list-main inStk-list-main' style='border-spacing:1px;'>" +
                    "<tr class='list-title'><td>銷帳單號</td><td>銷帳日期</td><td>廠商</td><td>進貨單號</td><td>商品</td><td style='text-align:right'>進貨金額</td><td style='text-align:right'>銷帳金額</td><td style='text-align:right'>未銷金額</td><td style='text-align:center'>銷貨狀態</td></tr>";
                    if (acctOutTbl.Rows.Count == 0)
                    {
                        Label2.Text += "<tr><td colspan='9' class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無 應付帳款銷帳 交易！</div>";
                    }
                    else
                    {
                    ThreeDot threeDot = new ThreeDot();
                    string preId = "", main_data_id = "", preInStkId = "", detail_data_id = "";
                    double inStkAmtTotal = 0, amtTotal = 0, dtlPayLeft = 0;
                    foreach (DataRow row in acctOutTbl.Rows)
                        {
                            main_data_id = preId != row["sn"].ToString() ? $"{row["sn"]}" : "";
                            detail_data_id = main_data_id == "" ? preInStkId != row["InStkId"].ToString() ? $"{row["InStkId"]}" : "" : $"{row["InStkId"]}";
                        double dtlAmt = toDouble.Numer (double.Parse ($"{row["dtlAmt"]}"), 2);
                        inStkAmtTotal += double.Parse ($"{row["inStkAmtTotal"]:#0.00}");
                        amtTotal += double.Parse ($"{row["amtTotal"]:#0.00}");
                        dtlPayLeft += double.Parse ($"{row["dtlPayLeft"]:#0.00}");

                        Label2.Text += "<tr>" +
                                $"<td class='row-main' data-id='{main_data_id}'><a href='/prg50/prg5002Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["acctOutId"]}</a></div>" +
                                $"<td class='row-main' data-id='{main_data_id}'>{row["acctOutDate"]:yyyy/MM/dd}</div>" +
                                $"<td class='row-main' data-id='{main_data_id}'>{row["pvId"]}-{row["pvNameS"]}</td>" +
                                $"<td class='row-main' data-id='{detail_data_id}'>{row["InStkId"]}</td>" +
                                $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}-{row["pNameS"]}</td>" +
                                $"<td style='text-align:right'>{threeDot.To3Dot(toDouble.Numer(double.Parse($"{row["inStkAmtTotal"]}"), pointQty).ToString())}</td>" +
                                $"<td style='text-align:right'>{row["amtTotal"]:#0.00}</td>" +
                                $"<td style='text-align:right'>{row["dtlPayLeft"]:#0.00}</td>" +
                                $"<td style='text-align:center'>{stock.PayTypeName(row["dtlPayType"].ToString())}</td>" +
                            "</tr>";
                            preId = row["sn"].ToString();
                            preInStkId = row["InStkId"].ToString();
                        }
                    Label2.Text += "<tr class='total-row'>" +
                        "<td colspan='5' style='text-align:right'>合計</td>" +
                        $"<td style='text-align:right'>{threeDot.To3Dot ($"{toDouble.Numer (inStkAmtTotal, 2):#0.00}")}</td>" +
                        $"<td style='text-align:right'>{threeDot.To3Dot ($"{toDouble.Numer (amtTotal, 2):#0.00}")}</td>" +
                        $"<td style='text-align:right'>{threeDot.To3Dot ($"{toDouble.Numer (dtlPayLeft, 2):#0.00}")}</td>" +
                        $"<td></td>" +
                    "</tr>";
                    }
                Label2.Text += "</table>";
            }
        }
    }
}