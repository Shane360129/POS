using System;
using System.Data;
using static Global;

public partial class prg2004 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2004";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_vEmployee WHERE empGrpId NOT IN ('admin','0000') ORDER BY sn";
                DataTable memTbl = getTbl.table("WP", SqlComm);
                if (memTbl.Rows.Count == 0)
                {
                    Label2.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無帳號！</div>";
                }
                else
                {
                    Label2.Text += "<table class='emp-list-main'>" +
                        "<tr class='emp-list-title'><td>登入帳號</td><td>登入名稱</td><td>是否已停用</td><td>群組別</td><td>備註</td></tr>";
                        foreach (DataRow row in memTbl.Rows)
                        {
                            Label2.Text += "<tr>" +
                                $"<td><a href='/prg20/prg2004Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i> {row["empId"]}</td>" +
                                $"<td>{row["empName"]}</td>" +
                                $"<td>{(row["isStop"].ToString() == "N" ? "否" : "是")}</td>" +
                                $"<td>{row["empGrpName"]}</td>" +
                                $"<td>{row["memo"]}</td>" +
                            "</tr>";
                        }
                    Label2.Text += "</table>";
                }
            }
        }
    }
}