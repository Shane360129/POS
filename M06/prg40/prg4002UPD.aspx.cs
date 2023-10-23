using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using static Global;

public partial class prg4002UPD : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "4002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                string sn = Request.QueryString["sn"];

                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷銷貨序號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg30/prg3002.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND sn={sn}";
                    DataTable outStkTbl = getTbl.table("WP", SqlComm);
                    if (outStkTbl.Rows.Count == 0) { Response.Redirect("/prg40/prg4002.aspx"); }
                    else
                    {
                        ToDouble toDouble = new ToDouble();
                        DataRow prgRow0 = emp.PrgTbl.Rows[0];

                        Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                        $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 修改銷貨資料</div>" +
                        $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                        string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                        DataRow row0 = outStkTbl.Rows[0];

                        SqlComm = $"SELECT * FROM WP_AcctInDtl WHERE isDel='N' AND OutStkId='{row0["OutStkId"]}'";
                        DataTable accInTbl = getTbl.table("WP", SqlComm);

                        Stock stock = new Stock();
                        Label2.Text = $"<input type='hidden' id='sn' value='{sn}'>" +
                        $"<input type='hidden' id='isUpd' value='{(accInTbl.Rows.Count == 0 ? $"{row0["PosOutStkId"]}"=="" ? "Y" : "N1" : "N")}'>" +
                        $"<input type='hidden' id='minDate' value='{stock.MinDate()}' />" +
                        $"<input type='hidden' id='OutStkDate' value='{row0["OutStkDate"]:yyyyMMdd}' />" +
                        $"<input type='hidden' id='discnt' value='{(int.Parse($"{row0["discount"]:#0}") + int.Parse($"{row0["discountShare"]:#0}"))}' />" +
                        "<div class='outStk-input-main'>" +
                            "<div style='margin-bottom:10px;'>" +
                                $"<span style='font-size:18px;margin-left:1%;margin-right:45px;''><i class='fas fa-caret-square-right'></i> <b>銷貨單號︰</b>{row0["outStkId"]}</span>" +
                                "<span style='font-size:18px;margin-right:72px;'><i class='fas fa-caret-square-right'></i> <b>會　員︰</b>";
                                    SqlComm = $"SELECT * FROM WP_Member ORDER BY memId";
                                    DataTable memTbl = getTbl.table("WP", SqlComm);
                                    foreach (DataRow row in memTbl.Rows)
                                    {
                                        Label2.Text += row["sn"].ToString() == row0["memSn"].ToString() ? $"{row["memName"]}" : "";
                                    }
                                Label2.Text += "</span>" +
                                "<button class='btn-admin btn-del' id='btn-outStk-del'><i class='fas fa-trash'></i> 刪除整筆</button>" +
                                "<span class='notice-txt' style='margin-left:135px;' id='notice-upd'></span>" +
                            "</div>" +
                            "<div style='margin-bottom:10px;'>" +
                                $"<span style='font-size:18px;margin-left:1%;margin-right:72px;'><i class='fas fa-caret-square-right'></i> <b>銷貨日期︰</b>{row0["OutStkDate"]:yyyy/MM/dd}</span>" +
                                "<span style='font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 發票號碼︰</span>" +
                                $"<input type='text' class='form-control' style='margin-right:45px;' id='reciptNo' name='reciptNo' size='20' maxlength='50' value='{row0["reciptNo"]}' />" +
                            "</div>" +
                            "<div style='display:flex;align-items:center;'>" +
                                $"<span style='font-size:18px;margin-left:1%;'><i class='fas fa-caret-square-right'></i> <b>付款方式︰</b></span>" +
                                $"{new PayKind().HTML2($"{row0["PayList"]}")}<span class='notice-txt' style='margin-left:40px;' id='notice-pay-upd'></span>" +
                            "</div>" +
                        "</div>";

                        double tmpDouble;
                        string dtlAmt;
                        int qty, recNo = 0;
                        foreach (DataRow row in outStkTbl.Rows)
                        {
                            dtlAmt = double.TryParse($"{row["dtlAmt"]}", out tmpDouble) ? tmpDouble.ToString(pointRule) : "0";
                            qty = int.Parse($"{row["qty"]}");    //數量為整數

                            Label3.Text += $"<div class='pd-row' data-id='{row["pNo"]}' data-sn='{row["dtlSn"]}' data-back='{(qty < 0 ? "true" : "false")}'>" +
                                "<div class='pNameS'>" +
                                    $"{(qty < 0 ? "<span class='back-dot'>退</span>" : "")}" +
                                    $"{row["pNameS"]}({(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())})" +
                                "</div>" +
                                "<div class='edit-single-main dtlAmt' data-id='dtlAmt'>" +
                                    $"<div class='edit-tag' style='justify-content:flex-end;'><i class='fas fa-edit btn-admin'></i><span>{dtlAmt}</span></div>" +
                                    "<div class='edit-zone' style='display: none;'>" +
                                        $"<input type='text' class='form-control edit-txt edit-dtlAmt chk-input' data-func='real_number' data-id='{dtlAmt}' maxlength='10' style='width:70px;' value='{dtlAmt}'>" +
                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                        "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                    "</div>" +
                                "</div>" +
                                "<div class='edit-single-main pd-qty' data-id='pdQty'>" +
                                    $"<div class='edit-tag' style='justify-content:flex-end;'><i class='fas fa-edit btn-admin'></i><span>{qty}</span></div>" +
                                    "<div class='edit-zone' style='display: none;'>" +
                                        $"<input type='text' class='form-control edit-txt edit-qty chk-input' data-func='number' data-id='{Math.Abs(qty)}' maxlength='6' style='width:70px;' value='{Math.Abs(qty)}'>" +
                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                        "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                    "</div>" +
                                "</div>" +
                                $"<div class='pUName'>{row["pUName"]}</div>" +
                                $"<div class='pd-discnt'>{(-1) * (int.Parse($"{row["dtlDiscnt"]:#0}") + int.Parse($"{row["dtlDiscntShare"]:#0}"))}</div>" +
                                $"<div class='pd-sub-total'>{row["pUName"]}</div>" +
                                $"<div class='txt-center isTax'>{(row["dtlIsTax"].ToString() == "Y" ? "應稅" : "免稅")}</div>" +
                                $"<div class='txt-center'><input class='pdLimitDate' {(qty < 0 ? "" : "style='display:none;'")}  value='{row["pdLimitDate"]:yyyy/MM/dd}' readonly data-id='{recNo}' id='pdLimitDate_{recNo}' /></div>" +
                                "<div class='txt-center'><button class='btn-admin btn-del pd-del'><i class='fas fa-trash'></i>刪除</button></div>" +
                            "</div>";
                            recNo++;
                        }
                    }
                }
            }
        }
    }
}