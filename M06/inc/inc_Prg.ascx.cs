using System;
using System.Data;
using static Global;

public partial class inc_inc_Prg : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Employee emp = new Employee();
            if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
            else
            {
                string EmpName = stringEncrypt.AesDecryptBase64(cookies.Read("EmpName"), CryptoKey);
                if (emp.EmpPrgIdGrp != "" || emp.EmpId == "9999")
                {
                    string SqlComm = $"SELECT * FROM WP_PrgMenu WHERE Exist='Y' AND prgId<>'0000'{(emp.EmpId == "9999" ? "" : $" AND ( prgType='NODE' OR prgId IN ('{emp.EmpPrgIdGrp.Replace(",", "','")}') )")}";
                    DataTable prgTbl = getTbl.table("WP", SqlComm);

                    string lblTxt1 = "", prePrgType = "";
                    foreach (DataRow row in prgTbl.Rows)
                    {
                        if (row["prgType"].ToString() == "NODE")
                        {
                            Label1.Text += $"{(lblTxt1 == "" ? prePrgType == "NODE" ? "</div></div>" : "" : $"{lblTxt1}</div></div>")}" +
                            "<div class='pageL-node'>" +
                                $"<div class='pageL-title'>{row["prgId"]} {row["prgName"]}</div>" +
                                "<div class='pageL-detail'>";
                            lblTxt1 = "";
                        }
                        else
                        {
                            lblTxt1 += $"<a href='{row["prgUrl"]}'><div class='detail-link'>{row["prgId"]} {row["prgName"]}</div></a>";
                        }
                        prePrgType = row["prgType"].ToString();
                    }
                    Label1.Text += $"{lblTxt1}</div></div>";
                }
                Label1.Text += "<div class='pageL-title'>其 他</div>" +
                "<div class='pageL-detail'>" +
                    "<a href='javascript:empLogout();'><div class='detail-link'><i class='fas fa-sign-out-alt'></i>後台登出</div></a>" +
                "</div>";

                Label1.Text = "<div>" +
                    $"<div style='font-weight:bold'>使用者︰{EmpName}</div>" +
                    $"{Label1.Text}" +
                "</div>";
            }
        }
    }
}