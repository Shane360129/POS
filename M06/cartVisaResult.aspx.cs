using System;
using System.Data;
using static Global;

public partial class cartVisaResult : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {

            //string OrdId = Request.Form["lidm"];			//------訂單編號
            //if (OrdId == "") { Response.Redirect("/index.aspx"); }
            //else
            //{
            //    string SqlComm = $"SELECT * FROM DG_Ord WHERE OrdId='{OrdId}'";
            //    DataTable ordTbl = getTbl.table("DonGon", SqlComm);
            //    if (ordTbl.Rows.Count == 0) { Response.Redirect("/index.aspx"); }
            //    else
            //    {
            string Status = Request.Form["status"];         //------交易結果
            string MerID = Request.Form["merID"];
            string LastPan4 = Request.Form["lastPan4"];     //------卡片末四碼

            string Authcode, AuthAmt, Authstatus;
            string Transno = "";
            if (Status == "0")
            {    //授權成功
                Authcode = Request.Form["authCode"];    //------授權碼
                AuthAmt = Request.Form["authAmt"];      //------授權金額
                Authstatus = "授權成功";                //------授權結果
                Transno = Request.Form["xid"];          //------交易序號
            }
            else    //授權失敗
            {
                Status = Status == "" ? "x" : Status;
                Authcode = Request.Form["errcode"] == "" ? "x" : Request.Form["errcode"]; //------錯誤碼
                AuthAmt = "0";
                string Note = "授權失敗";
                if (Status != "x" && Authcode != "x")
                {
                    //SqlComm = $"SELECT * FROM DG_ErrorCode WHERE Status='{Status}' AND ErrorCode='{Authcode}'";//------刷卡錯誤代碼檔
                    //DataTable ErrTbl = getTbl.table("DonGon", SqlComm);
                    //Note = ErrTbl.Rows.Count > 0 ? ErrTbl.Rows[0]["Note"].ToString() != "" ? ErrTbl.Rows[0]["Note"].ToString() : Note : Note;
                }
                Authstatus = $"({Status}:{Authcode}){Note}";
            }
            Label1.Text = $"Status︰{Status}//MerID︰{MerID}//LastPan4︰{LastPan4}";
            //        try
            //        {
            //            DataRow row = ordTbl.Rows[0];
            //            SqlComm = $"SELECT * FROM DG_VisaAuthres WHERE OrdId = '{OrdId}'";
            //            DataTable VisaTbl = getTbl.table("DonGon", SqlComm);
            //            if (VisaTbl.Rows.Count == 0)
            //            {
            //                SqlComm = "INSERT INTO DG_VisaAuthres ( OrdId, status, lastpan4, Authcode, AuthAmt, Authstatus, Transno) " +
            //                    $"VALUES ('{OrdId}', '{Status}', '{LastPan4}', '{Authcode}', '{AuthAmt}', '{Authstatus}', '{Transno}')";
            //            }
            //            else
            //            {
            //                SqlComm = $"UPDATE DG_VisaAuthres SET status='{Status}', lastpan4='{LastPan4}', Authcode='{Authcode}', AuthAmt='{AuthAmt}', Authstatus='{Authstatus}', Transno='{Transno}' WHERE OrdId = '{OrdId}'";
            //            }
            //            getTbl.updTbl("DonGon", SqlComm);
            //            if (Status == "0")
            //            {
            //                SqlComm = $"UPDATE DG_Ord SET OrdStatus='0' WHERE OrdId='{OrdId}'";
            //                getTbl.updTbl("DonGon", SqlComm);
            //                Response.Redirect($"/cartFinish.aspx?OrdId={OrdId}");
            //            }
            //            else
            //            {
            //                Label1.Text = $"<input type='hidden' id='OrdId' value='{OrdId}'>" +
            //                $"<div style='text-align:center;margin-top:50px;border-top:2px #9d9d9d dotted;border-bottom:2px #9d9d9d dotted;padding-top:24px;padding-bottom:10px;'><h3 style='margin-bottom:15px' class='txt-r'>刷卡失敗【錯誤碼︰{Authstatus}】</h3></div>";
            //            }
            //        }
            //        catch
            //        {

            //        }
            //    }
            //}
        }
    }
    
}