using System;
using System.Data;
using static Global;

public partial class prg9002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "9002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_EmpGrp WHERE isStop='N' ORDER BY empGrpId";
                var empGrpTbl = getTbl.table("WP", SqlComm);
                string empPrgGrp = "";
                Label1.Text += $"<div style='border-bottom:1px #080 dashed;padding-bottom:15px;width:98%;margin:0 auto;margin-bottom:15px;'><span style='font-size:17px;font-weight:bold;'>角色群組︰</span>" +
                    $"<select class='emp-group' name='empGrpId'>";
                        foreach(DataRow row in empGrpTbl.Select($"empGrpId NOT IN ('admin'{(emp.EmpGrpId == "admin" ? "" : ", '0000'")})"))
                        {
                            Label1.Text += $"<option value='{row["empGrpId"]}'>{row["empGrpName"]}</option>";
                            empPrgGrp += $"<div class='emp-prg-group' data-id='{row["empGrpId"]}'>{row["empPrgIdGrp"]}</div>";
                        }
                    Label1.Text += $"</select>" +
                "</div>" +
                $"{empPrgGrp}" +
                "<input type='hidden' name='empPrgIdGrp' class='prg-id-grp' />";

                DataRow[] empGrpRows = empGrpTbl.Select($"empGrpId='{emp.EmpGrpId}'");
                string prgIdGrp = $"'{$"{empGrpRows[0]["empPrgIdGrp"]}".Replace(",", "','")}'";
                SqlComm = $"SELECT * FROM WP_PrgMenu WHERE prgId<>'0000' AND Exist='Y' {(emp.EmpGrpId == "admin" ? "" : $"AND prgId IN ({prgIdGrp})")} OR prgType='NODE' ORDER BY prgId";
                var menuTbl = getTbl.table("WP", SqlComm);
                string treeOrgnl = "", preNode = "";
                foreach(DataRow row in menuTbl.Rows)
                {
                    treeOrgnl += row["prgType"].ToString() == "NODE"
                        ? $"{(preNode != row["prgId"].ToString() ? preNode != "" ? "</div></div>": "" : "")}" +
                          $"<div class='node-main' data-id='{row["prgId"]}'>" +
                              "<div class='tree-row prg-node'>" +
                                  $"<input type='checkbox' class='ckbox node-ckbox' name='node-checkbox' id='orgnl-{row["prgId"]}' />" +
                                  $"<label for='orgnl-{row["prgId"]}'>{row["prgId"]} {row["prgName"]}</label>" +
                              "</div>" +
                              $"<div class='prg-sub-main'>"
                        : "<div class='tree-row prg-row'>" +
                              $"<input type='checkbox' class='ckbox prg-ckbox' name='prg-checkbox' id='orgnl-{row["prgId"]}' data-id='{row["prgId"]}' />" +
                              $"<label for='orgnl-{row["prgId"]}'>{row["prgId"]} {row["prgName"]}</label>" +
                          "</div>";

                    preNode = row["prgType"].ToString() == "NODE" ? row["prgId"].ToString() : preNode;
                }
                treeOrgnl += treeOrgnl == "" ? "" : "</div></div>";
                Label2.Text = treeOrgnl;
                Label3.Text = treeOrgnl.Replace("orgnl-", "target-");
            }
        }
    }
}