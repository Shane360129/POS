<%@ Page Language="C#" %>

<%@ Import Namespace="System.Data" %>
<%@ Assembly Name="Newtonsoft.Json" %>
<%@ Import Namespace="Newtonsoft.Json.Linq" %>

<script runat="server">
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Response.Clear();
            Employee emp = new Employee();
            Stock stock = new Stock();
            ToDouble toDouble = new ToDouble();
            string SqlComm = "";
            DataTable pdTbl;
            try
            {
                if (!emp.IsEmp) { Response.Write("not-emp"); }              //非登入帳號不能執行
                else
                {
                    if (emp.EmpIsAdmin == "N") { Response.Write("UA"); }    //非管理者不能變更
                    else
                    {
                        switch (Request.Form["args0"])
                        {
                            #region T00 庫存試算
                            case "T00":
                                //Response.Write(InsertStkQtyYM("202208"));
                                int qty = 0, qtyNow;
                                SqlComm = $"SELECT * FROM WP_vPdKindProd WHERE pKExist ='Y' ORDER BY pKLId, pKMId, pKSId";
                                pdTbl = Global.getTbl.table("WP", SqlComm);

                                SqlComm = $"SELECT PNo, SUM(qty) AS qty FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' GROUP BY PNo";
                                DataTable iStk = Global.getTbl.table("WP", SqlComm);

                                SqlComm = $"SELECT PNo, SUM(qty) AS qty FROM WP_vOutStockUnion WHERE isDel='N' AND dtlIsDel='N' GROUP BY PNo";
                                DataTable oStk = Global.getTbl.table("WP", SqlComm);

                                DataRow[] iStkRow, oStkRow;
                                string result0 = "";
                                foreach (DataRow row in pdTbl.Rows)
                                {
                                    if ($"{row["isUpdStock"]}" == "N") {
                                        qty = 0;
                                    }
                                    else
                                    {
                                        iStkRow = iStk.Select($"PNo='{row["PNo"]}'");
                                        oStkRow = oStk.Select($"PNo='{row["PNo"]}'");
                                        qty = int.Parse($"{row["qtyInitial"]}");
                                        qty += iStkRow.Length != 0 ? int.Parse($"{iStkRow[0]["qty"]}") : 0;
                                        qty -= oStkRow.Length != 0 ? int.Parse($"{oStkRow[0]["qty"]}") : 0;
                                    }
                                    qtyNow = int.Parse($"{row["qtyNow"]:#0}");
                                    if (qty != qtyNow)
                                    {
                                        #region 使用完要註解回去喔 !
                                        SqlComm = $"UPDATE WP_Product SET qtyNow={qty} WHERE pNo='{row["PNo"]}'";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        #endregion
                                        result0 += $"<div>{row["pNameS"]}--目前庫存{qtyNow}--試算庫存{qty}</div>\n";
                                    }
                                    //SqlComm = $"UPDATE WP_Product SET qtyNow={qty} WHERE pNo='{row["PNo"]}'";
                                    //Global.getTbl.updTbl("WP", SqlComm);
                                }
                                emp.SysLogWrite("UPD","庫存試算");
                                Response.Write($"{result0}");
                                break;

                            #endregion
                            #region T01 日結帳作業
                            case "T01":
                                string sYMD = $"{Request.Form["sYMD"]}";    //結帳日
                                SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE isDel='N' ORDER BY yearMonth DESC";
                                DataTable chkYMTbl = Global.getTbl.table("WP", SqlComm);
                                string result = "Y";
                                if (chkYMTbl.Rows.Count == 0 && sYMD != Global.StartYM) { result= $"error"; }     //第一次結帳日非期初結帳日
                                else {
                                    if (chkYMTbl.Rows.Count > 0)
                                    {
                                        string rowYMD = $"{chkYMTbl.Rows[0]["yearMonth"]}";     //已結帳日
                                        rowYMD = DateTime.Parse($"{rowYMD.Substring(0,4)}-{rowYMD.Substring(4, 2)}-{rowYMD.Substring(6, 2)}").AddDays(1).ToString("yyyyMMdd");
                                        if(rowYMD != sYMD) { result = "error"; }    //結帳日非已結帳日後一天
                                    }
                                }
                                if (result == "error") { Response.Write("error"); }
                                else
                                {
                                    if (InsStkQtyYM(sYMD) == "error"){ Response.Write("error"); }   //商品前一月庫存數寫入
                                    else
                                    {
                                        UpdStkTrcOver(sYMD);    //超賣的記錄檔補足
                                        AddStkTrc(sYMD);        //寫入交易記錄檔


                                        emp.SysLogWrite("6001", $"{sYMD}");
                                        SqlComm = $"SELECT * FROM WP_ChkoutYM WHERE yearMonth='{sYMD}'";
                                        DataTable ymTbl = Global.getTbl.table("WP", SqlComm);
                                        SqlComm = ymTbl.Rows.Count == 0
                                            ? $"INSERT INTO WP_ChkoutYM (yearMonth) VALUES ('{sYMD}')"
                                            : $"UPDATE WP_ChkoutYM SET isDel='N' WHERE yearMonth='{sYMD}'";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        Response.Write("Y");
                                    }
                                }


                                break;
                            #endregion
                            #region T01_1 取消日結帳作業
                            case "T01_1":
                                string uYMD = $"{Request.Form["uYMD"]}";

                                SqlComm = $"UPDATE WP_ChkoutYM SET isDel ='Y', timeUpdate=GETDATE() WHERE YearMonth='{uYMD}'";
                                Global.getTbl.updTbl("WP", SqlComm);

                                SqlComm = uYMD == Global.StartYM       //如果取消日結年月日為啟始年月日，則每月庫存全部重新計算，否則該月庫存重新計算
                                    ? $"UPDATE WP_pdStkQtyYM SET isDel ='Y', timeUpdate=GETDATE()"
                                    : $"UPDATE WP_pdStkQtyYM SET isDel ='Y', timeUpdate=GETDATE() WHERE SUBSTRING(YearMonth, 1, 6)='{uYMD.Substring(0, 6)}'";
                                Global.getTbl.updTbl("WP", SqlComm);

                                SqlComm = $"UPDATE WP_StkTrace SET Kind ='D', timeUpdate=GETDATE() WHERE SUBSTRING(OutStkId, 1, 8)='{uYMD}'";
                                //SqlComm = $"UPDATE WP_StkTrace SET Kind ='D', timeUpdate=GETDATE() WHERE SUBSTRING(OutStkId, 1, 6)='{yearMonth}'";
                                Global.getTbl.updTbl("WP", SqlComm);

                                emp.SysLogWrite("6001", $"{uYMD}U");
                                Response.Write("Y");
                                break;
                            #endregion
                            #region T02 庫存提醒
                            case "T02":
                                string sYMD1 = $"{Request.Form["sYMD"]}";    //結帳日
                                SqlComm = $"select pNo,pNameS,pBarcode,qtyNow from WP_Product where qtyNow < 0";
                                DataTable qtyTbl = Global.getTbl.table("WP", SqlComm);
                                SqlComm = $"SELECT pNo, pNameS, SUM(qty) AS qtyTotal FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 8)='{sYMD1}' GROUP BY pNo, pNameS";
                                DataTable outqtyTbl = Global.getTbl.table("WP", SqlComm);
                                string result1 = "";
                                DataRow[] qtyRows;
                                foreach (DataRow row in outqtyTbl.Rows)
                                {
                                    qtyRows = qtyTbl.Select($"pNo='{row["pNo"]}'");
                                    if(qtyRows.Length != 0)
                                        result1 += $"<div>{qtyRows[0]["pNameS"]}--目前庫存{qtyRows[0]["qtyNow"]}</div>";
                                }
                                Response.Write($"{result1}");
                                break;

                                #endregion
                        }
                    }

                }
            }
            catch (InvalidOperationException ex)
            {
                Response.Write(ex.Message);
            }
            Response.End();
        }

    }

    #region 處理交易記錄檔

    /// <summary>
    /// 超賣的記錄檔補足
    /// </summary>
    private void UpdStkTrcOver(string _sYMD)
    {
        string SqlComm = $"SELECT *, OutStkDtlSn AS dtlSn FROM WP_vStkTraceUnion WHERE Kind<>'D' AND InStkId='over' ORDER BY pNo, OutStkId, dtlSn";
        DataTable overStkTbl = Global.getTbl.table("WP", SqlComm);
        //Response.Write($"a1.SqlComm={SqlComm}\n");
        if (overStkTbl.Rows.Count > 0)          //記錄檔內有超賣商品才需補足
        {
            //交易日進貨檔
            SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8)='{_sYMD}' ORDER BY InStkId, pNo, dtlSn";
            DataTable inStkTbl = Global.getTbl.table("WP", SqlComm);
            //Response.Write($"a2.SqlComm={SqlComm}\n");
            //Response.Write($"a3.SqlComm.count={inStkTbl.Rows.Count}\n");
            if (inStkTbl.Rows.Count > 0)    //交易日有進貨商品才需補足超賣商品
            {
                //未超賣交易記錄檔
                SqlComm = $"SELECT * FROM WP_vStkTraceUnion WHERE Kind NOT IN ('D') AND InStkId<>'over' ORDER BY OutStkId, pNo, OutStkDtlSn";
                DataTable stkTrace = Global.getTbl.table("WP", SqlComm);
                //Response.Write($"a4.SqlComm={SqlComm}\n");

                //交易檔記錄檔已刪除的資料(回收寫入新的交易資料，以免交易檔過大)
                SqlComm = $"SELECT * FROM WP_StkTrace WHERE Kind='D' ORDER BY sn";
                DataTable delTrcTbl = Global.getTbl.table("WP", SqlComm);
                //Response.Write($"a5.SqlComm={SqlComm}\n");

                string prePNo = "", JSON = "",
                    minDelSn = delTrcTbl.Rows.Count == 0 ? "empty" : $"{delTrcTbl.Rows[0]["sn"]}";     //可回收交易記錄檔最小序號

                JObject jsonObj = null;
                DataRow[] leastRows, inStkRows = null;
                string pNoIsOver = "N";
                foreach (DataRow row in overStkTbl.Rows)
                {
                    jsonObj = JSON == "" ? jsonObj : JObject.Parse(JSON);
                    minDelSn = JSON == "" ? minDelSn : $"{jsonObj["minDelSn"]}";
                    if (prePNo != $"{row["pNo"]}")
                    {
                        inStkRows = inStkTbl.Select($"pNo='{row["pNo"]}'", "InStkId ASC, dtlSn ASC");
                        leastRows = stkTrace.Select($"pNo='{row["pNo"]}'", "sn DESC");         //取得商品銷貨記錄檔
                        JSON = (leastRows.Length == 0)
                            ? $"{{\"InStkId\":\"empty\",\"InStkDtlSn\":\"0\",\"QtyLeft\":\"0\",\"minDelSn\":\"{minDelSn}\"}}"     //記錄檔尚未有任何該商品記錄
                            : $"{{\"InStkId\":\"{leastRows[0]["InStkId"]}\",\"InStkDtlSn\":\"{leastRows[0]["InStkDtlSn"]}\",\"QtyLeft\":\"{leastRows[0]["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
                        prePNo = $"{row["pNo"]}";
                        pNoIsOver = "N";
                        //Response.Write($"a6.JSON={JSON}\n");
                    }
                    else
                    {
                        JSON = $"{{\"InStkId\":\"{jsonObj["InStkId"]}\",\"InStkDtlSn\":\"{jsonObj["InStkDtlSn"]}\",\"QtyLeft\":\"{jsonObj["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
                        //Response.Write($"a7.JSON={JSON}\n");
                    }

                    //Response.Write($"a8.pNo={row["pNo"]}--Length={inStkRows.Length}\n");
                    if (inStkRows.Length > 0)
                    {
                        if (pNoIsOver == "N")
                        {
                            JSON = AddStkTrcMain(row, inStkRows, delTrcTbl, JSON);
                            SqlComm = $"UPDATE WP_StkTrace SET Kind='D' WHERE sn='{row["sn"]}'";
                            Global.getTbl.updTbl("WP", SqlComm);
                            //Response.Write($"a9.SqlComm={SqlComm}\n");
                            jsonObj = JObject.Parse(JSON);
                            //Response.Write($"a10.InStkId={jsonObj["InStkId"]}\n");
                            if ($"{jsonObj["InStkId"]}" == "over") { pNoIsOver = "Y"; }
                        }
                    }
                }
            }

        }
    }


    /// <summary>
    /// 寫入交易記錄檔
    /// </summary>
    private void AddStkTrc(string _sYMD)
    {
        //該日出貨檔
        string SqlComm = $"SELECT * FROM WP_vOutStockUnion WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 8)='{_sYMD}' ORDER BY pNo, OutStkId, dtlSn";
        DataTable outStk = Global.getTbl.table("WP", SqlComm);
        if (outStk.Rows.Count > 0)
        {
            //交易記錄檔
            SqlComm = $"SELECT * FROM WP_vStkTraceUnion WHERE Kind NOT IN ('D') ORDER BY OutStkId, pNo, OutStkDtlSn";
            DataTable stkTrace = Global.getTbl.table("WP", SqlComm);

            //進貨檔
            SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 8)<='{_sYMD}' AND (dtlSn NOT IN (SELECT InStkDtlSn FROM WP_StkTrace WHERE Kind NOT IN ('D') AND QtyLeft=0)) ORDER BY InStkId, pNo, dtlSn";
            DataTable inStkTbl = Global.getTbl.table("WP", SqlComm);

            //取得交易檔已刪除的資料(回收寫入新的交易資料，以免交易檔過大)
            SqlComm = $"SELECT * FROM WP_StkTrace WHERE Kind='D' ORDER BY sn";
            DataTable delTrcTbl = Global.getTbl.table("WP", SqlComm);

            string prePNo = "", JSON = "",
                minDelSn = delTrcTbl.Rows.Count == 0 ? "empty" : $"{delTrcTbl.Rows[0]["sn"]}";     //可回收交易記錄檔最小序號

            JObject jsonObj = null;
            foreach (DataRow row in outStk.Rows)
            {
                jsonObj = JSON == "" ? jsonObj : JObject.Parse(JSON);
                minDelSn = JSON == "" ? minDelSn : $"{jsonObj["minDelSn"]}";
                if (prePNo != $"{row["pNo"]}")
                {
                    DataRow[] leastRow = stkTrace.Select($"pNo='{row["pNo"]}'", "OutStkId DESC, OutStkDtlSn DESC");         //取得商品銷貨記錄檔
                    JSON = (leastRow.Length == 0)
                        ? $"{{\"InStkId\":\"empty\",\"InStkDtlSn\":\"0\",\"QtyLeft\":\"0\",\"minDelSn\":\"{minDelSn}\"}}"     //記錄檔尚未有任何該商品記錄
                        : $"{{\"InStkId\":\"{leastRow[0]["InStkId"]}\",\"InStkDtlSn\":\"{leastRow[0]["InStkDtlSn"]}\",\"QtyLeft\":\"{leastRow[0]["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
                    prePNo = $"{row["pNo"]}";
                }
                else
                {
                    JSON = $"{{\"InStkId\":\"{jsonObj["InStkId"]}\",\"InStkDtlSn\":\"{jsonObj["InStkDtlSn"]}\",\"QtyLeft\":\"{jsonObj["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
                }
                JSON = AddStkTrcMain(row, inStkTbl.Select($"pNo='{row["pNo"]}'", "InStkId ASC, dtlSn ASC"), delTrcTbl, JSON);
            }
        }
    }

    /// <summary>
    /// 依出貨單，寫入出貨記錄檔
    /// </summary>
    /// <param name="_outStk"></param>       出貨檔
    /// <param name="_inStkTbl"></param>     進貨檔
    /// <param name="_delTrcTbl"></param>    已刪除交易檔(回收已刪除交易檔，重新寫入，以免交易檔過大)
    /// <param name="_JSON"></param>         最後進貨資料 InStkId:進貨單號 / InStkDtlSn:進貨細明序號 / QtyLeft:剩餘未出數量 / minDelSn :記錄檔回收最小序號
    private string AddStkTrcMain(DataRow _outStk, DataRow[] _inStkTbl, DataTable _delTrcTbl, string _JSON)
    {
        JObject jsonObj, jsonReObj;
        string SqlComm, JSON, JSON_RESULT, minDelSn;
        int outQty, iStkRowQty, pdInitlQty;     //outQty:出貨數量, iStkRowQty:進貨商品數量, pdInitlQty:商品期初數量

        jsonObj = JObject.Parse(_JSON);
        minDelSn = $"{jsonObj["minDelSn"]}";
        //Response.Write($"166-{minDelSn}\n");
        pdInitlQty = int.Parse($"{_outStk["qtyInitial"]}");     //商品期初數量

        if ($"{jsonObj["InStkId"]}" == "empty")     //沒有銷貨記錄檔
        {
            if (pdInitlQty == 0)
            {
                outQty = int.Parse($"{_outStk["qty"]}");
                JSON = $"{{\"InStkId\":\"0\",\"InStkDtlSn\":\"0\",\"QtyLeft\":\"0\",\"minDelSn\":\"{minDelSn}\"}}";
            }
            else
            {
                JSON_RESULT = AddStkTrcTbl(_outStk, "0", "0", int.Parse($"{_outStk["qty"]}"), pdInitlQty, minDelSn);      //扣期初庫存的進貨單號及序號皆為預設0
                jsonReObj = JObject.Parse(JSON_RESULT);
                outQty = int.Parse($"{jsonReObj["overQty"]}");
                minDelSn = minDelSn == "empty" ? minDelSn : $"{jsonReObj["nextDelSnYN"]}" == "N" ? minDelSn : GetNewSn(_delTrcTbl, minDelSn);
                JSON = $"{{\"InStkId\":\"0\",\"InStkDtlSn\":\"0\",\"QtyLeft\":\"{jsonReObj["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
            }

            if (outQty > 0)
            {
                if (_inStkTbl.Length > 0)
                {
                    foreach (DataRow iStkRow in _inStkTbl)
                    {
                        iStkRowQty = int.Parse($"{iStkRow["qty"]}");    //進貨商品數量
                        JSON_RESULT = AddStkTrcTbl(_outStk, $"{iStkRow["InStkId"]}", $"{iStkRow["dtlSn"]}", outQty, iStkRowQty, minDelSn);
                        jsonReObj = JObject.Parse(JSON_RESULT);
                        outQty = int.Parse($"{jsonReObj["overQty"]}");
                        minDelSn = minDelSn == "empty" ? minDelSn : $"{jsonReObj["nextDelSnYN"]}" == "N" ? minDelSn : GetNewSn(_delTrcTbl, minDelSn);
                        JSON = $"{{\"InStkId\":\"{iStkRow["InStkId"]}\",\"InStkDtlSn\":\"{iStkRow["dtlSn"]}\",\"QtyLeft\":\"{jsonReObj["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
                        if (outQty == 0) { break; }
                    }
                }

                if (outQty > 0)        //超賣
                {
                    SqlComm =  (minDelSn == "empty")
                         ? "INSERT INTO WP_StkTrace (OutStkId, OutStkDtlSn, InStkId, InStkDtlSn, Qty, QtyLeft) " +
                             $"VALUES ('{_outStk["outStkId"]}', '{_outStk["dtlSn"]}', 'over', 0, {outQty}, {(-1) * outQty})"
                         : $"UPDATE WP_StkTrace SET OutStkId='{_outStk["outStkId"]}', OutStkDtlSn='{_outStk["dtlSn"]}', InStkId='over', InStkDtlSn=0, Qty={outQty}, " +
                             $"QtyLeft={(-1) * outQty}, Kind='O', timeUpdate=GETDATE() WHERE sn='{minDelSn}'";
                    Global.getTbl.updTbl("WP", SqlComm);
                    outQty = 0;
                    minDelSn = minDelSn == "empty" ? minDelSn : GetNewSn(_delTrcTbl, minDelSn);
                    JSON = $"{{\"InStkId\":\"over\",\"InStkDtlSn\":\"0\",\"QtyLeft\":\"{(-1) * outQty}\",\"minDelSn\":\"{minDelSn}\"}}";
                }
            }
        }
        else     //有銷貨記錄檔
        {
            string InStkId = $"{jsonObj["InStkId"]}",
                InStkDtlSn = $"{jsonObj["InStkDtlSn"]}";
            int QtyLeft = int.Parse($"{jsonObj["QtyLeft"]}");

            JSON_RESULT = AddStkTrcTbl(_outStk, $"{InStkId}", $"{InStkDtlSn}", int.Parse($"{_outStk["qty"]}"), QtyLeft, minDelSn);      //扣期初庫存的進貨單號及序號皆為預設0
            jsonReObj = JObject.Parse(JSON_RESULT);         //overQty:出貨數量,QtyLeft:進貨單剩餘可扣數量",nextDelSnYN:是否取下個回收序號
            outQty = int.Parse($"{jsonReObj["overQty"]}");
            minDelSn = minDelSn == "empty" ? minDelSn : $"{jsonReObj["nextDelSnYN"]}" == "N" ? minDelSn : GetNewSn(_delTrcTbl, minDelSn);
            JSON = $"{{\"InStkId\":\"{InStkId}\",\"InStkDtlSn\":\"{InStkDtlSn}\",\"QtyLeft\":\"{jsonReObj["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
            if (outQty > 0)
            {
                if (_inStkTbl.Length > 0)
                {
                    DataRow[] newInStkTbl = _inStkTbl.CopyToDataTable().Select($"((InStkId='{InStkId}' AND  dtlSn>'{InStkDtlSn}') OR (InStkId>'{InStkId}'))", " InStkId ASC, dtlSn ASC");
                    foreach (DataRow iStkRow in newInStkTbl)
                    {
                        JSON_RESULT = AddStkTrcTbl(_outStk, $"{iStkRow["InStkId"]}", $"{iStkRow["dtlSn"]}", outQty, int.Parse($"{iStkRow["qty"]}"), minDelSn);
                        jsonReObj = JObject.Parse(JSON_RESULT);
                        outQty = int.Parse($"{jsonReObj["overQty"]}");
                        minDelSn = minDelSn == "empty" ? minDelSn : $"{jsonReObj["nextDelSnYN"]}" == "N" ? minDelSn : GetNewSn(_delTrcTbl, minDelSn);
                        JSON = $"{{\"InStkId\":\"{iStkRow["InStkId"]}\",\"InStkDtlSn\":\"{iStkRow["dtlSn"]}\",\"QtyLeft\":\"{jsonReObj["QtyLeft"]}\",\"minDelSn\":\"{minDelSn}\"}}";
                        if (outQty == 0) { break; }
                    }
                }

                if (outQty > 0)        //超賣
                {
                    SqlComm =  (minDelSn == "empty")
                         ? "INSERT INTO WP_StkTrace (OutStkId, OutStkDtlSn, InStkId, InStkDtlSn, Qty, QtyLeft) " +
                            $"VALUES ('{_outStk["outStkId"]}', '{_outStk["dtlSn"]}', 'over', 0, {outQty}, {(-1) * outQty})"
                         : $"UPDATE WP_StkTrace SET OutStkId='{_outStk["outStkId"]}', OutStkDtlSn='{_outStk["dtlSn"]}', InStkId='over', InStkDtlSn=0, Qty={outQty}, " +
                            $"QtyLeft={(-1) * outQty}, Kind='O', timeUpdate=GETDATE() WHERE sn='{minDelSn}'";
                    Global.getTbl.updTbl("WP", SqlComm);
                    outQty = 0;
                    minDelSn = minDelSn == "empty" ? minDelSn : GetNewSn(_delTrcTbl, minDelSn);
                    JSON = $"{{\"InStkId\":\"over\",\"InStkDtlSn\":\"0\",\"QtyLeft\":\"{(-1) * outQty}\",\"minDelSn\":\"{minDelSn}\"}}";
                }
            }
        }
        return JSON;
    }

    /// <summary>
    /// 銷貨寫入記錄檔
    /// </summary>
    /// <param name="_outStk"></param>出貨檔
    /// <param name="_inStkId"></param>進貨檔單號
    /// <param name="_dtlSn"></param>進貨檔明細序號
    /// <param name="_outQty"></param>出貨數量
    /// <param name="_inStkQtyLeft"></param>進貨單剩餘庫存數量
    /// <param name="_minDelSn"></param>已刪除交易檔最小序號(empty:已無刪除序號)
    /// <returns></returns>
    private string AddStkTrcTbl(DataRow _outStk, string _inStkId, string _dtlSn, int _outQty, int _inStkQtyLeft, string _minDelSn)
    {
        //JSON 內容︰
        //overQty:出貨商品還剩多少數量沒記錄
        //QtyLeft:該筆進貨數量還剩多少可出貨
        //nextDelSnYN:是否要取下一個已刪除交易檔序號
        string SqlComm, JSON;
        int overQty;
        if (_inStkQtyLeft >= _outQty)
        {
            SqlComm = _minDelSn == "empty"
                ? "INSERT INTO WP_StkTrace (OutStkId, OutStkDtlSn, InStkId, InStkDtlSn, Qty, QtyLeft) " +
                    $"VALUES ('{_outStk["outStkId"]}', '{_outStk["dtlSn"]}','{_inStkId}', {_dtlSn}, {_outQty}, {_inStkQtyLeft - _outQty})"
                : $"UPDATE WP_StkTrace SET OutStkId='{_outStk["outStkId"]}', OutStkDtlSn='{_outStk["dtlSn"]}', InStkId='{_inStkId}', InStkDtlSn={_dtlSn}, Qty={_outQty}, " +
                    $"QtyLeft={_inStkQtyLeft - _outQty}, Kind='O', timeUpdate=GETDATE() WHERE sn='{_minDelSn}'";

            Global.getTbl.updTbl("WP", SqlComm);
            //Response.Write($"280--{SqlComm}\n");
            overQty = 0;
            JSON = $"{{\"overQty\":\"{overQty}\",\"QtyLeft\":\"{_inStkQtyLeft - _outQty}\",\"nextDelSnYN\":\"Y\"}}";
        }
        else
        {
            if (_inStkQtyLeft > 0)
            {
                SqlComm = _minDelSn == "empty"
                    ? "INSERT INTO WP_StkTrace (OutStkId, OutStkDtlSn, InStkId, InStkDtlSn, Qty, QtyLeft) " +
                        $"VALUES ('{_outStk["outStkId"]}', '{_outStk["dtlSn"]}', '{_inStkId}', {_dtlSn}, {_inStkQtyLeft}, 0)"
                    :$"UPDATE WP_StkTrace SET OutStkId='{_outStk["outStkId"]}', OutStkDtlSn='{_outStk["dtlSn"]}', InStkId='{_inStkId}', InStkDtlSn={_dtlSn}, Qty={_inStkQtyLeft}, " +
                        $"QtyLeft=0, Kind='O', timeUpdate=GETDATE() WHERE sn='{_minDelSn}'";

                Global.getTbl.updTbl("WP", SqlComm);
                //Response.Write($"304--{SqlComm}\n");
                overQty = _outQty - _inStkQtyLeft;
                JSON = $"{{\"overQty\":\"{overQty}\",\"QtyLeft\":\"0\",\"nextDelSnYN\":\"Y\"}}";
            }
            else  //該商品記錄檔最後一筆進貨單號已剩零庫存不必記錄，再抓下一筆
            {
                overQty = _outQty;
                JSON = $"{{\"overQty\":\"{overQty}\",\"QtyLeft\":\"0\",\"nextDelSnYN\":\"N\"}}";
            }
        }
        return JSON;
    }

    #endregion

    /// <summary>
    /// 寫入商品上月庫存檔
    /// </summary>
    /// <param name="_YMD"></param>庫存年月日
    /// <returns></returns>
    private string InsStkQtyYM(string _YMD)
    {
        int qty = 0;
        string preYM = DateTime.Parse($"{_YMD.Substring(0,4)}-{_YMD.Substring(4, 2)}-01").AddDays(-1).ToString("yyyyMM");

        string SqlComm = $"SELECT TOP(1) * FROM WP_pdStkQtyYM";
        DataTable stkQtyTbl = Global.getTbl.table("WP", SqlComm);

        SqlComm = $"SELECT * FROM WP_Product WHERE isSale NOT IN ('D') AND " +
                $"( qtyInitial > 0 OR qtyNow > 0 OR " +
                $"pNo IN (SELECT pNo FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 6)='{preYM}') OR " +
                $"pNo IN (SELECT pNo FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 6)='{preYM}') ) " +
                $"ORDER BY pNo";
        DataTable pdTbl = Global.getTbl.table("WP", SqlComm);

        if (stkQtyTbl.Rows.Count == 0)      //資料庫全無資料
        {
            if (_YMD != Global.StartYM) { return "error"; }
            else
            {
                foreach (DataRow row in pdTbl.Rows)
                {
                    SqlComm = $"INSERT INTO WP_pdStkQtyYM (pNo, qty, cost, YearMonth) VALUES ('{row["pNo"]}', '{row["qtyInitial"]}', '{row["costInitial"]}', '{preYM}')";
                    Global.getTbl.updTbl("WP", SqlComm);
                }
            }
        }
        else
        {
            SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE isDel='N' AND YearMonth='{preYM}'";
            stkQtyTbl = Global.getTbl.table("WP", SqlComm);
            if (stkQtyTbl.Rows.Count > 0) { return "Y"; }      //上個月庫存檔已存在不再處理
            else
            {
                SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE isDel='Y' ORDER BY sn";
                DataTable delStkQtyTbl = Global.getTbl.table("WP", SqlComm);
                string minSn = delStkQtyTbl.Rows.Count == 0 ? "empty" : $"{delStkQtyTbl.Rows[0]["sn"]}";

                string startYM = DateTime.Parse($"{Global.StartYM.Substring(0, 4)}-{Global.StartYM.Substring(4, 2)}-01").AddDays(-1).ToString("yyyyMM");
                if (preYM == startYM)
                {
                    foreach (DataRow row in pdTbl.Rows)
                    {
                        SqlComm = minSn == "empty"
                            ? $"INSERT INTO WP_pdStkQtyYM (pNo, qty, YearMonth) VALUES ('{row["pNo"]}', '{row["qtyInitial"]}', '{preYM}')"
                            : $"UPDATE WP_pdStkQtyYM SET pNo='{row["pNo"]}', qty='{row["qtyInitial"]}', YearMonth='{preYM}', isDel='N', timeUpdate=GETDATE() WHERE sn='{minSn}'";
                        Global.getTbl.updTbl("WP", SqlComm);
                        minSn = minSn == "empty" ? minSn : GetNewSn(delStkQtyTbl, minSn);
                    }
                }
                else
                {
                    string pre2YM = DateTime.Parse($"{_YMD.Substring(0, 4)}-{_YMD.Substring(4, 2)}-{_YMD.Substring(6, 2)}").AddMonths(-2).ToString("yyyyMM");
                    SqlComm = $"SELECT * FROM WP_pdStkQtyYM WHERE isDel='N' AND YearMonth='{pre2YM}'";
                    DataTable preStkQtyTbl = Global.getTbl.table("WP", SqlComm);

                    if (preStkQtyTbl.Rows.Count == 0) { return "error"; }
                    else
                    {
                        SqlComm = $"SELECT PNo, SUM(qty) AS qty FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(InStkId, 1, 6)='{preYM}' GROUP BY PNo";
                        DataTable iStk = Global.getTbl.table("WP", SqlComm);

                        SqlComm = $"SELECT PNo, SUM(qty) AS qty FROM WP_vOutStock WHERE isDel='N' AND dtlIsDel='N' AND SUBSTRING(OutStkId, 1, 6)='{preYM}' GROUP BY PNo";
                        DataTable oStk = Global.getTbl.table("WP", SqlComm);

                        DataRow[] preStkRow, iStkRow, oStkRow;

                        foreach (DataRow row in pdTbl.Rows)
                        {
                            preStkRow = preStkQtyTbl.Select($"PNo='{row["PNo"]}'");
                            iStkRow = iStk.Select($"PNo='{row["PNo"]}'");
                            oStkRow = oStk.Select($"PNo='{row["PNo"]}'");
                            qty = preStkRow.Length != 0 ? int.Parse($"{preStkRow[0]["qty"]}") : 0;
                            qty += iStkRow.Length != 0 ? int.Parse($"{iStkRow[0]["qty"]}") : 0;
                            qty -= oStkRow.Length != 0 ? int.Parse($"{oStkRow[0]["qty"]}") : 0;

                            SqlComm = minSn == "empty"
                                ? $"INSERT INTO WP_pdStkQtyYM (pNo, qty, YearMonth) VALUES ('{row["pNo"]}', '{qty}', '{preYM}')"
                                : $"UPDATE WP_pdStkQtyYM SET pNo='{row["pNo"]}', qty='{qty}', YearMonth='{preYM}', isDel='N', timeUpdate=GETDATE() WHERE sn='{minSn}'";
                            Global.getTbl.updTbl("WP", SqlComm);
                            minSn = minSn == "empty" ? minSn : GetNewSn(delStkQtyTbl, minSn);
                        }
                    }
                }
            }
        }
        return "Y";
    }

    /// <summary>
    /// 取得已刪除檔最小sn
    /// </summary>
    /// <param name="_delTbl"></param>   已刪除交易檔
    /// <param name="_minDelSn"></param> 目前已使用最小sn
    /// <returns></returns>
    private string GetNewSn(DataTable _delTbl, string _minDelSn)
    {
        string newminDelSn;
        if (_minDelSn == "empty") { newminDelSn = _minDelSn; }
        else
        {
            DataRow[] newRows = _delTbl.Select($"sn>{_minDelSn}", "sn ASC");
            newminDelSn = (newRows.Length == 0) ? "empty" : $"{newRows[0]["sn"]}";
        }
        return newminDelSn;
    }
</script>

