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
                if (!emp.IsEmp) { Response.Write("not-emp"); }
                else
                {
                    string snGrp = "";
                    if (emp.EmpIsAdmin == "N") { Response.Write("UA"); }    //非管理者不能變更
                    else
                    {
                        double amtNoneTax = 0;
                        switch (Request.Form["args0"])
                        {
                            #region C12 新增銷貨單
                            case "C12":
                                #region 計算未結金額
                                SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N'";
                                DataTable payTbl = Global.getTbl.table("WP", SqlComm);
                                JArray payJSON = JArray.Parse(Request.Form["payJSON"]);
                                double amtAccInY = 0, amtAccInN = 0;
                                foreach (var pay in payJSON)
                                {
                                    DataRow[] payRows = payTbl.Select($"PayKId='{pay["PAYID"]}'");
                                    if ($"{payRows[0]["isAcctIn"]}" == "Y")
                                        amtAccInY += double.Parse($"{pay["PAYAMT"]}");  //未結金額(產生應收帳款)
                                    else
                                        amtAccInN += double.Parse($"{pay["PAYAMT"]}");  //已結金額
                                }
                                string outType = amtAccInY == 0 ? "2" : amtAccInN == 0 ? "0" : "1";     //結帳狀態(0:未結/1:未結完/2:全結)
                                #endregion

                                string OutStkId = stock.OutStkMaxId(Request.Form["OutStkDate"]);      //取得銷貨單號
                                JArray pdLstJSON = JArray.Parse(Request.Form["pdList"]);
                                int pdCount = 0;
                                string pNoGrp = "";
                                double mAmtNoneTax = 0;
                                foreach (var pd in pdLstJSON.GroupBy(x => x["pNo"]).Select(group => group.First()))
                                {
                                    pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
                                    pdCount++;
                                }

                                SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
                                pdTbl = Global.getTbl.table("WP", SqlComm);

                                if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount) { Response.Write("Z"); }
                                else
                                {
                                    SqlComm = $"SELECT * FROM WP_vPdCombine WHERE pNo IN ({pNoGrp})";   //組合商品
                                    DataTable pdCbTbl = Global.getTbl.table("WP", SqlComm);

                                    double pdAmt, pdAmtNoneTax, pdAmtTotal, mAmtTotal = 0;
                                    int pdQty;
                                    string mIsTax = "N", payKind = Request.Form["payKind"];
                                    foreach (JToken pd in pdLstJSON.OrderBy(x => x["pNo"]))
                                    {
                                        pdAmt = double.Parse($"{pd["amount"]}");
                                        pdQty = int.Parse($"{pd["qty"]}");

                                        SqlComm = $"SELECT * FROM WP_Product WHERE pNo='{pd["pNo"]}'";      //重新抓取商品檔，以確認讀取更後的最新資料
                                        DataRow row = Global.getTbl.table("WP", SqlComm).Rows[0];

                                        mIsTax = row["isTax"].ToString() == "Y" ? "Y" : mIsTax;   //整張銷貨單商品都免稅才免稅
                                        pdAmtNoneTax = row["isTax"].ToString() == "N" ? pdAmt : toDouble.Numer(pdAmt * (1 - Global.TaxPercent), 2);  //商品未稅單價

                                        mAmtNoneTax += pdAmtNoneTax * pdQty;     //銷貨單未稅總額
                                        pdAmtTotal = int.Parse($"{pd["amtTotal"]}");       //商品小計
                                        mAmtTotal += pdAmtTotal;       //銷貨單總額
                                        SqlComm = "INSERT INTO WP_OutStockDtl (OutStkId, pNo, amount, amtNoneTax, qty, " +
                                            "amtTotal, isTax, costStd, outType, outLeft, " +
                                            "outDate, pdLimitDate) " +
                                            $"VALUES ('{OutStkId}', '{pd["pNo"]}', '{pd["amount"]}', '{pdAmtNoneTax}', '{pd["qty"]}', " +
                                            $"'{pdAmtTotal}', '{row["isTax"]}', '{row["costStd"]}', '{(payKind == "0" ? "2" : "0")}', '{(payKind == "0" ? 0 : pdAmtTotal)}', " +
                                            $"{(payKind == "0" ? "GETDATE()" : "'1911/1/1'")}, '{pd["pdLimitDate"]}')";
                                        Global.getTbl.updTbl("WP", SqlComm);

                                        if ($"{row["isUpdStock"]}" == "Y")
                                        {
                                            SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({pdQty}) WHERE pNo='{pd["pNo"]}'";    //更新商品數量
                                            Global.getTbl.updTbl("WP", SqlComm);
                                        }

                                        OutPdCb(pdCbTbl, pd, OutStkId);
                                    }

                                    SqlComm = $"INSERT INTO WP_OutStock (OutStkId, OutStkDate, memSn, amount, tax, " +
                                            "amtNoneTax, isTax, empId, outType, outLeft, " +
                                            "PayList, reciptNo, amtCargo, amtCoupon) " +
                                            $"VALUES ('{OutStkId}', '{Request.Form["OutStkDate"]}', '{Request.Form["memSn"]}', '{mAmtTotal}', '{mAmtTotal - (mAmtNoneTax)}', " +
                                            $"'{mAmtNoneTax}', '{mIsTax}', '{emp.EmpId}', '{outType}', '{amtAccInY}', " +
                                            $"'{Request.Form["payJSON"]}', '{Request.Form["reciptNo"]}', '{(Request.Form["amtCargo"] == "" ? "0" : Request.Form["amtCargo"])}', '{(Request.Form["amtCoupon"] == "" ? "0" : Request.Form["amtCoupon"])}')";
                                    Global.getTbl.updTbl("WP", SqlComm);
                                    Response.Write("Y");
                                }
                                break;
                            #endregion
                            #region C12A 修改銷貨單
                            case "C12A":
                                string sn = Request.Form["sn"];      //取得銷貨序號
                                SqlComm = $"SELECT * FROM WP_vOutStock WHERE sn = {sn}";
                                DataTable outStkTbl = Global.getTbl.table("WP", SqlComm);
                                if (outStkTbl.Rows.Count == 0) { Response.Write("E"); }
                                else
                                {
                                    OutStkId = outStkTbl.Rows[0]["OutStkId"].ToString();    //銷貨單號                                                    
                                    pdLstJSON = JArray.Parse(Request.Form["pdList"]);
                                    pdCount = 0;
                                    pNoGrp = "";
                                    snGrp = "";
                                    outType = "";
                                    string payKind = outStkTbl.Rows[0]["payKind"].ToString();    //付款方式
                                    string prePNo = "";
                                    amtNoneTax = 0;
                                    foreach (JToken pd in pdLstJSON.OrderBy(x => x["pNo"]))
                                    {
                                        if (prePNo != pd["pNo"].ToString())
                                        {
                                            pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
                                            pdCount++;
                                            prePNo = pd["pNo"].ToString();
                                        }
                                        snGrp += $"{(pd["dtlSn"].ToString() == "new" ? "" : $"{(snGrp == "" ? "" : ",")}{pd["dtlSn"]}")}";
                                    }

                                    #region 處理已刪除商品
                                    SqlComm = $"SELECT SUM(qty) AS qty, pNo FROM WP_OutStockDtl WHERE OutStkId='{OutStkId}' AND isDel='N' {(snGrp=="" ? "" : $"AND sn NOT IN ({snGrp})")} GROUP BY pNo ORDER BY pNo";    //刪除不存在的銷貨品項
                                    DataTable delOstkPdTbl = Global.getTbl.table("WP", SqlComm);
                                    foreach(DataRow pd in delOstkPdTbl.Rows)
                                    {
                                        SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow+({pd["qty"]}) WHERE pNo='{pd["pNo"]}'";    //加數量回庫存
                                        Global.getTbl.updTbl("WP", SqlComm);
                                    }
                                    //刪除商品
                                    SqlComm = $"UPDATE WP_OutStockDtl SET isDel='Y', timeUpdate=GETDATE() WHERE OutStkId='{OutStkId}' AND isDel='N' {(snGrp=="" ? "" : $"AND sn NOT IN ({snGrp})")}";    //刪除不存在的銷貨品項
                                    Global.getTbl.updTbl("WP", SqlComm);
                                    #endregion

                                    SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                    if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount) { Response.Write("Z"); }
                                    else
                                    {
                                        double pdAmt, pdAmtNoneTax, pdAmtTotal, amtTotal = 0, outLeft = 0, pdOutLeft = 0;
                                        string isTax = "N";
                                        int pdQty, oldPdQty;
                                        foreach (var pd in pdLstJSON)
                                        {
                                            pdAmt = double.Parse($"{pd["amount"]}");
                                            pdQty = int.Parse($"{pd["qty"]}");

                                            SqlComm = $"SELECT * FROM WP_Product WHERE pNo='{pd["pNo"]}'";      //重新抓取商品檔，以確認讀取更後的最新資料
                                            DataRow row = Global.getTbl.table("WP", SqlComm).Rows[0];

                                            isTax = $"{row["isTax"]}" == "Y" ? "Y" : isTax;   //整張銷貨單商品都免稅才免稅
                                            pdAmtNoneTax = $"{row["isTax"]}" == "N" ? pdAmt : toDouble.Numer(pdAmt * (1 - Global.TaxPercent), Global.pointQty);  //商品未稅單價(免稅品的未稅單價=銷貨價)
                                            amtNoneTax += pdAmtNoneTax * (pdQty);     //銷貨單未稅總額
                                            pdAmtTotal = int.Parse($"{pd["amtTotal"]}");       //商品小計
                                            amtTotal += pdAmtTotal;       //銷貨單總額
                                            if ($"{pd["dtlSn"]}" == "new")
                                            {
                                                oldPdQty = 0;
                                                pdOutLeft = pdAmtTotal;
                                                SqlComm = $"INSERT INTO WP_OutStockDtl " +
                                                        "(OutStkId, pNo, amount, amtNoneTax, qty, " +
                                                            "amtTotal, isTax, costStd, outType, outLeft, " +
                                                            "outDate, pdLimitDate) " +
                                                        $"VALUES ('{OutStkId}', '{pd["pNo"]}', '{pdAmt}', '{pdAmtNoneTax}', '{pdQty}', " +
                                                            $"'{pdAmtTotal}', '{row["isTax"]}', '{row["costStd"]}', '{(payKind == "0" ? "2" : "0")}', '{(payKind == "0" ? 0 : pdAmtTotal)}', " +
                                                            $"{(payKind == "0" ? "GETDATE()" : "'1911/1/1'")}, '{pd["pdLimitDate"]}')";
                                            }
                                            else
                                            {
                                                oldPdQty = int.Parse(outStkTbl.Select($"dtlSn='{pd["dtlSn"]}'")[0]["qty"].ToString());
                                                SqlComm = $"UPDATE WP_OutStockDtl SET " +
                                                        $"amount='{pd["amount"]}', amtNoneTax='{pdAmtNoneTax}', qty='{pdQty}', amtTotal='{pdAmtTotal}', isTax='{row["isTax"]}', " +
                                                        $"outType='{(payKind == "0" ? "2" : "0")}', outLeft='{(payKind == "0" ? 0 : pdAmtTotal)}', pdLimitDate='{pd["pdLimitDate"]}' " +
                                                        $"WHERE sn={pd["dtlSn"]}";
                                            }
                                            Global.getTbl.updTbl("WP", SqlComm);
                                            outLeft += pdOutLeft;

                                            SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({pdQty - (oldPdQty)}) WHERE pNo='{pd["pNo"]}'";    //扣庫存
                                            Global.getTbl.updTbl("WP", SqlComm);

                                        }

                                        JArray outTypeJSON = JArray.Parse(chkOutType(Request.Form["payJSON"]));
                                        SqlComm = $"UPDATE WP_OutStock SET " +
                                                $"amount='{amtTotal}', tax='{amtTotal - amtNoneTax}', amtNoneTax='{amtNoneTax}', isTax='{isTax}', outType='{outTypeJSON[0]["OutType"]}', " +
                                                $"outLeft='{outTypeJSON[0]["amtAccInY"]}', reciptNo='{Request.Form["reciptNo"]}', amtCargo='{(Request.Form["amtCargo"] == "" ? "0" : Request.Form["amtCargo"])}', amtCoupon='{(Request.Form["amtCoupon"] == "" ? "0" : Request.Form["amtCoupon"])}', " +
                                                $"PayList='{Request.Form["payJSON"]}' " +
                                                $"WHERE sn='{sn}'";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        Response.Write("Y");
                                    }
                                }
                                break;
                            #endregion
                            #region C12B 刪除銷貨單
                            case "C12B":
                                sn = Request.Form["sn"];      //取得銷貨單序號
                                SqlComm = $"SELECT * FROM WP_vOutStock WHERE sn = {sn} AND isDel='N' AND dtlIsDel='N' ORDER BY pNo";
                                outStkTbl = Global.getTbl.table("WP", SqlComm);
                                if (outStkTbl.Rows.Count == 0) { Response.Write("E"); }
                                else
                                {
                                    OutStkId = $"{outStkTbl.Rows[0]["OutStkId"]}";
                                    foreach (DataRow pd in outStkTbl.Rows)     //處理銷貨單每樣商品
                                    {
                                        SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow+({pd["qty"]}) WHERE pNo='{pd["pNo"]}' and isUpdStock='Y'";    //更新商品數量
                                        Global.getTbl.updTbl("WP", SqlComm);
                                    }

                                    SqlComm = $"UPDATE WP_OutStock SET isDel='Y', timeUpdate=GETDATE() WHERE sn='{sn}'";    //刪除銷貨主檔
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    SqlComm = $"UPDATE WP_OutStockDtl SET isDel='Y', timeUpdate=GETDATE() WHERE OutStkId='{OutStkId}' AND isDel='N'";    //刪除存在的銷貨  品項
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    DelPdCb(OutStkId);
                                    Response.Write("Y");
                                }
                                break;
                            #endregion
                            #region C13 接收POS銷貨單
                            case "C13":
                                SqlComm = $"SELECT * FROM WP_OutStockPos ORDER BY OutStkId";
                                DataTable posTbl = Global.getTbl.table("WP", SqlComm);

                                SqlComm = $"SELECT * FROM WP_OutStockPosDtl";
                                DataTable posDtlTbl = Global.getTbl.table("WP", SqlComm);

                                #region 寫入接收日期
                                SqlComm = $"SELECT * FROM WP_PosToDbDate WHERE ptdDate>='{posTbl.Rows[0]["OutStkId"].ToString().Substring(0, 4)}-{posTbl.Rows[0]["OutStkId"].ToString().Substring(4, 2)}-{posTbl.Rows[0]["OutStkId"].ToString().Substring(6, 2)}'";
                                DataTable ptdTbl = Global.getTbl.table("WP", SqlComm);
                                DataTable posDateTbl = posTbl.AsEnumerable()
                                    .GroupBy(r => new { tempDate = r["OutStkId"].ToString().Substring(0, 8) })
                                    .Select(g => g.First())
                                    .OrderBy(o => o["OutStkId"]).CopyToDataTable();

                                string posDateGrp = "";
                                foreach (DataRow row in posDateTbl.Rows)
                                {
                                    posDateGrp = $"{row["OutStkId"].ToString().Substring(0, 4)}-{row["OutStkId"].ToString().Substring(4, 2)}-{row["OutStkId"].ToString().Substring(6, 2)}";
                                    DataRow[] ptdRows = ptdTbl.Select($"ptdDate='{posDateGrp}'", "ptdDate ASC");
                                    SqlComm = ptdRows.Length == 0
                                        ? $"INSERT INTO WP_PosToDbDate (ptdDate) VALUES ('{posDateGrp}')"
                                        : $"UPDATE WP_PosToDbDate SET ptdDate = '{posDateGrp}', isDel='N', timeUpdate=GETDATE() WHERE sn='{ptdRows[0]["sn"]}'";
                                    Global.getTbl.updTbl("WP", SqlComm);
                                }
                                #endregion

                                if (posTbl.Rows.Count == 0) { Response.Write("empty"); }
                                else
                                {
                                    pdCount = 0;
                                    pNoGrp = "";
                                    foreach (var pd in posDtlTbl.AsEnumerable().GroupBy(x => x["pNo"]).Select(group => group.First()))
                                    {
                                        pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
                                        pdCount++;
                                    }
                                    SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
                                    pdTbl = Global.getTbl.table("WP", SqlComm);

                                    SqlComm = $"SELECT * FROM WP_vPdCombine WHERE pNo IN ({pNoGrp})";
                                    DataTable pdCbTbl = Global.getTbl.table("WP", SqlComm);
                                    if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount) { Response.Write($"pdTbl.Rows.Count={pdTbl.Rows.Count}\npNoGrp={pNoGrp}\npdCount={pdCount}\nZ"); }
                                    else
                                    {
                                        JArray posJSON;
                                        SqlComm = $"SELECT * FROM WP_Member WHERE isStop='N'";
                                        DataTable memTbl = Global.getTbl.table("WP", SqlComm);      //會員資料檔

                                        #region 處理已存在後台資料
                                        SqlComm = $"SELECT * FROM WP_OutStockPos WHERE (OutStkId IN (SELECT PosOutStkId FROM WP_OutStock WHERE isDel='N')) ORDER BY OutStkId";
                                        DataTable posOldTbl = Global.getTbl.table("WP", SqlComm);      //前台銷貨主檔

                                        int recNo, recCbNo;
                                        string memSn;
                                        bool isCbPd;
                                        DataRow[] memRows, posDtlRows, dtlOldRows, dtlOldCbRows;
                                        DataRow pdRow;
                                        if (posOldTbl.Rows.Count > 0)
                                        {
                                            ClearPosDel();      //前台重新上傳時已作癈銷貨單不傳，故將作癈的訂單刪除
                                            SqlComm = $"SELECT * FROM WP_vOutStock WHERE (PosOutStkId IN (SELECT OutStkId FROM WP_OutStockPos)) ORDER BY OutStkId";
                                            DataTable oldTbl = Global.getTbl.table("WP", SqlComm);      //後台已存在銷貨檔

                                            SqlComm = $"SELECT * FROM WP_vOutStockCb WHERE (PosOutStkId IN (SELECT OutStkId FROM WP_OutStockPos)) ORDER BY OutStkId";
                                            DataTable oldCbTbl = Global.getTbl.table("WP", SqlComm);      //後台已存在銷貨組合商品明細檔

                                            foreach (DataRow rowPos in posOldTbl.Rows)
                                            {
                                                OutStkId = $"{oldTbl.Select($"PosOutStkId='{rowPos["OutStkId"]}'")[0]["OutStkId"]}";      //取得後台銷貨單號
                                                delOutStk(OutStkId);    //先刪除該訂單

                                                if ($"{rowPos["isDel"]}" == "N")    //前台訂單已刪除則不處理
                                                {
                                                    #region 更新明細檔
                                                    posDtlRows = posDtlTbl.Select($"OutStkId='{rowPos["OutStkId"]}' AND isDel='N'", "pNo ASC");   //前台明細檔
                                                    dtlOldRows = oldTbl.Select($"OutStkId='{OutStkId}'", "dtlSn ASC");         //後台明細檔
                                                    dtlOldCbRows = oldCbTbl.Select($"OutStkId='{OutStkId}'", "dtlSn ASC");     //後台組合商品明細檔
                                                    recNo = 0;
                                                    recCbNo = 0;
                                                    foreach (DataRow pd in posDtlRows)
                                                    {
                                                        pdRow = pdTbl.Select($"pNo='{pd["pNo"]}'")[0];
                                                        if (recNo <= (dtlOldRows.Length - 1))
                                                        {
                                                            SqlComm = $"UPDATE WP_OutStockDtl SET OutStkId='{OutStkId}', pNo='{pd["pNo"]}', amount='{pd["amount"]}', amtNoneTax='{pd["amtNoneTax"]}', qty='{pd["qty"]}', " +
                                                                $"amtTotal='{pd["amtTotal"]}', isTax='{pd["isTax"]}', costStd='{pd["costStd"]}', outType='{pd["outType"]}', outLeft='{pd["outLeft"]}', " +
                                                                $"isDel='N', discount='{pd["discount"]}', discountPer='{pd["discountPer"]}', discountShare='{pd["discountShare"]}' " +
                                                                $"WHERE sn={dtlOldRows[recNo]["dtlSn"]}";
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                            recNo++;
                                                        }
                                                        else
                                                        {
                                                            SqlComm = "INSERT INTO WP_OutStockDtl (OutStkId, pNo, amount, amtNoneTax, qty, " +
                                                                "amtTotal, isTax, " +
                                                                "discount, discountPer, discountShare) " +
                                                                $"VALUES ('{OutStkId}', '{pd["pNo"]}', '{pd["amount"]}', '{pd["amtNoneTax"]}', '{pd["qty"]}', " +
                                                                $"'{pd["amtTotal"]}', '{pd["isTax"]}', " +
                                                                $"'{pd["discount"]}', '{pd["discountPer"]}', '{pd["discountShare"]}')";
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                        }

                                                        isCbPd = pdCbTbl.Rows.Count > 0 ? pdCbTbl.Select($"pNo={pd["pNo"]}").Length > 0 ? true : false : false;

                                                        if (isCbPd)
                                                        {
                                                            foreach (DataRow pdCb in pdCbTbl.Select($"pNo={pd["pNo"]}"))
                                                            {
                                                                if (recCbNo <= (dtlOldCbRows.Length - 1))       //已存在組合商品明細檔
                                                                {
                                                                    SqlComm = $"UPDATE WP_OutStockDtlCb SET OutStkId='{OutStkId}', pNo='{pdCb["pNoS"]}', qty='{(int.Parse($"{pd["qty"]}") * (int.Parse($"{pdCb["pQty"]}")))}', amount='{(double.Parse($"{pd["amount"]}") / (int.Parse($"{pdCb["pQty"]}")))}', " +
                                                                        $"amtTotal='{pd["amtTotal"]}', amtNoneTax='{(double.Parse($"{pd["amtNoneTax"]}") / (int.Parse($"{pdCb["pQty"]}")))}', isTax='{pd["isTax"]}', isDel='N', timeUpdate=GETDATE(), " +
                                                                        $"discount='{pd["discount"]}', discountPer='{pd["discountPer"]}', discountShare='{pd["discountShare"]}' " +
                                                                        $"WHERE sn={dtlOldCbRows[recCbNo]["dtlSn"]}";
                                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                                    recCbNo++;
                                                                }
                                                                else
                                                                {
                                                                    SqlComm = "INSERT INTO WP_OutStockDtlCb (OutStkId, pNo, qty, amount, " +
                                                                        "amtTotal, amtNoneTax, isTax, " +
                                                                        "discount, discountPer, discountShare) " +
                                                                        $"VALUES ('{OutStkId}', '{pdCb["pNoS"]}', '{(int.Parse($"{pd["qty"]}") * (int.Parse($"{pdCb["pQty"]}")))}', '{(double.Parse($"{pd["amount"]}") / (int.Parse($"{pdCb["pQty"]}")))}', " +
                                                                        $"'{pd["amtTotal"]}', '{(double.Parse($"{pd["amtNoneTax"]}") / (int.Parse($"{pdCb["pQty"]}")))}', '{pd["isTax"]}', " +
                                                                        $"'{pd["discount"]}', '{pd["discountPer"]}', '{pd["discountShare"]}')";
                                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                                }

                                                                if ($"{pdCb["sIsUpdStock"]}" == "Y")
                                                                {
                                                                    SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({(int.Parse($"{pd["qty"]}") * (int.Parse($"{pdCb["pQty"]}")))}) WHERE pNo='{pdCb["pNoS"]}'";    //更新商品數量
                                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                                }

                                                            }
                                                        }
                                                        else
                                                        {
                                                            if ($"{pdRow["isUpdStock"]}" == "Y")
                                                            {
                                                                SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({pd["qty"]}) WHERE pNo='{pd["pNo"]}'";    //更新商品數量
                                                                Global.getTbl.updTbl("WP", SqlComm);
                                                            }
                                                        }
                                                    }
                                                    #endregion

                                                    #region 更新主檔
                                                    memRows = memTbl.Select($"memId='{rowPos["memSn"]}'");     //取得會員序號
                                                    memSn = $"{rowPos["memSn"]}" == "" ? "0" : memRows.Length == 0 ? "0" : $"{memRows[0]["sn"]}";

                                                    posJSON = JArray.Parse(transPayList($"{rowPos["PayList"]}"));
                                                    SqlComm = $"UPDATE WP_OutStock SET OutStkId='{OutStkId}', OutStkDate='{rowPos["OutStkDate"]:yyyy-MM-dd}', memSn='{memSn}', amount='{rowPos["amount"]}', tax='{rowPos["tax"]}', " +
                                                        $"amtNoneTax='{rowPos["amtNoneTax"]}', isTax='{rowPos["isTax"]}', isDel='N', empId='{rowPos["empId"]}', outType='{posJSON[0]["OutType"]}', outLeft='{posJSON[0]["amtAccInY"]}', " +
                                                        $"PayList=N'{$"{posJSON[0]["payList"]}".Replace("\'", "\"")}', reciptNo='{rowPos["reciptNo"]}', PosOutStkId='{rowPos["OutStkId"]}', machId='{rowPos["machId"]}', " +
                                                        $"discount='{rowPos["discount"]}', discountShare='{rowPos["discountShare"]}' " +
                                                        $"WHERE OutStkId='{OutStkId}'";
                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                    #endregion

                                                }
                                            }
                                        }
                                        #endregion

                                        #region 未存在後台資料
                                        SqlComm = $"SELECT * FROM WP_OutStockPos WHERE (OutStkId NOT IN (SELECT PosOutStkId FROM WP_OutStock)) ORDER BY OutStkId";
                                        DataTable oStkPosNewTbl = Global.getTbl.table("WP", SqlComm);
                                        foreach (DataRow rowPos in oStkPosNewTbl.Rows)
                                        {
                                            OutStkId = stock.OutStkMaxId($"{rowPos["OutStkDate"]}");      //取得銷貨單號

                                            posDtlRows = posDtlTbl.Select($"OutStkId='{rowPos["OutStkId"]}' AND isDel='N'", "pNo ASC");
                                            foreach (DataRow pd in posDtlRows)
                                            {
                                                pdRow = pdTbl.Select($"pNo='{pd["pNo"]}'")[0];
                                                //加入銷貨明細檔
                                                SqlComm = "INSERT INTO WP_OutStockDtl (OutStkId, pNo, amount, amtNoneTax, qty, " +
                                                    "amtTotal, isTax, discount, discountPer, discountShare) " +
                                                    $"VALUES ('{OutStkId}', '{pd["pNo"]}', '{pd["amount"]}', '{pd["amtNoneTax"]}', '{pd["qty"]}', " +
                                                    $"'{pd["amtTotal"]}', '{pd["isTax"]}', '{pd["discount"]}', '{pd["discountPer"]}', '{pd["discountShare"]}')";
                                                Global.getTbl.updTbl("WP", SqlComm);

                                                isCbPd = pdCbTbl.Rows.Count > 0 ? pdCbTbl.Select($"pNo={pd["pNo"]}").Length > 0 ? true : false : false;
                                                if (isCbPd)
                                                {
                                                    recCbNo = 0;
                                                    foreach (DataRow pdCb in pdCbTbl.Select($"pNo={pd["pNo"]}"))
                                                    {
                                                        SqlComm = "INSERT INTO WP_OutStockDtlCb (OutStkId, pNo, qty, amount, " +
                                                            "amtTotal, amtNoneTax, isTax, discount, discountPer, discountShare) " +
                                                            $"VALUES ('{OutStkId}', '{pdCb["pNoS"]}', '{(int.Parse($"{pd["qty"]}") * (int.Parse($"{pdCb["pQty"]}")))}', '{(double.Parse($"{pd["amount"]}") / (int.Parse($"{pdCb["pQty"]}")))}', " +
                                                            $"'{pd["amtTotal"]}', '{(double.Parse($"{pd["amtNoneTax"]}") / (int.Parse($"{pdCb["pQty"]}")))}', '{pd["isTax"]}', " +
                                                            $"'{pd["discount"]}', '{pd["discountPer"]}', '{pd["discountShare"]}')";
                                                        Global.getTbl.updTbl("WP", SqlComm);

                                                        if ($"{pdCb["sIsUpdStock"]}" == "Y")
                                                        {
                                                            SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({(int.Parse($"{pd["qty"]}") * (int.Parse($"{pdCb["pQty"]}")))}) WHERE pNo='{pdCb["pNoS"]}'";    //更新商品數量
                                                            Global.getTbl.updTbl("WP", SqlComm);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if ($"{pdRow["isUpdStock"]}" == "Y")
                                                    {
                                                        SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({pd["qty"]}) WHERE pNo='{pd["pNo"]}'";    //更新商品數量
                                                        Global.getTbl.updTbl("WP", SqlComm);
                                                    }
                                                }
                                            }

                                            memRows = memTbl.Select($"memId='{rowPos["memSn"]}'");     //取得會員序號
                                            memSn = $"{rowPos["memSn"]}" == "" ? "0" : memRows.Length == 0 ? "0" : $"{memRows[0]["sn"]}";
                                            //加入銷貨主檔
                                            posJSON = JArray.Parse(transPayList($"{rowPos["PayList"]}"));
                                            SqlComm = $"INSERT INTO WP_OutStock (OutStkId, OutStkDate, memSn, amount, tax, " +
                                                    "amtNoneTax, isTax, empId, outType, outLeft, " +
                                                    "PayList, reciptNo, PosOutStkId, machId, " +
                                                    "discount, discountShare) " +
                                                    $"VALUES ('{OutStkId}', '{rowPos["OutStkDate"]:yyyy-MM-dd} {rowPos["OutStkTime"]:00}', '{memSn}', '{rowPos["amount"]}', '{rowPos["tax"]}', " +
                                                    $"'{rowPos["amtNoneTax"]}', '{rowPos["isTax"]}', '{rowPos["empId"]}', '{posJSON[0]["OutType"]}', '{posJSON[0]["amtAccInY"]}', " +
                                                    $"'{$"{posJSON[0]["payList"]}".Replace("\'", "\"")}', '{rowPos["reciptNo"]}', '{rowPos["OutStkId"]}', '{rowPos["machId"]}', " +
                                                    $"'{rowPos["discount"]}', '{rowPos["discountShare"]}')";
                                            Global.getTbl.updTbl("WP", SqlComm);
                                        }
                                        #endregion

                                        BackupPos();
                                        Response.Write("Y");
                                    }
                                }

                                break;
                            #endregion
                            #region C14 應收帳款銷帳
                            case "C14":
                                string AcctInId = stock.AcctInMaxId(Request.Form["AcctInDate"]);      //取得銷帳單號
                                string kind = Request.Form["kind"];
                                switch (kind) {
                                    #region 依訂單銷帳
                                    case "A":
                                        JArray oStkLstJSON = JArray.Parse(Request.Form["ouStkList"]);
                                        int oStkCount = 0;
                                        snGrp = "";
                                        foreach (var oStk in oStkLstJSON)
                                        {
                                            snGrp += $"{(snGrp == "" ? "" : ",")}{oStk["sn"]}";
                                            oStkCount++;
                                        }

                                        SqlComm = $"SELECT * FROM WP_OutStock WHERE sn IN ({snGrp}) ORDER BY OutStkId";
                                        outStkTbl = Global.getTbl.table("WP", SqlComm);
                                        if (outStkTbl.Rows.Count == 0 || outStkTbl.Rows.Count != oStkCount) { Response.Write("Z"); }
                                        else
                                        {
                                            double amtTotal = 0;
                                            foreach (var oStk in oStkLstJSON)
                                            {
                                                DataRow[] row = outStkTbl.Select($"sn={oStk["sn"]}");
                                                amtTotal += double.Parse($"{row[0]["amount"]}");
                                                SqlComm = $"INSERT INTO WP_AcctInDtl (acctInId, OutStkId, amount, amtNoneTax, isTax) " +
                                                        $"VALUES ('{AcctInId}', '{row[0]["OutStkId"]}', '{row[0]["amount"]}', '{row[0]["amtNoneTax"]}', '{row[0]["isTax"]}')";
                                                Global.getTbl.updTbl("WP", SqlComm);
                                                SqlComm = $"UPDATE WP_OutStockDtl SET outType='2', outLeft='0', timeUpdate=GETDATE() WHERE OutStkId='{row[0]["OutStkId"]}' AND isDel='N'";
                                                Global.getTbl.updTbl("WP", SqlComm);
                                            }

                                            SqlComm = $"UPDATE WP_OutStock SET outType='2', outLeft='0', timeUpdate=GETDATE() WHERE sn IN ({snGrp})";
                                            Global.getTbl.updTbl("WP", SqlComm);

                                            DataRow row0 = outStkTbl.Rows[0];
                                            SqlComm = $"INSERT INTO WP_AcctIn " +
                                                    "(acctInId, acctInDate, amount, empId) " +
                                                    $"VALUES ('{AcctInId}', '{Request.Form["AcctInDate"]}', '{amtTotal}', '{emp.EmpId}')";
                                            Global.getTbl.updTbl("WP", SqlComm);

                                            Response.Write("Y");

                                        }
                                        break;
                                    #endregion
                                    #region 依金額銷帳
                                    case "B":
                                        string mem = Request.Form["mem"];
                                        int memAmt = int.Parse(Request.Form["memAmt"]),
                                            amtLeft = memAmt;
                                        SqlComm = $"SELECT * FROM WP_OutStock WHERE isDel='N' AND memSn={mem} AND outType<>'2' ORDER BY OutStkId";
                                        outStkTbl = Global.getTbl.table("WP", SqlComm);
                                        if (outStkTbl.Rows.Count == 0) { Response.Write("Z"); }
                                        else
                                        {
                                            int amt;
                                            foreach (DataRow row in outStkTbl.Rows)
                                            {
                                                amt = int.Parse($"{row["outLeft"]:#0}");
                                                SqlComm = $"INSERT INTO WP_AcctInDtl (acctInId, OutStkId, amount, amtNoneTax, isTax) " +
                                                        $"VALUES ('{AcctInId}', '{row["OutStkId"]}', '{(amtLeft >= amt ? amt : amtLeft)}', '{row["amtNoneTax"]}', '{row["isTax"]}')";
                                                //Response.Write($"{SqlComm}\n");
                                                Global.getTbl.updTbl("WP", SqlComm);

                                                SqlComm = $"UPDATE WP_OutStock SET outType='{(amtLeft >= amt ? 2 : 1)}', outLeft='{(amtLeft >= amt ? 0 : (amt - (amtLeft)))}', timeUpdate=GETDATE() WHERE OutStkId='{row["OutStkId"]}'";
                                                //Response.Write($"{SqlComm}\n");
                                                Global.getTbl.updTbl("WP", SqlComm);
                                                amtLeft -= amt;
                                                if (amtLeft <= 0) { break; }
                                            }

                                            SqlComm = $"INSERT INTO WP_AcctIn " +
                                                    "(acctInId, acctInDate, amount, empId) " +
                                                    $"VALUES ('{AcctInId}', '{Request.Form["AcctInDate"]}', '{memAmt}', '{emp.EmpId}')";
                                            //Response.Write($"{SqlComm}\n");
                                            Global.getTbl.updTbl("WP", SqlComm);

                                            Response.Write("Y");

                                        }
                                        break;
                                    #endregion
                                    default:
                                        Response.Write("error");
                                        break;
                                }
                                break;
                            #endregion
                            #region C14B 刪除應收帳款銷帳單
                            case "C14B":
                                sn = Request.Form["sn"];      //取得應付帳銷帳序號
                                SqlComm = $"SELECT * FROM WP_vAcctIn WHERE isDel='N' AND sn = {sn}";
                                DataTable acctInTbl = Global.getTbl.table("WP", SqlComm);
                                if (acctInTbl.Rows.Count == 0) { Response.Write("E"); }
                                else
                                {
                                    SqlComm = $"SELECT * FROM WP_OutStock WHERE isDel='N' AND sn IN (SELECT oStkSn FROM WP_vAcctIn WHERE isDel='N' AND sn = {sn})";
                                    DataTable ostkTbl = Global.getTbl.table("WP", SqlComm);
                                    int jsonOutLeft, outLeft, acctLeft, oStkLeft;
                                    string preDtlSn = "";
                                    foreach (DataRow row in acctInTbl.Rows)
                                    {
                                        if (preDtlSn != $"{row["dtlSn"]}")
                                        {
                                            DataRow[] ostkRow = ostkTbl.Select($"sn='{row["oStkSn"]}'");
                                            JArray outTypeJSON = JArray.Parse(chkOutType($"{ostkRow[0]["PayList"]}"));

                                            jsonOutLeft = int.Parse($"{outTypeJSON[0]["amtAccInY"]}");
                                            //Response.Write($"outLeft={jsonOutLeft}\n");
                                            acctLeft = int.Parse($"{row["outStkAmtTotal"]:#0}");
                                            oStkLeft = int.Parse($"{ostkRow[0]["outLeft"]:#0}");
                                            //Response.Write($"acctLeft={acctLeft}\n");
                                            //Response.Write($"outLeft == acctLeft ={jsonOutLeft == acctLeft}\n");
                                            outLeft = jsonOutLeft == acctLeft ? jsonOutLeft : (oStkLeft + acctLeft);
                                            outType = jsonOutLeft == (oStkLeft + acctLeft) ? $"{outTypeJSON[0]["OutType"]}" : "1";

                                            SqlComm = $"UPDATE WP_OutStock SET " +
                                                    $"outType='{outType}', outLeft='{outLeft}', timeUpdate=GETDATE() " +
                                                    $"WHERE sn='{row["oStkSn"]}'";
                                            //Response.Write($"SqlComm={SqlComm}\n");
                                            Global.getTbl.updTbl("WP", SqlComm);
                                            preDtlSn = $"{row["dtlSn"]}";
                                        }
                                    }

                                    SqlComm = $"UPDATE WP_AcctInDtl SET isDel='Y', timeUpdate=GETDATE() WHERE sn IN ( SELECT dtlSn FROM WP_vAcctIn WHERE isDel='N' AND sn = {sn} )";
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    SqlComm = $"UPDATE WP_AcctIn SET isDel='Y', timeUpdate=GETDATE() WHERE sn = {sn}";
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    Response.Write("Y");
                                }
                                break;
                            #endregion
                            #region C15 商品庫存調整新增
                            case "C15":
                                string stkDate = Request.Form["stkDate"],
                                pNo = Request.Form["pNo"],
                                cost = Request.Form["cost"],
                                result;
                                int qty = int.Parse(Request.Form["qty"]);

                                SqlComm = $"SELECT * FROM WP_Product WHERE pNo = '{pNo}'";
                                pdTbl = Global.getTbl.table("WP", SqlComm);
                                if (pdTbl.Rows.Count == 0)
                                    result = "Z";
                                else
                                {
                                    SqlComm = $"SELECT * FROM WP_StkUpd WHERE pNo = '{pNo}' AND (CONVERT(datetime, '{stkDate}', 111) = stkDate)";
                                    DataTable updTbl = Global.getTbl.table("WP", SqlComm);

                                    int qtyDiff = 0;
                                    if (updTbl.Rows.Count > 0)
                                    {
                                        DataRow row0 = updTbl.Rows[0];
                                        if ($"{row0["isDel"]}" == "N")
                                            result = "A";
                                        else
                                        {
                                            qtyDiff = qty - (int.Parse($"{pdTbl.Rows[0]["qtyNow"]}"));
                                            SqlComm = $"UPDATE WP_StkUpd SET isDel='N', timeUpdate=GETDATE(), qty='{qty}', qtyDiff='{qtyDiff}', cost='{cost}' WHERE sn={row0["sn"]}";
                                            Global.getTbl.updTbl("WP", SqlComm);
                                            result = "Y";
                                        }
                                    }
                                    else
                                    {
                                        qtyDiff = qty - (int.Parse($"{pdTbl.Rows[0]["qtyNow"]}"));
                                        SqlComm = $"INSERT INTO WP_StkUpd " +
                                                "(stkDate, pNo, qty, qtyDiff, cost) " +
                                                $"VALUES ('{stkDate}', '{pNo}', '{qty}', '{qtyDiff}', '{cost}')";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        result = "Y";
                                    }

                                    if (result == "Y")
                                    {
                                        string pdList, payList;
                                        if (qtyDiff > 0)
                                        {
                                            pdList = $"[{{'pNo':'{pNo}','amount':'0','qty':'{qtyDiff}','amtTotal':'0','pdLimitDate':''}}]";
                                            result = new InStk().InStkAdd(stkDate, pdList, "-1", "");    //pvSn:-1庫存調整
                                        }else if (qtyDiff < 0)
                                        {
                                            payList = $"[{{\"PAYID\":\"01\",\"PAYAMT\":\"0\"}}]";
                                            pdList = $"[{{'pNo':'{pNo}','amount':'0','qty':'{qtyDiff*(-1)}','amtTotal':'0','pdLimitDate':''}}]";
                                            result = new OutStk().OutStkAdd(stkDate, payList, pdList, "-1", "");
                                        }
                                    }
                                    Response.Write(result);
                                }
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


    /// <summary>
    /// 計算未結金額
    /// </summary>
    /// <param name="_payJSON"></param>付款方式JASON
    private string chkOutType(string _payJSON)
    {
        string SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N'";
        DataTable payTbl = Global.getTbl.table("WP", SqlComm);
        JArray payJSON = JArray.Parse(_payJSON);
        double amtAccInY = 0, amtAccInN = 0;
        foreach (var pay in payJSON)
        {
            DataRow[] payRows = payTbl.Select($"PayKId='{pay["PAYID"]}'");
            if ($"{payRows[0]["isAcctIn"]}" == "Y")
                amtAccInY += double.Parse($"{pay["PAYAMT"]}");  //未結金額(產生應收帳款)
            else
                amtAccInN += double.Parse($"{pay["PAYAMT"]}");  //已結金額
        }
        string OutType = amtAccInY == 0 ? "2" : amtAccInN == 0 ? "0" : "1";     //結帳狀態(0:未結/1:未結完/2:全結)
        //JSON += $"{(JSON == "" ? "" : ",")}{{\"pKLId\":\"{row["pKLId"]}\",\"pKLName\":\"{row["pKLName"]}\",\"pKMId\":\"{row["pKMId"]}\",\"pKMName\":\"{row["pKMName"]}\",\"pKSId\":\"{row["pKSId"]}\",\"pKSName\":\"{row["pKSName"]}\"}}";
        return $"[{{\"OutType\":\"{OutType}\",\"amtAccInY\":\"{amtAccInY}\",\"amtAccInN\":\"{amtAccInN}\"}}]";

    }

    /// <summary>
    /// 轉換前台PayList
    /// </summary>
    /// <param name="_payList"></param>
    private string transPayList(string _payList)
    {
        string SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N'";
        DataTable payTbl = Global.getTbl.table("WP", SqlComm);
        JArray payJSON = JArray.Parse(_payList);
        double amtAccInY = 0, amtAccInN = 0;
        string newPayList = "";
        foreach (var pay in payJSON)
        {
            DataRow payRows = payTbl.Select($"PayKId='{pay["PayKId"]}'")[0];
            if ($"{pay["PayAmt"]}" != "0")
            {
                newPayList += $"{(newPayList == "" ? "" : ",")}{{\'PAYID\':\'{payRows["PayKId"]}\',\'PAYAMT\':\'{pay["PayAmt"]}\'}}";
                if ($"{payRows["isAcctIn"]}" == "Y")
                    amtAccInY += double.Parse($"{pay["PayAmt"]}");  //未結金額(產生應收帳款)
                else
                    amtAccInN += double.Parse($"{pay["PayAmt"]}");  //已結金額
            }
        }
        string OutType = amtAccInY == 0 ? "2" : amtAccInN == 0 ? "0" : "1";     //結帳狀態(0:未結/1:未結完/2:全結)
        return $"[{{\"OutType\":\"{OutType}\",\"amtAccInY\":\"{amtAccInY}\",\"amtAccInN\":\"{amtAccInN}\",\"payList\":\"[{newPayList}]\"}}]";
    }

    /// <summary>
    /// 刪除銷貨明細商品
    /// </summary>
    /// <param name="_OutStkId"></param>
    private void delOutStk(string _OutStkId)
    {
        string SqlComm = $"SELECT * FROM WP_vOutStock WHERE OutStkId = '{_OutStkId}' AND isDel='N' AND dtlIsDel='N' ORDER BY pNo";
        DataTable oStkTbl = Global.getTbl.table("WP", SqlComm);

        SqlComm = $"SELECT * FROM WP_vOutStockCb WHERE OutStkId = '{_OutStkId}' AND isDel='N' AND dtlIsDel='N' ORDER BY pNo";
        DataTable oStkCbTbl = Global.getTbl.table("WP", SqlComm);

        if (oStkTbl.Rows.Count > 0 || oStkCbTbl.Rows.Count > 0)
        {
            foreach (DataRow pd in oStkTbl.Rows)     //處理銷貨單每樣商品
            {
                if ($"{pd["isUpdStock"]}" == "Y")
                {
                    SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow+({pd["qty"]}) WHERE pNo='{pd["pNo"]}'";    //更新商品數量
                    Global.getTbl.updTbl("WP", SqlComm);
                }
            }

            foreach (DataRow pd in oStkCbTbl.Rows)     //處理銷貨單每樣商品
            {
                if ($"{pd["isUpdStock"]}" == "Y")
                {
                    SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow+({pd["qty"]}) WHERE pNo='{pd["pNo"]}'";    //更新商品數量
                    Global.getTbl.updTbl("WP", SqlComm);
                }
            }

            SqlComm = $"UPDATE WP_OutStock SET isDel='Y', timeUpdate=GETDATE() WHERE OutStkId='{_OutStkId}'";    //刪除銷貨主檔
            Global.getTbl.updTbl("WP", SqlComm);

            SqlComm = $"UPDATE WP_OutStockDtl SET isDel='Y', timeUpdate=GETDATE() WHERE OutStkId='{_OutStkId}' AND isDel='N'";    //刪除存在的銷貨商品
            Global.getTbl.updTbl("WP", SqlComm);

            SqlComm = $"UPDATE WP_OutStockDtlCb SET isDel='Y', timeUpdate=GETDATE() WHERE OutStkId='{_OutStkId}' AND isDel='N'";    //刪除存在的組合商品
            Global.getTbl.updTbl("WP", SqlComm);

            //Response.Write("Y");
        }

    }

    /// <summary>
    /// 備份前台POS資料
    /// </summary>
    private void BackupPos()
    {
        GetTbl getTbl = new GetTbl();
        string SqlComm = $"SELECT SUBSTRING(OutStkId, 1, 6) AS YM FROM WP_OutStockPos GROUP BY SUBSTRING(OutStkId, 1, 6) ORDER BY YM";
        DataTable ymTbl = getTbl.table("WP", SqlComm);

        DataTable newTbl;
        foreach (DataRow row in ymTbl.Rows)
        {
            SqlComm = $"SELECT COUNT(*) AS YN FROM sysobjects WHERE name='WP_OutStockPos_{row["YM"]}'";
            newTbl = getTbl.table("WP", SqlComm);
            if ($"{newTbl.Rows[0]["YN"]}" == "0")
            {
                SqlComm = $"SELECT * INTO WP_OutStockPos_{row["YM"]} FROM WP_OutStockPos WHERE SUBSTRING(OutStkId, 1, 6)='{row["YM"]}'";
                getTbl.updTbl("WP", SqlComm);
            }
            else
            {
                SqlComm = $"DELETE FROM WP_OutStockPos_{row["YM"]} WHERE OutStkId IN (SELECT OutStkId FROM WP_OutStockPos WHERE SUBSTRING(OutStkId, 1, 6)='{row["YM"]}');" +
                    $"INSERT INTO WP_OutStockPos_{row["YM"]} (OutStkId, OutStkDate, amount, tax, amtNoneTax, reciptNo, companyId, isTax, isBack, isDel, empId, timeCreate, " +
                        "timeUpdate, memo, memSn, outType, outLeft, payKind, amtCargo, amtCoupon, OutStkTime, PayList, machId, " +
                        "returnReciptNo, discount, discountShare) " +
                    "SELECT OutStkId, OutStkDate, amount, tax, amtNoneTax, reciptNo, companyId, isTax, isBack, isDel, empId, timeCreate, " +
                        "timeUpdate, memo, memSn, outType, outLeft, payKind, amtCargo, amtCoupon, OutStkTime, PayList, machId, " +
                        $"returnReciptNo, discount, discountShare FROM WP_OutStockPos WHERE SUBSTRING(OutStkId, 1, 6)='{row["YM"]}'";
                getTbl.updTbl("WP", SqlComm);
            }
            SqlComm = $"SELECT COUNT(*) AS YN FROM sysobjects WHERE name='WP_OutStockPosDtl_{row["YM"]}'";
            newTbl = getTbl.table("WP", SqlComm);
            if ($"{newTbl.Rows[0]["YN"]}" == "0")
            {
                SqlComm = $"SELECT * INTO WP_OutStockPosDtl_{row["YM"]} FROM WP_OutStockPosDtl";
                getTbl.updTbl("WP", SqlComm);
            }
            else
            {
                SqlComm = $"DELETE FROM WP_OutStockPosDtl_{row["YM"]} WHERE OutStkId IN (SELECT OutStkId FROM WP_OutStockPosDtl WHERE SUBSTRING(OutStkId, 1, 6)='{row["YM"]}');" +
                    $"INSERT INTO WP_OutStockPosDtl_{row["YM"]} (OutStkId, pNo, qty, amount, amtTotal, amtNoneTax, isTax, isDel, timeCreate, timeUpdate, costStd, "+
                        "outType, outLeft, outDate, payKind, machId, salCode, discount, discountPer, discountShare) " +
                    "SELECT OutStkId, pNo, qty, amount, amtTotal, amtNoneTax, isTax, isDel, timeCreate, timeUpdate, costStd, " +
                        $"outType, outLeft, outDate, payKind, machId, salCode, discount, discountPer, discountShare FROM WP_OutStockPosDtl WHERE SUBSTRING(OutStkId, 1, 6)='{row["YM"]}'";
                getTbl.updTbl("WP", SqlComm);
            }
        }

        SqlComm = $"TRUNCATE TABLE WP_OutStockPos;" +
                $"TRUNCATE TABLE  WP_OutStockPosDtl;";
        getTbl.updTbl("WP", SqlComm);

    }

    /// <summary>
    /// 刪除後台中前台POS已作癈資料
    /// </summary>
    private void ClearPosDel()
    {
        GetTbl getTbl = new GetTbl();
        string SqlComm = $"SELECT SUBSTRING(OutStkId, 1, 8) AS YMD FROM WP_OutStockPos GROUP BY SUBSTRING(OutStkId, 1, 8) ORDER BY YMD";
        DataTable ymdTbl = getTbl.table("WP", SqlComm);

        DataTable ClearTbl;
        foreach (DataRow rowYMD in ymdTbl.Rows)
        {
            SqlComm = $"SELECT OutStkId, PosOutStkId, SUBSTRING(OutStkId, 1, 6) AS YM FROM WP_OutStock WHERE (SUBSTRING(OutStkId, 1, 8) = '{rowYMD["YMD"]}') AND (PosOutStkId <> '') AND (isDel = 'N')" +
                    $" AND (PosOutStkId NOT IN (SELECT OutStkId FROM WP_OutStockPos WHERE SUBSTRING(OutStkId, 1, 8) = '{rowYMD["YMD"]}'))";
            ClearTbl = getTbl.table("WP", SqlComm);
            foreach(DataRow row in ClearTbl.Rows)
            {
                delOutStk($"{row["OutStkId"]}");
                SqlComm = $"UPDATE WP_OutStockPos_{row["YM"]} SET isDel='Y' WHERE OutStkId ='{row["PosOutStkId"]}';" +
                        $"UPDATE WP_OutStockPosDtl_{row["YM"]} SET isDel='Y' WHERE OutStkId ='{row["PosOutStkId"]}'";
                getTbl.updTbl("WP", SqlComm);
            }

        }
    }

    /// <summary>
    /// 組合商品寫入
    /// </summary>
    /// <param name="pdCbTbl"></param>
    /// <param name="pd"></param>
    /// <param name="OutStkId"></param>
    private void OutPdCb(DataTable pdCbTbl, JToken pd, string OutStkId)
    {
        ToDouble toDouble = new ToDouble();
        bool isCbPd = pdCbTbl.Rows.Count > 0 ? pdCbTbl.Select($"pNo={pd["pNo"]}").Length > 0 ? true : false : false;
        int pdQty, pdCbQty, pdAmtTotal;
        double pdAmt, pdAmtNoneTax;
        if (isCbPd)
        {
            pdAmt = double.Parse($"{pd["amount"]}");
            pdQty = int.Parse($"{pd["qty"]}");
            pdAmtTotal = int.Parse($"{pd["amtTotal"]}");
            pdAmtNoneTax = $"{pd["isTax"]}" == "N" ? pdAmt : toDouble.Numer(pdAmt * (1 - Global.TaxPercent), Global.pointQty);  //商品未稅單價
            try
            {
                foreach (DataRow pdCb in pdCbTbl.Select($"pNo={pd["pNo"]}"))
                {
                    pdCbQty = int.Parse($"{pdCb["pQty"]}");
                    string SqlComm = "INSERT INTO WP_OutStockDtlCb (OutStkId, pNo, qty, amount, " +
                        "amtTotal, amtNoneTax, isTax) " +
                        $"VALUES ('{OutStkId}', '{pdCb["pNoS"]}', '{(pdQty * pdCbQty)}', '{(pdAmtTotal / pdCbQty)}', " +
                        $"'{pdAmtTotal}', '{(pdAmtNoneTax / pdCbQty)}', '{pd["isTax"]}')";
                    Global.getTbl.updTbl("WP", SqlComm);

                    if ($"{pdCb["sIsUpdStock"]}" == "Y")
                    {
                        SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({(pdQty * pdCbQty)}) WHERE pNo='{pdCb["pNoS"]}'";    //更新商品數量
                        Global.getTbl.updTbl("WP", SqlComm);
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }

    /// <summary>
    /// 組合商品刪除
    /// </summary>
    /// <param name="OutStkId"></param>
    private void DelPdCb(string _outStkId)
    {
        string SqlComm = $"SELECT * FROM WP_OutStockDtlCb WHERE isDel='N' AND OutStkId='{_outStkId}' ORDER BY pNo";
        DataTable outStkCbTbl = Global.getTbl.table("WP", SqlComm);
        if (outStkCbTbl.Rows.Count > 0) {
            SqlComm = "";
            foreach (DataRow pd in outStkCbTbl.Rows)
            {
                SqlComm += $"UPDATE WP_Product SET qtyNow=qtyNow+({pd["qty"]}) WHERE pNo='{pd["pNo"]}';";    //更新商品數量
            }

            SqlComm += $"UPDATE WP_OutStockDtlCb SET isDel='Y', timeUpdate=GETDATE() WHERE OutStkId='{_outStkId}' AND isDel='N';";    //刪除存在的銷貨品項
            Global.getTbl.updTbl("WP", SqlComm);
        }
    }
</script>

