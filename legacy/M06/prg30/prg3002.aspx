<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg3002.aspx.cs" Inherits="prg3002" %>

<%-- EnableEventValidation="false" --%>
<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <link href="/Content/jquery-ui.css" rel="stylesheet" />
    <link href="/Content/jquery-ui.theme.css" rel="stylesheet" />
    <style>
        .search-main {
            border: 1px #080 solid;
            border-radius: 3px;
            margin-bottom: 20px;
            padding: 20px 10px;
            padding-bottom: 0;
        }

            .search-main > div {
                display: flex;
                align-items: center;
                flex-wrap: wrap;
                margin-bottom: 10px;
            }

            .search-main .search-sub {
                display: flex;
                align-items: center;
                margin-bottom: 10px;
                margin-right: 40px;
            }

        .list-main {
            width: 100%;
        }

            .list-main tr:first-child {
                border-top: 2px #c9c9c9 solid;
            }

            .list-main tr:last-child {
                border-bottom: 2px #c9c9c9 solid;
            }

            .list-main td {
                padding: 5px;
                border-right: 1px #fff solid;
            }

            .list-main tr:not(:nth-child(1),:nth-child(2)) td {
                border-top: 2px #c9c9c9 dotted;
            }

            .list-main .list-title td {
                background-color: #e9e9e9;
                font-weight: bold;
            }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <article>
        <div class="page-body prg-menu">
            <uc1:inc_Prg runat="server" ID="inc_Prg" />
        </div>
        <div class="page-body prg-content">
            <asp:Label ID="Label1" runat="server"></asp:Label>
            <div class="page-main pd-list-main">
                <asp:Label ID="Label2" runat="server"></asp:Label>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <%--    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>--%>
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/Scripts/printThis.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("sDate", null);
                $.cookie("eDate", null);
                $.cookie("pvSn", null);
                $.cookie("pvFilter", null);
                $.cookie("reciptNo", null);
                $.cookie("pName", null);
                $.cookie("pNo", null);
                $.cookie("isTax", null);
                $.cookie("payType", null);
                $.cookie("InStkId", null);
                window.location.reload();
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

        function CalcTotal() {
            $obj = $(".list-main");
            var totalQty = 0, totalAmt = 0;
            $obj.find(".dtl-qty").each(function () {
                totalQty += parseInt($(this).text());
            });
            $("#total-qty").text(totalQty);
            $obj.find(".dtl-amt").each(function () {
                totalAmt += parseInt($(this).text());
            });
            $("#total-amt").text(totalAmt.toFixed(0));
        }

        function Initial() {
            getJson("pdJson", "IO");
            if ($("#pvType").val() == "2") { getJson("pvJson", "") };   //廠商是autocomplete
            $("#processing").hide();
            $(".row-main[data-id='']").text("").css("border-top", "0");
            CalcTotal();
        }



        $(function () {
            Initial();

            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                yearRange: "2021:+0",
                setDate: "today"
            });

            $("#btn-search").click(function () {
                if ($("#sDate").val() == "" || $("#eDate").val() == "") {
                    return error_focus("日期區間不得為空白！", $("#sDate").val() == "" ? $("#sDate") : $("#eDate"));
                }

                $("#processing").show();
                $(this).attr("disabled", true);

                $.cookie("sDate", $("#sDate").val());
                $.cookie("eDate", $("#eDate").val());
                $.cookie("pvSn", $("#pvType").val() == "1" ? $("#pvSn").val() : $('#act-pvSn').val());
                if ($("#pvType").val() == "2") { $.cookie("pvFilter", $("#pv-filter").val()); }
                $.cookie("reciptNo", $("#reciptNo").val());
                $.cookie("pName", $("#pd-filter").val());
                $.cookie("pNo", $("#act-pNo").val());
                $.cookie("isTax", $("#isTax").val());
                $.cookie("outType", $("#outType").val());
                $.cookie("InStkId", $("#InStkId").val());
                location.reload();
            });

            $("#pd-filter")
                .on('input', function () { $("#act-pNo").val(""); })     //異動時清空pno欄位
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term;
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 2);
                        var tbl = pdJson.filter((x) => { return (x.pNameS.indexOf(str) >= 0 || x.pName.indexOf(str) >= 0 || x.pBarcode.indexOf(str) >= 0); })
                        response($.map(tbl, function (item) { // 此處是將返回資料轉換為 JSON物件
                            return {
                                label: item.pBarcode + "．" + item.pNameS, // 下拉項顯示內容
                                value: item.pBarcode + "．" + item.pNameS,  // 下拉項對應數值
                                actPno: item.pNo
                                //另外可以自定義其它引數
                            }
                        }));
                    },
                    select: function (event, ui) { //event引數是事件物件，ui物件只有一個item屬性，對應資料來源中被選中的物件
                        $("#pd-filter").val(ui.item.value);
                        $("#act-pNo").val(ui.item.actPno);
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () { $("#act-pNo").val(""); });

            $("#pv-filter")
                .on('input', function () { $('#act-pvSn').val(""); })     //異動時清空pno欄位
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term;
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 2);
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
    <script type="text/javascript">
        <%--$(function () {
            $("#dialog").dialog({
                autoOpen: false,
            });
            $("#btn_CalPerBonusCount").click(function () {
                $.ajax({
                    type: "POST",
                    url: "prg3002.aspx/SaveExcel",
                    data: "",
                    //contentType dataType 這2個型態後面要用單引號，不可用雙引號不然會發生錯誤。
                    contentType: 'application/json; charset=utf-8',// 要送到server的資料型態
                    dataType: 'json',// 預期從server接收的資料型態
                    success: function (data) {
                        data.d;

                        //alert("下載完成!\n已將檔案存至資料夾【下載】中。\n檔案名稱為【4002銷貨交易查詢】加上當下時間。")
                        //$("#dialog").dialog("open");
                    },
                    error: function (err) {
                        alert("下載失敗。\n請重新整理網頁後再嘗試一次。\n若依舊無法執行下載作業，請聯絡工程師，工程師會為您服務。")
                    }
                });
            });
        });--%>
        function DownloadFile() {
            $.ajax({
                type: "POST",
                url: "prg3002.aspx/SaveExcel",
                //data: '{fileName: "' + fileName + '"}',
                //data: "3002 進貨交易查詢.xlsx",
                data: "",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    //Convert Base64 string to Byte Array.
                    var bytes = Base64ToBytes(r.d);

                    //Convert Byte Array to BLOB.
                    var blob = new Blob([bytes], { type: "application/octetstream" });//application/vnd.ms-excel   octetstream

                    //Check the Browser type and download the File.
                    var isIE = false || !!document.documentMode;
                    if (isIE) {
                        window.navigator.msSaveBlob(blob, "3002 進貨交易查詢.xlsx");
                    } else {
                        var url = window.URL || window.webkitURL;
                        link = url.createObjectURL(blob);
                        var a = $("<a />");
                        a.attr("download","3002 進貨交易查詢.xlsx");
                        a.attr("href", link);
                        $("body").append(a);
                        a[0].click();
                        $("body").remove(a);
                    }
                }
            });
        };
        function Base64ToBytes(base64) {
            var s = window.atob(base64);
            var bytes = new Uint8Array(s.length);
            for (var i = 0; i < s.length; i++) {
                bytes[i] = s.charCodeAt(i);
            }
            return bytes;
        };
    </script>
</asp:Content>

