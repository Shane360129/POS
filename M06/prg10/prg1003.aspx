<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg1003.aspx.cs" Inherits="prg1003" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .memk-row {border-bottom:2px #c9c9c9 dotted;padding:5px 0;display:flex;align-items:center;}
    .prg-content .memk-input-main .memk-id {width:45px;}
    .prg-content .memk-input-main .memk-name {width:150px;margin-left:5px;}

</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
<script>
    function ChkFinish(_$obj) {
        return noneEmptyObj(_$obj);
    }

    function memkAdd(_$obj) {
        if (ChkFinish(_$obj)) {
            var data = "args0=C03&" + _$obj.find(":input").serialize();
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: data,
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--memkAdd()--請洽工程人員');
                },
                success: function (response) {
                    switch (response) {
                        case "not-emp":
                            location.replace("/login.aspx");
                            break;
                        case "UA":
                            alert("非管理者不能新增！");
                            break;
                        case "A":
                            error_focus("這個類別編號已存在！", _$obj.find(".add-memKId"));
                            break;
                        case "Y":
                            alert("新增成功！");
                            location.reload();
                            break;
                        default:
                            console.log(response);
                            alert("新增失敗請聯絡工程人員！");
                    }
                }
            });
        }
    }

    function memkUpd(_$obj, _updKind) {
        var data = "args0=C03A&updKind=" + _updKind + "&" + _$obj.find(":input").serialize();
        $.ajax({
            url: "/AjaxMain.aspx",
            type: "POST",
            async: false,
            data: data,
            error: function (xhr) {
                console.log(xhr.responseText);
                alert('Ajax request 發生錯誤--memkUpd()--請洽工程人員');
            },
            success: function (response) {
                switch (response) {
                    case "not-emp":
                        location.replace("/login.aspx");
                        break;
                    case "UA":
                        alert("非管理者不能" + (_updKind == "del" ? "刪除" :"修改") + "！");
                        break;
                    case "Y":
                        alert( (_updKind == "del" ? "刪除" :"修改") + "成功！");
                        location.reload();
                        break;
                    default:
                        console.log(response);
                        alert( (_updKind == "del" ? "刪除" :"修改") + "失敗請聯絡工程人員！");
                }
            }
        });
    }

    function Initial() {
        $(".edit-zone").hide();
    }



    $(function () {
        Initial();

        $(".memk-add-btn").click(function () {
            //var id = $(this).attr("data-id");
            //memkAdd(id);
            memkAdd($(this).closest(".add-main"));
        });

        /* 更新 [START] */
        $(".edit-tag").click(function () {
            $(".edit-tag").show();
            $(".edit-zone").hide();
            $(this).hide();

            $obj = $(this).closest(".edit-single-main");
            $obj.find(".edit-txt").val($obj.find(".edit-txt").attr("data-id"));
            $obj.find(".edit-zone").show();
        });

        $(".edit-btn").click(function () {
            var id = $(this).attr("data-id");
            var $obj = $(this).closest(".edit-single-main");
            switch (id) {
                case "abort":
                    $(".edit-tag").show();
                    $obj.find(".edit-zone").hide();
                    break;
                case "submit":
                    if ($obj.find("edit-txt").val() == "") { return error_focus("類別名稱不得空白！", $obj.find("edit-txt")); }
                    memkUpd($obj, id);
                    break;
                case "del":
                    if (confirm("確定刪除？")) { memkUpd($obj, id); }
                    break;
            }
        });
        /* 更新 [END]*/
    });
</script>
</asp:Content>
