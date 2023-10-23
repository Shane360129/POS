<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg9001.aspx.cs" Inherits="prg9001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .empGp-add-btn{color:#080;}
    .empGp-row {border-bottom:2px #c9c9c9 dotted;padding:5px 0;display:flex;align-items:center;}
    .empGp-name-main {display:flex;align-items:center;margin-left:5px;}
    .prg-content .empGp-input-main .empGp-id {width:45px;}
    .prg-content .empGp-input-main .empGp-name {width:150px;}
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
    function ChkFinish(_id) {
        return noneEmpty(_id);
    }

    function pvkAdd(_id) {
        if (ChkFinish(_id)) {
            var $obj = $("." + _id);
            var pvkind = _id.split('-');
            var data = "args0=C09&" + $obj.find(":input").serialize();
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: data,
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--pvkAdd()--請洽工程人員');
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
                            error_focus("這個群組編號已存在！", $obj.find(".add-" + pvkind[0] + "Id"));
                            break;
                        case "Y":
                            alert("新增成功！");
                            location.reload();
                            break;
                        default:
                            alert("新增失敗請聯絡工程人員！");
                    }
                }
            });
        }
    }

    function pkUpd(_id, _updKind) {
        if (_updKind == "submit") { if (!ChkFinish(_id)) return false; }
        var $obj = $("." + _id);
        var data = "args0=C09A&updKind=" + _updKind + "&" + $obj.find(":input").serialize();
        $.ajax({
            url: "/AjaxMain.aspx",
            type: "POST",
            async: false,
            data: data,
            error: function (xhr) {
                console.log(xhr.responseText);
                alert('Ajax request 發生錯誤--pkUpd()--請洽工程人員');
            },
            success: function (response) {
                //alert(response);
                switch (response) {
                    case "UA":
                        alert("非管理者不能" + (_updKind == "del" ? "刪除" :"修改") + "！");
                        break;
                    case "Y":
                        alert( (_updKind == "del" ? "刪除" :"修改") + "成功！");
                        location.reload();
                        break;
                    default:
                        alert( (_updKind == "del" ? "刪除" :"修改") + "失敗請聯絡工程人員！");
                }
            }
        });
    }




    function Initial() {
        $(".empGp-input-main").hide();
    }

    $(function () {
        Initial();

        $(".empGp-add-btn").click(function () {
            var id = $(this).attr("data-id");
            pvkAdd(id);
        });

        /* 更新 [START] */
        $(".edit-btn").click(function () {
            $(".edit-btn").show();
            $(".empGp-input-main").hide();
            $(this).hide();
            $obj = $(this).closest(".empGp-name-main");
            $obj.find(".empGp-input-main").show();
        });

        $(".empGp-upd-btn").click(function () {
            var id = $(this).attr("data-id");
            var $obj = $(this).closest(".empGp-name-main");
            switch (id) {
                case "abort":
                    $(".edit-btn").show();
                    $obj.find(".empGp-name").val($obj.find(".empGp-name").attr("data-id"));
                    $obj.find(".empGp-input-main").hide();
                    break;
                case "submit":
                case "del":
                    if (confirm("確定" + (id == "submit" ? "修改" : "刪除") + "？")) {
                        var input_id = "empGp-input-main-" + $obj.attr("data-id");
                        pkUpd(input_id, id);
                    } else {
                        $(".edit-btn").show();
                        $obj.find(".empGp-name").val($obj.find(".empGp-name").attr("data-id"));
                        $obj.find(".empGp-input-main").hide();
                    }
                    break;
            }
        });
        /* 更新 [END]*/
    });
</script>
</asp:Content>
