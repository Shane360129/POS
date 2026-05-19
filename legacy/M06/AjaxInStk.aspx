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
                        switch (Request.Form["args0"])
                        {
                            #region C11 新增進貨單
                            case "C11":
                                string InStkId = stock.InStkMaxId(Request.Form["InStkDate"]);      //取得進貨單號
                                JArray pdLstJSON = JArray.Parse(Request.Form["pdList"]);
                                int pdCount = 0;
                                string pNoGrp = "", prePNo = "";
                                foreach (JToken pd in pdLstJSON.GroupBy(x => x["pNo"]).Select(group => group.First()))    //取得所有商品序號
                                {
                                    pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
                                    pdCount++;
                                }

                                SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
                                pdTbl = Global.getTbl.table("WP", SqlComm);
                                if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount) { Response.Write("Z"); }  //商品有誤
                                else
                                {
                                    double M_amtTotal = 0, M_amtNoneTax = 0;
                                    string M_isTax = "N";

                                    double ognlCostAvg = 0,
                                        newPdAmt = 0, newPdAmtNoneTax, newPdAmtTotal, newCostAvg = 0, newCostInitl = 0;

                                    int newPdQty = 0, ognlQtyNow = 0;

                                    string newPno = "", prePno = "";
                                    DataRow row;

                                    //取得記錄檔時間序號
                                    SqlComm = $"SELECT CONVERT( int, ISNULL(MAX(timeNo), 0)) + 1 AS MaxTimeNo FROM WP_StockTrace";
                                    DataTable MaxTNoTbl = Global.getTbl.table("WP", SqlComm);

                                    foreach (JToken newPd in pdLstJSON.OrderBy(x => x["pNo"]))
                                    {
                                        row = pdTbl.Select($"pNo={newPd["pNo"]}")[0];   //商品資料
                                        newPno = newPd["pNo"].ToString();
                                        newCostInitl = double.Parse($"{row["costInitial"]}");

                                        if (prePno == newPno)
                                        {
                                            ognlCostAvg = newCostAvg;
                                            ognlQtyNow = ognlQtyNow + (newPdQty);
                                        }
                                        else
                                        {
                                            ognlCostAvg = double.Parse(row["costAvg"].ToString());
                                            ognlQtyNow = int.Parse(row["qtyNow"].ToString());
                                            prePno = newPno;
                                        }

                                        newPdAmt = double.Parse(newPd["amount"].ToString());  //商品單價
                                        newPdQty = int.Parse(newPd["qty"].ToString());        //商品數量

                                        newPdAmtNoneTax = row["isTax"].ToString() == "N" ? newPdAmt : toDouble.Numer(newPdAmt * (1 - Global.TaxPercent), 2);  //商品未稅單價
                                        newPdAmtTotal = int.Parse(newPd["amtTotal"].ToString());        //商品小計

                                        //整張進貨單處理
                                        M_isTax = row["isTax"].ToString() == "Y" ? "Y" : M_isTax;   //整張進貨單商品都免稅才免稅
                                        M_amtNoneTax += newPdAmtNoneTax * (newPdQty);                       //整張進貨單未稅總額
                                        M_amtTotal += newPdAmtTotal;                                   //整張進貨單總額

                                        //寫入進貨明細檔
                                        SqlComm = "INSERT INTO WP_InStockDtl " +
                                                "(InStkId, pNo, amount, amtNoneTax, qty, " +
                                                "amtTotal, isTax, payLeft, qtyLeft, pdLimitDate) " +
                                                $"VALUES ('{InStkId}', '{newPno}', '{newPdAmt}', '{newPdAmtNoneTax}', '{newPdQty}', " +
                                                $"'{newPdAmtTotal}', '{row["isTax"]}', '{newPdAmtTotal}', '{newPdQty}', '{newPd["pdLimitDate"]}')";
                                        Global.getTbl.updTbl("WP", SqlComm);

                                        //取得進貨明細檔序號
                                        SqlComm = $"SELECT ISNULL(MAX(sn), 0) AS NewInDtlSn FROM WP_InStockDtl WHERE InStkId='{InStkId}' AND pNo='{newPno}'";
                                        DataTable newDtlTbl = Global.getTbl.table("WP", SqlComm);

                                        //平均成本計算及更改庫存
                                        newCostAvg = ognlQtyNow <= 0
                                            ? newPdQty <= 0 ? ognlCostAvg : newPdAmt
                                            : ((ognlQtyNow + (newPdQty)) <= 0 || ((ognlCostAvg * (ognlQtyNow)) + (newPdAmt * (newPdQty))) <= 0)
                                                ? ognlCostAvg
                                                : toDouble.Numer(((ognlCostAvg * (ognlQtyNow)) + (newPdAmt * (newPdQty))) / (ognlQtyNow + (newPdQty)), 2);    //無庫存情況下平均成本為進貨價
                                        SqlComm = $"UPDATE WP_Product SET {(newPdQty <= 0 ? "" : $"costStd='{newPdAmt}', ")}costAvg='{newCostAvg}', qtyNow='{ognlQtyNow + (newPdQty)}' WHERE PNo = '{newPno}'";    //更新商品平均成本及庫存,標準成本變更為本次進貨價(退貨不改本次成本)
                                        Global.getTbl.updTbl("WP", SqlComm);

                                        //寫入庫存記錄檔
                                        SqlComm = $"INSERT INTO WP_StockTrace " +
                                                $"(timeNo, InStkId, InStkDtlSn, Kind, pNo, amount, Qty, QtyNow, costAvg, costInitial, empId) " +
                                                $"VALUES ('{MaxTNoTbl.Rows[0]["MaxTimeNo"]}', '{InStkId}', '{newDtlTbl.Rows[0]["NewInDtlSn"]}', 'I', '{newPno}', '{newPdAmt}', '{newPdQty}', '{ognlQtyNow + (newPdQty)}', '{newCostAvg}', '{newCostInitl}', '{emp.EmpId}')";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                    }

                                    //寫入進貨主檔
                                    SqlComm = $"INSERT INTO WP_InStock " +
                                        "(InStkId, InStkDate, pvSn, amount, tax, " +
                                        "amtNoneTax, isTax, empId, payLeft, reciptNo) " +
                                        $"VALUES ('{InStkId}', '{Request.Form["InStkDate"]}', '{Request.Form["pvSn"]}', '{M_amtTotal}', '{M_amtTotal - (M_amtNoneTax)}', " +
                                        $"'{M_amtNoneTax}', '{M_isTax}', '{emp.EmpId}', '{M_amtTotal}', '{Request.Form["reciptNo"]}')";
                                    Global.getTbl.updTbl("WP", SqlComm);


                                    Response.Write("Y");
                                }
                                break;
                            #endregion
                            #region C11A 修改進貨單
                            case "C11A":
                                string sn = Request.Form["sn"];      //取得進貨序號
                                SqlComm = $"SELECT * FROM WP_vInStock WHERE sn = {sn}";
                                DataTable inStkTbl = Global.getTbl.table("WP", SqlComm);
                                double amtNoneTax;
                                if (inStkTbl.Rows.Count == 0) { Response.Write("E"); }
                                else
                                {
                                    DataRow inStkRow0 = inStkTbl.Rows[0];
                                    InStkId = $"{inStkRow0["InStkId"]}";    //進貨單號
                                    string reciptNo = Request.Form["reciptNo"];
                                    if (reciptNo != $"{inStkRow0["reciptNo"]}")
                                    {
                                        SqlComm = $"UPDATE WP_InStock SET reciptNo='{reciptNo}', timeUpdate=GETDATE() WHERE sn='{sn}'";    //更新發票號碼
                                        Global.getTbl.updTbl("WP", SqlComm);
                                    }
                                    pdLstJSON = JArray.Parse(Request.Form["pdList"]);
                                    pdCount = 0;
                                    pNoGrp = "";
                                    snGrp = "";
                                    string payType = "";
                                    prePNo = "";
                                    amtNoneTax = 0;
                                    foreach (JToken pd in pdLstJSON.OrderBy(x => x["pNo"]))
                                    {
                                        if (prePNo != $"{pd["pNo"]}")
                                        {
                                            pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
                                            pdCount++;
                                            prePNo = $"{pd["pNo"]}";
                                        }
                                        snGrp += $"{($"{pd["dtlSn"]}" == "new" ? "" : $"{(snGrp == "" ? "" : ",")}{pd["dtlSn"]}")}";      //修改時已存在的明細檔Sn
                                    }

                                    //取出商品檔
                                    SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
                                    pdTbl = Global.getTbl.table("WP", SqlComm);
                                    if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount) { Response.Write("Z"); }
                                    else
                                    {
                                        //進貨單修改，商品刪除
                                        #region 處理已刪除商品
                                        SqlComm = $"SELECT SUM(qty) AS qty, pNo FROM WP_InStockDtl WHERE InStkId='{InStkId}' AND isDel='N' {(snGrp=="" ? "" : $"AND sn NOT IN ({snGrp})")} GROUP BY pNo ORDER BY pNo";    //庫存扣掉不存在的進貨品項數量
                                        DataTable delIstkPdTbl = Global.getTbl.table("WP", SqlComm);
                                        foreach(DataRow pd in delIstkPdTbl.Rows)
                                        {
                                            SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-({pd["qty"]}) WHERE pNo='{pd["pNo"]}'";    //減目前庫存數量
                                            Global.getTbl.updTbl("WP", SqlComm);
                                        }
                                        //刪除商品
                                        SqlComm = $"UPDATE WP_InStockDtl SET isDel='Y', timeUpdate=GETDATE() WHERE InStkId='{InStkId}' {(snGrp == "" ? "" : $"AND (sn NOT IN ({snGrp}))")}";    //刪除不存在的進貨品項
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        #endregion

                                        SqlComm = $"SELECT * FROM WP_StockTrace WHERE InStkId='{InStkId}'";
                                        DataTable tcInStkTbl = Global.getTbl.table("WP", SqlComm);
                                        string tcTimeNo = $"{tcInStkTbl.Rows[0]["timeNo"]}";
                                        stkTraceDel(tcInStkTbl.Select($"Kind='I' {(snGrp == "" ? "" : $"AND (InStkDtlSn NOT IN ({snGrp}))")}", "InStkDtlSn ASC"));    //進貨單修改，商品刪除時更新進出貨記錄檔

                                        //進貨單修改，商品新增及異動
                                        double pdAmtNoneTax, pdAmt, pdCostInitl;
                                        int oldPdQty, pdQty, pdAmtTotal, amtTotal = 0, payLeft = 0, pdPayLeft = 0, pdPaid;
                                        string isTax = "N", pdPayType, pdLimitDate;
                                        foreach (JToken pd in pdLstJSON)
                                        {

                                            SqlComm = $"SELECT * FROM WP_Product WHERE pNo='{pd["pNo"]}'";      //重新抓取商品檔，以確認讀取更後的最新資料
                                            DataRow row = Global.getTbl.table("WP", SqlComm).Rows[0];

                                            //DataRow row = pdTbl.Select($"pNo='{pd["pNo"]}'")[0];
                                            pdCostInitl = double.Parse($"{row["costInitial"]}");    //商品期初成本
                                            pdAmt = double.Parse($"{pd["amount"]}");
                                            pdQty = int.Parse($"{pd["qty"]}");
                                            pdAmtTotal = int.Parse($"{pd["amtTotal"]}");            //商品小計
                                            pdPaid = int.Parse($"{pd["paid"]}");                    //商品已結金額
                                            pdPayLeft = pdAmtTotal - (pdPaid);                        //商品未結金額          
                                            amtTotal += pdAmtTotal;                                 //進貨單總額
                                            pdPayType = pdPaid == 0 ? "0" : pdPaid == pdAmtTotal ? "2" : "1";    //銷帳狀態(0:未結/1:未結完/2:全結)
                                            pdLimitDate = $"{pd["pdLimitDate"]}";                   //保存期限

                                            isTax = $"{row["isTax"]}" == "Y" ? "Y" : isTax;   //整張進貨單商品都免稅才免稅
                                            pdAmtNoneTax = $"{row["isTax"]}" == "N" ? pdAmt : toDouble.Numer(pdAmt * (1 - Global.TaxPercent), 2);  //商品未稅單價(免稅品的未稅單價=進貨價)
                                            amtNoneTax += pdAmtNoneTax * (pdQty);     //進貨單未稅總額

                                            if ($"{pd["dtlSn"]}" == "new")    //進貨單新增商品
                                            {
                                                oldPdQty = 0;
                                                SqlComm = $"INSERT INTO WP_InStockDtl " +
                                                        $"(InStkId, pNo, amount, amtNoneTax, qty, amtTotal, isTax, payLeft, pdLimitDate) " +
                                                        $"VALUES ('{InStkId}', '{pd["pNo"]}', '{pd["amount"]}', '{pdAmtNoneTax}', '{pd["qty"]}', '{pdAmtTotal}', '{row["isTax"]}', '{pdAmtTotal}', '{pdLimitDate}')";
                                                Global.getTbl.updTbl("WP", SqlComm);

                                                stkTraceAdd(tcTimeNo, pd, InStkId, emp.EmpId, row);      //進貨單商品新增時，更新進出貨記錄檔
                                            }
                                            else
                                            {
                                                DataRow inStkRow = inStkTbl.Select($"dtlSn='{pd["dtlSn"]}'")[0];
                                                if (pdAmt != double.Parse($"{inStkRow["dtlAmt"]}") || pdQty != int.Parse($"{inStkRow["qty"]}") || pdAmtTotal != int.Parse($"{inStkRow["amtTotal"]:#0}") || pdLimitDate != $"{inStkRow["pdLimitDate"]:yyyy/MM/dd}")  //有異動價格或數量才更新
                                                {
                                                    stkTraceUpd(tcTimeNo, pd, inStkRow);      //進貨單商品修改時，更新進出貨記錄檔

                                                    SqlComm = $"UPDATE WP_InStockDtl SET " +
                                                            $"amount='{pd["amount"]}', amtNoneTax='{pdAmtNoneTax}', qty='{pd["qty"]}', amtTotal='{pdAmtTotal}', isTax='{row["isTax"]}', " +
                                                            $"payType='{pdPayType}', payLeft={pdPayLeft}, pdLimitDate='{pdLimitDate}', timeUpdate=GETDATE() " +
                                                            $"WHERE sn={pd["dtlSn"]}";
                                                    Global.getTbl.updTbl("WP", SqlComm);
                                                }
                                            }
                                            payLeft += pdPayLeft;

                                            //更新商品的標準成本

                                            SqlComm = $"UPDATE WP_Product SET costStd='{pdCostStd($"{pd["pNo"]}", pdCostInitl)}' WHERE pNo = '{pd["pNo"]}'";
                                            Global.getTbl.updTbl("WP", SqlComm);
                                        }

                                        payType = payLeft == 0 ? "2" : payLeft == amtTotal ? "0" : "1";    //銷帳狀態(0:未結/1:未結完/2:全結)
                                        SqlComm = $"UPDATE WP_InStock SET " +
                                                $"amount='{amtTotal}', tax='{amtTotal - (amtNoneTax)}', amtNoneTax='{amtNoneTax}', isTax='{isTax}', payType='{payType}', payLeft='{payLeft}' WHERE sn='{sn}'";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        Response.Write("Y");
                                    }
                                }
                                break;
                            #endregion
                            #region C11B 刪除進貨單
                            case "C11B":
                                sn = Request.Form["sn"];      //取得進貨序號
                                SqlComm = $"SELECT * FROM WP_InStock WHERE sn = {sn}";
                                inStkTbl = Global.getTbl.table("WP", SqlComm);

                                if (inStkTbl.Rows.Count == 0) { Response.Write("E"); }
                                else
                                {
                                    InStkId = inStkTbl.Rows[0]["InStkId"].ToString();

                                    SqlComm = $"UPDATE WP_InStock SET isDel='Y', timeUpdate=GETDATE() WHERE sn='{sn}'";    //刪除進貨單主檔
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    SqlComm = $"UPDATE WP_InStockDtl SET isDel='Y', timeUpdate=GETDATE() WHERE InStkId='{InStkId}' AND isDel='N'";    //刪除存在的進貨品項
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    //更新交易記錄檔刪除
                                    SqlComm = $"SELECT * FROM WP_StockTrace WHERE InStkId='{InStkId}'";
                                    DataTable tcInStkTbl = Global.getTbl.table("WP", SqlComm);
                                    string tcTimeNo = tcInStkTbl.Rows[0]["timeNo"].ToString();
                                    stkTraceDel(tcInStkTbl.Select("Kind='I'", "InStkDtlSn ASC"));    //進貨單刪除，商品刪除時更新進出貨記錄檔

                                    Response.Write("Y");
                                }
                                break;
                            #endregion
                            #region C13 應付帳款銷帳
                            case "C13":
                                string AcctOutId = stock.AcctOutMaxId(Request.Form["AcctOutDate"]);      //取得銷帳單號
                                pdLstJSON = JArray.Parse(Request.Form["pdList"]);
                                pdCount = 0;
                                string dtlSnGrp = "";
                                amtNoneTax = 0;
                                foreach (var pd in pdLstJSON.OrderBy(x => x["dtlSn"]))
                                {
                                    dtlSnGrp += $"{(dtlSnGrp == "" ? "" : ",")}{pd["dtlSn"]}";
                                    pdCount++;
                                }

                                SqlComm = $"SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND dtlSn IN ({dtlSnGrp}) ORDER BY InStkId, dtlSn";
                                inStkTbl = Global.getTbl.table("WP", SqlComm);
                                if (inStkTbl.Rows.Count == 0 || inStkTbl.Rows.Count != pdCount) { Response.Write("Z"); }
                                else
                                {
                                    DataRow row;
                                    double pdAmtNoneTax = 0, pdAmtTotal = 0, amtTotal = 0;
                                    string preInStkId = "", isTax = "N";
                                    foreach (var pd in pdLstJSON)
                                    {
                                        row = inStkTbl.Select($"dtlSn='{pd["dtlSn"]}'", "InStkId ASC")[0];
                                        if (preInStkId != $"{row["InStkId"]}" && preInStkId != "")
                                        {
                                            updInStk(preInStkId);
                                        }
                                        isTax = $"{row["isTax"]}" == "Y" ? "Y" : isTax;   //整張銷帳單商品都免稅才免稅
                                        pdAmtNoneTax = toDouble.Numer(double.Parse($"{row["dtlAmtNoneTax"]}"), Global.pointQty);  //商品未稅單價
                                        amtNoneTax += pdAmtNoneTax * (int.Parse($"{pd["qty"]}"));     //銷帳單未稅總額
                                        pdAmtTotal = double.Parse($"{row["amtTotal"]}");       //商品小計(不開放部份數量銷帳)
                                        amtTotal += pdAmtTotal;       //銷帳單總額

                                        SqlComm = $"INSERT INTO WP_AcctOutDtl " +
                                                $"(acctOutId, InStkId, InStkDtlSn, pNo, amount, " +
                                                    $"amtNoneTax, qty, amtTotal, isTax, payType, payLeft) " +
                                                $"VALUES ('{AcctOutId}', '{row["InStkId"]}', '{row["dtlSn"]}', '{row["pNo"]}', '{row["dtlAmt"]}', " +
                                                    $"'{row["dtlAmtNoneTax"]}', '{pd["qty"]}', '{pdAmtTotal}', '{row["isTax"]}', '2', '0')";
                                        Global.getTbl.updTbl("WP", SqlComm);
                                        SqlComm = $"UPDATE WP_InStockDtl SET payType='2', payLeft='0', timeUpdate=GETDATE(), payDate=GETDATE() WHERE sn='{row["dtlSn"]}'";
                                        Global.getTbl.updTbl("WP", SqlComm);

                                        preInStkId = $"{row["InStkId"]}";
                                    }
                                    updInStk(preInStkId);

                                    DataRow row0 = inStkTbl.Rows[0];
                                    SqlComm = $"INSERT INTO WP_AcctOut " +
                                            "(acctOutId, acctOutDate, pvSn, amount, empId) " +
                                            $"VALUES ('{AcctOutId}', '{Request.Form["AcctOutDate"]}', '{Request.Form["pvSn"]}', '{amtTotal}', '{emp.EmpId}')";
                                    Global.getTbl.updTbl("WP", SqlComm);

                                    Response.Write("Y");
                                    //Response.Write(SqlComm);

                                }
                                break;
                            #endregion
                            #region C13B 刪除應付帳款銷帳單
                            case "C13B":
                                sn = Request.Form["sn"];      //取得應付帳銷帳序號
                                SqlComm = $"SELECT * FROM WP_vAcctOut WHERE isDel='N' AND sn = {sn} ORDER BY InStkId";
                                DataTable acctOutTbl = Global.getTbl.table("WP", SqlComm);
                                if (acctOutTbl.Rows.Count == 0) { Response.Write("E"); }
                                else
                                {
                                    SqlComm = $"SELECT * FROM WP_AcctOutDtl WHERE isDel='N' AND InStkDtlSn IN (SELECT InStkDtlSn FROM WP_vAcctOut WHERE dtlIsDel='N' AND sn = {sn}) AND acctOutId > '{acctOutTbl.Rows[0]["acctOutId"]}'";
                                    DataTable acctOutDtlTbl = Global.getTbl.table("WP", SqlComm);
                                    string preInStkId = "";
                                    double dtlAmt;
                                    foreach (DataRow row in acctOutTbl.Rows)
                                    {
                                        //1.逐一更新acctOutDtl 的尚未銷帳金額及狀態
                                        //2.逐一更新inStkDtl 的尚未銷帳金額及狀態
                                        //3.更新inStkDtl的尚未銷帳金額及狀態
                                        //4.更新inStk的尚未銷帳金額及狀態
                                        //var results = from myRow in acOutDtlTbl.AsEnumerable()
                                        //              where myRow.Field<int>("InStkDtlSn") == int.Parse(row["InStkDtlSn"].ToString())
                                        //              select myRow;
                                        dtlAmt = double.Parse(row["amtTotal"].ToString());
                                        SqlComm = $"UPDATE WP_InStockDtl SET payLeft=payLeft+({dtlAmt}), timeUpdate=GETDATE() WHERE sn='{row["InStkDtlSn"]}'";    //加回銷帳檔金額至進貨檔的未銷帳金額
                                        Global.getTbl.updTbl("WP", SqlComm);

                                        var results = from DataRow myRow in acctOutDtlTbl.Rows
                                                      where ((string)myRow["InStkId"] == row["InStkId"].ToString()
                                                      && (int)myRow["InStkDtlSn"] == int.Parse(row["InStkDtlSn"].ToString()))
                                                      orderby myRow["InStkId"]
                                                      select myRow;
                                        foreach (var rowDtl in results)
                                        {
                                            SqlComm = $"UPDATE WP_AcctOutDtl SET payLeft=payLeft+({dtlAmt}), payType='1', timeUpdate=GETDATE() WHERE InStkId='{rowDtl["InStkId"]}' AND InStkDtlSn='{rowDtl["InStkDtlSn"]}'";    //加回本刪除銷帳檔金額至後續同一進貨檔的同商品未銷帳金額
                                            Global.getTbl.updTbl("WP", SqlComm);
                                        }

                                        if (preInStkId != row["InStkId"].ToString())
                                        {
                                            if (preInStkId != "") { updInStk(preInStkId); }
                                            preInStkId = row["InStkId"].ToString();
                                        }

                                    }
                                    if (preInStkId != "") { updInStk(preInStkId); }

                                    SqlComm = $"UPDATE WP_AcctOut SET isDel='Y', timeUpdate=GETDATE() WHERE sn='{sn}'";    //刪除銷帳檔主檔
                                    Global.getTbl.updTbl("WP", SqlComm);
                                    SqlComm = $"UPDATE WP_AcctOutDtl SET isDel='Y', timeUpdate=GETDATE() WHERE acctOutId='{acctOutTbl.Rows[0]["acctOutId"]}' AND isDel='N'";    //刪除不存在的進貨品項
                                    Global.getTbl.updTbl("WP", SqlComm);
                                    Response.Write("Y");
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
    /// 更新進貨單(銷帳狀態及銷帳剩餘金額)
    /// </summary>
    /// <param name="_InStkId"></param>進貨單號
    private void updInStk(string _InStkId)
    {
        string SqlComm = $"SELECT * FROM WP_InStockDtl WHERE isDel='N' AND InStkId='{_InStkId}'";
        //Response.Write(SqlComm);
        DataTable inStkDtlTbl = Global.getTbl.table("WP", SqlComm);
        double payLeft;
        string payType;
        foreach(DataRow row in inStkDtlTbl.Rows)
        {
            payLeft = double.Parse(row["payLeft"].ToString());
            payType = payLeft == 0 ? "2" : payLeft == double.Parse(row["amtTotal"].ToString()) ? "0" : "1";
            if (payType != row["payType"].ToString())
            {
                SqlComm = $"UPDATE WP_InStockDtl SET payType='{payType}', timeUpdate=GETDATE() WHERE sn='{row["sn"]}'";
                //Response.Write(SqlComm);
                Global.getTbl.updTbl("WP", SqlComm);
            }
        }

        SqlComm = $"SELECT SUM(dtlPayLeft) AS payLeft, Max(amount) AS amount FROM WP_vInStock WHERE dtlIsDel='N' AND InStkId='{_InStkId}'";
        //SqlComm = $"SELECT SUM(payLeft) AS payLeft FROM WP_InStockDtl WHERE isDel='N' AND InStkId='{_InStkId}'";
        //Response.Write(SqlComm);
        DataTable inStkTbl = Global.getTbl.table("WP", SqlComm);

        if (!inStkTbl.Rows[0].IsNull("payLeft"))
        {
            DataRow inStkRow0 = inStkTbl.Rows[0];
            payLeft = double.Parse($"{inStkRow0["payLeft"]}");
            SqlComm = $"UPDATE WP_InStock SET payType='{(payLeft == 0 ? "2" : payLeft == double.Parse($"{inStkRow0["amount"]}") ? "0" : "1")}', payLeft='{payLeft}', timeUpdate=GETDATE() WHERE InStkId='{_InStkId}'";
            //Response.Write(SqlComm);
            Global.getTbl.updTbl("WP", SqlComm);
        }
    }

    /// <summary>
    /// 取得最後進貨成本
    /// </summary>
    /// <param name="_pNo"></param>商品編號
    /// <param name="_pdCostInitl"></param>商品期初成本
    private double pdCostStd(string _pNo, double _pdCostInitl)
    {
        string SqlComm = $"SELECT TOP(1) * FROM WP_InStockDtl WHERE pNo = '{_pNo}' AND isDel='N' ORDER BY InStkId DESC, Sn DESC";
        DataTable lastInStkAmtTbl = Global.getTbl.table("WP", SqlComm);
        return lastInStkAmtTbl.Rows.Count == 0 ? _pdCostInitl : double.Parse($"{lastInStkAmtTbl.Rows[0]["amount"]}");

    }

    /// <summary>
    /// 進貨單商品--[刪除時]--更新進出貨記錄檔
    /// </summary>
    /// <param name="_tcStkTbl"></param>進貨單記錄檔欲刪除的資料
    private void stkTraceDel(DataRow[] _tcStkTbl)
    {
        ToDouble toDouble = new ToDouble();

        int mQtyNow, ognlQty, ognlQtyNow, delQty = 0, updTraceNo = 0;
        double mCostAvg, ognlAmt, ognlCostAvg, delAmt = 0, ognlCostInitl = 0;
        foreach (DataRow rowInStk in _tcStkTbl)
        {
            updTraceNo = 0;
            mQtyNow = 0;
            mCostAvg = 0;

            string SqlComm = $"SELECT * FROM WP_StockTrace " +
                $"WHERE ((timeNo = '{rowInStk["timeNo"]}' AND InStkDtlSn >='{rowInStk["InStkDtlSn"]}') OR timeNo > '{rowInStk["timeNo"]}') AND Kind NOT IN('D') AND pNo = '{rowInStk["pNo"]}' " +
                $"ORDER BY timeNo, Sn";
            DataTable stkTraceTbl = Global.getTbl.table("WP", SqlComm);
            foreach (DataRow rowStrace in stkTraceTbl.Rows)
            {
                ognlQty = int.Parse($"{rowStrace["Qty"]}");
                ognlQtyNow = int.Parse($"{rowStrace["QtyNow"]}");
                ognlAmt = double.Parse($"{rowStrace["amount"]}");
                ognlCostAvg = double.Parse($"{rowStrace["costAvg"]}");
                ognlCostInitl = double.Parse($"{rowStrace["costInitial"]}");

                if (updTraceNo == 0) {  //刪除不存在的進貨品項
                    SqlComm = $"UPDATE WP_StockTrace SET Kind='D', timeUpdate=GETDATE() WHERE sn='{rowStrace["sn"]}'";
                    Global.getTbl.updTbl("WP", SqlComm);
                    delQty = ognlQty;
                    delAmt = ognlAmt;
                    mQtyNow =  ognlQtyNow - (delQty);
                    mCostAvg = mQtyNow <= 0 ? ognlCostInitl :  toDouble.Numer(((ognlCostAvg * (ognlQtyNow)) - (delAmt * (delQty))) / (mQtyNow), 2);
                }
                else    //更新刪除後記錄檔的平均成本及庫存
                {
                    mQtyNow =  ognlQtyNow - (delQty);
                    mCostAvg = mQtyNow <= 0 ? ognlCostInitl : toDouble.Numer(((ognlCostAvg * (ognlQtyNow)) - (delAmt * (delQty))) / (mQtyNow), 2);
                    SqlComm = $"UPDATE WP_StockTrace SET QtyNow='{mQtyNow}', costAvg='{mCostAvg}', timeUpdate=GETDATE() WHERE sn='{rowStrace["sn"]}'";    //刪除不存在的進貨品項
                    Global.getTbl.updTbl("WP", SqlComm);
                }

                updTraceNo++;
            }

            SqlComm = $"UPDATE WP_Product SET costStd='{pdCostStd($"{rowInStk["pNo"]}", ognlCostInitl)}', qtyNow=qtyNow-({delQty}), costAvg='{mCostAvg}' WHERE pNo ='{rowInStk["pNo"]}'";    //更新庫存及平均成本
            Global.getTbl.updTbl("WP", SqlComm);
        }
    }

    /// <summary>
    /// 進貨單商品--[新增時]--更新進出貨記錄檔
    /// </summary>
    /// <param name="_tcTimeNo"></param>進貨單寫入順序序號
    /// <param name="_pd"></param>進貨單商品明細資料檔
    /// <param name="_inStkId"></param>進貨單單號
    /// <param name="_empId"></param>操作者員工代號
    /// <param name="_row"></param>商品資料檔
    private void stkTraceAdd(string _tcTimeNo, JToken _pd, string _inStkId, string _empId, DataRow _row)
    {
        ToDouble toDouble = new ToDouble();

        int mQtyNow = 0, ognlQty, ognlQtyNow,
            updTraceNo = 0, preQtyNow,
            addQty = int.Parse($"{_pd["qty"]}"),
            rowQty = int.Parse($"{_row["qtyNow"]}");
        double mCostAvg = 0, ognlAmt, ognlCostAvg, preCostAvg,
            addAmt = double.Parse($"{_pd["amount"]}"),
            rowCostAvg = double.Parse($"{_row["costAvg"]}"),
            rowCostInitl = double.Parse($"{_row["costInitial"]}");

        string pNo = $"{_pd["pNo"]}";

        //取得進貨明細檔序號
        string SqlComm = $"SELECT ISNULL(MAX(sn), 0) AS NewInDtlSn FROM WP_InStockDtl WHERE InStkId='{_inStkId}' AND pNo='{pNo}'";
        DataTable newDtlTbl = Global.getTbl.table("WP", SqlComm);

        SqlComm = "SELECT * FROM WP_StockTrace " +
            $"WHERE timeNo>'{_tcTimeNo}' AND Kind NOT IN('D') AND pNo ='{pNo}' " +
            "ORDER BY timeNo, Sn";
        DataTable stkTraceTbl = Global.getTbl.table("WP", SqlComm);
        if (stkTraceTbl.Rows.Count == 0)    //新增該筆商品並無後續進出貨記錄，以目前該商品庫存及平均成本開始計算庫存及成本
        {
            mQtyNow = rowQty + (addQty);
            mCostAvg = toDouble.Numer(((rowCostAvg * (rowQty)) + (addAmt * (addQty))) / (mQtyNow), 2);
            //寫入庫存記錄檔
            SqlComm = $"INSERT INTO WP_StockTrace " +
                    $"(timeNo, InStkId, InStkDtlSn, Kind, pNo, amount, Qty, QtyNow, costAvg, costInitial, empId) " +
                    $"VALUES ('{_tcTimeNo}', '{_inStkId}', '{newDtlTbl.Rows[0]["NewInDtlSn"]}', 'I', '{_pd["pNo"]}', '{addAmt}', '{addQty}', '{mQtyNow}', '{mCostAvg}', '{rowCostInitl}', '{_empId}')";
            Global.getTbl.updTbl("WP", SqlComm);
        }
        else
        {
            foreach (DataRow rowStrace in stkTraceTbl.Rows)
            {
                ognlQty = int.Parse($"{rowStrace["Qty"]}");
                ognlQtyNow = int.Parse($"{rowStrace["QtyNow"]}");
                ognlAmt = double.Parse($"{rowStrace["amount"]}");
                ognlCostAvg = double.Parse($"{rowStrace["costAvg"]}");

                if (updTraceNo == 0)
                {
                    //由新增後第一筆反算回去新增前的庫存及平均成本
                    preQtyNow = ognlQtyNow - (ognlQty);
                    preCostAvg = toDouble.Numer(((ognlCostAvg * (ognlQtyNow)) - (ognlAmt * (ognlQty))) / (ognlQtyNow - (ognlQty)), 2);

                    mQtyNow = preQtyNow + (addQty);
                    mCostAvg = toDouble.Numer(((preCostAvg * (preQtyNow)) + (addAmt * (addQty))) / (mQtyNow), 2);
                    //寫入庫存記錄檔
                    SqlComm = $"INSERT INTO WP_StockTrace " +
                        $"(timeNo, InStkId, InStkDtlSn, Kind, pNo, amount, Qty, QtyNow, costAvg, costInitial, empId) " +
                        $"VALUES ('{_tcTimeNo}', '{_inStkId}', '{newDtlTbl.Rows[0]["NewInDtlSn"]}', 'I', '{_pd["pNo"]}', '{addAmt}', '{addQty}', '{mQtyNow}', '{mCostAvg}', '{rowCostInitl}', '{_empId}')";
                    Global.getTbl.updTbl("WP", SqlComm);
                }

                mQtyNow = ognlQtyNow + (addQty);
                mCostAvg = toDouble.Numer(((ognlCostAvg * (ognlQtyNow)) + (addAmt * (addQty))) / (mQtyNow), 2);
                SqlComm = $"UPDATE WP_StockTrace SET QtyNow='{mQtyNow}', costAvg='{mCostAvg}', timeUpdate=GETDATE() WHERE sn='{rowStrace["sn"]}'";    //更新記錄檔庫存及平均成本
                Global.getTbl.updTbl("WP", SqlComm);

                updTraceNo++;
            }
        }
        SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow+({addQty}), costAvg='{mCostAvg}' WHERE pNo ='{pNo}'";    //更新庫存及平均成本
        Global.getTbl.updTbl("WP", SqlComm);
    }

    /// <summary>
    /// 進貨單商品--[修改時]--更新進出貨記錄檔
    /// </summary>
    /// <param name="_tcTimeNo"></param>進貨單寫入順序序號
    /// <param name="_pd"></param>進貨單商品明細資料檔
    /// <param name="_inStkRow"></param>進貨單原始明細資料檔
    /// <param name="_empId"></param>操作者員工代號
    private void stkTraceUpd(string _tcTimeNo, JToken _pd, DataRow _inStkRow)
    {
        ToDouble toDouble = new ToDouble();

        int mQtyNow = 0, ognlQty, ognlQtyNow, updTraceNo = 0, newQty,
            updQtyTotal = 0;
        double mCostAvg = 0, ognlAmt, ognlCostAvg, ognlCostInitl, newAmt,
            updAmtTotal = 0;

        string SqlComm = $"SELECT * FROM WP_StockTrace " +
            $"WHERE ((timeNo = '{_tcTimeNo}' AND InStkDtlSn >='{_pd["dtlSn"]}') OR timeNo > '{_tcTimeNo}') AND Kind NOT IN('D') AND pNo = '{_pd["pNo"]}' " +
            $"ORDER BY timeNo, Sn";
        DataTable stkTraceTbl = Global.getTbl.table("WP", SqlComm);
        foreach (DataRow rowStrace in stkTraceTbl.Rows)
        {
            ognlAmt = double.Parse($"{rowStrace["amount"]}");
            ognlQty = int.Parse($"{rowStrace["Qty"]}");
            ognlQtyNow = int.Parse($"{rowStrace["QtyNow"]}");
            ognlCostAvg = double.Parse($"{rowStrace["costAvg"]}");
            ognlCostInitl = double.Parse($"{rowStrace["costInitial"]}");

            if (updTraceNo == 0)
            {  //異動的進貨品項
                newAmt = double.Parse($"{_pd["amount"]}");
                newQty = int.Parse($"{_pd["qty"]}");
                updAmtTotal = -(ognlAmt * (ognlQty)) + (newAmt * (newQty));
                updQtyTotal = -ognlQty + newQty;
            }
            else
            {
                newAmt = ognlAmt;
                newQty = ognlQty;
            }

            mQtyNow = ognlQtyNow + updQtyTotal;
            mCostAvg = mQtyNow <= 0 ? ognlCostInitl : toDouble.Numer(((ognlCostAvg * (ognlQtyNow)) + (updAmtTotal)) / (mQtyNow), 2);
            SqlComm = $"UPDATE WP_StockTrace SET amount='{newAmt}', Qty='{newQty}', QtyNow='{mQtyNow}', costAvg='{mCostAvg}', timeUpdate=GETDATE() WHERE sn='{rowStrace["sn"]}'";    //刪除不存在的進貨品項
            Global.getTbl.updTbl("WP", SqlComm);


            updTraceNo++;
        }
        SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow+({updQtyTotal}), costAvg='{mCostAvg}' WHERE pNo ='{_pd["pNo"]}'";    //更新庫存及平均成本
        Global.getTbl.updTbl("WP", SqlComm);
    }

</script>

