using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.DataVisualization.Charting;
using System.Collections;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Drawing;

public partial class Chart : System.Web.UI.Page
{
    DataTable charDt;
    protected void Page_Load(object sender, EventArgs e)
    {

        using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["WPRd"].ConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand("spDay30ProdSaleSum", db))
            {
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    this.charDt = new DataTable();
                    da.Fill(this.charDt);
                }
            }
        }


        //Step 1 重新撈資料給Chart Control
        Chart1.Series[0].ChartType = SeriesChartType.Pie;
        Chart1.Series[0].IsValueShownAsLabel = true;
        Chart1.Series[0].Label = "#PERCENT{P2}";
        Chart1.Series[0].LegendText = "#VALX";
        Chart1.Legends[0].Enabled = true;
        //Chart1.Legends[0].Font = new Font("標楷體", 6.8f);
        Chart1.Titles.Add(
            new Title(
                $"{Global.PageTitle.Replace("POS進銷存管理系統", String.Empty)}近30天商品銷量前十名",
                Docking.Top,
                new Font("標楷體", 10f, FontStyle.Bold),
                Color.Black
            )
        );

        ArrayList x = new ArrayList { };
        ArrayList y = new ArrayList { };
        foreach (DataRow dr in charDt.Rows)
        {

            x.Add($"{dr["pName"].ToString()}(庫:{dr["qtyNow"].ToString()}/銷:{dr["salQty"].ToString()})");
            y.Add(dr["salQty"].ToString());
        }

        Chart1.Series[0].Points.DataBindXY(x, y);

        //Step 2.把Chart Control儲存圖片在MemoryStream
        //Step 3.Chart儲存至網站目錄
        using (MemoryStream ms = new MemoryStream())
        {
            Chart1.SaveImage(ms);
            var fileName = $"Chart_002.jpg";
            string tempFilePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/") + "charts/" + fileName);
            using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                ms.WriteTo(fs);
            }
        }

        //Step 3. Response給用戶端下載  
        //Response.Clear();
        //Response.AddHeader("content-disposition", "attachment; filename=chart.jpg");//強制下載  
        //Response.ContentType = "image/jpg";
        //Response.BinaryWrite(ms.ToArray());
        //ms.Close();
        //Response.End();
    }
}