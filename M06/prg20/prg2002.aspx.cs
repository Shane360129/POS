using System;
using System.Data;
using static Global;

public partial class prg2002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_vProvider ORDER BY pvId";
                DataTable pKPdTbl = getTbl.table("WP", SqlComm);
                if (pKPdTbl.Rows.Count == 0)
                {
                    Label2.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無廠商！</div>";
                }
                else
                {
                    Label2.Text += "<table class='pv-list-main'>" +
                        "<tr class='pv-list-title'><td>廠商名稱</td><td>類別</td><td>統一編號</td><td>負責人</td><td>聯絡人</td><td>聯絡電話</td><td>傳真</td></tr>";
                    foreach (DataRow row in pKPdTbl.Rows)
                    {
                        Label2.Text += $"<tr>" +
                            $"<td><a href='/prg20/prg2002Upd.aspx?sn={row["sn"]}' class='link-tag'><i class='far fa-hand-point-up'></i>{row["pvId"]}-{row["pvName"]}</td>" +
                            $"<td>{row["pvKName"]}</td>" +
                            $"<td>{row["taxId"]}</td>" +
                            $"<td>{row["pvBoss"]}</td>" +
                            $"<td>{row["ctactName"]}</td>" +
                            $"<td>{row["ctactTel"]}</td>" +
                            $"<td>{row["fax"]}</td>" +
                        "</tr>";
                    }
                    Label2.Text += "</table>";
                }
            }
        }
    }
}