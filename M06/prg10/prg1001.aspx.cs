using System;
using System.Data;
using static Global;

public partial class prg1001 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "1001";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string SqlComm = $"SELECT * FROM WP_vPdKind ORDER BY pKLId, pKMId, pKSId";
                var pkTbl = getTbl.table("WP", SqlComm);
                int pkLQty = 0, pkMQty = 0, pkSQty = 0;
                foreach (DataRow row in pkTbl.Rows)
                {
                    pkLQty += row["pKLId"].ToString() != "" ? 1 : 0;
                    pkMQty += row["pKMId"].ToString() != "" ? 1 : 0;
                    pkSQty += row["pKSId"].ToString() != "" ? 1 : 0;
                }
                Label1.Text += $"<input type='hidden' id='pkQty' type=text value='{pkLQty},{pkMQty},{pkSQty}'>" +
                "<div style='display:flex;align-items:top;padding-top:10px;'>" +
                    "<div class='pk-node-main' style='width:50%;padding:0 10px;'>" +
                        "<div style='border-bottom:2px #797979 dotted;display:flex;'>" +
                            "<div class='prg-subtitle' style='width:50%;'><i class='fas fa-arrow-alt-circle-down'></i> 商品類別</div>" +
                            "<div style='width:50%;text-align:right;padding-right:10px;font-size:18px;'>" +
                                "<i class='fas fa-folder node-btn node-all-close' title='全部關閉' style='margin-right:8px;'></i>" +
                                "<i class='fas fa-folder-open node-btn node-all-open' title='全部打開'></i>" +
                            "</div>" +
                        "</div>";
                        if (pkTbl.Rows.Count == 0)
                        {
                            Label1.Text += "<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 目前無類別！</div>";
                        }
                        else
                        {
                            SqlComm = $"SELECT * FROM  WP_PdKindL WHERE pKLExist='Y' ORDER BY pKLId";
                            DataTable pkLTbl = getTbl.table("WP", SqlComm);
                            SqlComm = $"SELECT * FROM  WP_PdKindM WHERE pKMExist='Y' ORDER BY pKMId";
                            DataTable pkMTbl = getTbl.table("WP", SqlComm);
                            SqlComm = $"SELECT * FROM  WP_PdKindS WHERE pKSExist='Y' ORDER BY pKSId";
                            DataTable pkSTbl = getTbl.table("WP", SqlComm);

                            string dtlM = "", dtlS = "";
                            foreach (DataRow rowL in pkLTbl.Rows)
                            {
                                dtlM = "";
                                foreach(DataRow rowM in pkMTbl.Rows)
                                {
                                    if(rowL["pKLId"].ToString()== rowM["pKLId"].ToString())
                                    {
                                        dtlS = "";
                                        foreach(DataRow rowS in pkSTbl.Rows)
                                        {
                                            dtlS += rowS["pKMId"].ToString() == rowM["pKMId"].ToString()
                                              ? $"<div class='edit-single-main pk-row' data-id='{rowS["pKSId"]}'>" +
                                                    $"<span class='row-id'>{rowS["pKSId"]}</span><div class='edit-tag'><i class='fas fa-edit btn-admin'></i> {rowS["pKSName"]}</div>" +
                                                    $"<div class='edit-zone'>" +
                                                        $"<input type='hidden' name='pKind' value='S' />" +
                                                        $"<input type='hidden' name='pKId' value='{rowS["pKSId"]}' />" +
                                                        $"<input type='text' class='form-control edit-txt' name='pKName' value='{rowS["pKSName"]}' data-id='{rowS["pKSName"]}' data-alt='MI_分類名稱' maxlength='20' />" +
                                                        "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                                        "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                                        "<button class='btn-del edit-btn' data-id='del'><i class='fas fa-trash'></i>刪除</button>" +
                                                    "</div>" +
                                                "</div>"
                                              : "";
                                        }
                                        dtlM += "<div class='pk-row row-pKM'>";
                                            dtlM += dtlS == ""
                                                ? "<i class='fas fa-caret-square-right' style='color:#c9c9c9'></i>"
                                                : $"<i class='fas fa-caret-square-right node-btn node-right' data-alt='{rowM["pKMId"]}'></i>" +
                                                  $"<i class='fas fa-caret-square-down node-btn node-down' data-alt='{rowM["pKMId"]}'></i>";

                                            dtlM += $"<div class='edit-single-main' data-id='{rowM["pKMId"]}'>" +
                                                $"<span class='row-id'>{rowM["pKMId"]}</span><div class='edit-tag'><i class='fas fa-edit btn-admin'></i> {rowM["pKMName"]}</div>" +
                                                $"<div class='edit-zone'>" +
                                                    $"<input type='hidden' name='pKind' value='M' />" +
                                                    $"<input type='hidden' name='pKId' value='{rowM["pKMId"]}' />" +
                                                    $"<input type='text' class='form-control edit-txt' name='pKName' value='{rowM["pKMName"]}' data-id='{rowM["pKMName"]}' data-alt='MI_分類名稱' maxlength='20' />" +
                                                    "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                                    "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                                    $"{(dtlS != "" ? "" : "<button class='btn-del edit-btn' data-id='del'><i class='fas fa-trash'></i>刪除</button>")}" +
                                                "</div>" +
                                            "</div>" +
                                        "</div>";
                                        dtlM += $"{(dtlS != "" ? $"<div class='pk-node node-pKS' data-alt='{rowM["pKMId"]}'>{dtlS}</div>" : "")}";
                                    }
                                }
                                Label1.Text += "<div class='pk-row row-pKL'>";
                                    Label1.Text += dtlM == ""
                                        ? "<i class='fas fa-caret-square-right' style='color:#c9c9c9'></i>"
                                        : $"<i class='fas fa-caret-square-right node-btn node-right' data-alt='{rowL["pKLId"]}'></i>" +
                                            $"<i class='fas fa-caret-square-down node-btn node-down' data-alt='{rowL["pKLId"]}'></i>";

                                    Label1.Text += $"<div class='edit-single-main' data-id='{rowL["pKLId"]}'>" +
                                        $"<span class='row-id'>{rowL["pKLId"]}</span><div class='edit-tag'><i class='fas fa-edit btn-admin'></i> {rowL["pKLName"]}</div>" +
                                        $"<div class='edit-zone'>" +
                                            $"<input type='hidden' name='pKind' value='L' />" +
                                            $"<input type='hidden' name='pKId' value='{rowL["pKLId"]}' />" +
                                            $"<input type='text' class='form-control edit-txt' name='pKName' value='{rowL["pKLName"]}' data-id='{rowL["pKLName"]}' data-alt='MI_分類名稱' maxlength='20' />" +
                                            "<button class='btn-abort edit-btn' data-id='abort'><i class='fas fa-times-circle'></i>放棄</button>" +
                                            "<button class='btn-submit edit-btn' data-id='submit'><i class='fas fa-check-circle'></i>確定</button>" +
                                            $"{(dtlM != "" ? "" : "<button class='btn-del edit-btn' data-id='del'><i class='fas fa-trash'></i>刪除</button>")}" +
                                        "</div>" +
                                    "</div>" +
                                "</div>" +
                                $"{(dtlM != "" ? $"<div class='pk-node node-pKM' data-alt='{rowL["pKLId"]}'>{dtlM}</div>" : "")}";
                            }
                        }
                    Label1.Text += "</div>" +
                    "<div style='width:50%;display:inline;'>" +
                        "<div style='display:flex;align-items:center;'>" +
                            "<span class='prg-subtitle' style='padding-bottom:0;margin-right:45px'><i class='fas fa-arrow-alt-circle-down'></i> 新增類別</span>" +
                            "<input type='radio' class='radio-pk-add' name='radio-pk-add' id='radio-pkL-add' checked style='margin-left:0;' value='pkL'><label for='radio-pkL-add'>主類別</label>" +
                            "<input type='radio' class='radio-pk-add' name='radio-pk-add' id='radio-pkM-add' value='pkM'><label for='radio-pkM-add'>次類別</label>" +
                            "<input type='radio' class='radio-pk-add' name='radio-pk-add' id='radio-pkS-add' value='pkS'><label for='radio-pkS-add'>小類別</label>" +
                        "</div>" +
                        "<div style='border:1px #9e9e9e solid;border-radius:3px;padding:10px;margin-top:5px;'>" +
                            "<div class='add-main pkL-add-main' data-id='pkL'>" +
                                "<div>" +
                                    "<span>主類別編號︰</span><input type='text' class='form-control add-pkLId' name='pkLId' data-alt='MI_主類別編號' maxlength='2' style='width:50px;'>" +
                                "</div>" +
                                "<div style='margin-top:5px;'>" +
                                    "<span style=''>名　　　稱︰</span><input type='text' class='form-control' name='pkLName' maxlength='20' data-alt='MI_主類別名稱' style=''>" +
                                    "<button style='margin-left:10px;' class='btn-submit pk-add-btn btn-admin' data-id='pkL-add-main'><i class='fas fa-check-circle'></i> 確定送出</button>" +
                                "</div>" +
                            "</div>" +
                            "<div class='add-main pkM-add-main' data-id='pkM'>" +
                                "<div>" +
                                    "<span>主類別編號︰</span>" +
                                    "<select class='add-pkLId' name='pkLId' data-alt='MI_主類別編號'>" +
                                        "<option value='0'>請選擇</option>";
                                        string prePKL = "";
                                        foreach (DataRow row in pkTbl.Rows)
                                        {
                                            if (prePKL != row["pKLId"].ToString())
                                            {
                                                Label1.Text += $"<option value='{row["pKLId"]}'>{row["pKLId"]} {row["pKLName"]}</option>";
                                                prePKL = row["pKLId"].ToString();
                                            }
                                        }
                                    Label1.Text += "</select>" +
                                    "<span style='margin-left:20px;'>次類別編號︰</span><input type='text' class='add-pkMId' name='pkMId' data-alt='MI_次類別編號' maxlength='3' style='width:50px;'>" +
                                "</div>" +
                                "<div style='margin-top:5px;'>" +
                                    "<span style=''>名　　　稱︰</span><input type='text' name='pkMName' maxlength='20' data-alt='MI_次類別名稱' style=''>" +
                                    "<button style='margin-left:10px;' class='btn-submit pk-add-btn btn-admin' data-id='pkM-add-main'><i class='fas fa-check-circle'></i> 確定送出</button>" +
                                "</div>" +
                            "</div>" +
                            "<div class='add-main pkS-add-main' data-id='pkS'>" +
                                "<div>" +
                                    "<span>主類別編號︰</span>" +
                                    "<select class='add-pkLId' name='pkLId' data-alt='MI_主類別編號'>" +
                                        "<option value='0'>請選擇</option>";
                                        string prePK = "";
                                        foreach (DataRow row in pkTbl.Rows)
                                        {
                                            if (prePK != row["pKLId"].ToString())
                                            {
                                                Label1.Text += row["pKMId"].ToString() == "0" ? "" : $"<option value='{row["pKLId"]}'>{row["pKLId"]} {row["pKLName"]}</option>";
                                                prePK = row["pKLId"].ToString();
                                            }
                                        }
                                    Label1.Text += "</select>" +
                                    "<span style='margin-left:20px;'>次類別編號︰</span>" +
                                    "<select class='add-pkMId' name='pkMId' data-alt='MI_次類別編號'>" +
                                        "<option value='0'>請選擇</option>" +
                                    "</select>" +
                                "</div>" +
                                "<div style='margin-top:5px;'>" +
                                    "<div style='margin-bottom:5px;'><span>小類別編號︰</span><input type='text' class='add-pkSId' name='pkSId' data-alt='MI_小類別編號' maxlength='3' style='width:50px;'></div>" +
                                    "<span style=''>名　　　稱︰</span><input type='text' name='pkSName' maxlength='20' data-alt='MI_小類別編號' style=''>" +
                                    "<button style='margin-left:10px;' class='btn-submit pk-add-btn btn-admin' data-id='pkS-add-main'><i class='fas fa-check-circle'></i> 確定送出</button>" +
                                "</div>" +
                            "</div>" +
                        "</div>" +
                    "</div>" +
                "</div>";
            }
        }
    }
}