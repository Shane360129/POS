using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using static Global;

public partial class prg2001Upd : System.Web.UI.Page
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
                string PNo = Request.QueryString["PNo"];

                if (string.IsNullOrEmpty(PNo)) { Response.Redirect("/index.aspx"); }
                else
                {
                    Regex reg1 = new Regex(@"^[\d]+$");      //判斷商品分類編號只能0-9所組成
                    if (!reg1.IsMatch(PNo)) { Response.Redirect("/prg20/prg2001.aspx"); }
                    string SqlComm = $"SELECT * FROM WP_Product WHERE PNo={PNo}";
                    DataTable pdTbl = getTbl.table("WP", SqlComm);

                    SqlComm = $"SELECT * FROM WP_Product WHERE (PNo={PNo})" +
                        $" AND (pNo NOT IN (SELECT pNo FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N'))" +
                        $" AND (pNo NOT IN (SELECT pNo FROM  WP_vOutStockUnion WHERE isDel='N' AND dtlIsDel='N'))";
                    DataTable ioStkTbl = getTbl.table("WP", SqlComm);

                    if (pdTbl.Rows.Count == 0) { Response.Redirect("/prg20/prg2001.aspx"); }
                    else
                    {
                        DataRow prgRow0 = emp.PrgTbl.Rows[0];
                        Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                        $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]} <i class='fas fa-angle-right'></i> 修改商品</div>" +
                        $"<input type='hidden' id='pointQty' value='{pointQty}' />";
                        string pointRule = $"#0.{string.Concat(Enumerable.Repeat("0", pointQty))}";

                        DataRow row0 = pdTbl.Rows[0];
                        Label2.Text = $"<input type='hidden' id='pvType' value='{pvType}' />" +
                        "<div class='page-main pd-upd-main'>" +
                            $"<input type='hidden' name='pNo' value='{PNo}' />" +
                            $"<input type='hidden' id='is-upd-stk' value='{(ioStkTbl.Rows.Count > 0 ? "Y" : "N")}' />" +
                            "<div class='input-row'>" +
                                "<div class='input-title'>名　　稱</div>" +
                                "<div class='input-text'>" +
                                    "<span class='input-tag'>商品全名︰</span>" +
                                    $"<input type='text' name='pName' class='form-control' data-alt='MI_商品全名' maxlength='50' style='width:300px;margin-right:20px;' value='{row0["pName"]}' />" +
                                    "<span class='input-tag'>簡稱︰</span>" +
                                    $"<input type='text' name='pNameS' class='form-control' data-alt='MI_商品簡稱' maxlength='10' value='{row0["pNameS"]}' />" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row'>" +
                                "<div class='input-title'>條　　碼</div>" +
                                "<div class='input-text'>" +
                                    "<span class='input-tag'>國際條碼︰</span>" +
                                    $"<input type='text' name='pBarcode' class='form-control' maxlength='20' style='margin-right:115px;' value='{row0["pBarcode"]}' />" +
                                    "<span class='input-tag'>店內碼︰</span>" +
                                    $"<input type='text' name='pCode' class='form-control' maxlength='20' value='{row0["pCode"]}' />" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>分　　類</div>" +
                                "<div class='input-text'>" +
                                    "<div style='display:inline-flex;align-items:flex-start;'>" +
                                        "<div class='pk-grp-main'>";
                                            SqlComm = $"SELECT * FROM WP_PdKindProd WHERE pNo='{PNo}' AND pKExist='Y'";
                                            DataTable pkPdTbl = getTbl.table("WP", SqlComm);
                                            string PKAdd_HTML = "<div class='pk-select-grp'>" +
                                                "<SELECT class='form-control PKind PKLPd' data-id='PKL' data-alt='0'><OPTION value='0'>請選擇</OPTION></SELECT>" +
                                                "<SELECT class='form-control PKind PKMPd' data-id='PKM' data-alt='0' style='display:none'></SELECT>" +
                                                "<SELECT class='form-control PKind PKSPd' data-id='PKS' data-alt='0' style='display:none'></SELECT>" +
                                                "<button class='btn-del btn-PK btn-PK-del'><i class='fas fa-trash'></i> 刪除</button>" +
                                            "</div>";
                                            if (pkPdTbl.Rows.Count == 0)
                                            {
                                                Label2.Text += PKAdd_HTML;
                                            }
                                            else
                                            {
                                                foreach (DataRow row in pkPdTbl.Rows)
                                                {
                                                    Label2.Text += $"<div class='pk-select-grp'>" +
                                                        $"<SELECT class='form-control PKind PKLPd' data-id='PKL' data-alt='{row["PKLId"]}'></SELECT>" +
                                                        $"<SELECT class='form-control PKind PKMPd' data-id='PKM' data-alt='{row["PKMId"]}'></SELECT>" +
                                                        $"<SELECT class='form-control PKind PKSPd' data-id='PKS' data-alt='{row["PKSId"]}'></SELECT>" +
                                                        "<button class='btn-del btn-PK btn-PK-del'><i class='fas fa-trash'></i> 刪除</button>" +
                                                    "</div>";
                                                }
                                            }
                                        Label2.Text += "</div>" +
                                        "<div class='btnGrp'>" +
                                            "<button class='btn-submit btn-PK btn-PK-add'><i class='fas fa-check-circle'></i> 再新增一筆</button>" +
                                        "</div>" +
                                    "</div>" +
                                    $"<div style='display:none;' id='append-pkind' >{PKAdd_HTML}</div>" +
                                    $"<input type='hidden' id='pkind-json' />" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>廠　　商</div>" +
                                "<div class='input-text'>";
                                    string pvSn = $"{row0["pvSn"]}";
                                    SqlComm = $"SELECT * FROM WP_Provider ORDER BY pvId";
                                    DataTable pvTbl = getTbl.table("WP", SqlComm);
                                    if (pvType == "1")
                                    {    //下拉選單
                                        Label2.Text += "<select name='pvSn' class='form-control'>";
                                            foreach(DataRow row in pvTbl.Rows)
                                            {
                                                Label2.Text += $"<option value='{row["sn"]}' {(pvSn == $"{row["sn"]}" ? "selected" : "")}>{row["pvId"]}-{row["pvNameS"]}</option>";
                                            }                                    
                                        Label2.Text += "</select>";
                                    }
                                    else    //autocomplete
                                    {
                                        DataRow[] pvRows = pvTbl.Select($"sn={pvSn}");
                                        Label2.Text += "<div class='del-txt-group pv-txt-group' style='margin-right:65px;'>" +
                                            $"<input type='text' class='form-control del-txt-input' placeholder='請輸入廠商名稱或代號' maxlength='42' style='width:266px;' id='pv-filter'  value='{pvRows[0]["pvId"]}-{pvRows[0]["pvNameS"]}' />" +
                                            "<div class='del-txt-button'><i class='fas fa-times-circle'></i></div>" +
                                            $"<input type='hidden' name='pvSn' id='act-pvSn' value='{pvSn}' />" +
                                        "</div>";
                                    }
                                    
                                Label2.Text += "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>規　　格</div>" +
                                "<div class='input-text'>" +
                                    "<span class='input-tag'>商品單位︰</span>";
                                    SqlComm = $"SELECT * FROM WP_PdUnit WHERE pUExist ='Y' ORDER BY pUName";
                                    DataTable pUTbl = getTbl.table("WP", SqlComm);
                                    Label2.Text += "<select name='pUnit' class='form-control'><option value='0'>請選擇</option>";
                                    foreach (DataRow row in pUTbl.Rows)
                                    {
                                        Label2.Text += $"<option value='{row["pUId"]}' {(row["pUId"].ToString() == row0["pUnit"].ToString() ? "selected": "")}>{row["pUName"]}</option>";
                                    }
                                    Label2.Text += "</select>";

                                Label2.Text += "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>銷　　售</div>" +
                                "<div class='input-text'>" +
                                    "<select name='isSale' class='form-control' style='margin-right:88px;'>" +
                                        $"<option value='0' {(row0["isSale"].ToString() == "0" ? "selected" : "")}>正常進銷貨</option>" +
                                        $"<option value='1' {(row0["isSale"].ToString() == "1" ? "selected" : "")}>只停止進貨</option>" +
                                        $"<option value='2' {(row0["isSale"].ToString() == "2" ? "selected" : "")}>只停止銷貨</option>" +
                                        $"<option value='3' {(row0["isSale"].ToString() == "3" ? "selected" : "")}>停止進銷貨</option>" +
                                    "</select>" +
                                    "<span class='input-tag'>稅　　別︰</span>" +
                                    "<select name='isTax' class='form-control' style='margin-right:69px;'>" +
                                        $"<option value='Y' {(row0["isTax"].ToString() == "Y" ? "selected" : "")}>應稅</option>" +
                                        $"<option value='N' {(row0["isTax"].ToString() == "N" ? "selected" : "")}>免稅</option>" +
                                    "</select>" +
                                    "<span class='input-tag'>扣庫存︰</span>" +
                                    "<select name='isUpdStock' class='form-control' style='margin-right:51px;'>" +
                                        $"<option value='Y' {(row0["isUpdStock"].ToString() == "Y" ? "selected" : "")}>是</option>" +
                                        $"<option value='N' {(row0["isUpdStock"].ToString() == "N" ? "selected" : "")}>否</option>" +
                                    "</select>" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>價　　格</div>" +
                                "<div class='input-text'>" +
                                    "<span class='input-tag'>標準售價︰</span>" +
                                    $"<input type='text' name='priceStd' maxlength='5' class='form-control align-r chk-input' data-func='number' data-alt='MI_標準售價' style='width:72px;margin-right:35px;' value='{row0["priceStd"]}' />" +
                                    "<span class='input-tag'>最低應售價︰</span>" +
                                    $"<input type='text' name='priceLow' maxlength='5' class='form-control align-r chk-input' data-func='number' style='width:70px;margin-right:67px;' value='{row0["priceLow"]}' />" +
                                    "<span class='input-tag'>會員價︰</span>" +
                                    $"<input type='text' name='priceMem' maxlength='5' class='form-control align-r chk-input' data-func='number' style='width:70px;margin-right:65px;' value='{row0["priceMem"]}' />" +
                                    "<span class='input-tag'>大批價︰</span>" +
                                    $"<input type='text' name='priceBat' maxlength='5' class='form-control align-r chk-input' data-func='number' style='width:70px;' value='{row0["priceBat"]}' />" +
                                    $"{spriceUpd(PNo)}" +   //查詢變價
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>成　　本</div>" +
                                "<div class='input-text'>" +
                                    "<span class='input-tag'>標準成本︰</span>" +
                                    $"<input type='text' name='costStd' maxlength='10' class='form-control pd-cost align-r chk-input' data-func='real_number' data-alt='MI_標準成本' style='width:103px;margin-right:20px;' value='{double.Parse($"{row0["costStd"]}").ToString(pointRule)}' />" +
                                    "<span class='input-tag'>平均成本︰</span>" +
                                    $"<input type='text' name='costAvg' maxlength='10' class='form-control pd-cost align-r chk-input' data-func='real_number' style='width:103px;margin-right:17px;' value='{double.Parse($"{row0["costAvg"]}").ToString(pointRule)}' />" +
                                    "<span class='input-tag'>期初成本︰</span>" +
                                    $"<input type='text' name='costInitial' maxlength='10' class='form-control pd-cost align-r chk-input' data-func='real_number' style='width:103px;' value='{double.Parse($"{row0["costInitial"]}").ToString(pointRule)}'  />" +
                                    $"<span class='note note-cost'></span>" +
                                "</div>" +
                            "</div>" +
                            "<div class='input-row' style='align-items:flex-start;'>" +
                                "<div class='input-title'>庫　　存</div>" +
                                "<div class='input-text'>" +
                                    "<span class='input-tag'>目前庫存︰</span>" +
                                    $"<input type='text' name='qtyNow' maxlength='5' class='form-control pd-stk align-r chk-input' data-func='number' data-alt='目前庫存' style='width:70px;margin-right:55px;' value='{row0["qtyNow"]}' />" +
                                    "<span class='input-tag'>期初庫存︰</span>" +
                                    $"<input type='text' name='qtyInitial' maxlength='5' class='form-control pd-stk align-r chk-input' data-func='number' data-alt='期初庫存' style='width:70px;' value='{row0["qtyInitial"]}' />" +
                                    $"<span class='note note-stk'></span>" +
                                "</div>" +
                            "</div>" +
                        "</div>";
                    }
                }
            }
        }
    }

    /// <summary>
    /// 查詢變價
    /// </summary>
    /// <returns>HTML</returns>
    private string spriceUpd(string _PNo)
    {
        string HTML = "";
        string SqlComm = $"SELECT * FROM WP_pdPriceUpd WHERE PNO='{_PNo}'";
        DataTable priceTbl = getTbl.table("WP", SqlComm);
        if (priceTbl.Rows.Count > 0)
        {
            HTML += "<button style='margin-left:26px;cursor:pointer;' id='btn-spriceUpd'><i class='fas fa-search' style='margin-right:5px;'></i>查詢變價</button>" +
            "<div id='price-upd-main'>";
            string preStd = "", preLow = "", preMem = "", preBat = "";
            foreach (DataRow row in priceTbl.Rows)
            {
                HTML += "<div class='data-row'>" +
                    $"<div class='{(preStd == row["priceStd"].ToString() ? "" : preStd == "" ? "" : "txt-dp")}'>{row["priceStd"]}</div>" +
                    $"<div class='{(preLow == row["priceLow"].ToString() ? "" : preLow == "" ? "" : "txt-dp")}'>{row["priceLow"]}</div>" +
                    $"<div class='{(preMem == row["priceMem"].ToString() ? "" : preMem == "" ? "" : "txt-dp")}'>{row["priceMem"]}</div>" +
                    $"<div class='{(preBat == row["priceBat"].ToString() ? "" : preBat == "" ? "" : "txt-dp")}'>{row["priceBat"]}</div>" +
                    $"<div class='upd-date'>{(preStd == "" ? "初始價格" : $"{row["timeCreate"]:yyyy-MM-dd}")}</div>" +
                "</div>";
                preStd = row["priceStd"].ToString();
                preLow = row["priceLow"].ToString();
                preMem = row["priceMem"].ToString();
                preBat = row["priceBat"].ToString();
            }
            HTML += "</div>";
        }
        return HTML;
    }
}