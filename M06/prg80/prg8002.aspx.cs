using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using static Global;

public partial class prg8002 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "8002";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";
            }
        }
    }
    protected void UploadButton1_Click(object sender, EventArgs e) { uploadFile("1"); }
    public bool uploadFile(string _index)
    {
        FileUpload fileUpload = (FileUpload)Master.FindControl("ContentPlaceHolder1").FindControl($"FileUpload{_index}");
        Label statusLabel = (Label)Master.FindControl("ContentPlaceHolder1").FindControl($"UploadStatus{_index}");
        if (fileUpload.HasFile)
        {
            statusLabel.ForeColor = Color.FromArgb(0, 136, 0);
            statusLabel.Text = $"成功上傳{fileUpload.FileName}";
            StreamReader str = new StreamReader(fileUpload.PostedFile.InputStream, System.Text.Encoding.Default);
            string line = "";
            DataTable txtTbl = new DataTable();
            DataRow rowTxt;

            // 建立欄位
            txtTbl.Columns.Add("pBarcode", typeof(string));
            txtTbl.Columns.Add("qty", typeof(int));
            txtTbl.Columns.Add("dateTime", typeof(string));

            string[] lineAry;
            while ((line = str.ReadLine()) != null)
            {
                if (line != "")
                {
                    lineAry = line.Split(',');

                    rowTxt = txtTbl.NewRow();
                    rowTxt["pBarcode"] = lineAry[0];
                    rowTxt["qty"] = lineAry[1];
                    rowTxt["dateTime"] = lineAry[2];
                    txtTbl.Rows.Add(rowTxt);
                }
            }
            str.Close();

            string SqlComm = $"SELECT * FROM WP_Product WHERE isSale IN ('0', '1', '2') ORDER BY pNo";
            DataTable pdTbl = getTbl.table("WP", SqlComm);
            int pageNo = 1;
            string rptrTitle = "<div style='width:100%;'><div class='rptr-title'>商品庫存盤點表</div>" +
            "<div class='rptr-data'>" +
                $"<div style='text-align:left;width:50%;display:inline-block;'>盤點日期︰stkDate</div>" +
                $"<div style='text-align:right;width:50%;display:inline-block;'>頁次︰pageNo / <span class='page-total'></span>　列印日期︰{DateTime.Today.ToString("yyyy/MM/dd")}</div>" +
            "</div>",
            tblTitle = "<table class='list-main inStk-list-main' style='border-spacing:1px;width:99%;margin-right:1%;'>" +
                "<tr class='list-title'>" +
                    "<td style='text-align:left'>商品代號</td><td>名稱</td><td>電腦庫存</td><td>盤點庫存</td>" +
                "</tr>";
            Label2.Text = "";
            if (pdTbl.Rows.Count == 0)
            {
                Label2.Text += $"{rptrTitle.Replace("stkDate", $"{DateTime.Now.ToString("yyyy/MM/dd")}")}{tblTitle}<tr><td colspan='4' class='empty-data'>查無資料！</td></tr></table></div>";
            }
            else
            {
                string chkId = DateTime.Now.ToString("yyyyMMdd0001");

                SqlComm = $"DELETE FROM WP_StkChk WHERE ChkId='{chkId}';" +
                    $"DELETE FROM WP_StkChkDtl WHERE ChkId='{chkId}';";
                getTbl.updTbl("WP", SqlComm);

                SqlComm = $"INSERT INTO WP_StkChk (ChkId) VALUES ('{chkId}')";
                getTbl.updTbl("WP", SqlComm);

                Label2.Text += $"<div class='rptr-main'>";
                int recNo = 1, tblNo = 1;
                DataTable txtGrpTbl = txtTbl.AsEnumerable()
                    .GroupBy(r => r.Field<string>("pBarcode"))
                    .Select(g =>
                    {
                        var row = txtTbl.NewRow();
                        row["pBarcode"] = g.Key;
                        row["qty"] = g.Sum(r => r.Field<int>("qty"));
                        row["dateTime"] = g.Min(r => r.Field<string>("dateTime"));
                        return row;
                    }).CopyToDataTable();
                DataRow[] rowsTxt;

                int txtQty, tblQty;

                foreach (DataRow row in pdTbl.Rows)
                {
                    if (recNo % 22 == 1)
                    {
                        Label2.Text += $"{(recNo == 1 ? "" : $"</table></td>")}";
                        if (tblNo % 2 == 1)
                        {
                            Label2.Text += tblNo == 1 ? "" : $"</tr></table></div></div><p style='page-break-after:always'></p>";
                            Label2.Text += $"{rptrTitle.Replace("pageNo", $"{pageNo}").Replace("stkDate", $"{txtGrpTbl.Rows[0]["dateTime"]}".Substring(0, 8))}<div class='page-row'><table style='width:100%;'><tr>";
                                pageNo++;
                        }
                        Label2.Text += $"<td style='width:50%;vertical-align:top;'>{tblTitle}";
                        tblNo++;
                    }

                    tblQty = int.Parse($"{row["qtyNow"]}");
                    rowsTxt = txtGrpTbl.Select($"pBarcode='{row["pBarcode"]}'");
                    txtQty = rowsTxt.Length == 0 ? 0 : int.Parse($"{rowsTxt[0]["qty"]}");

                    rowsTxt = txtGrpTbl.Select($"pBarcode='{row["pBarcode"]}'");
                    Label2.Text += "<tr class='tr-row'>" +
                        $"<td style='text-align:left'>{row["pBarcode"]}</td>" +
                        $"<td style='text-align:left'>{row["pNameS"]}</td>" +
                        $"<td style='text-align:right'>{row["qtyNow"]}</td>" +
                        $"<td style='text-align:right'>{(rowsTxt.Length == 0 ? "" : ($"{row["qtyNow"]}" == $"{rowsTxt[0]["qty"]}" ? $"{rowsTxt[0]["qty"]}" : $"<span style='color:#f00'>{$"{rowsTxt[0]["qty"]}"}</span>"))}</td>" +
                    "</tr>";
                    recNo++;
                    SqlComm = $"INSERT INTO WP_StkChkDtl (ChkId, pNo, DataQty, RealQty) VALUES ('{chkId}', '{row["pNo"]}', '{tblQty}', '{txtQty}')";
                    getTbl.updTbl("WP", SqlComm);

                }

                Label2.Text += "</table></td>" +
            $"</tr></table></div></div></div><input type='hidden' id='page-total' value='{(pageNo - 1)}' />" +
            "<div style='' class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080'>列　印</button></div>";
            }
            //string sno = Request.QueryString["sno"];
            //string SqlComm = $"SELECT * FROM yk_Board WHERE BoardSno='{sno}'";
            //DataTable boardTbl = getTbl.table("YK", SqlComm);

            //string savePath = $"{Server.MapPath("/upload/")}{sno}-{_index}{fileUpload.FileName}";
            //fileUpload.SaveAs(savePath);
            //statusLabel.ForeColor = Color.FromArgb(0, 136, 0);
            //statusLabel.Text = $"成功上傳{fileUpload.FileName}";
            //Label lblPix = (Label)Master.FindControl("ContentPlaceHolder1").FindControl($"Lbl_Pix{_index}");
            //lblPix.Text = $"<i class='fas fa-download'></i><a href='/upload/{sno}-{_index}{fileUpload.FileName}' class='pdPix' data-alt='{sno}-{_index}{fileUpload.FileName}' target='_blank'>{fileUpload.FileName}</a>";

            //DataRow row0 = boardTbl.Rows[0];
            //if ($"{row0["Filename"]}" == "")
            //{
            //    SqlComm = $"UPDATE yk_Board SET Filename=',{sno}-{_index}{fileUpload.FileName}' WHERE BoardSno='{sno}'";
            //    getTbl.updTbl("YK", SqlComm);
            //}
            //else
            //{
            //    SqlComm = $"UPDATE yk_Board SET Filename=REPLACE(Filename, ',{sno}-{_index}{fileUpload.FileName}', '')+',{sno}-{_index}{fileUpload.FileName}' WHERE BoardSno='{sno}'";
            //    getTbl.updTbl("YK", SqlComm);
            //}
            return true;
        }
        else
        {
            statusLabel.ForeColor = Color.FromArgb(233, 0, 128);
            statusLabel.Text = "你尚未選擇檔案！";
            return false;
        }
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {

        string filePath = FileUpload1.PostedFile.FileName;

        //  c#
        //一行一行的取讀取文字檔內的資料  System.Text.Encoding.Default才不會讀到亂碼
        StreamReader stmRdr = new StreamReader(filePath, System.Text.Encoding.Default);

        string line = stmRdr.ReadLine();
        while (line != null)
        {
            Console.WriteLine(line);
            line = stmRdr.ReadLine();
        }

    }
}