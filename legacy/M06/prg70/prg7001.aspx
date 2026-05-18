<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg7001.aspx.cs" Inherits="prg7001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="/ext/css/7001.css" rel="stylesheet" />
    <style>
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
            <div id="print-div" style="display: none;"></div>
            <div style="border-top: 2px #080 dotted; padding-top: 15px; text-align: center; margin-top: 30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i>回上一頁</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script src="/Scripts/printThis.js"></script>
    <script>
        var pointQty = parseInt($("#pointQty").val());
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("rptrYM", null);
                $.cookie("pKSId", null);

                window.location.reload();
            }
        };

        function CalcTotal() {
            if ($("#chkYM").val() == "Y") {
                var preQty, preCost, iQty, iAmt, oQty, oAmt, stkQty, stkCost, oCost, oProfit, pNo;
                var preQtyTotal = 0, preCostTotal = 0, iQtyTotal = 0, iAmtTotal = 0, oQtyTotal = 0, oAmtTotal = 0, stkQtyTotal = 0, stkCostTotal = 0, oCostTotal = 0, oProfitTotal = 0;
                var oProfitFinal;
                var $this;
                $(".tr-row").each(function () {
                    $this = $(this);
                    pNo = parseInt($this.find(".pNo").attr("data-val"));
                    preQty = parseInt($this.find(".preQty").attr("data-val"));
                    preCost = parseFloat($this.find(".preCost").attr("data-val"));
                    iQty = parseInt($this.find(".iQty").attr("data-val"));
                    iAmt = parseFloat($this.find(".iAmt").attr("data-val"));
                    oQty = parseInt($this.find(".oQty").attr("data-val"));
                    oAmt = parseInt($this.find(".oAmt").attr("data-val"));
                    stkQty = parseInt($this.find(".stkQty").attr("data-val"));
                    stkCost = parseFloat($this.find(".stkCost").attr("data-val"));
                    oCost = parseFloat($this.find(".oCost").attr("data-val"));
                    oProfit = parseFloat($this.find(".oProfit").attr("data-val"));

                    preQtyTotal += preQty;
                    preCostTotal += preCost;
                    if (pNo != 182 && pNo != 183 && pNo != 184 && pNo != 185 && pNo != 251 && pNo != 252
                        //&& pNo != 321
                    )
                    {
                        iQtyTotal += iQty;
                        oQtyTotal += oQty;
                        iAmtTotal += iAmt;
                        stkQtyTotal += stkQty;
                        stkCostTotal += stkCost;
                        oCostTotal += oCost;
                        oProfitTotal += oProfit;
                    }
                    oAmtTotal += oAmt;
                });
                $(".preQty-total").text(to3dot(preQtyTotal)); 
                $(".preCost-total").text(to3dot(roundToPoint(preCostTotal, pointQty)));
                $(".iQty-total").text(to3dot(iQtyTotal));
                $(".iAmt-total").text(to3dot(iAmtTotal));
                $(".oQty-total").text(to3dot(oQtyTotal));
                discntTotal = parseInt($(".oAmt-total").attr("data-discntVal"));
                $(".oAmt-total").html((discntTotal == 0 ? "" : "<span style='margin-right:5px;'>(全折:" + discntTotal + ")</span>") + to3dot(oAmtTotal - discntTotal));
                $(".stkQty-total").text(to3dot(stkQtyTotal));
                $(".stkCost-total").text(to3dot(roundToPoint(stkCostTotal, pointQty)));
                $(".oCost-total").text(to3dot(roundToPoint(oCostTotal, pointQty)));
                oProfitTotal = oProfitTotal - discntTotal;
                $(".oProfit-total").text(to3dot(roundToPoint(oProfitTotal, pointQty)));
                oProfitFinal = oProfitTotal - parseInt($("#amtCargo").attr("data-val")) - parseInt($("#amtCoupon").attr("data-val"));
                $("#oProfit-final").attr("data-val", oProfitFinal).text(to3dot(oProfitFinal));
            }
        }

        function SetRptrM() {
            var startYM = $("#startYM").val();
            var $obj = $("#rptr-month");
            var val = $("#rptrYM").val();
            $obj.empty();
            if ($("#rptr-year").val() == "") {
                $obj.append("<option value=''>請選擇</option>");
            }
            else {
                var sMonth = ($("#rptr-year").val() == startYM.substr(0, 4)) ? parseInt(startYM.substr(4, 2)) : 1;

                for (i = sMonth; i <= 12; i++) {
                    selected = val == "" ? "" : (i == parseInt(val.substr(4, 2))) ? "selected" : "";
                    $obj.append("<option value='" + padLeft(i, 2) + "' " + selected + ">" + i + "</option>");
                }
            }
        }


        function SetCookies() {
            $.cookie("rptrYM", $("#rptr-year").val() + $("#rptr-month").val());
            $.cookie("pKSId", $("#pKSId").val());

        }

        function Initial() {
            $("#processing").hide();
            CalcTotal();
            SetCookies();
            SetRptrM();
            $(".page-total").text($("#page-total").val());
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //確定送出查詢進出貨報表
            $("#btn-submit").click(function () {
                if ($("#rptr-year").val() == "" || $("#rptr-month").val() == "") {
                    return error_focus("請先輸入報表年月！", $("#rptr-year").val() == "" ? $("#rptr-year") : $("#rptr-month"))
                }
                $(this).attr("disabled", true);
                $("#processing").show();
                SetCookies();
                location.reload();
            });

            $("#btn-prn").click(function () {
                $("#rptr-main").printThis(
                    {
                        printContainer: true,
                        importCSS: true,
                        loadCSS: "../ext/css/7001print.css"
                    });
            });

            $("#rptr-year").change(function () {
                SetRptrM();
            });
        });
    </script>
    <script type="text/javascript">
        function DownloadFile() {
            $.ajax({
                type: "POST",
                url: "prg7001.aspx/SaveExcel",
                data: "",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    //Convert Base64 string to Byte Array.
                    var bytes = Base64ToBytes(r.d);

                    //Convert Byte Array to BLOB.
                    var blob = new Blob([bytes], { type: "application/octetstream" });

                    //Check the Browser type and download the File.
                    var isIE = false || !!document.documentMode;
                    if (isIE) {
                        window.navigator.msSaveBlob(blob, "7001進銷存月報表.xlsx");
                    } else {
                        var url = window.URL || window.webkitURL;
                        link = url.createObjectURL(blob);
                        var a = $("<a />");
                        a.attr("download", "7001進銷存月報表.xlsx");
                        a.attr("href", link);
                        $("body").append(a);
                        a[0].click();
                        $("body").remove(a);
                    }
                }
            });
        };
        function Base64ToBytes(base64) {
            var s = window.atob(base64);
            var bytes = new Uint8Array(s.length);
            for (var i = 0; i < s.length; i++) {
                bytes[i] = s.charCodeAt(i);
            }
            return bytes;
        };
    </script>
</asp:Content>
