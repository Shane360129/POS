<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg7003.aspx.cs" Inherits="prg7003" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="/ext/css/7003.css" rel="stylesheet" />
    <link href="/ext/css/7003print.css" rel="stylesheet" />
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
    <script src="/Scripts/printThis.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("rptrYM", null);
                window.location.reload();
            }
        };

        function CalcTotal() {
            var amtTotal;
            $(".list-main").find(".amt-row").each(function () {
                amtTotal = 0;
                $(this).find(".amt").each(function () {
                    amtTotal += parseInt($(this).attr("data-val"));
                });
                $(this).find(".mem-total").attr("data-val", amtTotal).text(to3dot(amtTotal));
            });

            for (j = 1; j <= 8; j++) {
                amtTotal = 0;
                $(".list-main").find(".amt[data-id='" + padLeft(j, 2) + "']").each(function () {
                    amtTotal += parseInt($(this).attr("data-val"));
                });
                $(".rptr-total").find("td[data-id='" + padLeft(j, 2) + "']").text(to3dot(amtTotal));
                if (amtTotal == 0 && !(j == 1 || j == 2 || j == 5)) { $(".list-main").find("td[data-id='" + padLeft(j, 2) + "']").hide();}
            }

            amtTotal = 0;
            $(".list-main").find(".mem-total").each(function () {
                amtTotal += parseInt($(this).attr("data-val"));
            });
            $(".rptr-total").find("[data-id='total']").attr("data-val", amtTotal).text(to3dot(amtTotal));

        }

        function SetCookies() {
            $.cookie("rptrYM", ($("#rptr-year").val() + $("#rptr-month").val()));
        }

        function SetRptrM() {
            var startYM = $("#startYM").val();
            var $obj = $("#rptr-month");
            var val = $("#rptrYM").val();
            $obj.empty();
            var sMonth = ($("#rptr-year").val() == startYM.substr(0, 4)) ? parseInt(startYM.substr(4, 2)) : 1;

            for (i = sMonth; i <= 12; i++) {
                selected = (i == parseInt(val.substr(4,2))) ? "selected" : "";
                $obj.append("<option value='" + padLeft(i, 2) + "' " + selected + ">" + i + "</option>");
            }
        }

        function Initial() {
            CalcTotal();
            SetRptrM();
            SetCookies();
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            //確定送出查詢進出貨報表
            $("#btn-submit").click(function () {
                SetCookies();
                location.reload();
            });

            $("#btn-prn").click(function () {
                var title = "<div class='rptr-title'>" + $("#rptr-year").val() + "年" + $("#rptr-month").val() + "月份扣帳明細表</div>" +
                    "<div style='text-align:right;font-size:14px;width:100%;'>列印日期︰" + $.date(new Date()) + "</div>";
                $(".list-main").printThis(
                    {
                        printContainer: true,
                        importCSS: true,
                        loadCSS: "../ext/css/7003print.css",
                        header: title
                    });
            });

            $("#rptr-year").change(function () {
                SetRptrM();
            });
        });
    </script>
</asp:Content>
