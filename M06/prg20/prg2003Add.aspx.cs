using System;
using System.Data;
using static Global;

public partial class prg2003Add : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
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
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 新增會員</div>";

                string SqlComm = $"SELECT * FROM WP_MemKind WHERE memKExist ='Y' ORDER BY memKId";
                DataTable pvTbl = getTbl.table("WP", SqlComm);
                Label2.Text = $"<select name='memKId' class='form-control' dat-alt='MI_會員分類'>";
                foreach (DataRow row in pvTbl.Rows)
                {
                    Label2.Text += $"<option value='{row["memKId"]}'>{row["memKId"]} {row["memKName"]}</option>";
                }
                Label2.Text += "</select>";
            }
        }
    }
}