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
    protected void Page_Load(object sender, EventArgs e)
    {
        using (SqlConnection db = new SqlConnection(ConfigurationManager.ConnectionStrings["WPRd"].ConnectionString))
        {
            using (SqlCommand cmd = new SqlCommand("spMonthSaleAmt", db))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    db.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex.GetBaseException();
                }
                finally
                {
                    db.Close();
                }
            }
        }

        string SqlComm = $"SELECT * FROM wp_tempMonthAmtChart";
        DataTable charDt = Global.getTbl.table("WP", SqlComm);

        //Step 1 重新撈資料給Chart Control
        Chart1.Series[0].ChartType = SeriesChartType.Column;
        Chart1.Series[0].IsValueShownAsLabel = true;
        Chart1.ChartAreas[0].Area3DStyle.Enable3D = false;//顯示3D繪圖
        Chart1.Titles.Add(
            new Title(
                $"{Global.PageTitle.Replace("POS進銷存管理系統", String.Empty)}{DateTime.Now.Year}年每月營收",
                Docking.Top,
                new Font("標楷體", 8f, FontStyle.Bold),
                Color.Black
            )
        );
        ArrayList x = new ArrayList { };
        ArrayList y = new ArrayList { };
        foreach (DataRow dr in charDt.Rows)
        {

            x.Add($"{dr["mon"].ToString()}月");
            y.Add(dr["a1"].ToString());
        }
        Chart1.Series[0].Points.DataBindXY(x, y);

        //Step 2.把Chart Control儲存圖片在MemoryStream
        //Step 3.Chart儲存至網站目錄
        using (MemoryStream ms = new MemoryStream())
        {
            Chart1.SaveImage(ms);
            var fileName = $"Chart_001.jpg";
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