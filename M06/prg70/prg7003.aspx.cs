using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Net;
using static Global;

public partial class prg7003 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Employee emp = new Employee();
        ToDouble toDouble = new ToDouble();
        if (!emp.IsEmp) { Response.Redirect("/Login.aspx"); }
        else
        {
            string PrgId = "7003";
            if (!emp.ChkPrg(PrgId)) { Response.Redirect("/index.aspx"); }
            else
            {
                DataRow prgRow0 = emp.PrgTbl.Rows[0];
                Label1.Text = $"<input type='hidden' id='is-admin' value='{emp.EmpIsAdmin}' />" +
                $"<div class='prg-title'><i class='fas fa-map-marker-alt'></i> {prgRow0["prgId"]} {prgRow0["prgName"]}</div>";

                string rptrYM = WebUtility.UrlDecode(cookies.Read("rptrYM"));
                rptrYM = rptrYM == "" ? DateTime.Now.AddMonths(-1).ToString("yyyyMM") : rptrYM;
                rptrYM = int.Parse(rptrYM) < int.Parse(StartYM.Substring(0, 6)) ? StartYM.Substring(0, 6) : rptrYM;

                string SqlComm = $"SELECT * FROM WP_Member WHERE isStop='N' AND memKId = '2' ORDER BY memId";
                DataTable memTbl = getTbl.table("WP", SqlComm);

                SqlComm = $"SELECT * FROM WP_OutStock WHERE isDel='N' AND SUBSTRING(OutStkId, 1, 6)='{rptrYM}' ORDER BY OutStkId";
                DataTable oStkTbl = getTbl.table("WP", SqlComm);

                Label2.Text = $"<input type='hidden' id='prgName' value='{prgRow0["prgName"]}'>" +
                $"<input type='hidden' id='rptrYM' value='{rptrYM}'>" +
                $"<input type='hidden' id='startYM' value='{StartYM}'>" +
                "<div class='prg-subtitle' style='margin-bottom:0;'><i class='fas fa-arrow-alt-circle-down'></i> 報表條件</div>" +
                "<div class='search-main'>" +
                    "<div>" +
                        "<div class='search-sub'>" +
                            $"<span class='input-title'>年　　月</span>" +
                            "<select style='margin-right:3px;' id='rptr-year'>";
                                for (int i = int.Parse(StartYM.Substring(0, 4)); i <= int.Parse($"{DateTime.Now:yyyy}"); i++) { Label2.Text += $"<option value='{i}' {(i == int.Parse(rptrYM.Substring(0, 4)) ? "selected" : "")}>{i}</option>"; };
                            Label2.Text += "</select>年" +
                            "<select style='margin-left:10px;margin-right:3px;' id='rptr-month'></select>月" +
                        "</div>" +
                    "</div>" +
                    "<div class='page-submit'><button class='btn-admin btn-submit' id='btn-submit' style='margin-left:30px;'>確認送出</button></div>" +
                "</div>" +
                "<table class='list-main' style='border-spacing:1px;'>" +
                    "<tr class='list-title'>" +
                        "<td style='text-align:left'>姓名</td>";
                        SqlComm = $"SELECT * FROM WP_PayKind WHERE isDel='N' ORDER BY PayKId";
                        DataTable payKindTbl = getTbl.table("WP", SqlComm);
                        foreach (DataRow row in payKindTbl.Rows)
                        {
                            Label2.Text += $"<td class='payK_{row["PayKId"]}' data-id='{row["PayKId"]}'>{row["PayKName"]}</td>";
                        }
                        Label2.Text += "<td></td><td>本月金額</td>" +
                    "</tr>";
                    if (oStkTbl.Rows.Count == 0)
                    {
                        Label2.Text += $"<tr><td colspan='10' class='empty-data'>查無資料！</td></tr>";
                    }
                    else
                    {
                        ThreeDot threeDot = new ThreeDot();
                        string pJSON;
                        foreach (DataRow row in memTbl.Rows)
                        {
                            Label2.Text += $"<tr class='amt-row'><td style='text-align:left;'>{row["memName"]}</td>";
                            pJSON = CalcPayList(oStkTbl.Select($"memSn='{row["sn"]}'"));
                            JArray payJSON = JArray.Parse(pJSON);

                            foreach (var pay in payJSON)
                            {
                                Label2.Text += $"<td class='amt' data-id='01' data-val='{pay["pay01"]}'>{threeDot.To3Dot($"{pay["pay01"]}")}</td>" +
                                    $"<td class='amt' data-id='02' data-val='{pay["pay02"]}'>{threeDot.To3Dot($"{pay["pay02"]}")}</td>" +
                                    $"<td class='amt' data-id='03' data-val='{pay["pay03"]}'>{threeDot.To3Dot($"{pay["pay03"]}")}</td>" +
                                    $"<td class='amt' data-id='04' data-val='{pay["pay04"]}'>{threeDot.To3Dot($"{pay["pay04"]}")}</td>" +
                                    $"<td class='amt' data-id='05' data-val='{pay["pay05"]}'>{threeDot.To3Dot($"{pay["pay05"]}")}</td>" +
                                    $"<td class='amt' data-id='06' data-val='{pay["pay06"]}'>{threeDot.To3Dot($"{pay["pay06"]}")}</td>" +
                                    $"<td class='amt' data-id='07' data-val='{pay["pay07"]}'>{threeDot.To3Dot($"{pay["pay07"]}")}</td>" +
                                    $"<td class='amt' data-id='08' data-val='{pay["pay08"]}'>{threeDot.To3Dot($"{pay["pay08"]}")}</td>";
                            }
                            Label2.Text += "<td>　</td><td class='mem-total'></td></tr>";
                        }
                        Label2.Text += $"<tr class='rptr-total'>" +
                            $"<td>合計︰</td>" +
                            $"<td data-id='01'></td>" +
                            $"<td data-id='02'></td>" +
                            $"<td data-id='03'></td>" +
                            $"<td data-id='04'></td>" +
                            $"<td data-id='05'></td>" +
                            $"<td data-id='06'></td>" +
                            $"<td data-id='07'></td>" +
                            $"<td data-id='08'></td>" +
                            $"<td></td>" +
                            $"<td data-id='total'></td>" +
                        "</tr>";
                    }
                Label2.Text += "</table>" +
                "<div class='align-r' style='margin-top:15px;'><button id='btn-prn' style='color:#080'>列　印</button></div>";

            }
        }
    }

    private string CalcPayList(DataRow[] _oStkTbl)
    {
        int pay01 = 0, pay02 = 0, pay03 = 0, pay04 = 0, pay05 = 0, pay06 = 0, pay07 = 0, pay08 = 0;
        JArray payJSON;
        foreach (DataRow row in _oStkTbl)
        {
            payJSON = JArray.Parse($"{row["payList"]}");
            foreach (var pay in payJSON)
            {
                switch ($"{pay["PAYID"]}")
                {
                    case "01":
                        pay01 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "02":
                        pay02 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "03":
                        pay03 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "04":
                        pay04 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "05":
                        pay05 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "06":
                        pay06 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "07":
                        pay07 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                    case "08":
                        pay08 += int.Parse($"{pay["PAYAMT"]}");
                        break;
                }
            }
        }
        return $"[{{\"pay01\":\"{pay01}\",\"pay02\":\"{pay02}\",\"pay03\":\"{pay03}\",\"pay04\":\"{pay04}\",\"pay05\":\"{pay05}\",\"pay06\":\"{pay06}\",\"pay07\":\"{pay07}\",\"pay08\":\"{pay08}\"}}]";
    }
}