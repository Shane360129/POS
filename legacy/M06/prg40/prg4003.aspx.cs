using System;
using System.Data;
using System.Linq;
using static Global;

public partial class prg4003 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "4003";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";
            
                string SqlComm = $"SELECT * FROM WP_OutStockPos ORDER BY OutStkId";
                DataTable oStkPosTbl = getTbl.table("WP", SqlComm);
                Label2.Text = $"<input type='hidden' id='isEmpty' value='{(oStkPosTbl.Rows.Count == 0 ? "Y" : "N")}' />" +
                "<div class='chk-main'>" +
                    "<div>" +
                        "<div class='chkoutY-main'>" +
                            $"<span><i class='fas fa-caret-square-right'></i> 開始接收前台資料</span>" +
                            "<button class='btn-admin btn-submit' id='btn-submit' style='margin-left:10px;'>確認送出</button>" +
                        "</div>" +
                        "<div id='processing' style='margin-left:10px;color:#080;'><img src='/images/loadingPix.gif' style='width:28px;margin-right:5px;'>處理中，請稍待！</div>" +
                    "</div>" +
                "</div>";
                    
            }
        }
    }
}