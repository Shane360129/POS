using System;
using System.Data;
using System.Linq;
using System.Net;
using static Global;

public partial class prg2001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Stock stock = new Stock();
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>" +
                $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                string isSale = cookies.Read("isSale");
                isSale = isSale == "" ? "0" : isSale;

                string SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo NOT IN (SELECT pNo FROM WP_vPdKindProd WHERE pKExist ='Y') " +
                    $"{(isSale=="" ? "" : $"AND isSale='{isSale}'")} ORDER BY pNo";
                DataTable pdTbl = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT * FROM WP_vPdKindProd WHERE pKExist ='Y'{(isSale == "ALL" ? "" : $"AND isSale='{isSale}'")} ORDER BY pKLId, pKMId, pKSId";
                DataTable pKPdTbl = getTbl.table("WP", SqlComm);

                string thisPKind = "", prePKind = "";
                Label2.Text = "<div style='margin-bottom:0;'>" +
                    "<div class='prg-subtitle' style='width:50%;display:inline-block;'><i class='fas fa-arrow-alt-circle-down'></i> 搜尋條件</div>" +
                    "<div class='btn-admin' style='width:50%;display:inline-block;text-align:right;'><button class='btn-submit' id='btn-pd-add'><i class='fas fa-plus-circle'></i> 新增商品</button></div>" +
                "</div>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            "<span class='input-title'>銷售狀態</span>" +
                            $"<select id='isSale' class='form-control txt-input'>{stock.IsSaleOptionHTML(isSale, "Y")}</select>" +
                        "</div>" +
                    "</div>" +
                    "<div class='page-submit'><button class='btn-submit' id='btn-search'><i class='fas fa-search'></i> 查詢</button></div>" +
                "</div>";
                Label2.Text += $"<div style='display:flex'>" +
                    "<div class='prg-subtitle' style='width:50%;'><i class='fas fa-arrow-alt-circle-down'></i> 商品資料</div>" +
                    "<div style='width:50%;text-align:right;padding-right:10px;font-size:18px;'>" +
                        "<i class='fas fa-folder node-all-btn' data-alt='close' title='全部關閉' style='margin-right:8px;'></i>" +
                        "<i class='fas fa-folder-open node-all-btn' data-alt='open' title='全部打開'></i>" +
                    "</div>" +
                "</div>" +
                "<table class='pd-list-main'>" +
                    "<tr class='pd-list-title'><td>商品名稱</td><td>條碼</td><td>廠商</td><td>稅別</td><td class='align-r'>標準售價</td><td class='align-r'>標準成本</td><td class='align-r'>目前庫存</td><td class='align-c'>商品狀態</td></tr>";

                    if (pdTbl.Rows.Count == 0 && pKPdTbl.Rows.Count == 0)
                    {
                        Label2.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無商品！</div>";
                    }
                    else
                    {
                        double costStd;
                        if (pdTbl.Rows.Count > 0)
                        {
                            Label2.Text += "<tr><td colspan=8 style='color:#f00'>▼未分類商品▼</td></tr>";
                            foreach (DataRow row in pdTbl.Rows)
                            {
                                costStd = double.Parse($"{row["costStd"]}");
                                Label2.Text += $"<tr class='node-detail'>" +
                                    $"<td style='padding-left:30px;'><a href='/prg20/prg2001Upd.aspx?PNo={row["PNo"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["PName"]}</td>" +
                                    $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}</td>" +
                                    $"<td>{row["pvId"]}-{row["pvNameS"]}</td>" +
                                    $"<td>{(row["isTax"].ToString() == "Y" ? "應稅" : "免稅")}</td>" +
                                    $"<td class='align-r'>{row["priceStd"]}</td>" +
                                    $"<td class='align-r'>{threeDot.To3Dot(costStd.ToString(pointRule))}</td>" +
                                    $"<td class='align-r'>{row["qtyNow"]}</td>" +
                                    $"<td class='align-c'>{stock.IsSaleName(row["isSale"].ToString())}</td>" +
                                "</tr>";
                            }
                            Label2.Text += "<tr><td colspan=8 style='color:#f00'>▲未分類商品▲</td></tr>";
                        }

                        foreach (DataRow row in pKPdTbl.Rows)
                        {
                            costStd = double.Parse($"{row["costStd"]}");
                            thisPKind = $"{row["pKLId"]}-{row["pKMId"]}-{row["pKSId"]}";
                            Label2.Text += prePKind != thisPKind ? $"<tr>" +
                                $"<td colspan='8' style='font-weight:bold' class='node-main' data-id='{thisPKind}'>" +
                                    $"<i class='fas fa-caret-square-right node-btn node-right' data-alt='open'></i>" +
                                    $"<i class='fas fa-caret-square-down node-btn node-down' data-alt='close'></i> " +
                                    $"{row["pKLName"]}" +
                                    $"{(row["PKMId"].ToString() != "0" ? $" <i class='fas fa-angle-right'></i> {row["PKMName"]}{(row["PKSId"].ToString() != "0" ? $" <i class='fas fa-angle-right'></i> {row["PKSName"]}" : "")}" : "")}" +
                                "</td>" +
                            "</tr>" : "";
                            Label2.Text += $"<tr class='node-detail' data-id='{thisPKind}'>" +
                                $"<td style='padding-left:30px;'><a href='/prg20/prg2001Upd.aspx?PNo={row["PNo"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["PName"]}</td>" +
                                $"<td>{(row["pBarcode"].ToString() == "" ? row["pCode"].ToString() : row["pBarcode"].ToString())}</td>" +
                                $"<td>{row["pvId"]}-{row["pvNameS"]}</td>" +
                                $"<td>{(row["isTax"].ToString() == "Y" ? "應稅" : "免稅")}</td>" +
                                $"<td class='align-r'>{row["priceStd"]}</td>" +
                                $"<td class='align-r'>{threeDot.To3Dot(costStd.ToString(pointRule))}</td>" +
                                $"<td class='align-r'>{row["qtyNow"]}</td>" +
                                $"<td class='align-c'>{stock.IsSaleName(row["isSale"].ToString())}</td>" +
                            "</tr>";
                            prePKind = thisPKind;
                        }
                    }
                Label2.Text += "</table>";
            }
        }
    }
}