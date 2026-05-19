<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg1001.aspx.cs" Inherits="prg1001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .node-pKM, .node-pKS {margin-left:20px;}
    .pk-row {border-bottom:2px #c9c9c9 dotted;padding:5px 0;display:flex;align-items:center;}
    .node-btn {cursor:pointer;}
    .prg-content .edit-single-main .edit-txt {width:150px;}
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
    function chkFinish(_$obj) {
        return noneEmptyObj(_$obj);
    }

    function pkAdd(_$obj) {
        if (chkFinish(_$obj)) {
            var pkind = _$obj.attr("data-id");
            var data = "args0=C01&pkind=" + pkind + "&" + _$obj.find(":input").serialize();
            console.log(data);
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: data,
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--pkAdd()--請洽工程人員');
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
                            error_focus("這個類別編號已存在！", _$obj.find(".add-" + pkind + "Id"));
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

    function pkUpd(_$obj, _updKind) {
        var data = "args0=C01A&updKind=" + _updKind + "&" + _$obj.find(":input").serialize();
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
                switch (response) {
                    case "not-emp":
                        location.replace("/login.aspx");
                        break;
                    case "UA":
                        alert("非管理者不能" + (_updKind == "del" ? "刪除" :"修改") + "！");
                        break;
                    case "A":
                        alert( "尚有下層分類不得刪除！");
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

    function SwitchPKindAdd() {
        $(".add-main").hide();
        $("." + $(".radio-pk-add:checked").val() + "-add-main").show();

    }

    //取得分類JSON [START]
    var jsonPKind;
    function loadPKindJson() {
        $.ajax({
            url: "/AjaxMain.aspx",
            type: "POST",
            async: false,
            data: {
                args0 : "B02"
            },
            dataType: "json",
            //如果連線成功
            success: function (data) {
                if (data == "not-emp") {
                    location.replace("/login.aspx");
                } else {
                    jsonPKind = data;
                }
            },
            error: function (xhr) {
                console.log(xhr.responseText);
                alert('Ajax request 發生錯誤--loadPKindJson()--請洽工程人員');
            },
        });
    }

    function chang_pkind(_$obj) {
        var pkind_id = _$obj.val();
        var $obj_pkM = _$obj.closest(".add-main").find(".add-pkMId");

        $obj_pkM.empty();
        var prePKMId = "";
        for (i = 0; i < jsonPKind.length; i++) {
            if (pkind_id == jsonPKind[i].pKLId) {
                if (prePKMId != jsonPKind[i].pKMId) {
                    $obj_pkM.append("<option value='" + jsonPKind[i].pKMId + "'>" + jsonPKind[i].pKMId + " " + jsonPKind[i].pKMName + "</option>");
                    prePKMId = jsonPKind[i].pKMId;
                }
            }
        }
    }

    function initlPKind() {
        loadPKindJson();
        $(".pkS-add-main").each(function () {
            var $obj = $(this);
            var $obj_pkLId = $obj.find(".add-pkLId");
            chang_pkind($obj_pkLId);

        });
    }
    //取得分類JSON [END]

    function ChkNode() {
        $(".node-right, .node-down").hide();
        $(".pk-node").each(function () {
            if ($(this).is(":hidden")) {
                $(".node-right[data-alt='" + $(this).attr("data-alt") + "']").show();
            } else {
                $(".node-down[data-alt='" + $(this).attr("data-alt") + "']").show();
            }
        });
    }

    function Initial() {
        var pkQty = $("#pkQty").val().split(',');
        if (pkQty[0] == 0) {
            $("#radio-pkM-add").attr("disabled", true);
            $("#radio-pkS-add").attr("disabled", true);
        } else {
            if (pkQty[1] == 0) {
                $("#radio-pkS-add").attr("disabled", true);
            }
        };

        $(".edit-zone").hide();

        SwitchPKindAdd();
        loadPKindJson();
        ChkNode();
    }



    $(function () {
        Initial();

        $(".pk-add-btn").click(function () {
            pkAdd($(this).closest(".add-main"));
        });

        $(".radio-pk-add").click(function () {
            SwitchPKindAdd();
        });

        $(".pkS-add-main").find(".add-pkLId").change(function () { chang_pkind($(this)); });

        /* 目錄控制 [START]*/
        $(".node-right").click(function () {
            $(this).hide();
            $(".node-down[data-alt='" + $(this).attr("data-alt") + "']").show();
            $(".pk-node[data-alt='" + $(this).attr("data-alt") + "']").show();
        });
        $(".node-down").click(function () {
            $(this).hide();
            $(".node-right[data-alt='" + $(this).attr("data-alt") + "']").show();
            $(".pk-node[data-alt='" + $(this).attr("data-alt") + "']").hide();
        });

        $(".node-all-close").click(function () {
            $(".pk-node").hide();
            ChkNode();
        });
        $(".node-all-open").click(function () {
            $(".pk-node").show();
            ChkNode();
        });

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
                    pkUpd($obj, id);
                    break;
                case "del":
                    if (confirm("確定刪除？")) { pkUpd($obj, id); }
                    break;
            }
        });
        /* 目錄控制 [END]*/
    });
</script>
</asp:Content>
