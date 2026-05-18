var memJson;      //所有產品(含已下架)
function getMem() {
    var str = $("#mem-filter").val();
    str = str.substr(str.indexOf("．") < 0 ? 0 : str.indexOf("．") + 1);
    console.log(str);
    $.ajax({
        url: "/AjaxMain.aspx",
        type: "POST",
        async: false,
        data: {
            args0: "B03_2",
            memFilter: str
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
                    memJson = jQuery.parseJSON("[]");
                    break;
                default:
                    memJson = jQuery.parseJSON(response);
                    //console.log(pdJson);
                    break;
            }
        }
    });
}

$(function () {
    $("#mem-filter")
        .on('input', function () {      //異動時清空pno欄位
            $("#memSn").val("");
        })
        .autocomplete({
            source: function (request, response) {
                // request物件只有一個term屬性，對應使用者輸入的文字
                // response在你自行處理並獲取資料後，將JSON資料交給該函式處理，以便於autocomplete根據資料顯示列表
                getMem();
                response($.map(memJson, function (item) { // 此處是將返回資料轉換為 JSON物件
                    return {
                        label: item.memId + "．" + item.memName, // 下拉項顯示內容
                        value: item.memId + "．" + item.memName,  // 下拉項對應數值
                        actMemSn: item.memSn
                        //另外可以自定義其它引數
                    }
                }));
            },
            select: function (event, ui) { //event引數是事件物件，ui物件只有一個item屬性，對應資料來源中被選中的物件
                $("#mem-filter").val(ui.item.value);
                $("#memSn").val(ui.item.actMemSn);
                return false;
            }
        })
        .closest(".del-txt-group").find(".del-txt-button").click(function () {
            if ($(this).closest(".del-disabled").length == 0) {
                $("#memSn").val("");
            }
        });
});