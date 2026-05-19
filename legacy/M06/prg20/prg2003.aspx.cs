using System;
using System.Data;
using static Global;

public partial class prg2003 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Stock stock = new Stock();
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2003";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_vMember ORDER BY sn";
                DataTable memTbl = getTbl.table("WP", SqlComm);
                if (memTbl.Rows.Count == 0)
                {
                    Label2.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無會員！</div>";
                }
                else
                {
                    Label2.Text += "<table class='mem-list-main'>" +
                        "<tr class='mem-list-title'><td>會員名稱</td><td>類別</td><td>手機</td><td>市內電話</td><td>預設售價</td><td>狀態</td></tr>";
                        foreach (DataRow row in memTbl.Rows)
                        {
                            Label2.Text += "<tr>" +
                                $"<td><a href='/prg20/prg2003Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["memName"]}</td>" +
                                $"<td>{row["memKName"]}</td>" +
                                $"<td>{row["memMobil"]}</td>" +
                                $"<td>{row["memTel"]}</td>" +
                                $"<td>{stock.PriceName(row["priceKind"].ToString())}</td>" +
                                $"<td>{(row["isStop"].ToString() == "N" ? "正常往來" : "停止往來")}</td>" +
                            "</tr>";
                        }
                    Label2.Text += "</table>";
                }
            }
        }
    }
}