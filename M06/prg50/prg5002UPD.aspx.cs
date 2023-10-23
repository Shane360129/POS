using System;
using System.Data;
using System.Net;
using System.Text.RegularExpressions;
using static Global;

public partial class prg5002UPD : System.Web.UI.Page
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
                string sn = Request.QueryString["sn"];
                if (string.IsNullOrEmpty(sn)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷銷帳序號只能0-9所組成
                    if (!reg1.IsMatch(sn)) { Response.Redirect("/prg50/prg5002.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_vAcctOut WHERE isDel='N' AND dtlIsDel='N' AND sn={sn}";
                    DataTable acctOutTbl = getTbl.table("WP", SqlComm);
                    if (acctOutTbl.Rows.Count == 0) { Response.Redirect("/prg50/prg5002.aspx"); }
                    else
                    {
                        ToDouble toDouble = new ToDouble();

                        DataRow prgRow0 = emp.PrgTbl.Rows[0];
                        Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                        $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 銷帳單內容</div>";

                        DataRow row0 = acctOutTbl.Rows[0];
                        Label2.Text = $"<input type='hidden' id='sn' value='{sn}'>" +
                        "<div style='border:1px #000 solid;border-radius:3px;padding:10px 0;'>" +
                            "<div style='margin-bottom:10px;'>" +
                                $"<span style='font-weight:bold;font-size:18px;margin-left:1%;'><i class='fas fa-caret-square-right'></i> 銷帳單號︰{row0["acctOutId"]}</span>" +
                                "<button style='margin-left:45px;' class='btn-admin btn-del' id='btn-acctOut-del'><i class='fas fa-trash'></i> 刪除整筆</button>" +
                            "</div>" +
                            $"<span style='font-weight:bold;font-size:18px;margin-left:1%;'><i class='fas fa-caret-square-right'></i> 銷帳日期︰{row0["acctOutDate"]:yyyy/MM/dd}</span>" +
                            $"<span style='margin-left:73px;font-weight:bold;font-size:18px;'><i class='fas fa-caret-square-right'></i> 廠　　商︰{row0["pvId"]}-{row0["pvNameS"]}" +
                            "</span>" +
                        "</div>" +
                        "<table class='list-main inStk-list-main' style='border-spacing:1px;'>" +
                            "<tr class='list-title'><td>銷帳單號</td><td>銷帳日期</td><td>廠商</td><td>進貨單號</td><td>商品</td><td style='text-align:right'>進貨金額</td><td style='text-align:right'>銷帳金額</td><td style='text-align:right'>未銷金額</td><td style='text-align:center'>銷貨狀態</td></tr>";
                            string preId = "", main_data_id = "", preInStkId = "", detail_data_id = "";
                            foreach (DataRow row in acctOutTbl.Rows)
                            {
                                main_data_id = preId != row["sn"].ToString() ? $"{row["sn"]}" : "";
                                detail_data_id = main_data_id == "" ? preInStkId != row["InStkId"].ToString() ? $"{row["InStkId"]}" : "" : $"{row["InStkId"]}";
                                double dtlAmt = toDouble.Numer(double.Parse($"{row["dtlAmt"]}"), 2);

                                Label2.Text += "<tr>" +
                                    $"<td class='row-main' data-id='{main_data_id}'>{row["acctOutId"]}</div>" +
                                    $"<td class='row-main' data-id='{main_data_id}'>{row["acctOutDate"]:yyyy/MM/dd}</div>" +
                                    $"<td class='row-main' data-id='{main_data_id}'>{row["pvId"]}-{row["pvNameS"]}</td>" +
                                    $"<td class='row-main' data-id='{detail_data_id}'>{row["InStkId"]}</td>" +
                                    $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}-{row["pNameS"]}</td>" +
                                    $"<td style='text-align:right'>{row["inStkAmtTotal"]:#0.00}</td>" +
                                    $"<td style='text-align:right' class='inStkAmtTotal'>{row["amtTotal"]:#0.00}</td>" +
                                    $"<td style='text-align:right'>{row["dtlPayLeft"]:#0.00}</td>" +
                                    $"<td style='text-align:center'>{stock.PayTypeName(row["dtlPayType"].ToString())}</td>" +
                                "</tr>";
                                preId = row["sn"].ToString();
                                preInStkId = row["InStkId"].ToString();
                            }
                            Label2.Text += "<tr><td colspan='7' style='color:#080;font-weight:bold;text-align:right;border-top:1px solid;'>共計︰<span style='font-style:inherit;color:inherit' id='acctOut-total'></span></td><td colspan=2 style='border-top:1px #080 solid;'></td></tr>" +
                        "</table>";
                    }
                }
            }
        }
    }
}