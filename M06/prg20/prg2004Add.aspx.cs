using System;
using System.Data;
using static Global;

public partial class prg2004Add : System.Web.UI.Page
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
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 新增帳號</div>";

                string SqlComm = $"SELECT * FROM WP_EmpGrp WHERE isStop ='N' AND empGrpId NOT IN ('admin','0000') ORDER BY empGrpId";
                DataTable empGrpTbl = getTbl.table("WP", SqlComm);
                Label2.Text = $"<select name='empGrpId' class='form-control' dat-alt='MI_群組'>";
                foreach (DataRow row in empGrpTbl.Rows)
                {
                    Label2.Text += $"<option value='{row["empGrpId"]}'>{row["empGrpId"]} {row["empGrpName"]}</option>";
                }
                Label2.Text += "</select>";
            }
        }
    }
}