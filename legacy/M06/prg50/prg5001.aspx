<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg5001.aspx.cs" Inherits="prg5001" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        #inStk-list-main {width:100%;}
        .inStk-row, .inStk-dtl-row {display:flex;align-items:flex-start;}
        .inStk-row {padding:7px 0;}
        .inStk-row:not(:first-child), .inStk-dtl-row:not(:first-child){border-top:2px #c9c9c9 dotted;}
        .inStk-dtl-row:not(:first-child){padding-top:3px;}
        .inStk-dtl-row:not(:last-child) {padding-bottom:6px;}

        .inStk-row > div, .inStk-dtl-row > div {padding:0 3px;border-right:1px #fff solid;}
        .inStk-dtl-list > .inStk-dtl-row:nth-child(1) > div {padding-top:0;}
        .inStk-title > div {padding:5px 3px;background-color:#e9e9e9;font-weight:bold;}
        .inStk-dtl-row:not(:first-child) > div{margin-top:3px;}

        .inStk-title > div:nth-child(1), .inStk-row > div:nth-child(1){width:12%;}
        .inStk-title > div:nth-child(2), .inStk-row > div:nth-child(2){width:8%;}
        /*.inStk-title > div:nth-child(3), .inStk-row > div:nth-child(3){width:15%;}*/
        .inStk-title > div:nth-child(3){width:41%;}                     /*商品*/
        .inStk-title > div:nth-child(4){width:8%;text-align:right;}     /*含稅單價*/
        .inStk-title > div:nth-child(5){width:5%;text-align:center;}    /*稅別*/
        .inStk-title > div:nth-child(6){width:13%;text-align:right;}    /*數量*/
        .inStk-title > div:nth-child(7){width:8%;text-align:right;}     /*小計*/
        .inStk-title > div:nth-child(8){width:5%;text-align:center;}    /*銷貨狀態*/

        .inStk-dtl-list {width:80%;}
        .inStk-dtl-row > div:nth-child(1){width:51.3%;}                    /*商品*/
        .inStk-dtl-row > div:nth-child(2){width:10.1%;text-align:right;}    /*含稅單價*/
        .inStk-dtl-row > div:nth-child(3){width:6.3%;text-align:center;}      /*稅別*/
        .inStk-dtl-row > div:nth-child(4){width:15.9%;text-align:right;}       /*數量*/
        .inStk-dtl-row > div:nth-child(5){width:10.1%;text-align:right;}   /*小計*/
        .inStk-dtl-row > div:nth-child(6){width:5.8%;text-align:center;}   /*銷貨狀態*/


        .inStk-list-total {text-align:right;font-weight:bold;padding:5px;color:#080;border-top:1px #7c7c7c solid;}
        .inStk-list-total span {font-size:18px;color:#080;}
        .inStk-list-total #inStk-total {margin-right:5%;}
    
        .prg-content .node-main .ckbox { margin-left:0;margin-right:5px;width:16px;height:16px;}
        .prg-content .node-main .ckbox:checked { color:#000;background-color:#444;}
        

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main inStk-add-main" style="margin-top:10px;">
            <asp:Label ID="Label2" runat="server"></asp:Label>
            <div class="inStk-row inStk-title">
                <div style="display:flex;align-items:center;"><input type="checkbox" class="ckbox" style="margin-left:0;width:16px;" disabled id="all-ckbox">進貨單號</div>
                <div>進貨日期</div>
                <div>商品</div>
                <div>含稅單價</div>
                <div>稅別</div>
                <div>數量</div>
                <div>小計</div>
                <div>狀態</div>
            </div>
            <div id="inStk-list-main"></div>
            <div class="inStk-list-total" style="display:none;"><span>總計︰</span><span id="inStk-total">0.00</span></div>
        </div>
        <div style="border-top:2px #080 dotted;padding-top:15px;text-align:center;margin-top:30px;">
            <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i> 回上一頁</button>
            <button class="btn-submit btn-page btn-admin" id="btn-submit" data-alt="pd-add-main" disabled><i class="fas fa-arrow-alt-circle-right"></i> 確定送出</button>
        </div>
        <div id="inStk-append-master" style="display:none;">
            <div class="inStk-row node-main">
                <div class="row-main" style="display:flex;align-items:center;"><input type="checkbox" class="ckbox node-ckbox" name="node-checkbox"><span class="inStkId"></span></div>
                <div class="row-main inStkDate"></div>
                <div class="inStk-dtl-list"></div>
            </div>
        </div>
        <div id="inStk-append-detail" style="display:none;">
            <div class="inStk-dtl-row">
                <div style="display:flex; align-items:center;"><input type="checkbox" class="ckbox prg-ckbox" name="node-checkbox"><span class="product"></span></div>
                <div class="dtlAmt"></div>
                <div class="dtlIsTax"></div>
                <div class="qty edit-single-main" style="justify-content:flex-end;">
                    <div class="edit-tag"><label class="ognl-qty" style="color:#f00"></label></div>
                </div>
                <div class="dtlAmtTotal"></div>
                <div class="dtlPayType"></div>
            </div>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script>
        var pointQty = parseInt($("#pointQty").val());
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            }
        };

        //設定日期選擇器為中文語系
        $.datepicker.regional['zh-TW'] = {
            dayNames: ["星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"],
            dayNamesMin: ["日", "一", "二", "三", "四", "五", "六"],
            monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
            monthNamesShort: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"],
            prevText: "上月",
            nextText: "次月",
            weekHeader: "週"
        };

        $.datepicker.setDefaults($.datepicker.regional["zh-TW"]);

        //選擇廠商後，初始化商品資料主檔
        function intlStkMaster(_$obj, _$row) {
            _$obj.find(".row-main").attr("data-id", _$row["sn"]);
            _$obj.find(".inStkId").text(_$row["InStkId"]);
            _$obj.find(".inStkDate").text(jsonDateToString(_$row["InStkDate"]));
        }

        //選擇廠商後，初始化商品資料明細資料
        function intlStkDetail(_$obj, _$row) {
            _$obj.find(".prg-ckbox").attr("data-id", _$row["dtlSn"]);
            if (_$row["dtlPayType"] == "2") {   //全結
                _$obj.find(".prg-ckbox").prop({ "checked": true, "disabled": true });
            }

            _$obj.find(".product").text((_$row["pBarcode"] == "" ? _$row["pCode"] : _$row["pBarcode"]) + "-" + _$row["pNameS"]);
            var dtlAmt = roundToPoint(_$row["dtlAmt"], pointQty);
            _$obj.find(".dtlAmt").text(dtlAmt);
            _$obj.find(".dtlIsTax").text((_$row["dtlIsTax"] == "Y" ? "應稅" : "免稅"));

            _$obj.find(".qty").html("<span" + (_$row["dtlPayType"] == "2" ? " style='color:#f00'" : "") + ">" + _$row["qty"] + "</span>");

            _$obj.find(".dtlAmtTotal").attr("data-val", roundToPoint(_$row["amtTotal"], 0)).text(to3dot(_$row["amtTotal"]));
            _$obj.find(".dtlPayType").text(PayTypeName(_$row["dtlPayType"]));
        }

        //計算商品小計及總計
        function pdCalc() {
            var pdTotal, listPdTotal = 0;
            $("#inStk-list-main").find(".inStk-dtl-row").each(function () {
                pdTotal = parseInt($(this).find(".dtlAmtTotal").attr("data-val"));
                if ($(this).find(".prg-ckbox:not(:disabled)").prop("checked")) {
                    listPdTotal += parseInt(pdTotal);
                }
            });
            $("#inStk-total").text(to3dot(listPdTotal+""));
            $("#btn-submit").prop("disabled", $(".prg-ckbox:not(:disabled):checked").length == 0);    //無任何勾選則隱藏送出按鈕
        }

        //處理資料更新按鈕
        function chkEditBtn(_$obj, _id) {
            switch (_id) {
                case "abort":
                    var orgnl_value = _$obj.find(".edit-txt").attr("data-alt");
                    _$obj.find(".edit-txt").val(orgnl_value);
                    break;
                case "submit":
                    $prgCkbox = _$obj.closest(".inStk-dtl-row").find(".prg-ckbox");
                    if (!$prgCkbox.prop("checked")) { $prgCkbox.click(); }
                    var new_value = _$obj.find(".edit-txt").val();
                    if (new_value == "") { return error_focus("請填入數字！", _$obj.find(".edit-txt")); }
                    if (_$obj.find(".edit-qty").length > 0) {
                        maxQty = _$obj.find(".edit-txt").attr("data-payLeftQty");
                        if (parseInt(new_value) > parseInt(maxQty) || new_value == 0) { return error_focus("請填入 1 - " + maxQty + " 數字！", _$obj.find(".edit-txt")); }
                    }
                    _$obj.find(".edit-txt").attr("data-alt", new_value);
                    _$obj.find(".edit-tag span").text(new_value);
                    pdCalc();
                    break;
            };
            _$obj.find(".edit-zone").hide();
            _$obj.find(".edit-tag").show();
        }

        //取得進貨資料
        var inStkTbl;
        function getInStkData() {
            var pvSn = $("#act-pvSn").val();
            if (pvSn == "")
                return error_focus("請先選擇廠商!", $("#act-pvSn"));
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                dataType: "json",
                data: {
                    args0: "B001",
                    pvSn: pvSn
                    //args0: "B001",
                    //SqlComm: "SELECT * FROM WP_vInStock WHERE isDel='N' AND dtlIsDel='N' AND payType IN (0,1) ORDER BY InStkId, sn, dtlSn"
                },
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--getInStkData()--請洽工程人員');
                },
                success: function (response) {
                    //console.log(response);
                    inStkTbl = response;
                    makeInStkList();
                    ///----使用指令----
                    ///inStkTbl.length 資料筆數;
                    ///jsonDateToString(inStkTbl[0].InStkDate); //json取回日期格式("/Date(1637251200000)/")轉字串yyyy/MM/dd
                }
            });
        }

        function clearStkList() {
            $("#inStk-list-main").empty();
            $(".inStk-list-total").hide();
            $(".ckbox:not(:disabled)").prop("checked", false); //取消所有勾選
            $("#all-ckbox").attr("disabled", true);
            pdCalc();
        }

        //產生清單
        function makeInStkList() {
            var $objList = $("#inStk-list-main");
            clearStkList();

            var pvSn = $("#pvType").val() == "1" ? $("#pvSn").val() : $("#act-pvSn").val();
            if (pvSn != "") {
                var preId;
                var $pvInStk = $.grep(inStkTbl, function (o) { return o.pvSn == pvSn });
                if ($pvInStk.length == 0) {
                    $objList.append("<div class='empty-data'><i class='fas fa-exclamation-circle'></i> 查無該廠商未結進貨交易！</div>");
                    $(".inStk-list-total").hide();
                    $("#all-ckbox").prop("disabled", true);
                }
                else {
                    $("#all-ckbox").prop("disabled", false);
                    preId = "";
                    $.each($pvInStk, function (i, row) {
                        if (preId != row["sn"]) {
                            $objList.append($("#inStk-append-master").children().clone(true));
                            var $newRow = $objList.find(".inStk-row").last();
                            intlStkMaster($newRow, row);
                            $.each($.grep(inStkTbl, function (o) { return o.sn == row["sn"] }), function (i, dtlRow) {
                                $newRow.find(".inStk-dtl-list").append($("#inStk-append-detail").children().clone(true));
                                var $newDtlRow = $newRow.find(".inStk-dtl-row").last();
                                intlStkDetail($newDtlRow, dtlRow);
                            });
                            preId = row["sn"];
                        }
                    });

                    $(".inStk-list-total").show();
                }
                pdCalc();
            }
        }

        function PayTypeName(_id)
        {
            var name;
            switch (_id) {
                case "0": name = "未結"; break;
                case "1": name = "未全結"; break;
                case "2": name = "全結"; break;
                default: name = "未知"; break;
            }
            return name;
        }

        function ChkAllChkbox() {
            $("#all-ckbox").prop("checked", $(".prg-ckbox:visible:not(:checked)").length == 0);
        }

        var pdJson;      //所有產品(可進貨)
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
                            ? _json == "pdJson"
                                ? pdJson = jQuery.parseJSON("[]")
                                : pvJson = jQuery.parseJSON("[]")
                            : _json == "pdJson"
                                ? pdJson = jQuery.parseJSON(response)
                                : pvJson = jQuery.parseJSON(response);
                    }
                }
            });
        }

        function ChgProvider(_pvType) {   //選擇廠商時處理  _pvType 1:廠商下拉式 / 2:autocomplete
            var $obj = _pvType == "1" ? $("#pvSn") : $("#act-pvSn")
            $obj.change(function () {
                if ($("#inStk-list-main .prg-ckbox:checked").not(":disabled").length > 0) {
                    if (!confirm("變更廠商，目前所勾選商品將取消，是否繼續？")) {
                        $(this).val($.data(this, "current"));     //變更前的值
                        if (_pvType == "2")
                            $("#pv-filter").val($("#pv-filter").attr("data-val"));     //變更前的值
                        return false;
                    }
                }
                clearStkList();
                $.data(this, "current", $(this).val());     //變更後的值
                return true;
            });
        }

        function setPvSn(myValue) {
            $('#act-pvSn').val(myValue)
                .trigger('change');
        }

        function Initial() {
            //getInStkData();
            $("#processing").hide();
            if ($("#pvType").val() == "2") { getJson("pvJson", "") };   //廠商是autocomplete
        }



        $(function () {
            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "-1:+0"
            });

            Initial();

            $("#btn-back").click(function () { history.back(-1); });

            ChgProvider($("#pvType").val());    //選擇廠商異動

            //--------------勾選控制--[START]--------------
            $("#all-ckbox").click(function () {
                $(".ckbox").prop("checked", $(this).prop("checked"));
                pdCalc();
            });

            $(".node-ckbox").click(function () {
                $(this).closest(".node-main").find(".prg-ckbox").prop("checked", $(this).is(":checked"));
                pdCalc();
                ChkAllChkbox()
            });

            $(".prg-ckbox").click(function () {
                var $obj = $(this).closest(".node-main");
                $obj.find(".node-ckbox").prop("checked", $obj.find(".prg-ckbox:visible:not(:checked)").length == 0);

                pdCalc();
                ChkAllChkbox();
            });
            //--------------勾選控制--[END]--------------

            $("#btn-search").click(function () {
                getInStkData();
            });

            //確定送出銷帳單
            $("#btn-submit").click(function () {
                var pd_list = "";
                $("#inStk-list-main .inStk-dtl-row").find(".prg-ckbox:not(:disabled):checked").each(function () {
                    var $this = $(this);
                    pd_list += (pd_list == "" ? "" : ",") +
                        "{'dtlSn':'" + $this.attr("data-id") + "'," +
                        "'qty':'" + $this.closest(".inStk-dtl-row").find(".qty span").text() + "'}";
                });
                pd_list = "[" + pd_list + "]";
                //console.log(pd_list);

                $.ajax({
                    url: "/AjaxInStk.aspx",
                    type: "POST",
                    async: false,
                    data: {
                        args0: "C13",
                        AcctOutDate: $("#acctOutDate").val(),
                        pvSn: $("#pvSn").val(),
                        pdList: pd_list
                    },
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
                                alert("非管理者不能新增！");
                                break;
                            case "Z":
                                alert("出貨清單有誤！");
                                break;
                            case "Y":
                                alert("新增成功！");
                                //location.href = "/prg50/prg5002.aspx";
                                location.reload();
                                break;
                            default:
                                console.log(response);
                                alert("新增失敗請聯絡工程人員！");
                        }
                    }
                });
            });

            //---------- edit-button[START]----------
            //開啟變更商品清單的數量
            $(".edit-tag").click(function () {
                $("#inStk-list-main .inStk-row").find(".edit-zone:visible").each(function () {
                    chkEditBtn($(this).closest(".edit-single-main"), "abort");
                });
                $obj = $(this).closest(".edit-single-main");
                $(this).hide()
                $obj.find(".edit-zone").show();
                $obj.find(".edit-txt").focus(function () { $(this).select(); }).focus();

            });

            //處理商品清單的商品數量變更按鈕
            $(".edit-btn").click(function () {
                var id = $(this).attr("data-id");   //abort:放棄 / submit:確定變更
                $obj = $(this).closest(".edit-single-main");
                chkEditBtn($obj, id);
            });
            //---------- edit-button[END]----------

            $("#pv-filter")
                .on('input', function () {      //異動時清空pno欄位
                    setPvSn("");
                })
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
                        setPvSn(ui.item.actPvSn);
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    setPvSn("");
                    if ($("#act-pvSn").val() == ""){ $("#pv-filter").attr("data-val", ""); }
                });
        });
    </script>
</asp:Content>
