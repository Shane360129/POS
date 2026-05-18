<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2001Upd.aspx.cs" Inherits="prg2001Upd" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    /* pdKind.js [START] */

    .pk-grp-main .pk-select-grp {display:flex;align-items:flex-start;padding:5px 0;padding-right:5px;border-bottom:2px #c9c9c9 dotted;}
    .pk-grp-main .pk-select-grp:first-child {padding-top:0;}
    .pk-grp-main .pk-select-grp:last-child {padding-bottom:0;border:0;}
    .pk-grp-main .pk-select-grp .PKind {margin-left:5px;}
    .pk-grp-main .pk-select-grp .PKind:first-child {margin-left:0;}
    .ARight .PKMPd {margin-left:0;}
    .btn-PK-del, .btn-Mc-del {margin-left:10px;}
    .PKGrp {margin-bottom: 5px;}
    /* pdKind.js [END] */

    #price-upd-main {margin-top:5px;display:none;}
    .data-row {border-top:2px #e7e7e7 dotted;display:flex;}
    .data-row > div {width:72px;text-align:right;padding:5px;margin-left:129px;}
    .data-row > div:nth-child(1){margin-left:78px;}
    .data-row > div.upd-date {margin-left:27px;width:100px;}

    .note {font-size:16px;color:#f00;margin-left:8px}
   
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <asp:Label ID="Label2" runat="server"></asp:Label>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回商品列表</button>
            <button class="btn-submit btn-page btn-admin" id="btn-upd"><i class="fas fa-check-circle"></i> 修改資料</button>
            <button class="btn-abort btn-page" id="btn-upd-abort"><i class="fas fa-times-circle"></i> 放棄修改</button>
            <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="pd-upd-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/ext/js/PdKind.js"></script>
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        var pointQty = parseInt($("#pointQty").val());
        function ChkFinish(_id) {
            if ($(".PKind:visible option[value=0]:selected").length > 0) { return error_focus("商品分類未點選！", $(".PKind:visible option[value=0]:selected").closest(".PKind")); }
            if ($("[name='pBarcode']").val() == "" && $("[name='pCode']").val() == "") { return error_focus("國際條碼及店內碼至少擇一輸入！", $("[name='pBarcode']")); }
            if ($("[name='pvSn']").val() == "") { return error_focus("請選擇廠商！", $("#pvType").val() == 2 ? $("#pv-filter") : $("[name='pvSn']")); }
            return noneEmpty(_id);
        }

        //是否可更動庫存
        function IsUpdStk() {
            console.log($("#is-upd-stk").val() == "N");
            if ($("#is-upd-stk").val() == "N") {
                $(".pd-cost").attr("disabled", true);
                $(".note-cost").html("<i class='fas fa-exclamation-circle'></i>已進銷貨無法修改成本！");
                $(".pd-stk").attr("disabled", true);
                $(".note-stk").html("<i class='fas fa-exclamation-circle'></i>已進銷貨無法修改庫存！");
            }
        }

        var pvJson;      //所有廠商
        function getJson(_json, _kind) {        //取得商品資料
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "B03_1",
                    json: _json,
                    kind: _kind
                },
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--getJson()--請洽工程人員');
                },
                success: function (response) {
                    if (response == "not-emp") {
                        location.replace("/login.aspx");
                    } else {
                        response == "N"
                            ? pvJson = jQuery.parseJSON("[]")
                            : pvJson = jQuery.parseJSON(response);
                    }
                }
            });
        }

        function closePvFilter(_bool) {     //廠商搜尋開啟/關閉
            if (_bool) {
                $(".del-txt-group.pv-txt-group").addClass("del-disabled");
                $("#pv-filter").prop("disabled", true);
            } else {
                $(".del-txt-group").removeClass("del-disabled");
                $("#pd-filter").prop("disabled", false);
            }
        }

        function Initial() {
            $(".pd-upd-main").find(":input").attr("disabled", true);
            $("#btn-spriceUpd").attr("disabled", false);
            $("#btn-upd-abort, #btn-submit").hide();
            closePvFilter(true);
            if ($("#pvType").val() == "2") { getJson("pvJson", "") };   //廠商是autocomplete
        }

        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-upd").click(function () {
                $(".pd-upd-main").find(":input").attr("disabled", false);
                closePvFilter(false);
                $(this).hide();
                $("#btn-upd-abort, #btn-submit").show();
                IsUpdStk();
            });

            $("#btn-upd-abort").click(function () {
                location.reload();
            });

            $("#btn-submit").click(function () {
                var id = $(this).attr("data-alt");
                if (ChkFinish(id)) {
                    var $obj = $("." + id);
                    makePKGrpToJson();      //商品分類To JSON
                    $obj.find(":input").attr("disabled", false);
                    var data = "args0=C05A&PKJson=" + $("#pkind-json").val() + "&" + $obj.find(":input").serialize();
                    $obj.find(":input").attr("disabled", true);
                    $.ajax({
                        url: "/AjaxMain.aspx",
                        type: "POST",
                        async: false,
                        data: data,
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--btn-submit.click--請洽工程人員');
                        },
                        success: function (response) {
                            console.log(response);
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能修改！");
                                    break;
                                case "B":
                                    error_focus("其他商品已使用這個國際條碼！", $obj.find("[name='pBarcode']"));
                                    break;
                                case "C":
                                    error_focus("其他商品已使用這個店內碼！", $obj.find("[name='pCode']"));
                                    break;
                                case "Y":
                                    alert("修改成功！");
                                    location.reload();
                                    break;
                                default:
                                    alert("新增失敗請聯絡工程人員！");
                            }
                        }
                    });
                }

            });

            $("#btn-spriceUpd").click(function () {
                $("#price-upd-main").slideToggle();
            });

            $("#pv-filter")
                .on('input', function () { $('#act-pvSn').val(""); })     //異動時清空pno欄位
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term;
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        var tbl = pvJson.filter((x) => { return (x.pvName.indexOf(str) >= 0 || x.pvNameS.indexOf(str) >= 0 || x.pvId.indexOf(str) >= 0); })
                        response($.map(tbl, function (item) { // 此處是將返回資料轉換為 JSON物件
                            return {
                                label: item.pvId + "．" + item.pvNameS, // 下拉項顯示內容
                                value: item.pvId + "．" + item.pvNameS,  // 下拉項對應數值
                                actPvSn: item.pvSn
                                //另外可以自定義其它引數
                            }
                        }));
                    },
                    select: function (event, ui) { //event引數是事件物件，ui物件只有一個item屬性，對應資料來源中被選中的物件
                        $("#pv-filter").val(ui.item.value).attr("data-val", ui.item.value);
                        $('#act-pvSn').val(ui.item.actPvSn);
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    $('#act-pvSn').val("");
                    $("#pv-filter").attr("data-val", "");
                });
        });
    </script>
</asp:Content>
