<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg9002.aspx.cs" Inherits="prg9002" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .prg-main {display:flex;width:98%;margin:0 auto;}
    .prg-content .move-btn-main {width:10%;text-align:center;padding-top:65px;}
    .move-btn-main button {display:block;}
    .prg-zone {width:45%}
    .prg-tree {width:100%;border:1px #c9c9c9 solid;border-radius:3px;min-height:500px;padding:10px;}
    .btn-move {text-align:center;cursor:pointer;margin:0 auto;margin-bottom:10px;padding:3px 5px;font-weight:bold;display:block;min-width:32px;}

    .tree-row {padding: 3px 0;display:flex;align-items:center;}
    .tree-row label {margin-left:2px;}
    .prg-row {margin-left:45px;}
    .prg-row label {font-weight:normal;}
    .prg-content .node-main input[type='checkbox'] {margin-left:0;width:16px;height:16px;}

    .node-chked, .prg-chked {color:#c9c9c9;}
    .node-unchked, .prg-unchked {color:#000;}
    .emp-prg-group {display:none;}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="prg-main">
            <div class="prg-zone prg-orgnl">
                <div class="prg-subtitle" style="padding-bottom:2px;"><i class="fas fa-arrow-alt-circle-down"></i> 所有作業</div>
                <div class="prg-tree"><asp:Label ID="Label2" runat="server"></asp:Label></div>
            </div>
            <div class="move-btn-main">
                <button class="btn-move btn-add-all">全部 <i class="fas fa-chevron-circle-right"></i></button>
                <button class="btn-move btn-del-all"><i class="fas fa-chevron-circle-left"></i> 全部</button>
                <button class="btn-move btn-add-chked" style="margin-top:20px;"><i class="fas fa-chevron-circle-right"></i></button>
                <button class="btn-move btn-del-chked"><i class="fas fa-chevron-circle-left"></i></button>
                <button class="btn-move btn-abort" style="color:#f00;margin-top:45px;width:66px;"><i class="fas fa-times-circle"></i> 放棄</button>
                <button class="btn-move btn-submit btn-admin"><i class="fas fa-check-circle"></i> 確定</button>
            </div>
            <div class="prg-zone prg-target">
                <div class="prg-subtitle" style="padding-bottom:2px;"><i class="fas fa-arrow-alt-circle-down"></i> 開放權限</div>
                <div class="prg-tree"><asp:Label ID="Label3" runat="server"></asp:Label></div>
            </div>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
<script>
    var $objOrgnl = $(".prg-orgnl");
    var $objTarget = $(".prg-target");

    function chkOrgnlPrg() {
        $objOrgnl.find("label").removeClass("prg-chked");
        $objOrgnl.find(".prg-ckbox").prop("disabled", false);
        $(".prg-ckbox").prop("checked", false);
        $objTarget.find(".prg-ckbox:visible").each(function () {
            var $obj = $objOrgnl.find(".prg-ckbox[data-id='" + $(this).attr("data-id") + "']");
            $obj.prop({ "checked": true, "disabled": true });
            $obj.closest(".prg-row").find("label").addClass("prg-chked");
        });
        chkNode();
    }

    function chkNode() {
        $objOrgnl.find(".node-main").each(function () {
            if ($(this).find(".prg-ckbox:not(:checked)").length == 0) {
                $(this).find(".prg-node label").addClass("node-chked");
                $(this).find(".node-ckbox").prop({ "checked": true, "disabled": true });
            } else {
                $(this).find(".prg-node label").removeClass("node-chked");
                $(this).find(".node-ckbox").prop({ "checked": false, "disabled": false });
            };
        });

        $objTarget.find(".node-main").each(function () {
            $(this).find(".prg-node").toggle($(this).find(".prg-ckbox:visible").length != 0);
            $(this).find(".prg-node").find(".node-ckbox").prop("checked", $(this).find(".prg-ckbox:visible:not(:checked)").length == 0);
        });
    }

    function markTarget() {
        var empGrpId = $(".emp-group option:selected").val();
        var empPrgGrp = $(".emp-prg-group[data-id='" + empGrpId + "']").text();
        
        $objTarget.find(".prg-row").hide();
        if (empPrgGrp != "") {
            $.each(empPrgGrp.split(','), function (index, value) {
                $objTarget.find(".prg-ckbox[data-id='" + value + "']").closest(".prg-row").show();
            });
        };
        chkOrgnlPrg();
    };


    function Initial() {
        markTarget();
    }

    $(function () {
        Initial();

        $(".emp-group").change(function () {
            markTarget();
        });

        $(".node-ckbox").click(function () {
            $(this).closest(".node-main").find(".prg-ckbox").prop("checked", $(this).is(":checked"));
        });

        $(".btn-add-all").click(function () {
            $objTarget.find(".prg-row").show();
            chkOrgnlPrg();
        });

        $(".btn-del-all").click(function () {
            $objTarget.find(".prg-row").hide();
            chkOrgnlPrg();
        });

        $(".btn-abort").click(function () {
            location.reload();
        });

        $(".prg-ckbox").click(function () {
            var $obj = $(this).closest(".node-main");
            $obj.find(".node-ckbox").prop("checked", $obj.find(".prg-ckbox:visible:not(:checked)").length == 0);
        });

        $(".btn-add-chked").click(function () {
            var $objChked = $objOrgnl.find(".prg-ckbox:not(:disabled):checked");
            if ($objChked.length == 0) {
                alert("請先勾選左側要開放權限的作業！");
            } else {
                $objChked.each(function () {
                    $objTarget.find(".prg-ckbox[data-id='" + $(this).attr("data-id") + "']").closest(".prg-row").show();
                });
                chkOrgnlPrg();
            };
        });

        $(".btn-del-chked").click(function () {
            var $objChked = $objTarget.find(".prg-ckbox:visible:checked");
            if ($objChked.length == 0) {
                alert("請先勾選右側要取消開放權限的作業！");
            } else {
                $objChked.each(function () {
                    $(this).closest(".prg-row").hide();
                });
                chkOrgnlPrg();
            };
        });

        $(".btn-submit").click(function () {
            var prgIdGrp = "";
            $objTarget.find(".prg-ckbox:visible").each(function () {
                prgIdGrp += (prgIdGrp == "" ? "" : ",") + $(this).attr("data-id");
            });
            console.log(prgIdGrp);
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "C10",
                    empGrpId: $(".emp-group option:selected").val(),
                    empPrgIdGrp: prgIdGrp
                },
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--btn-submit.click--請洽工程人員');
                },
                success: function (response) {
                    switch (response) {
                        case "not-emp":
                            location.replace("/login.aspx");
                            break;
                        case "UA":
                            alert("非管理者不能修改！");
                            break;
                        case "Y":
                            alert("修改成功！");
                            location.reload();
                            break;
                        default:
                            alert("修改失敗請聯絡工程人員！");
                    }
                }
            });
        });


    });
</script>
</asp:Content>
