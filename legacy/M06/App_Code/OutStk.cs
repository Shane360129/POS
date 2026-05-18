using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using static Global;

/// <summary>
/// Stock 進銷存
/// </summary>
public class OutStk
{
    Stock stock = new Stock();
    Employee emp = new Employee();
    ToDouble toDouble = new ToDouble();

    /// <summary>
    /// 新增銷貨單
    /// </summary>
    /// <param name="_oStkDate">銷貨日期</param>
    /// <param name="_payJSON">付款方式JSON</param>
    /// <param name="_pdList">商品清單</param>
    /// <param name="_memSn">會員序號</param>
    /// <param name="_reciptNo">發票號碼</param>
    /// <returns></returns>
    public string OutStkAdd(string _oStkDate, string _payJSON, string _pdList, string _memSn, string _reciptNo)
    {
        DataTable payTbl, pdTbl;
        DataRow[] payRows;
        JArray payJSON, pdLstJSON;

        string SqlComm, result, outType, OutStkId, pNoGrp, mIsTax;

        double amtAccInY, amtAccInN, mAmtNoneTax,
        pdAmt, pdAmtNoneTax, pdAmtTotal, mAmtTotal;

        int pdCount, pdQty;

        #region 計算未結金額
        SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N'";
            payTbl = getTbl.table("WP", SqlComm);

            payJSON = JArray.Parse(_payJSON);

            amtAccInY = 0;  //未結金額(產生應收帳款)
            amtAccInN = 0;  //已結金額
            foreach (var pay in payJSON)
            {
                payRows = payTbl.Select($"PayKId='{pay["PAYID"]}'");    //付款方式集合
                if ($"{payRows[0]["isAcctIn"]}" == "Y")
                    amtAccInY += double.Parse($"{pay["PAYAMT"]}");
                else
                    amtAccInN += double.Parse($"{pay["PAYAMT"]}");
            }
            outType = amtAccInY == 0 ? "2" : amtAccInN == 0 ? "0" : "1";     //結帳狀態(0:未結/1:未結完/2:全結)
        #endregion

        OutStkId = stock.OutStkMaxId(_oStkDate);      //取得銷貨單號
        pdLstJSON = JArray.Parse(_pdList);      //商品清單JSON
        pdCount = 0;        //商品數
        pNoGrp = "";        //商品編號集合
        mAmtNoneTax = 0;    //主檔免稅額
        foreach (var pd in pdLstJSON.GroupBy(x => x["pNo"]).Select(group => group.First()))
        {
            pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
            pdCount++;
        }

        SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
        pdTbl = getTbl.table("WP", SqlComm);

        if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount)
            result = "Z";
        else
        {
            try
            {
                mAmtTotal = 0;
                mIsTax = "N";
                foreach (JToken pd in pdLstJSON.OrderBy(x => x["pNo"]))
                {
                    pdAmt = double.Parse($"{pd["amount"]}");
                    pdQty = int.Parse($"{pd["qty"]}");

                    SqlComm = $"SELECT * FROM WP_Product WHERE pNo='{pd["pNo"]}'";      //重新抓取商品檔，以確認讀取更後的最新資料
                    DataRow row = getTbl.table("WP", SqlComm).Rows[0];

                    mIsTax = row["isTax"].ToString() == "Y" ? "Y" : mIsTax;   //整張銷貨單商品都免稅才免稅
                    pdAmtNoneTax = row["isTax"].ToString() == "N" ? pdAmt : toDouble.Numer(pdAmt * (1 - TaxPercent), 2);  //商品未稅單價

                    mAmtNoneTax += pdAmtNoneTax * pdQty;     //銷貨單未稅總額
                    pdAmtTotal = int.Parse($"{pd["amtTotal"]}");       //商品小計
                    mAmtTotal += pdAmtTotal;       //銷貨單總額
                    SqlComm = "INSERT INTO WP_OutStockDtl (OutStkId, pNo, amount, amtNoneTax, qty, " +
                        "amtTotal, isTax, costStd, pdLimitDate) " +
                        $"VALUES ('{OutStkId}', '{pd["pNo"]}', '{pd["amount"]}', '{pdAmtNoneTax}', '{pd["qty"]}', " +
                        $"'{pdAmtTotal}', '{row["isTax"]}', '{row["costStd"]}', '{pd["pdLimitDate"]}')";
                    getTbl.updTbl("WP", SqlComm);

                    SqlComm = $"UPDATE WP_Product SET qtyNow=qtyNow-{pdQty} WHERE pNo='{pd["pNo"]}'";    //更新商品數量
                    getTbl.updTbl("WP", SqlComm);
                }

                SqlComm = $"INSERT INTO WP_OutStock (OutStkId, OutStkDate, memSn, amount, tax, " +
                        "amtNoneTax, isTax, empId, outType, outLeft, " +
                        "PayList, reciptNo) " +
                        $"VALUES ('{OutStkId}', '{_oStkDate}', '{_memSn}', '{mAmtTotal}', '{mAmtTotal - mAmtNoneTax}', " +
                        $"'{mAmtNoneTax}', '{mIsTax}', '{emp.EmpId}', '{outType}', '{amtAccInY}', " +
                        $"'{_payJSON}', '{_reciptNo}')";
                getTbl.updTbl("WP", SqlComm);
                result = "Y";
            }
            catch (InvalidOperationException ex)
            {
                result = ex.Message;
            }
        }
        return result;
    }
}

