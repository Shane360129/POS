using System;
using System.Data;
using static Global;

public partial class prg1002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "1002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_PvKind WHERE pvExist ='Y' ORDER BY pvKId";
                var pvkTbl = getTbl.table("WP", SqlComm);
                Label1.Text += "<div style='display:flex;align-items:top;padding-top:10px;'>" +
                    "<div class='pvk-node-main' style='width:50%;padding:0 10px;'>" +
                        "<div class='prg-subtitle' style='border-bottom:2px #797979 dotted'><i class='fas fa-arrow-alt-circle-down'></i> 廠商類別</div>";
                        if (pvkTbl.Rows.Count == 0)
                        {
                            Label1.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無類別！</div>";
                        }
                        else
                        {
                            foreach (DataRow row in pvkTbl.Rows)
                            {
                                Label1.Text += $"<div class='edit-single-main pvk-row' data-id='{row["pvKId"]}'>" +
                                    $"<span class='row-id'>{row["pvKId"]}</span><div class='edit-tag'><i class='fas fa-edit btn-admin'></i> {row["pvKName"]}</div>" +
                                    $"<div class='edit-zone'>" +
                                        $"<input type='hidden' name='pvKId' value='{row["pvKId"]}' />" +
                                        $"<input type='text' class='form-control edit-txt' name='pvKName' value='{row["pvKName"]}' data-id='{row["pvKName"]}' data-alt='MI_分類名稱' maxlength='80' />" +
                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                        "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                        $"<button class='btn-del edit-btn' data-id='del'><i class='fas fa-trash'></i>刪除</button>" +
                                    "</div>" +
                                "</div>";
                            }
                        }
                    Label1.Text += "</div>" +
                    "<div style='width:50%;display:inline;'>" +
                        "<div class='prg-subtitle' style='padding-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 新增廠商類別</div>" +
                        "<div style='border:1px #9e9e9e solid;border-radius:3px;padding:10px;margin-top:5px;'>" +
                            "<div class='add-main'>" +
                                "<div>" +
                                    "<span>類別編號︰</span><input type='text' class='form-control add-pvKId' name='pvKId' data-alt='MI_類別編號' maxlength='3' style='width:50px;'>" +
                                "</div>" +
                                "<div style='margin-top:5px;'>" +
                                    "<span style=''>名　　稱︰</span><input type='text' class='form-control' name='pvKName' maxlength='20' data-alt='MI_類別名稱' style=''>" +
                                    "<button style='margin-left:10px;' class='btn-submit pvk-add-btn btn-admin'><i class='fas fa-check-circle'></i> 確定送出</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                "</div>";
            }
        }
    }
}