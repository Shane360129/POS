using System;
using System.Data;
using static Global;

public partial class prg9001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "9001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_EmpGrp WHERE empGrpId NOT IN ('admin', '0000') AND isStop ='N' ORDER BY empGrpId";
                var empGrpTbl = getTbl.table("WP", SqlComm);
                Label1.Text += "<div style='display:flex;align-items:top;padding-top:10px;'>" +
                    "<div class='empGp-node-main' style='width:50%;padding:0 10px;'>" +
                        "<div class='prg-subtitle' style='border-bottom:2px #797979 dotted;'><i class='fas fa-arrow-alt-circle-down'></i> 角色群組</div>";
                        if (empGrpTbl.Rows.Count == 0)
                        {
                            Label1.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無角色群組！</div>";
                        }
                        else
                        {
                            foreach (DataRow row in empGrpTbl.Rows)
                            {
                                Label1.Text += $"<div class='empGp-name-main empGp-row' data-id='{row["empGrpId"]}'>" +
                                    $"<span class='row-id'>{row["empGrpId"]}</span><div class='edit-tag edit-btn'><i class='fas fa-edit btn-admin'></i> {row["empGrpName"]}</div>" +
                                    $"<div class='edit-zone empGp-input-main empGp-input-main-{row["empGrpId"]}'>" +
                                        $"<input type='hidden' name='empGrpId' value='{row["empGrpId"]}' />" +
                                        $"<input type='text' class='empGp-input empGp-name' name='empGrpName' value='{row["empGrpName"]}' data-id='{row["empGrpName"]}' data-alt='MI_分類名稱' maxlength='80' />" +
                                        "<button class='btn-abort edit-btn empGp-upd-btn' data-id='abort'><i class='fas fa-times-circle'></i> 放棄</button>" +
                                        "<button class='btn-submit edit-btn empGp-upd-btn btn-admin' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                        "<button class='btn-del edit-btn empGp-upd-btn btn-admin' data-id='del'><i class='fas fa-trash'></i>刪除</button>" +
                                    "</div>" +
                                "</div>";
                            }
                        }
                    Label1.Text += "</div>" +
                    "<div style='width:50%;display:inline;'>" +
                        "<div class='prg-subtitle' style='padding-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 新增角色群組</div>" +
                        "<div style='border:1px #9e9e9e solid;border-radius:3px;padding:10px;margin-top:5px;'>" +
                            "<div class='add-main empGp-add-main'>" +
                                "<div>" +
                                    "<span>群組編號︰</span><input type='text' name='empGrpId' data-alt='MI_角色群組編號' maxlength='4' style='width:55px;'>" +
                                "</div>" +
                                "<div style='margin-top:5px;'>" +
                                    "<span style=''>名　　稱︰</span><input type='text' name='empGrpName' maxlength='20' data-alt='MI_角色群組名稱' style=''>" +
                                    "<button style='margin-left:10px;' class='empGp-add-btn btn-admin' data-id='empGp-add-main'><i class='fas fa-check-circle'></i> 確定送出</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                "</div>";
            }
        }
    }
}