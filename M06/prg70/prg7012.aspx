﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg7012.aspx.cs" Inherits="prg7012" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="/ext/css/7004.css" rel="stylesheet" />
    <style>
        .search-main {border:1px #080 solid;border-radius:3px;margin-bottom:20px;padding:20px 10px;padding-bottom:0;}
        .search-main > div {display:flex;align-items:center;flex-wrap:wrap;margin-bottom:10px;}
        .search-main .search-sub {display:flex;align-items:center;margin-bottom:10px;margin-right:40px;}
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
    <script src="/Scripts/printThis.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("sDate", null);
                $.cookie("eDate", null);
                $.cookie("ChkDate", null);
                window.location.reload();
            }
        };

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

        function CalcTotal() {
            var amtTotal;
            var kindAry = new Array("qty_p", "amt_p", "qty_m", "amt_m", "amt_in", "qty_sum", "amt_sum");
            var $obj = $(".list-main")
            kindAry.forEach(function (value) {
                amtTotal = 0;
                $this = $obj.find("." + value);
                $this.each(function () {
                    amtTotal += parseInt($(this).attr("data-val"));
                });
                $obj.find("." + value + "_total").attr("data-val", amtTotal).text(to3dot(amtTotal));

            });
        }

        function SetCookies() {
            $.cookie("sDate", $("#sDate").val());
            $.cookie("eDate", $("#eDate").val());
            $.cookie("ChkDate", $("#ChkDate").val());

        }

        function Initial() {
            $("#processing").hide();
            CalcTotal();
            SetCookies();
            $(".page-total").text($("#page-total").val());
        }

        $(function () {
            Initial();

            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                maxDate: 0,
                minDate: new Date($("#startYM").val().substr(0,4)+"-"+$("#startYM").val().substr(4,2)+"-"+$("#startYM").val().substr(6,2))
            });

            $("#btn-back").click(function () { history.back(-1); });

            //確定送出查詢進出貨報表
            $("#btn-search").click(function () {
                if ($("#sDate").val() == "" || $("#eDate").val() == "") {
                    return error_focus("日期區間不得為空白！", ($("#sDate").val() == "" ? $("#sDate") : $("#eDate")));
                }
                $(this).attr("disabled", true);
                $("#processing").show();
                SetCookies();
                location.reload();
            });

            $("#btn-prn").click(function () {
                //var title = "<div class='rptr-title'>廠商進退貨統計排行</div>";
                $(".rptr-main").printThis(
                    {
                        printContainer: true,
                        importCSS: true,
                        loadCSS: "../ext/css/7004print.css"
                    });
            });

        });
    </script>
    <script type="text/javascript">
        function DownloadFile() {
            $.ajax({
                type: "POST",
                url: "prg7012.aspx/SaveExcel",
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
                        window.navigator.msSaveBlob(blob, "7012門市盤點明細表.xlsx");
                    } else {
                        var url = window.URL || window.webkitURL;
                        link = url.createObjectURL(blob);
                        var a = $("<a />");
                        a.attr("download", "7012門市盤點明細表.xlsx");
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
