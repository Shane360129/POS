using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg8001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "8001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_Product WHERE isSale IN ('0', '1', '2') ORDER BY pNo";
                DataTable pdTbl = getTbl.table("WP", SqlComm);
                int pageNo = 1;
                string rptrTitle = "<div style='width:100%;'><div class='rptr-title'>商品庫存盤點表</div>" +
                "<div class='rptr-data'>" +
                    $"<div style='text-align:left;width:50%;display:inline-block;'>頁次︰pageNo / <span class='page-total'></span></div>" +
                    $"<div style='text-align:right;width:50%;display:inline-block;'>列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
                "</div>",
                tblTitle = "<table class='list-main inStk-list-main' style='border-spacing:1px;width:99%;margin-right:1%;'>" +
                    "<tr class='list-title'>" +
                        "<td style='text-align:left'>商品代號</td><td>名稱</td><td>儲位</td><td>盤點庫存</td>" +
                    "</tr>";

                if (pdTbl.Rows.Count == 0)
                {
                    Label2.Text += $"{rptrTitle}{tblTitle}<tr><td colspan='4' class='empty-data'>查無資料！</td></tr></table></div>";
                }
                else
                {
                    Label2.Text = "<div class='rptr-main'>";
                        int recNo = 1, tblNo = 1;
                        foreach (DataRow row in pdTbl.Rows)
                        {
                            if (recNo % 22 == 1) {
                                Label2.Text += $"{(recNo == 1 ? "" : $"</table></td>")}";
                                if(tblNo % 2 == 1) {
                                    Label2.Text += tblNo == 1 ? "" : $"</tr></table></div></div><p style='page-break-after:always'></p>";
                                    Label2.Text+=$"{rptrTitle.Replace("pageNo", $"{pageNo}")}<div class='page-row'><table style='width:100%;'><tr>";
                                    pageNo ++;
                                }
                                Label2.Text += $"<td style='width:50%;vertical-align:top;'>{tblTitle}";
                                tblNo ++;
                            }
                            Label2.Text += "<tr class='tr-row'>" +
                                $"<td style='text-align:left'>{row["pBarcode"]}</td>" +
                                $"<td style='text-align:left'>{row["pNameS"]}</td>" +
                                $"<td></td>" +
                                $"<td></td>" +
                            "</tr>";
                            recNo++;
                        }
                        Label2.Text += "</table></td>" +
                    $"</tr></table></div></div></div><input type='hidden' id='page-total' value='{(pageNo-1)}' />" +
                    "<div style='' class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080'>列　印</button></div>";
                }
            }
        }
    }
}