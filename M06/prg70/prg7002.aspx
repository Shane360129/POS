<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg7002.aspx.cs" Inherits="prg7002" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        .list-main tr:last-child td {border-bottom:1px #c9c9c9 solid;}
        .list-main {border:1px #aaa solid;}
            .list-main td {border-top: 1px #c9c9c9 solid;border-right: 1px #ccc solid;text-align:right;}
            .list-main td.empty-data {text-align:center;}

        .tr-row > td, .total-row > td {border-right:1px #ccc solid;}
        .total-row > td {border-top:2px #666 solid; border-bottom:0!important;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <article>
        <div class="page-body prg-menu">
            <uc1:inc_Prg runat="server" ID="inc_Prg" />
        </div>
        <div class="page-body prg-content">
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <div class="page-main pd-list-main">
                <asp:Label ID="Label2" runat="server"></asp:Label>
            </div>
            <div id="print-div" style="display:none;"></div>
            <div style="border-top: 2px #080 dotted; padding-top: 15px; text-align: center; margin-top: 30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i>回上一頁</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        var pointQty = parseInt($("#pointQty").val());
        //設定日期選擇器為中文語系
        $.datepicker.regional['zh-TW'] = {
            dayNames: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"],
            dayNamesMin: ["日", "一", "二", "三", "四", "五", "六"],
            monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
            monthNamesShort: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
            prevText: "上月",
            nextText: "次月",
            weekHeader: "週"
        };

        $.datepicker.setDefaults($.datepicker.regional["zh-TW"]);

        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("rptrYear", null);
                $.cookie("rptrMonth", null);
                window.location.reload();
            }
        };

        function CalcTotal() {
            if ($("#chkYM").val() == "Y") {
                var preQty, iQty, iAmt, oQty, oAmt, stkQty, stkCost, oCost, oProfit;
                var preQtyTotal = 0, iQtyTotal = 0, iAmtTotal = 0, oQtyTotal = 0, oAmtTotal = 0, stkQtyTotal = 0, stkCostTotal = 0, oCostTotal = 0, oProfitTotal = 0;
                var oProfitFinal;
                var $this;
                $(".tr-row").each(function () {
                    $this = $(this);
                    preQty = parseInt($this.find(".preQty").attr("data-val"));
                    iQty = parseInt($this.find(".iQty").attr("data-val"));
                    iAmt = parseFloat($this.find(".iAmt").attr("data-val"));
                    oQty = parseInt($this.find(".oQty").attr("data-val"));
                    oAmt = parseInt($this.find(".oAmt").attr("data-val"));
                    stkQty = preQty + iQty - oQty;
                    stkCost = parseFloat($this.find(".stkCost").attr("data-val"));
                    oCost = parseFloat($this.find(".oCost").attr("data-val"));
                    oProfit = oQty == 0 ? 0 : roundToPoint(oAmt - oCost, pointQty);

                    $this.find(".stkQty").attr("data-val", stkQty).text(stkQty);
                    $this.find(".oProfit").attr("data-val", oProfit).text(oProfit);

                    preQtyTotal += preQty;
                    iQtyTotal += iQty;
                    iAmtTotal += iAmt;
                    oQtyTotal += oQty;
                    oAmtTotal += oAmt;
                    stkQtyTotal += stkQty;
                    stkCostTotal += stkCost;
                    oCostTotal += oCost;
                    oProfitTotal += oProfit;
                });
                $(".preQty-total").text(preQtyTotal);
                $(".iQty-total").text(iQtyTotal);
                $(".iAmt-total").text(iAmtTotal);
                $(".oQty-total").text(oQtyTotal);
                $(".oAmt-total").text(oAmtTotal);
                $(".stkQty-total").text(stkQtyTotal);
                $(".stkCost-total").text(stkCostTotal);
                $(".oCost-total").text(oCostTotal);
                $(".oProfit-total").text(oProfitTotal);
                oProfitFinal = oProfitTotal - parseInt($("#amtCargo").attr("data-val")) - parseInt($("#amtCoupon").attr("data-val"));
                $("#oProfit-final").attr("data-val", oProfitFinal).text(oProfitFinal);
            }
        }

        function SetCookies() {
            $.cookie("rptrYear", $("#rptr-year").val());
            $.cookie("rptrMonth", $("#rptr-month").val());
        }

        function Initial() {
            CalcTotal();
            SetCookies();
        }

        $(function () {
            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "-1:+0"
            });

            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //確定送出查詢進出貨報表
            $("#btn-submit").click(function () {
                SetCookies();
                location.reload();
            });

            $("#btn-prn").click(function () {
                var title = "<div class='rptr-title'>" + $("#rptr-year").val() + "年" + $("#rptr-month").val() + "月" + $("#prgName").val() + "</div>" +
                    "<div style='text-align:right;font-size:14px;width:100%;'>列印日期︰" + $.date(new Date()) + "</div>";
                $(".list-main").printThis(
                    {
                        printContainer: true,
                        importCSS: true,
                        loadCSS: "../ext/css/7001print.css",
                        header: title
                    });
            });

            $("#rptr-year").change(function () {
                var month = $("#chkout-sMonth").val();
                var $obj = $("#rptr-month");
                $obj.empty();
                if ($(this).find("option:selected").val() != $("#sYear").val()) {
                    for (i = 1; i < parseInt($("#sMonth").val()); i++) {
                        selected = (i == parseInt(month)) ? "selected" : "";
                        $obj.append("<option value='" + i + "' " + selected + ">" + i + "</option>");
                    }
                }
                for (i = parseInt($("#sMonth").val()); i <= 12; i++) {
                    selected = (i == parseInt(month)) ? "selected" : "";
                    $obj.append("<option value='" + i + "' " + selected + ">" + i + "</option>");
                }
            });
        });
    </script>
</asp:Content>
