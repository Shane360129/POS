<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="prg5004.aspx.cs" Inherits="prg5004" %>

<%@ Register Src="~/inc/inc_Prg.ascx" TagPrefix="uc1" TagName="inc_Prg" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
    .foot-total td {text-align:right;border-top:1px #000 solid;border-bottom:0;font-weight:bold;}
    .list-main tr:last-child td {border-bottom:0;}
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<article>
    <div class="page-body prg-menu"><uc1:inc_Prg runat="server" ID="inc_Prg" /></div>
    <div class="page-body prg-content">
        <asp:Label ID="Label1" runat="server"></asp:Label>
        <div class="page-main pd-list-main">            
            <asp:Label ID="Label2" runat="server"></asp:Label>
        </div>
    </div>
</article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/Scripts/jquery-ui.js"></script>
    <script src="/Scripts/printThis.js"></script>
    <script>
        window.onpageshow = function (event) {
            if (event.persisted || (window.performance && window.performance.navigation.type == 2)) {       //type 0:初次到訪/ 1:reload 2:按了back or forward
                window.location.reload()
            } else if (window.performance && window.performance.navigation.type == 0) {
                $.cookie("sDate", null);
                $.cookie("eDate", null);
                $.cookie("memSn", null);
                $.cookie("acctInId", null);
                $.cookie("OutStkId", null);
                $.cookie("pName", null);
                $.cookie("pNo", null);
                $.cookie("isTax", null);
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

        var pdJson;      //所有產品(含已下架)
        //var pvPdJson;    //已選廠商之所有產品
        function getPd() {
            $.ajax({
                url: "/AjaxMain.aspx",
                type: "POST",
                async: false,
                data: {
                    args0: "B03"
                },
                error: function (xhr) {
                    console.log(xhr.responseText);
                    alert('Ajax request 發生錯誤--getPd()--請洽工程人員');
                },
                success: function (response) {
                    switch (response) {
                        case "not-emp":
                            location.replace("/login.aspx");
                            break;
                        case "N":
                            pdJson = jQuery.parseJSON("[]");
                            //pvPdJson = jQuery.parseJSON("[]");
                            break;
                        default:
                            pdJson = jQuery.parseJSON(response);
                            //console.log(pdJson);
                            break;
                    }
                }
            });
        }

        function ResetInPdAct() {
            var actPNo = $("#act-pNo").val();
            if (actPNo != "") {
                var chkPvPd = pdJson.filter((x) => { return x.pNo == actPNo; })
                if (chkPvPd.length == 0) {
                    $("#pd-filter").val("");
                    $("#act-pNo").val("");
                }
            }
        }

        //計算商品小計及總計
        function pdCalc() {
            var totalDiscnt = parseInt($("#total-discnt").attr("data-val"));
            totalDiscnt ==  0 ? $("#total-discnt").hide() : $("#total-discnt").show();
            var totalDtlDiscnt = parseInt($("#total-dtlDiscnt").attr("data-val"));
            totalDtlDiscnt == 0 ? $("#total-dtlDiscnt").hide() : $("#total-dtlDiscnt").show();
            (totalDiscnt == 0 && totalDtlDiscnt == 0) ? $("#total-discnt-row").hide() : $("#total-discnt-row").show();
        }

        function Initial() {
            $(".row-main[data-id='']").text("").css("border-top", "0");
            getPd();
            ResetInPdAct();
            pdCalc();
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
                $.cookie("sDate", $("#sDate").val());
                $.cookie("eDate", $("#eDate").val());
                $.cookie("memSn", $("#memSn").val());
                $.cookie("acctInId", $("#acctInId").val());
                $.cookie("OutStkId", $("#OutStkId").val());
                $.cookie("pName", $("#pd-filter").val());
                $.cookie("pNo", $("#act-pNo").val());
                location.reload();
            });

            $("#pd-filter")
                .on('input', function () { $("#act-pNo").val(""); })     //異動時清空pno欄位
                .autocomplete({
                    source: function (request, response) {
                        // request物件只有一個term屬性，對應使用者輸入的文字
                        // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                        var str = request.term
                        str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
                        console.log(str);
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

            $("#btn-prn").click(function () {
                $("#rptr-main").printThis(
                    {
                        printContainer: true,
                        importCSS: true,
                        loadCSS: "../ext/css/5004print.css",
                        header: "<div class='rptr-title'>應收帳款銷帳表</div>"
                    });
            });
        });
    </script>
</asp:Content>
