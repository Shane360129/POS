//------去除字符串首尾空格　用法︰someString.trim()
String.prototype.trim = function()
{
	return this.replace(/(^\s*)|(\s*$)/g, "");
}

//日期元件轉字串yyyy/MM/dd
//使用方法$.date(yourDateObject);
$.date = function (dateObject) {
    var d = new Date(dateObject);
    var day = d.getDate();
    var month = d.getMonth() + 1;
    var year = d.getFullYear();
    if (day < 10) {
        day = "0" + day;
    }
    if (month < 10) {
        month = "0" + month;
    }
    var date = year + "/" + month + "/" + day;

    return date;
};

//json日期格式(Microsoft JSON date︰ /Date(1224043200000)/) 轉日期格式字串yyyy/MM/dd
function jsonDateToString(_dateObj) {
    return $.date(new Date(parseInt(_dateObj.substr(6))))
}

function chk_string(_kind, _string) {
    switch (_kind) {
        case "az_number":   //--- 是否為英文(大小寫)及數字
            var regex = /^[a-zA-Z0-9]+$/;
            break;
        case "cellphone":   //--- 台灣手機格式
            var regex = /^[09]{2}[0-9]{8}$/;
            break;
        case "chinese_az_number":   //--- 是否為中、英文(大小寫)及數字及空白
            var regex = /^[\u4e00-\u9fa5 a-zA-Z0-9]+$/;
            break;
        case "email":       //--- email
            var regex = /^([a-zA-Z0-9_.+-])+\@(([a-zA-Z0-9-])+\.)+([a-zA-Z0-9]{2,4})+$/;
            break;
        case "number":      //--- 是否為數字(0-9)
            var regex = /^[0-9]*$/;
            break;
        case "real_number":      //--- 是否為數字(0-9+-跟小數點pointQty位以下)
            var regex = new RegExp("^[0-9]+(.[0-9]{1," + pointQty + "})?$");
            //var regex = /^[0-9]+(.[0-9]{1,2})?$/;
            break;
        case "special":     //--- 是否為特殊字元
            var regex = /^[~'!@$%^&*()+_=:]/;
            break;
        default:
            var regex = /^~/;
    }
    return regex.exec(_string);
}

function chk_input(_kind, _obj) {
    if (_obj.val() == "") { return true; }
    else {
        switch (_kind) {
            case "az_number":    //判斷英文(大小寫)及數字
                var string = "僅允許輸入英數字！";
                break;
            case "cellphone":    //判斷手機號碼
                var string = "手機格式有誤！";
                break;
            case "chinese_az_number":   //--- 是否為中、英文(大小寫)及數字及空白
                var string = "請勿輸入中英文以外的文字！";
                break;
            case "email":    //判斷email
                var string = "Email格式有誤！";
                break;
            case "number":    //判斷數字
                var string = "僅限輸入數字！";
                break;
            case "real_number":    //判斷數字
                var string = "僅限輸入小數點最多" + pointQty + "位的數字！";
                break;
            case "special":     //--- 是否為特殊字元
                var string = "請勿輸入特殊字元！";
                break;
            default:
                var string = "error-data-func!";
        }
        return chk_string(_kind, _obj.val()) ? true : error_focus(string, _obj);
    }
}

//------判斷日期格式 　dateKind:1/格式︰yyyy-mm-dd   dateKind:2/格式︰yyyy-mm-dd hh:mi:ss
function isDate(_dateKind, _dateString){
    if (_dateString.trim()=="")return true;
	switch(_dateKind){
		case 2:
			//年月日時分秒正規表達式
            var r = _dateString.match(/^(\d{1,4})\-(\d{1,2})\-(\d{1,2}) (\d{1,2}):(\d{1,2}):(\d{1,2})$/); 
			if(r==null){
				alert("请输入格式正確的日期\n\n\r日期格式：yyyy-mm-dd hh:mi:ss\n\r例　　如：2008-08-08 00:00:00\n\r");
				return false;
			}
			var d=new Date(r[1],r[2]-1,r[3],r[4],r[5],r[6]);     
			var num = (d.getFullYear()==r[1]&&(d.getMonth()+1)==r[2]&&d.getDate()==r[3]&&d.getHours()==r[4]&&d.getMinutes()==r[5]&&d.getSeconds()==r[6]);
			if(num==0){
				alert("请输入格式正確的日期\n\n\r日期格式：yyyy-mm-dd hh:mi:ss\n\r例　　如：2008-08-08 00:00:00\n\r");
			}
			return (num!=0);
		default:	//------dateKind預設為1
            var r = _dateString.match(/^(\d{1,4})(-|\/)(\d{1,2})\2(\d{1,2})$/); 
			if(r==null){
				alert("请输入格式正確的日期\n\r日期格式：yyyy-mm-dd\n\r例    如：2008-08-08\n\r");
				return false;
			}
			var d=new Date(r[1],r[3]-1,r[4]);   
			var num = (d.getFullYear()==r[1]&&(d.getMonth()+1)==r[3]&&d.getDate()==r[4]);
			if(num==0){
				alert("请输入格式正確的日期\n\r日期格式：yyyy-mm-dd\n\r例    如：2008-08-08\n\r");
			}
			return (num!=0);
	}
}

//------判斷日期區間內 result:Y:日期內 B:還沒到 A:過了
function isOnDate(_Date, _SDate, _EDate) {
    var result;
    ((Date.parse(_EDate)).valueOf() >= (Date.parse(_Date)).valueOf() && (Date.parse(_SDate)).valueOf() <= (Date.parse(_Date)).valueOf())
        ? result = "Y"
        : ((Date.parse(_SDate)).valueOf() > (Date.parse(_Date)).valueOf())
            ? result = 'B'
            : result = 'A';
    return result;
}

//------全形字元轉半形
function fullToHalf(_val) {
	var value = _val || "";
	var result = "";
	if (value) {
		for (i = 0; i <= value.length; i++) {
			if (value.charCodeAt(i) == 12288) {
				result += " ";
			} else {
				if (value.charCodeAt(i) > 65280 && value.charCodeAt(i) < 65375) {
					result += String.fromCharCode(value.charCodeAt(i) - 65248);
				} else {
					result += String.fromCharCode(value.charCodeAt(i));
				}
			}
		}
	} 
	return result;
}

//------輸入欄位內容防止SQL injection
function antiSqlValid(_obj) {
    re = /select|update|delete|exec|count|'|"|=|;|>|<|%/i;
    if (re.test(_obj.val())) {
        return error_focus("請勿輸入特殊字元和攻擊關鍵字！", _obj); //注意中文亂碼
    } else {
        return true;
    }
}

function isChinaOrLetter(_obj) {
    return chk_string("chinese_az_number", _obj.val())
}

//載具編號判斷
function isVehicleNo(_kind, _obj) {
    switch (_kind) {
        case "1":   //手機載具
            var regs = "^\/[0-9A-Z\+\.\-]{7}$";
            var msg = "手機載具";
            break;
        case "0":   //自然人憑證
            var regs = "^[A-Z]{2}[0-9]{14}$";
            var msg = "自然人載具";
            break;
        case "2":   //捐贈碼
            var regs = "^[0-9]{3,7}$";
            var msg = "捐贈碼";
            break;
    }
    var re = new RegExp(regs);
    if (re.test(_obj.val())) {
        return true;
    } else {
        return error_focus(msg + "輸入錯誤！", _obj);
    }
}

//公司統編判斷
function isCompanyId(_obj) {
    var regs = "^[0-9]{8}";
    var reBool = true;
    var re = new RegExp(regs);
    if ( re.test(_obj.val()) ) {
        str = _obj.val()
        idArr = str.split("");
        calcArr = [1, 2, 1, 2, 1, 2, 4, 1];
        result = 0;
        for (i in idArr) {
            multiply = parseInt(idArr[i]) * calcArr[i]
            tmp = (multiply > 9) ? (parseInt(multiply / 10) + (multiply % 10)) : (multiply);
            result += (multiply > 9) ? (parseInt(multiply / 10) + (multiply % 10)) : (multiply);
        };
        reBool = (result % 10 == 0 || ((parseInt(idArr[6]) == 7) && ((result + 1) % 10 == 0)));
    } else {
        reBool = false;
    }

    if (reBool) {
        return true;
    } else {
        return error_focus("統一編號輸入錯誤！", _obj);
    }

}

//------檢核電話(市及手機)或地址輸入
function isTelephoneAddr(_obj) {
    var value = _obj.val();
    if (value != "") {
        var regex = /^[\u4e00-\u9fa5 a-zA-Z0-9._+-?、,#]+$/;       //--- 僅能中英文及數字空白以及._+-?、,#
        if (!regex.exec(value)) { return error_focus("請勿輸入特殊符號！", _obj); }
        else {
            var str = value.replace("-", "");
            if (chk_string("number", str) && str.substring(0, 2) == "09") {
                return chk_string("cellphone", str) ? true : error_focus("手機格式有誤！", _obj);
            }
        }
    }
    return true;
}


//參數1 str：需要補0的字串
//參數2 len：要補0的長度
// 左邊補0
function padLeft(str, lenght) {
    return ((str+"").length >= lenght)
        ? str
        : padLeft("0" + str, lenght);
}


//參數1 str：需要補0的字串
//參數2 len：要補0的長度
//右邊補0
function padRight(str, lenght) {
    return ((str+"").length >= lenght)
        ? str
        : padRight(str + "0", lenght);
}

//將數字3位一點
function to3dot(_str) {
    var str = _str.toString();
    var dotStr = "", dotBE = "";
    var anchor = 0;
    if (str == "") { dotStr = ""; }
    else {
        str = str.substr(0, 1) == "-" ? str.substr(1) : str;
        var strAry = str.split(".");

        //處理小數點前
        if (strAry[0].length <= 3) { dotBE = strAry[0]; }
        else {
            anchor = strAry[0].length % 3;
            dotBE = strAry[0].substring(0, anchor);
            for (i = anchor; i < strAry[0].length; i = i + 3) {
                dotBE += (dotBE == "" ? "" : ",") + strAry[0].substr(i, 3);
            }
        }

        // 處理小數點後
        var dotAF = (strAry.length > 1) ? ("." + strAry[1]) : "";
        dotStr = dotBE + dotAF;
    }
    return (_str.toString().substr(0, 1) == "-" ? "-" : "") + dotStr;
}


//將數字四捨五入到小數點第幾位
function roundToPoint(_num, _qty) {
    var val = +(Math.round(_num + "e+" + _qty) + "e-" + _qty);
    return toFixeds(val, _qty);
}

function toFixeds(val, pre) {
    const num = parseFloat(val);
    // eslint-disable-next-line no-restricted-globals
    if (isNaN(num)) {
        return false;
    }
    const p = 10 ** pre;
    const value = num * p;
    let f = (Math.round(value) / p).toString();
    let rs = f.indexOf('.');
    if (rs < 0) {
        rs = f.length;
        f += '.';
    }
    while (f.length <= rs + pre) {
        f += '0';
    }
    return f;
}


//精確小數點
//number：爲你要轉換的數字
//format：要保留幾位小數；譬如要保留2位，則值爲2
//zerFill:是否補零。不需要補零可以不填寫此參數
function accurateDecimal(number, format, zeroFill) {
    //判斷非空
    if (!isEmpty(number)) {
        //正則匹配:正整數，負整數，正浮點數，負浮點數
        if (!/^\d+(\.\d+)?$|^-\d+(\.\d+)?$/.test(number))
            return number;
        var n = 1;
        for (var i = 0; i < format; i++) {
            n = n * 10;
        }

        //四捨五入
        number = Math.round(number * n) / n;
        var str = number.toString();

        //是否補零
        if (zeroFill) {
            var index;
            if (str.indexOf(".") == -1) {
                index = format;
                str += '.';
            } else {
                index = format - ((str.length - 1) - str.indexOf("."));
            }

            for (var i = 0; i < index; i++) {

                str += '0';
            }
        }
        return str;
    }
    return number;
};

//非空驗證
function isEmpty(ObjVal) {
    if ((ObjVal == null || typeof (ObjVal) == "undefined") || (typeof (ObjVal) == "string" && ObjVal == "" && ObjVal != "undefined")) {
        return true;
    } else {
        return false;
    }
}

//輸入錯誤,alert訊息後回到原輸入欄位
function error_focus(_text, _obj) {
    alert(_text);
    setTimeout(function () { _obj.focus(); }, 200);
    return false;
}

//按視窗其他地方則關閉該視窗
function clickElseHide(_id, _btnId) {   //_id:視窗id  _btnId:開啟視窗id
    $('body').click(function (evt) {
        if (_btnId != "") {
            if ($(evt.target).parents("#" + _id).length == 0 &&
                evt.target.id != _id && evt.target.id != _btnId) {
                $('#' + _id).hide();
            }
        } else {
            if ($(evt.target).parents("#" + _id).length == 0 &&
                evt.target.id != _id) {
                $('#' + _id).hide();
            }
        }
    });
}

//開啟新連結方法實現

//windowOpen('http://www.45it.net/', '_blank');//新視窗開啟
//windowOpen('http://www.45it.net/', '_self');
function windowOpen() {
    var a = document.createElement("a");
    a.setAttribute("href", url);
    if (target == null) {
        target = '';
    }
    a.setAttribute("target", target);
    document.body.appendChild(a);
    if (a.click) {
        a.click();
    } else {
        try {
            var evt = document.createEvent('Event');
            a.initEvent('click', true, true);
            a.dispatchEvent(evt);
        } catch (e) {
            window.open(url);
        }
    }
    document.body.removeChild(a);
    alert("TEST");

}


//****jquery使用cookies 範例***
//
//$.cookie('cookieName','1',{expires: 7});      給值 1 有效期限7天
//$.cookie('cookieName','1');                   給值 1 視窗關閉即清除
//$.cookie('cookieName',null);                  清除cookies
//$.cookie('cookieName')                        取cookies值
//
//*****************************
jQuery.cookie = function (name, value, options) {
    if (typeof value != 'undefined') {
        options = options || {};
        if (value === null) {
            value = '';
            options.expires = -1;
        }
        var expires = '';
        if (options.expires && (typeof options.expires == 'number' || options.expires.toUTCString)) {
            var date;
            if (typeof options.expires == 'number') {
                date = new Date();
                date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000));
            } else {
                date = options.expires;
            }
            expires = '; expires=' + date.toUTCString();
        }
        var path = options.path ? '; path=' + (options.path) : '';
        var domain = options.domain ? '; domain=' + (options.domain) : '';
        var secure = options.secure ? '; secure' : '';
        document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
    } else {
        var cookieValue = null;
        if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]);
                if (cookie.substring(0, name.length + 1) == (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
}

///**
//* 判斷是否是webview環境
//*/
//function isWebview() {
//    alert("TEST2");
//    var ua = navigator.userAgent;
//    var platform = navigator.platform;
//    var chrome = ua.match(/Chrome/ / ([/d.]+)/) || ua.match(/CriOS/ / ([/d.]+)/);
//    var webview = !chrome &;&; ua.match(/(iPhone|iPod|iPad).*AppleWebKit(?!.*Safari)/); return webview;
//} 
