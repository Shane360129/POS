using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using static Global;

public partial class prg3002UPD : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "3002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                string sn = Request.QueryString["sn"];

                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷進貨序號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg30/prg3002.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND sn={sn}";
                    DataTable inStkTbl = getTbl.table("WP", SqlComm);
                    if (inStkTbl.Rows.Count == 0) { Response.Redirect("/prg30/prg3002.aspx"); }
                    else
                    {
                        ToDouble toDouble = new ToDouble();
                        DataRow prgRow0 = emp.PrgTbl.Rows[0];
                        Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                        $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 修改進貨資料</div>" +
                        $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                        string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                        DataRow row0 = inStkTbl.Rows[0];

                        Stock stock = new Stock();
                        Label2.Text = $"<input type='hidden' id='sn' value='{sn}'>" +
                        $"<input type='hidden' id='minDate' value='{stock.MinDate()}' />" +
                        $"<input type='hidden' id='InStkDate' value='{row0["InStkDate"]:yyyyMMdd}' />" +
                        "<div style='border:1px #000 solid;border-radius:3px;padding:10px 0;'>" +
                            "<div style='margin-bottom:10px;'>" +
                                $"<span style='font-size:18px;margin-left:1%;margin-right:46px;'><i class='fas fa-caret-square-right'></i> <b>進貨單號︰</b>{row0["inStkId"]}</span>" +
                                "<button class='btn-del btn-admin' id='btn-inStk-del'><i class='fas fa-trash'></i> 刪除整筆</button>" +
                                "<span id='notice-upd'></span>" +
                            "</div>" +
                            "<div style='margin-bottom:10px;'>" +
                                $"<span style='font-size:18px;margin-left:1%;'><i class='fas fa-caret-square-right'></i> <b>進貨日期︰</b>{row0["InStkDate"]:yyyy/MM/dd}</span>" +
                                $"<span style='margin-left:73px;font-size:18px;'><i class='fas fa-caret-square-right'></i> <b>廠　　商︰</b>{row0["pvName"]}</span>" +
                            "</div>" +
                            "<div>" +
                                "<span style='font-size:18px;margin-left:1%;'><i class='fas fa-caret-square-right'></i> <b>發票號碼︰</b></span>" +
                                $"<input type='text' class='form-control' style='margin-right:45px;' id='reciptNo' name='reciptNo' size='20' maxlength='50' value='{row0["reciptNo"]}' />" +
                            "</div>" +
                        "</div>";

                        double paid = 0;
                        string dtlAmt;
                        int qty, dtlAmtTotal, recNo = 0;
                        foreach (DataRow row in inStkTbl.Rows)
                        {
                            dtlAmt = $"{toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), pointQty).ToString(pointRule)}";
                            qty = int.Parse($"{row["qty"]}");    //數量為整數
                            dtlAmtTotal = int.Parse($"{row["amtTotal"]:#0}");    //小計為整數
                            paid = double.Parse($"{row["amtTotal"]}") - double.Parse($"{row["dtlPayLeft"]}");

                            Label3.Text += $"<div class='pd-row' data-id='{row["pNo"]}' data-sn='{row["dtlSn"]}' data-costInitl='{row["costInitial"]}' data-back='{(qty < 0 ? "true" : "false")}'>" +
                                "<div class='pNameS'>" +
                                    $"{(qty < 0 ? "<span class='back-dot'>退</span>" : "")}" +
                                    $"{row["pNameS"]}({(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())})" +
                                    $"{($"{row["pvSn"]}" == $"{row["pdPvSn"]}" ? "" : $"<span class='diff-pv' style='white-space:nowrap'>({row["pdPvNameS"]})</span>")}" +
                                "</div>" +
                                "<div class='edit-single-main costStd' data-id='costStd'>" +
                                    $"<div class='edit-tag' style='justify-content:flex-end;'><i class='fas fa-edit btn-admin'></i><span>{dtlAmt}</span></div>" +
                                    "<div class='edit-zone' style='display: none;'>" +
                                        $"<input type='text' class='form-control align-r edit-txt edit-costStd chk-input' data-func='real_number' data-alt='{dtlAmt}' maxlength='10' style='width:70px;' value='{dtlAmt}'>" +
                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                        "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                    "</div>" +
                                "</div>" +
                                "<div class='edit-single-main pd-qty'>" +
                                    $"<div class='edit-tag' style='justify-content:flex-end;'><i class='fas fa-edit btn-admin'></i><span>{qty}</span></div>" +
                                    "<div class='edit-zone' style='display: none;'>" +
                                        $"<input type='text' class='form-control align-r edit-txt edit-qty chk-input' data-func='number' data-alt='{Math.Abs(qty)}' maxlength='6' style='width:60px;' value='{Math.Abs(qty)}'>" +
                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                        "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                    "</div>" +
                                "</div>" +
                                $"<div class='pUName'>{row["pUName"]}</div>" +
                                "<div class='subTotal edit-single-main' data-id='subTotal' style='justify-content:flex-end;'>" +
                                    $"<div class='edit-tag'><i class='fas fa-edit btn-admin'></i><span>{dtlAmtTotal}</span></div>" +
                                    "<div class='edit-zone' style='display: none;'>" +
                                        $"<input type='text' class='form-control align-r edit-txt edit-subTotal chk-input' data-func='number' data-alt='{Math.Abs(dtlAmtTotal)}' maxlength='10' style='width:70px;' value='{Math.Abs(dtlAmtTotal)}'>" +
                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                        "<button class='btn-submit edit-btn btn-admin' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                    "</div>" +
                                "</div>" +
                                $"<div class='align-c isTax'>{(row["dtlIsTax"].ToString() == "Y" ? "應稅" : "免稅")}</div>" +
                                $"<div class='align-c payType'>{(row["dtlPayType"].ToString() == "0" ? "未結" : row["payType"].ToString() == "1" ? "未結完" : "全結")}</div>" +
                                $"<div class='payLeft' data-paid='{paid}'>{row["dtlPayLeft"]:#0}</div>" +
                                $"<div class='align-c'><input type='text' class='form-control open-datepicker pdLimitDate' readonly data-id='{recNo}' value='{row["pdLimitDate"]:yyyy/MM/dd}' /></div>" +
                                "<div class='align-c'><button class='btn-del btn-admin pd-del'><i class='fas fa-trash'></i> 刪除</button></div>" +
                            "</div>";
                            recNo++;
                        }
                    }
                }
            }
        }
    }
}