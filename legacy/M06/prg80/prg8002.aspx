<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg8002.aspx.cs" Inherits="prg8002" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        #form1 {display:flex;}
        #upload-main {display:flex;border:1px #000 solid;width:auto;padding:10px;border-radius:3px;align-items:center;}
        .div-upload {display:flex;}
        .div-status {margin-left:5px;}

        .list-main tr:last-child td {border-bottom:1px #c9c9c9 solid;}
        .list-main {border:1px #aaa solid;}
            .list-main td {border-top: 1px #c9c9c9 solid;border-right: 1px #ccc solid;text-align:right;}
            .list-main td.empty-data {text-align:center;}
            .list-main .list-title td{text-align:center;}
        .tr-row > td, .total-row > td {border-right: 1px #ccc solid;}
        .total-row > td {border-top:2px #666 solid; border-bottom:0!important;}
        .rptr-title {width:100%;text-align:center;font-size:18px;font-weight:bold;}

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <article>
        <div class="page-body prg-menu">
            <uc1:inc_Prg runat="server" ID="inc_Prg" />
        </div>
        <div class="page-body prg-content">
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <form id="form1" runat="server" class="formCLS">
                <div id="upload-main">
                    <div class="div-upload">
                        <div>
                            <asp:FileUpload ID="FileUpload1" runat="server"></asp:FileUpload>
                        </div>
                        <div>
                            <asp:Button ID="UploadButton1" Text="確定上傳" OnClick="UploadButton1_Click" runat="server"></asp:Button>
                        </div>
                    </div>
                    <div class="div-status"><asp:Label ID="UploadStatus1" class="upload-status" runat="server"></asp:Label></div>
                </div>
            </form>
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
            } else if (window.performance && window.performance.navigation.type != 2) {
                $(".upload-status").text("");
            }
        };

        function Initial() {
            $(".page-total").text($("#page-total").val());
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-prn").click(function () {
                $(".rptr-main").printThis(
                    {
                        printContainer: true,
                        importCSS: true,
                        loadCSS: "../ext/css/7050print.css",
                    });
            });
        });
    </script>
</asp:Content>
