<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="login.aspx.cs" Inherits="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <style>
        .empLogin-input { width: 100%;max-width: 370px;margin: 0 auto;margin-top: 3%; }
            .empLogin-input > div { margin-bottom: 3%; }
            .empLogin-input input[type='text'], 
            .empLogin-input input[type='password'] { font-size: 18px;padding: 0.5% 1%;display: inline;margin-left: 10px;border: 1px #dedede solid;border-radius: 3px; }
            .empLogin-input > div > input { width: 70%; }
            .empLogin-input img { vertical-align: middle; }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <article>
        <div style="margin-top:100px;text-align:center;color:#5C9160;font-size:25px;font-weight:bold">使用者登入</div>
        <div style="border:2px #5C9160 solid;border-radius:5px;margin:1% auto;padding:2%;max-width:600px;overflow:hidden;">
            <form id="form1" class="empLogin-input">
                <div><span>員工代號</span><input type="text" class="form-control chk-input" data-func="az_number" name="EmpId" data-alt="MI_[員工代號]" maxlength="12" /></div>
                <div style="display:flex;align-items:center;"><span>密　　碼</span>
                    <div class="pw-group" style="width:70%;margin-left:10px;">
                        <input type="password" class="form-control chk-input" data-func="az_number" style="border:0;margin-left:0;" name="EmpPW" data-alt="MI_[密碼]" maxlength="12" />
                        <i class="fas fa-eye-slash pw-grp-btn" data-id="show"></i>
                        <i class="fas fa-eye pw-grp-btn" data-id="hide" style="display:none;"></i>
                    </div>
                </div>
                <div style="display:flex;align-items:center" class="validate-main" data-id="empLogin-input">
                   <span>驗證圖碼</span><input type="text" class="form-control text-validate" style="width:60px;margin-right:0;" data-alt="MI_[驗證圖碼]" maxlength="4" />
                   <img src="/" class="src-validate" style="margin-left:5px;width:70px" />
                   <img src="/ext/validate/Reload.png" class="btn-validate" style="width:26px;height:auto;margin:auto 5px;cursor:pointer">
                </div>
            </form>
            <div style="margin-top:3%;border-top:2px #D5D5D5 solid;padding-top:3%">
                <button class="box_shadow btn-hover" style="background-color:#80C26A;border:0;" id="Btn_Submit">確認送出</button>
            </div>
        </div>
    </article>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" Runat="Server">
    <script src="/ext/validate/validate.js"></script>
    <script type="text/javascript">
        function ChkFinish(_id) {
            return noneEmpty(_id) ? isPassVdCode(_id) : false;
        }

        $(function () {
            $("#Btn_Submit").click(function () {
                var id = "empLogin-input";
                if (ChkFinish(id)) {
                    $.ajax({
                        url: "/AjaxMain.aspx",
                        type: "POST",
                        async: false,
                        data: {
                            args0: 'A01',
                            EmpId: $("[name='EmpId']").val(),
                            EmpPW: $("[name='EmpPW']").val()
                        },
                        error: function (xhr) {
                            console.log(xhr.responseText);
                            alert('Ajax request 發生錯誤--Btn_Submit.click--請洽工程人員');
                        },
                        success: function (response) {
                            switch (response) {
                                case "N":
                                    error_focus("帳號或密碼輸入錯誤，無法登入！", $("[name='EmpId']"));
                                    reVdCode(id);
                                    break;
                                case "Y":
                                    location.href = "/index.aspx";
                                    break;
                                default:
                                    alert("登入失敗請聯絡工程人員！");
                            }
                        }
                    });
                }
            });
        })
    </script>

</asp:Content>
