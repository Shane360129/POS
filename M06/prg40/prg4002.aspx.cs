using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg4002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        Stock stock = new Stock();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "4002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string sDate = WebUtility.UrlDecode(cookies.Read("sDate")),
                    eDate = WebUtility.UrlDecode(cookies.Read("eDate")),
                    memSn = cookies.Read("memSn"),
                    outStkId = cookies.Read("outStkId"),
                    pName = WebUtility.UrlDecode(cookies.Read("pName")),
                    reciptNo = cookies.Read("reciptNo"),
                    pNo = cookies.Read("pNo"),
                    isTax = cookies.Read("isTax"),
                    outType = cookies.Read("outType"),
                    SqlComm;

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
                        "<div class='search-sub'>" +
                            "<span class='input-title'>發票號碼</span>" +
                            $"<input type='text' class='form-control' id='reciptNo' size='20' maxlength='50' value='{reciptNo}' />" +
                        "</div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>銷帳狀態</span>" +
                            "<select id='outType' class='form-control'>" +
                                "<option value=''>全部</option>" +
                                $"<option value='0' {(outType == "0" ? "selected" : "")}>未結</option>" +
                                $"<option value='1' {(outType == "1" ? "selected" : "")}>未結完</option>" +
                                $"<option value='2' {(outType == "2" ? "selected" : "")}>全結</option>" +
                            "</select>" +
                        "</div>" +
                    "</div>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>會　　員</span>";
                            SqlComm = $"SELECT * FROM WP_Member WHERE isStop='N' ORDER BY memId";
                            DataTable memTbl = getTbl.table("WP", SqlComm);
                            if (memType == "1")
                            {
                                Label2.Text += "<select id='memSn' class='form-control'>" +
                                    "<option value=''>全部</option>";
                                foreach (DataRow row in memTbl.Rows)
                                {
                                    Label2.Text += $"<option value='{row["sn"]}' {(memSn == row["sn"].ToString() ? "selected" : "")}>{row["memId"]}-{row["memName"]}</option>";
                                }
                                Label2.Text += "</select>";
                            }
                            else
                            {
                                DataRow[] memRows = memSn == "" ? null : memTbl.Select($"sn={memSn}");
                                string memName = memRows == null ? "" : $"{memRows[0]["memId"]}．{memRows[0]["memName"]}";
                                Label2.Text += "<div class='del-txt-group'>" +
                                    $"<input type='text' class='form-control del-txt-input' placeholder='請輸入會員名稱或編號' maxlength='42' style='width:266px;' id='mem-filter' value='{memName}' />" +
                                    $"<input type='hidden' id='memSn' value='{memSn}' />" +
                                    "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                                "</div>";
                            }
                        Label2.Text += "</div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>銷貨單號</span>" +
                            $"<input type='text' class='form-control' id='outStkId' size='20' maxlength='50' value='{outStkId}' />" +
                        "</div>" +
                        "<div class='search-sub' style='margin-right:110px;'>" +
                            "<span class='input-title'>稅　　別</span>" +
                            "<select id='isTax' class='form-control'>" +
                                "<option value=''>全部</option>" +
                                $"<option value='Y' {(isTax == "Y" ? "selected" : "")}>應稅</option>" +
                                $"<option value='N' {(isTax == "N" ? "selected" : "")}>免稅</option>" +
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
                    SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND " +
                        $"(OutStkDate BETWEEN '{sDate}' AND '{eDate} 23:59:59') " +
                        $"{(memSn == "" ? "" : $"AND memSn='{memSn}'")} " +
                        $"{(outStkId == "" ? "" : $"AND OutStkId='{outStkId}'")} " +
                        $"{(pNo == "" ? pName == "" ? "" : $"AND (pNameS like'%{pName}%' OR pBarcode like'%{pName}%' OR pCode like'%{pName}%')" : $"AND (pNo='{pNo}')")} " +
                        $"{(reciptNo == "" ? "" : $"AND (reciptNo like'%{reciptNo}%')")} " +
                        $"{(isTax == "" ? "" : $"AND isTax='{isTax}'")} " +
                        $"{(outType == "" ? "" : $"AND outType='{outType}'")} " +
                        $"ORDER BY OutStkId, sn, dtlSn";
                    DataTable outStkTbl = getTbl.table("WP", SqlComm);

                    ToDouble toDouble = new ToDouble();
                    ThreeDot threeDot = new ThreeDot();
                    int discntTotal = 0;

                    Label2.Text += "<table class='list-main outStk-list-main' style='border-spacing:1px;'>" +
                        "<tr class='list-title'><td>銷貨單號</td><td>銷貨日期</td><td>會員</td><td style='text-align:center'>銷帳狀態</td><td style='text-align:right;'>全折</td><td>商品</td><td style='text-align:right'>含稅單價</td><td style='text-align:center'>稅別</td><td style='text-align:right'>數量</td><td style='text-align:right;'>單折</td><td style='text-align:right'>小計</td></tr>";
                    if (outStkTbl.Rows.Count == 0)
                    {
                        Label2.Text += "<tr><td colspan='9' class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無銷貨交易！</div>";
                    }
                    else
                    {
                        string preId = "", main_data_id = "";
                        int discnt, dtlDiscnt, amtTotal;
                        foreach (DataRow row in outStkTbl.Rows)
                        {
                            main_data_id = preId != row["sn"].ToString() ? $"{row["sn"]}" : "";
                            double dtlAmt = toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), 2);
                            discnt = int.Parse($"{row["discount"]:#0}") + int.Parse($"{row["discountShare"]:#0}");
                            discnt = (discnt == 0 ? 1 : -1) * discnt;
                            discntTotal += preId != row["sn"].ToString() ? discnt : 0;
                            dtlDiscnt = int.Parse($"{row["dtlDiscnt"]:#0}") + int.Parse($"{row["dtlDiscntShare"]:#0}");
                            dtlDiscnt = (dtlDiscnt == 0 ? 1 : -1) * dtlDiscnt;
                            amtTotal = int.Parse($"{row["amtTotal"]:#0}") + dtlDiscnt;
                            Label2.Text += "<tr>" +
                                $"<td data-id='{main_data_id}' class='row-main'><a href='/prg40/prg4002Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["OutStkId"]}</a></div>" +
                                $"<td data-id='{main_data_id}' class='row-main'>{row["OutStkDate"]:yyyy/MM/dd}</div>" +
                                $"<td data-id='{main_data_id}' class='row-main'>{row["memId"]}-{row["memName"]}</td>" +
                                $"<td data-id='{main_data_id}' class='row-main' style='text-align:center'>{stock.OutTypeName(row["outType"].ToString())}</td>" +
                                $"<td data-id='{main_data_id}' class='row-main discnt' style='text-align:right;' data-val='{discnt}'>{threeDot.To3Dot($"{discnt}")}</td>" +
                                $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}-{row["pNameS"]}</td>" +
                                $"<td style='text-align:right'>{row["dtlAmt"]:#0}</td>" +
                                $"<td style='text-align:center'>{(row["dtlIsTax"].ToString() == "Y" ? "應稅" : "免稅")}</td>" +
                                $"<td style='text-align:right' class='dtl-qty' data-val='{row["qty"]}'>{threeDot.To3Dot($"{row["qty"]}")}</td>" +
                                $"<td style='text-align:right' class='dtl-discnt' data-val='{dtlDiscnt}'>{threeDot.To3Dot($"{dtlDiscnt}")}</td>" +
                                $"<td style='text-align:right' class='dtl-amt' data-val='{amtTotal}'>{threeDot.To3Dot($"{amtTotal}")}</td>" +
                            "</tr>";
                            preId = row["sn"].ToString();
                        }
                    }
                    Label2.Text += "</table>" +
                    "<div style='text-align:right'>總計︰<span id='total-qty'></span>(數量)　銷售金額︰<span style='text-align:right' id='total-amt'></span>" +
                        "<span id='total-discnt-row' style='margin-left:20px;'>(" +
                            $"<span style='text-align:right;' id='total-discnt' data-val='{discntTotal}'>{threeDot.To3Dot($"{discntTotal}")}[全折] / </span>" +
                            "<span style='text-align:right;' id='total-dtlDiscnt'><span class='amt'></span>[單折]</span>" +
                        ")</span>" +
                    "</div>" +
                    "<div style='width:100%;text-align:right;margin-top:15px;'><a href='prg4002PRN.aspx' target='_blank'><button id='btn-prn' style='color:#080'>應收銷貨單列印</button></a></div>";
                }
            }
        }
    }
}