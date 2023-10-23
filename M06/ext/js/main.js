document.write('<script src="/ext/js/func.js"></script>');
jQuery(document).ready(function ($) {
    // browser window scroll (in pixels) after which the "back to top" link is shown
    var offset = 300,
        //browser window scroll (in pixels) after which the "back to top" link opacity is reduced
        offset_opacity = 1200,
        //duration of the top scrolling animation (in ms)
        scroll_top_duration = 700,
        //grab the "back to top" link
        $back_to_top = $('.cd-top');

    //hide or show the "back to top" link
    $(window).scroll(function () {
        ($(this).scrollTop() > offset) ? $back_to_top.addClass('cd-is-visible') : $back_to_top.removeClass('cd-is-visible cd-fade-out');
        if ($(this).scrollTop() > offset_opacity) {
            $back_to_top.addClass('cd-fade-out');
        }
    });

    //smooth scroll to top
    $back_to_top.on('click', function (event) {
        event.preventDefault();
        $('body,html').animate({
            scrollTop: 0,
        }, scroll_top_duration);
    });


});


//
jQuery(document).ready(function () {
    var accordionsMenu = $('.cd-accordion-menu');

    if (accordionsMenu.length > 0) {

        accordionsMenu.each(function () {
            var accordion = $(this);
            //detect change in the input[type="checkbox"] value
            accordion.on('change', 'input[type="checkbox"]', function () {
                var checkbox = $(this);
                console.log(checkbox.prop('checked'));
                (checkbox.prop('checked')) ? checkbox.siblings('ul').attr('style', 'display:none;').slideDown(300) : checkbox.siblings('ul').attr('style', 'display:block;').slideUp(300);
            });
        });
    }
});




//**********************  OscarDefault  ***********************

//------ 必填欄位檢核     _area:某Area Class下的data-alt屬性為"MI_"的欄位
function noneEmpty(_area) {
    var returnBool = true;
    var isEmpty = false;
    var msgKind, elemName, nodeName;

    $("." + _area + " [data-alt^='MI_']").each(function () {
        $this = $(this);
        nodeName = $this.prop('nodeName');
        switch (nodeName) {
            case "INPUT":
            case "TEXTAREA":
                switch ($this.attr("type")) {
                    case "radio":
                    case "checkbox":
                        elemName = $this.attr("name");
                        isEmpty = ($("." + _area + " [name='" + elemName + "']:checked").length == 0) ? (true) : (false)
                        msgKind = "B";
                        break;
                    default:
                        isEmpty = ($this.val() == "") ? (true) : (false)
                        msgKind = "A";
                        break;
                }
                break;
            case "SELECT":
                isEmpty = ($this.val() == "0") ? (true) : (false)
                msgKind = "B";
                break;
        }

        if (isEmpty) {
            var altMsg = $this.attr("data-alt").split("_")[1];
            error_focus(msgKind == "A" ? altMsg + "不可為空白！" : "請選擇" + altMsg + "！", this);
            returnBool = false;
            return false;
        };
    });
    return returnBool;
}

//------ 必填欄位檢核     _$obj:某_$obj下的data-alt屬性為"MI_"的欄位
function noneEmptyObj(_$obj) {
    var returnBool = true;
    var isEmpty = false;
    var msgKind, elemName, nodeName;

    _$obj.find("[data-alt^='MI_']").each(function () {
        $this = $(this);
        nodeName = $this.prop('nodeName');
        switch (nodeName) {
            case "INPUT":
            case "TEXTAREA":
                switch ($this.attr("type")) {
                    case "radio":
                    case "checkbox":
                        elemName = $this.attr("name");
                        //isEmpty = ($("." + _area + " [name='" + elemName + "']:checked").length == 0) ? (true) : (false)
                        isEmpty = (_$obj.find("[name='" + elemName + "']:checked").length == 0) ? (true) : (false)
                        msgKind = "B";
                        break;
                    default:
                        isEmpty = ($this.val() == "") ? (true) : (false)
                        msgKind = "A";
                        break;
                }
                break;
            case "SELECT":
                isEmpty = ($this.val() == "0") ? (true) : (false)
                msgKind = "B";
                break;
        }

        if (isEmpty) {
            var altMsg = $this.attr("data-alt").split("_")[1];
            error_focus(msgKind == "A" ? altMsg + "不可為空白！" : "請選擇" + altMsg + "！", this);
            returnBool = false;
            return false;
        };
    });
    return returnBool;
}

//------ 必填欄位檢核     _IdGrp:欲檢查的id，data-alt屬性需為"MI_"的欄位
function noneEmptyIdGrp(_IdGrp) {
    var returnBool = true;
    var isEmpty = false;
    var msgKind, elemName, nodeName;
    var idArr = _IdGrp.split(",");
    $.each(idArr, function (key, value) {
        $this = $("#" + value);
        nodeName = $this.prop('nodeName');
        switch (nodeName) {
            case "INPUT":
                switch ($this.attr("type")) {
                    case "radio":
                    case "checkbox":
                        elemName = $this.attr("name");
                        isEmpty = ($("." + _area + " [name='" + elemName + "']:checked").length == 0) ? (true) : (false)
                        msgKind = "B";
                        break;
                    default:
                        isEmpty = ($this.val() == "") ? (true) : (false)
                        msgKind = "A";
                        break;
                }
                break;
            case "SELECT":
                isEmpty = ($this.val() == "0") ? (true) : (false)
                msgKind = "B";
                break;
        }

        if (isEmpty) {
            var altMsg = $this.attr("data-alt").split("_")[1];
            error_focus(msgKind == "A" ? altMsg + "不可為空白！" : "請選擇" + altMsg + "！", this);
            returnBool = false;
            return false;
        };
    });
    return returnBool;
}


