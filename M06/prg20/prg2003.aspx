﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2003.aspx.cs" Inherits="prg2003" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .mem-list-main {width:100%;}
    .mem-list-main .mem-list-title td {background-color:#e9e9e9;font-weight:bold;}
    .mem-list-main td {border-bottom:2px #c9c9c9 dotted;padding:5px;}
    .node-all-btn, .node-btn {cursor:pointer;}

</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main mem-list-main">
            <div class="btn-admin" style="text-align:right;border-bottom:1px #080 solid;margin-bottom:15px;padding-bottom:5px;"><button class="btn-submit" id="btn-mem-add"><i class="fas fa-plus-circle"></i> 新增會員</button></div>
            <asp:Label ID="Label2" runat="server"></asp:Label>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            }
        };

        $(function () {
            $("#btn-mem-add").click(function () {
                location.href = "/prg20/prg2003Add.aspx";
            });

        });
    </script>
</asp:Content>