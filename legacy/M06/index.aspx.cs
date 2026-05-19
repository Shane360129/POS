using System;
using System.Data;
using static Global;

public partial class index : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp){ Response.Redirect("/Login.aspx"); }
        else
        {
            string EmpName = stringEncrypt.AesDecryptBase64(cookies.Read("EmpName"), CryptoKey);

            string SqlComm = $"SELECT TOP(2) * FROM WP_SysLog WHERE empId = '{emp.EmpId}' ORDER BY sn DESC";
            DataTable lastTimeTbl = getTbl.table("WP", SqlComm);
            Label1.Text = $"<div class='last-login'>{EmpName} 您好" +
                $"{(lastTimeTbl.Rows.Count < 2 ? "，初次見面！" : $"！您上次登入的時間︰{lastTimeTbl.Rows[1]["timeCreate"]}")}" +
            "</div>";
        }
    }
}