//按瀏覽器的返回上一頁後，不能按下一頁
function unGoBack() {
    if (window.history && window.history.pushState) {
        $(window).on('popstate', function () {
            window.history.pushState('forward', null, '');
            window.history.forward(1);
            //alert("不可回退");  
            location.replace(document.referrer);//刷新
        });
    }

    window.history.pushState('forward', null, ''); //在IE中必須得有這兩行  
    window.history.forward(1);
}

function ChkCartRedDot(_qty) {
    $(".CartRedDot").text(_qty);    //--- 購物車商品數量
    _qty == "0" ? $(".CartRedDot").hide() : $(".CartRedDot").show();
}

$(function () {
    if ($("#is-admin").val() != "Y") {     //帳號為非管理者，隱藏變更資料的按鈕
        $(".btn-admin").remove();
        //$(".edit-tag").css("cursor", "default").removeClass("edit-btn");
        $(".edit-tag").removeClass("edit-tag");
    }

    //============= 輸入欄位時自動檢查規格 [START]===========
    //--- password 檢查只能為英文(大小寫)及數字，長度6-12
    //--- text 檢查只能為中文字、字母组成以及空白
    //--- telephone_or_addr 僅能中英文及數字空白以及._-、,#，檢查是否為台灣手機格式(去除減號後，若輸入為09開頭的數字才檢查)
    $(".chk-input").blur(function () {
        var $obj = $(this);
        var value = $obj.val();
        if (value != "") {
            var id = $obj.attr("data-func");
            switch (id) {
                case "password":
                    return chk_input("az_number", $obj) ? (value.length >= 6 && value.length <= 12) ? true : error_focus("密碼請輸入 6-12 個英數字！", $obj) : false;
                    break;
                case "text":
                    return chk_input("chinese_az_number", $obj) ? antiSqlValid($obj) : false;
                    break;
                case "telephone_or_addr":
                    return isTelephoneAddr($obj);
                    break;
                default:
                    return chk_input(id, $obj);
                    break;
            }
        }
        return true;
    });

    //============= 輸入欄位時自動檢查規格 [END]===========

    //產品搜尋 [START]

    $(".btn-pd-search").click(function () {
        $obj = $(this).closest(".form-pd-search").find(".text-pd-search");
        if ($obj.val() == "") {
            return error_focus("請輸入商品關鍵字！", $obj);
        } else {
            if (antiSqlValid($obj) && isChinaOrLetter($obj)) {
                $.ajax({
                    url: "/AjaxMain.aspx",
                    type: "POST",
                    async: false,
                    data: {
                        args0: "A1",
                        args1: $obj.val()
                    },
                    error: function (xhr) {
                        console.log(xhr.responseText);
                        alert('Ajax request 發生錯誤--btn-pd-search.click--請洽工程人員！');
                        _result = false;
                    },
                    success: function (response) {
                        location.href = "/pdListSrch.aspx";
                    }
                });
            }
        };
    });

    $("[id^='KeyWordRest']").click(function () {
        var vID = $(this).attr("id").substr($(this).attr("id").indexOf("_") + 1);
        $("#KeyWordProd_" + vID).val("");
    });

    //產品搜尋 [END]

    $(".PListAlertMC").click(function () {
        alert("本商品有規格必選，請先選規格！");
        location.href = "/product.aspx?PNo=" + $(this).attr("data-alt");
    })

    //欄位清除按鈕
    $(".del-txt-group").find(".del-txt-button").click(function () {
        $(this).closest(".del-txt-group").find(".del-txt-input").val("").focus();
    });

    //*******密碼欄位顯示與隱藏[START]*********
    //****CSS***
    //.pw-group{ border:1px #ccc solid;display:flex;align-items:center; }
    //.pw-group .fas {margin:0 5px;cursor:pointer;}
    //
    //****HTML***
    //<div class="pw-group">
    //    <input type="password" class="form-control" style="border:0;" />
    //    <i class="fas fa-eye-slash pw-grp-btn" data-id="show"></i>
    //    <i class="fas fa-eye pw-grp-btn" data-id="hide" style="display:none;"></i>
    //</div >
    //***
    $(".pw-group").find(".pw-grp-btn").click(function () {
        var act = $(this).attr("data-id");
        var $obj = $(this).closest(".pw-group");
        $(this).hide();
        $obj.find(".pw-grp-btn[data-id='" + (act == "show" ? "hide" : "show") + "']").show();
        $obj.find("input").attr("type", act == "show" ? "text" : "password");
    });
    //*******密碼欄位顯示與隱藏[END]*********
});

