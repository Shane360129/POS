using System;
using System.Data;
using static Global;

public partial class prg2001Add : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "2001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                string SqlComm;
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 新增商品</div>";

                string PKAdd_HTML = "<div class='pk-select-grp'>" +
                    "<SELECT class='form-control PKind PKLPd' data-id='PKL' data-alt='0'><OPTION value='0'>請選擇</OPTION></SELECT>" +
                    "<SELECT class='form-control PKind PKMPd' data-id='PKM' style='display:none'></SELECT>" +
                    "<SELECT class='form-control PKind PKSPd' data-id='PKS' style='display:none'></SELECT>" +
                    "<button class='btn-del btn-PK btn-PK-del'><i class='fas fa-trash'></i> 刪除</button>" +
                "</div>";
                Label2.Text = $"<input type='hidden' id='pvType' value='{pvType}' />" + 
                "<div class='pk-main'>" +
                    "<div style='display:inline-flex;align-items:flex-start;'>" +
                        $"<div class='pk-grp-main'>{PKAdd_HTML}</div>" +
                        "<div class='btnGrp'>" +
                            "<button class='btn-submit btn-PK btn-PK-add'><i class='fas fa-check-circle'></i> 新增一筆</button>" +
                        "</div>" +
                    "</div>" +
                    $"<div style='display:none;' id='append-pkind' >{PKAdd_HTML}</div>" +
                    $"<input type='hidden' id='pkind-json' />" +
                "</div>";

                if (pvType == "1")
                {    //下拉選單
                    SqlComm = $"SELECT * FROM WP_Provider ORDER BY pvId";
                    DataTable pvTbl = getTbl.table("WP", SqlComm);
                    Label3.Text = "<select name='pvSn' class='form-control' data-alt='MI_廠商'><option value='0'>請選擇</option>";
                        foreach(DataRow row in pvTbl.Rows)
                        {
                            Label3.Text += $"<option value='{row["sn"]}'>{row["pvNameS"]}</option>";
                        }
                    Label3.Text += "</select>";
                }
                else    //autocomplete
                {
                    Label3.Text += "<div class='del-txt-group pv-txt-group' style='margin-right:65px;'>" +
                        $"<input type='text' class='form-control del-txt-input' placeholder='請輸入廠商名稱或代號' maxlength='42' style='width:266px;' id='pv-filter' />" +
                        "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                        $"<input type='hidden' name='pvSn' id='act-pvSn' />" +
                    "</div>";
                }

                SqlComm = $"SELECT * FROM WP_PdUnit WHERE pUExist ='Y' ORDER BY pUName";
                DataTable pUTbl = getTbl.table("WP", SqlComm);
                Label4.Text = "<select name='pUnit' class='form-control' data-alt='MI_商品單位'><option value='0'>請選擇</option>";
                foreach(DataRow row in pUTbl.Rows)
                {
                    Label4.Text += $"<option value='{row["pUId"]}'>{row["pUName"]}</option>";
                }
                Label4.Text += "</select>";
            }
        }
    }
}