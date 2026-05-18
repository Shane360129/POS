using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Linq;
using static Global;

/// <summary>
/// Stock 進銷存
/// </summary>
public class InStk
{
    Stock stock = new Stock();
    Employee emp = new Employee();
    ToDouble toDouble = new ToDouble();

    /// <summary>
    /// 新增進貨單
    /// </summary>
    /// <param name="_stkDate">進貨日期</param>
    /// <param name="_pdList">進貨商品JSON</param>
    /// <param name="_pvSn">供貨商 -1:庫存調整</param>
    /// <param name="_reciptNo">發票編號</param>
    /// <returns></returns>
    public string InStkAdd(string _stkDate, string _pdList, string _pvSn, string _reciptNo)
    {
        DataTable pdTbl, MaxTNoTbl, newDtlTbl;
        JArray pdLstJSON;

        string SqlComm, result, pNoGrp,
        InStkId = stock.InStkMaxId(_stkDate);      //取得進貨單號

        int pdCount = 0;

        double M_amtTotal = 0, M_amtNoneTax = 0;

        pdLstJSON = JArray.Parse(_pdList);
        pNoGrp = "";
        foreach (JToken pd in pdLstJSON.GroupBy(x => x["pNo"]).Select(group => group.First()))    //取得所有商品序號
        {
            pNoGrp += $"{(pNoGrp == "" ? "" : ",")}{pd["pNo"]}";
            pdCount++;
        }

        SqlComm = $"SELECT * FROM WP_vProduct WHERE pNo IN ({pNoGrp})";
        pdTbl = getTbl.table("WP", SqlComm);
        if (pdTbl.Rows.Count == 0 || pdTbl.Rows.Count != pdCount)
            result = "Z";  //商品有誤
        else
        {
            try
            {
                string M_isTax = "N";

                double ognlCostAvg = 0,
                    newPdAmt = 0, newPdAmtNoneTax, newPdAmtTotal, newCostAvg = 0, newCostInitl = 0;

                int newPdQty = 0, ognlQtyNow = 0;

                string newPno = "", prePno = "";
                DataRow row;

                //取得記錄檔時間序號
                SqlComm = $"SELECT CONVERT( int, ISNULL(MAX(timeNo), 0)) + 1 AS MaxTimeNo FROM WP_StockTrace";
                MaxTNoTbl = getTbl.table("WP", SqlComm);

                foreach (JToken newPd in pdLstJSON.OrderBy(x => x["pNo"]))
                {
                    row = pdTbl.Select($"pNo={newPd["pNo"]}")[0];   //商品資料
                    newPno = newPd["pNo"].ToString();
                    newCostInitl = double.Parse($"{row["costInitial"]}");

                    if (prePno == newPno)
                    {
                        ognlCostAvg = newCostAvg;
                        ognlQtyNow = ognlQtyNow + newPdQty;
                    }
                    else
                    {
                        ognlCostAvg = double.Parse(row["costAvg"].ToString());
                        ognlQtyNow = int.Parse(row["qtyNow"].ToString());
                        prePno = newPno;
                    }

                    newPdAmt = double.Parse(newPd["amount"].ToString());  //商品單價
                    newPdQty = int.Parse(newPd["qty"].ToString());        //商品數量

                    newPdAmtNoneTax = row["isTax"].ToString() == "N" ? newPdAmt : toDouble.Numer(newPdAmt * (1 - TaxPercent), 2);  //商品未稅單價
                    newPdAmtTotal = int.Parse(newPd["amtTotal"].ToString());        //商品小計

                    //整張進貨單處理
                    M_isTax = row["isTax"].ToString() == "Y" ? "Y" : M_isTax;   //整張進貨單商品都免稅才免稅
                    M_amtNoneTax += newPdAmtNoneTax * newPdQty;                       //整張進貨單未稅總額
                    M_amtTotal += newPdAmtTotal;                                   //整張進貨單總額

                    //寫入進貨明細檔
                    SqlComm = "INSERT INTO WP_InStockDtl " +
                            "(InStkId, pNo, amount, amtNoneTax, qty, " +
                            "amtTotal, isTax, payLeft, qtyLeft, pdLimitDate) " +
                            $"VALUES ('{InStkId}', '{newPno}', '{newPdAmt}', '{newPdAmtNoneTax}', '{newPdQty}', " +
                            $"'{newPdAmtTotal}', '{row["isTax"]}', '{newPdAmtTotal}', '{(_pvSn == "-1" ? 0 : newPdQty)}', '{newPd["pdLimitDate"]}')";
                    getTbl.updTbl("WP", SqlComm);

                    //取得進貨明細檔序號
                    SqlComm = $"SELECT ISNULL(MAX(sn), 0) AS NewInDtlSn FROM WP_InStockDtl WHERE InStkId='{InStkId}' AND pNo='{newPno}'";
                    newDtlTbl = getTbl.table("WP", SqlComm);

                    //平均成本計算及更改庫存
                    newCostAvg = ognlQtyNow <= 0
                        ? newPdQty <= 0 ? ognlCostAvg : newPdAmt
                        : ((ognlQtyNow + newPdQty) <= 0 || ((ognlCostAvg * ognlQtyNow) + (newPdAmt * newPdQty)) <= 0)
                            ? ognlCostAvg
                            : toDouble.Numer(((ognlCostAvg * ognlQtyNow) + (newPdAmt * newPdQty)) / (ognlQtyNow + newPdQty), 2);    //無庫存情況下平均成本為進貨價
                    SqlComm = $"UPDATE WP_Product SET {(newPdQty <= 0 ? "" : $"costStd='{newPdAmt}', ")}costAvg='{newCostAvg}', qtyNow='{ognlQtyNow + newPdQty}' WHERE PNo = '{newPno}'";    //更新商品平均成本及庫存,標準成本變更為本次進貨價(退貨不改本次成本)
                    getTbl.updTbl("WP", SqlComm);

                    //寫入庫存記錄檔
                    SqlComm = $"INSERT INTO WP_StockTrace " +
                            $"(timeNo, InStkId, InStkDtlSn, Kind, pNo, amount, Qty, QtyNow, costAvg, costInitial, empId) " +
                            $"VALUES ('{MaxTNoTbl.Rows[0]["MaxTimeNo"]}', '{InStkId}', '{newDtlTbl.Rows[0]["NewInDtlSn"]}', 'I', '{newPno}', '{newPdAmt}', '{newPdQty}', '{ognlQtyNow + newPdQty}', '{newCostAvg}', '{newCostInitl}', '{emp.EmpId}')";
                    getTbl.updTbl("WP", SqlComm);
                }

                //寫入進貨主檔
                SqlComm = $"INSERT INTO WP_InStock " +
                    "(InStkId, InStkDate, pvSn, amount, tax, " +
                    "amtNoneTax, isTax, empId, payLeft, reciptNo) " +
                    $"VALUES ('{InStkId}', '{_stkDate}', '{_pvSn}', '{M_amtTotal}', '{M_amtTotal - M_amtNoneTax}', " +
                    $"'{M_amtNoneTax}', '{M_isTax}', '{emp.EmpId}', '{(_pvSn == "-1" ? 0 : M_amtTotal)}', '{_reciptNo}')";      //_pvSn=-1:庫存調整預設已結帳
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

