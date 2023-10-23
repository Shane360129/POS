<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2001.aspx.cs" Inherits="prg2001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .pd-list-main {width:100%;}
    .pd-list-main .pd-list-title td {background-color:#e9e9e9;font-weight:bold;}
    .pd-list-main td {border-bottom:2px #c9c9c9 dotted;padding:5px;}
    .node-all-btn, .node-btn {cursor:pointer;}

</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main pd-list-main">
            <%--<div class="btn-admin" style="text-align:right;border-bottom:1px #080 solid;margin-bottom:15px;padding-bottom:5px;"><button class="btn-submit" id="btn-pd-add"><i class='fas fa-plus-circle'></i> 新增商品</button></div>--%>
            <asp:Label ID="Label2" runat="server"></asp:Label>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script>
        var pointQty = parseInt($("#pointQty").val());

        //"navigate"（導航）：表示页面導航是通過點擊連結、輸入 URL 或其他方式導航到該頁面的。
        //"reload"（重新加載）：表示頁面是通過刷新（重新加載）操作導航到的，例如使用者按下重新整理按鈕或使用瀏覽器的重新整理功能。
        //"back_forward"（後退或前進）：表示頁面導航是通過瀏覽器的後退或前進按鈕導航到的。
        //"prerender"（預渲染）：表示頁面是通過預渲染操作導航到的，某些瀏覽器可能會在後台預加載頁面以提高加載速度。
        window.onpageshow = function (event) {
            if (event.persisted || (performance && performance.getEntriesByType("navigation")[0].type === "back_forward")) {
                window.location.reload();
            } else if (performance && performance.getEntriesByType("navigation")[0].type === "navigate") {
                $.cookie("pvSn", null);
                window.location.reload();
            }
        };

        function Initial() {
            $(".node-right").hide();
        }



        $(function () {
            Initial();

            $("#btn-pd-add").click(function () {
                location.href = "/prg20/prg2001Add.aspx";
            });

            $(".node-all-btn").click(function () {
                switch ($(this).attr("data-alt")) {
                    case "close":
                        $(".node-detail").hide();
                        $(".node-btn[data-alt='open']").show();
                        $(".node-btn[data-alt='close']").hide();
                        break;
                    case "open":
                        $(".node-detail").show();
                        $(".node-btn[data-alt='open']").hide();
                        $(".node-btn[data-alt='close']").show();
                        break;
                };
            });

            $(".node-btn").click(function () {
                var id = $(this).closest(".node-main").attr("data-id");
                switch ($(this).attr("data-alt")) {
                    case "close":
                        $(".node-detail[data-id='" + id + "']").hide();
                        $(this).hide();
                        $(this).closest(".node-main").find(".node-btn[data-alt='open']").show();
                        break;
                    case "open":
                        $(".node-detail[data-id='" + id + "']").show();
                        $(this).hide();
                        $(this).closest(".node-main").find(".node-btn[data-alt='close']").show();
                        break;
                };
            });

            $("#btn-search").click(function () {
                $(this).attr("disabled", true);
                var isSale = $("#isSale").val();
                isSale = isSale == "" ? "ALL" : isSale;
                $.cookie("isSale", isSale);
                location.reload();
            });
        });
    </script>
</asp:Content>
