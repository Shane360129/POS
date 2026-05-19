/*
*  使用時 class 務必以使用範例結構命名
*  
* *** 使用範例 ***
* data-id︰值自取，如同一頁有兩個以上驗證碼data-id的值不可重複
* 
<div style="display:flex;align-items:center" class="validate-main" data-id="mem-input">
   <span>驗證圖碼</span><input type="text" class="form-control text-validate" style="width:60px;margin-right:0;" data-alt="MI_[驗證圖碼]" maxlength="4" autocomplete="off" />
   <img src="/" class="src-validate" style="margin-left:5px;width:70px" />
   <img src="/ext/validate/Reload.png" class="btn-validate" style="width:26px;height:auto;margin:auto 5px;cursor:pointer">
</div>
*
* *** validate.js 要 link 到使用頁面
* *** path_validate 路徑要配合實際路徑修改
*/

var path_validate = "/ext/validate/";    //程式絕對路徑
//------ 驗證碼判斷是否符合
function isPassVdCode(_id) {
    var $obj = $(".validate-main[data-id='" + _id + "']");
    var nowValidateNumber = jQuery.ajax({
        url: path_validate + "readSessionValidateNumber.ashx?id=" + _id,
        type: "post",
        async: false,
        data: {},
        success: function (htmlVal) { }
    }).responseText;

    var $objText = $obj.find(".text-validate");
    if (nowValidateNumber == $objText.val()) {
        return true;
    } else {
        reVdCode(_id);
        $objText.val("");
        return error_focus("驗證圖碼不符，請重新輸入！", $objText);
    }
}

function reVdCode(_id) {
    var $obj = $(".validate-main[data-id='" + _id + "']");
    $obj.find(".src-validate").attr("src", path_validate + "ValidateNumber.ashx?id=" + _id + "&" + Math.random());
    $obj.find(".text-validate").val("");
}


function initlVdCode() {
    $(".validate-main").each(function () {
        var $obj = $(this);
        var id = $obj.attr("data-id");
        $obj.find(".src-validate").attr("src", path_validate + "ValidateNumber.ashx?id=" + id);
    });
}

$(function () {
    initlVdCode();

    $(".btn-validate").click(function () {  //--- 驗證碼重整
        var id = $(this).closest(".validate-main").attr("data-id");
        reVdCode(id);
    })

    $(".text-validate").blur(function () {
        var value = $(this).val();
        if (value != "") {
            var regex = /^[0-9]{4}$/;
            return regex.exec(value) ? true : error_focus("請輸入4個數字！", $(this)); 
        }
    });
});