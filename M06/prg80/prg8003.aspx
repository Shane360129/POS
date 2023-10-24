<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg8003.aspx.cs" Inherits="prg8003" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        .list-main tr:last-child td {border-bottom:1px #c9c9c9 solid;}
        .list-main {border:1px #aaa solid;}
            .list-main td {border-top: 1px #c9c9c9 solid;border-right: 1px #ccc solid;text-align:left;}
            .list-main td.empty-data {text-align:center;}
            .list-main .list-title td{text-align:left;}
        .tr-row > td, .total-row > td {border-right: 1px #ccc solid;}
        .total-row > td {border-top:2px #666 solid; border-bottom:0!important;}
        .rptr-title {width:100%;text-align:center;font-size:18px;font-weight:bold;}

        .edit-zone {justify-content:flex-start;border: 1px #080 solid;border-radius: 3px;margin-bottom: 20px;padding: 20px 10px;}

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
            <div id="print-div" style="display:none;"></div>
            <div style="border-top: 2px #080 dotted; padding-top: 15px; text-align: center; margin-top: 30px;">
                <button class="btn-back btn-page" id="btn-back"><i class="fas fa-arrow-alt-circle-left"></i>回上一頁</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/Scripts/printThis.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("sDate", null);
                $.cookie("eDate", null);
            }
        };

        var pointQty = parseInt($("#pointQty").val());

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

            var cost;
            var costTotal = 0;
            var $this;
            $(".tr-row").each(function () {
                $this = $(this);
                cost = parseFloat($this.find(".cost").attr("data-val"));
                costTotal += cost;
            });
            console.log(costTotal);
            $("#costTotal").text(to3dot(roundToPoint(costTotal, pointQty)));
        }
        

        function chkFinish(_$obj) {
            if (noneEmptyObj(_$obj)) {
                return $("#act-pNo").val() == "" ? error_focus("商品沒有正確選擇！", $("#pd-filter")) : true;
            } else
                return false;
        }



        function Initial() {
            getJson("pdJson", "IO");
            $("#processing").hide();
            CalcTotal();
        }

        $(function () {
            Initial();

            $(".open-datepicker").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                minDate: new Date($("#StartYM").val().substr(0, 4), $("#StartYM").val().substr(4, 2) - 1, $("#StartYM").val().substr(6, 2)),
                maxDate: +1,
                setDate: "today"
            });

            $("#stkDate").datepicker({
                dateFormat: 'yy/mm/dd',
                changeYear: true,
                changeMonth: true,
                minDate: new Date($("#sYMD").val().substr(0, 4), $("#sYMD").val().substr(4, 2) - 1, $("#sYMD").val().substr(6, 2)),
                maxDate: +1,
                setDate: "today"
            });

            $("#btn-back").click(function () { history.back(-1); });

            $("#btn-search").click(function () {
                if ($("#sDate").val() == "" || $("#eDate").val() == "") {
                    return error_focus("日期區間不得為空白！", $("#sDate").val() == "" ? $("#sDate") : $("#eDate"));
                }

                $("#processing").show();
                $(this).attr("disabled", true);

                $.cookie("sDate", $("#sDate").val());
                $.cookie("eDate", $("#eDate").val());
                location.reload();
            });

            $("#btn-add").click(function () {
                $(this).hide();
                $("#add-main").show();
            });

            $(".btn-abort").click(function () {
                $("#btn-add").show();
                $("#add-main").hide().find("input").val("");
            });

            $("#btn-add-submit").click(function () {
                $obj = $(".edit-zone");
                if (chkFinish($obj)) {
                    var data = "args0=C15&" + $obj.find(":input").serialize();
                    console.log(data);
                    $.ajax({
                        url: "/AjaxOutStk.aspx",
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
                                case "Z":
                                    error_focus("該商品不存在！", $("#pd-filter"));
                                    break;
                                case "A":
                                    error_focus("該商品庫存已調整！", $("#pd-filter"));
                                    break;
                                case "Y":
                                    if ($.cookie("sDate") == "" || $.cookie("sDate") == null) {$.cookie("sDate", $("#stkDate").val());}
                                    if ($.cookie("eDate") == "" || $.cookie("eDate") == null) {$.cookie("eDate", $("#stkDate").val());}
                                    alert("新增成功！");
                                    location.reload();
                                    break;
                                default:
                                    alert("新增失敗請聯絡工程人員！");
                            }
                        }
                    });
                }
    
            });

            $("#pd-filter")
                .on('input', function () {      //異動時清空pno欄位
                    $("#act-pNo").val("");
                    $("#pd-add-submit").prop("disabled", true);
                })
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term;
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
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
                        $("#pd-add-submit").prop("disabled", false);
                        return false;
                    }
                })
                .closest(".del-txt-group").find(".del-txt-button").click(function () {
                    if ($(this).closest(".del-disabled").length == 0) {
                        $("#act-pNo").val("");
                        $("#pd-add-submit").prop("disabled", true);
                    }
                });
        });
    </script>

    <script type="text/javascript">
        function DownloadFile() {
            $.ajax({
                type: "POST",
                url: "prg8003.aspx/SaveExcel",
                data: "",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    //Convert Base64 string to Byte Array.
                    var bytes = Base64ToBytes(r.d);

                    //Convert Byte Array to BLOB.
                    var blob = new Blob([bytes], { type: "application/octetstream" });

                    //Check the Browser type and download the File.
                    var isIE = false || !!document.documentMode;
                    if (isIE) {
                        window.navigator.msSaveBlob(blob, "庫存調整報表.xlsx");
                    } else {
                        var url = window.URL || window.webkitURL;
                        link = url.createObjectURL(blob);
                        var a = $("<a />");
                        a.attr("download", "庫存調整報表.xlsx");
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
