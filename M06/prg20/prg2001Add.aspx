<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg2001Add.aspx.cs" Inherits="prg2001Add" %>

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


</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main pd-add-main">
            <div class="input-row">
                <div class="input-title">名　　稱</div>
                <div class="input-text">
                    <span class="input-tag">商品全名︰</span><input type="text" name="pName" class="form-control" data-alt="MI_商品全名" maxlength="50" style="width:300px;margin-right:20px;" />
                    <span class="input-tag">簡稱︰</span><input type="text" name="pNameS" class="form-control" data-alt="MI_商品簡稱" maxlength="10" />
                </div>
            </div>
            <div class="input-row">
                <div class="input-title">條　　碼</div>
                <div class="input-text">
                    <span class="input-tag">國際條碼︰</span><input type="text" name="pBarcode" class="form-control" maxlength="20" style="margin-right:115px;" />
                    <span class="input-tag">店內碼︰</span><input type="text" name="pCode" class="form-control" maxlength="20" />
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">分　　類</div>
                <div class="input-text">
                    <asp:Label ID="Label2" runat="server"></asp:Label>
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">廠　　商</div>
                <div class="input-text">
                    <asp:Label ID="Label3" runat="server"></asp:Label>
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">規　　格</div>
                <div class="input-text">
                    <span class="input-tag">商品單位︰</span>
                    <asp:Label ID="Label4" runat="server"></asp:Label>
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">銷　　售</div>
                <div class="input-text">
                    <select name="isSale" class="form-control" style="margin-right:91px;"><option value="0">正常進銷貨</option><option value="1">只停止進貨</option><option value="2">只停止銷貨</option><option value="3">停止進銷貨</option></select>
                    <span class="input-tag">稅　　別︰</span>
                    <select name="isTax" class="form-control" style="margin-right:69px;"><option value="Y">應稅</option><option value="N">免稅</option></select>
                    <span class="input-tag" style="display:none">扣庫存︰</span>
                    <select name="isUpdStock" class="form-control" style="display:none;margin-right:51px;"><option value="Y">是</option><option value="N">否</option></select>
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">價　　格</div>
                <div class="input-text">
                    <span class="input-tag">標準售價︰</span>
                    <input type="text" name="priceStd" maxlength="5" class="form-control align-r chk-input" data-func="number" data-alt="MI_標準售價" style="width:72px;margin-right:35px;" value="0" />
                    <span class="input-tag">最低應售價︰</span>
                    <input type="text" name="priceLow" maxlength="5" class="form-control align-r chk-input" data-func="number" data-alt="MI_最低應售價" style="width:70px;margin-right:67px;" value="0" />
                    <span class="input-tag">會員價︰</span>
                    <input type="text" name="priceMem" maxlength="5" class="form-control align-r chk-input" data-func="number" data-alt="MI_會員價" style="width:70px;margin-right:65px;" value="0" />                    
                    <span class="input-tag">大批價︰</span>
                    <input type="text" name="priceBat" maxlength="5" class="form-control align-r chk-input" data-func="number" data-alt="MI_大批價" style="width:70px;" value="0" />                    
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">成　　本</div>
                <div class="input-text">
<%--                    <span class="input-tag">標準成本︰</span>
                    <input type="text" name="costStd" maxlength="10" class="form-control align-r chk-input" data-func="real_number" data-alt="MI_標準成本" style="width:103px;margin-right:20px;" value="0" />
                    <span class="input-tag">平均成本︰</span>
                    <input type="text" name="costAvg" maxlength="10" class="form-control align-r chk-input" data-func="real_number" data-alt="MI_平均成本" style="width:103px;margin-right:18px;" value="0" />--%>
                    <span class="input-tag">期初成本︰</span>
                    <input type="text" name="costInitial" maxlength="10" class="form-control align-r chk-input" data-func="real_number" data-alt="MI_期初成本" style="width:103px" value="0" />                    
                </div>
            </div>
            <div class="input-row" style="align-items:flex-start;">
                <div class="input-title">庫　　存</div>
                <div class="input-text">
<%--                    <span class="input-tag">目前庫存︰</span>
                    <input type="text" name="qtyNow" maxlength="5" class="form-control align-r chk-input" data-func="number" data-alt="MI_目前庫存" style="width:70px;margin-right:55px;" value="0" />--%>
                    <span class="input-tag">期初庫存︰</span>
                    <input type="text" name="qtyInitial" maxlength="5" class="form-control align-r chk-input" data-func="number" data-alt="MI_期初庫存" style="width:70px;" value="0" />
                </div>
            </div>
        </div>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回商品列表</button>
            <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="pd-add-main"><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/ext/js/PdKind.js"></script>
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        function ChkFinish(_id) {
            if ($(".PKind:visible option[value=0]:selected").length > 0) { return error_focus("商品分類未點選！", $(".PKind:visible option[value=0]:selected").closest(".PKind")); }
            if ($("[name='pBarcode']").val() == "" && $("[name='pCode']").val() == "") { return error_focus("國際條碼及店內碼至少擇一輸入！", $("[name='pBarcode']")); }
            if ($("[name='pvSn']").val() == "") { return error_focus("請選擇廠商！", $("#pvType").val() == 2 ? $("#pv-filter") : $("[name='pvSn']")); }
            return noneEmpty(_id);
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

        function Initial() {
            //$(".pvk-input-main").hide();
            if ($("#pvType").val() == "2") { getJson("pvJson", "") };   //廠商是autocomplete
        }



        $(function () {
            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-submit").click(function () {
                var id = $(this).attr("data-alt");
                if (ChkFinish(id)) {
                    var $obj = $("." + id);
                    makePKGrpToJson();      //商品分類To JSON
                    var data = "args0=C05&PKJson=" + $("#pkind-json").val() + "&" + $obj.find(":input").serialize();
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
                            switch (response) {
                                case "not-emp":
                                    location.replace("/login.aspx");
                                    break;
                                case "UA":
                                    alert("非管理者不能新增！");
                                    break;
                                case "B":
                                    error_focus("國際條碼已存在！", $obj.find("[name='pBarcode']"));
                                    break;
                                case "C":
                                    error_focus("店內碼已存在！", $obj.find("[name='pCode']"));
                                    break;
                                case "Y":
                                    alert("新增成功！");
                                    location.href = "/prg20/prg2001.aspx";
                                    break;
                                default:
                                    console.log(response);
                                    alert("新增失敗請聯絡工程人員！");
                            }
                        }
                    });
                }

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
