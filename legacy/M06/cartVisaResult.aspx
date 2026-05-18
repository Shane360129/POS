<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="cartVisaResult.aspx.cs" Inherits="cartVisaResult" %>

<%--<%@ Register TagPrefix="inc" TagName="PKind" Src="~/inc/inc_PKind.ascx" %>--%>

<script runat="server">

</script>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="g-container">
        <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label>
<%--        <main class="g-pageMain" role="main">
            <!-- コンテンツ部 -->
            <div class="pageHeader">
                <p class="backwardParent">
                    <a href="/index.aspx"><i class="fas fa-chevron-circle-right nav-right"></i>首頁</a>
                    <i class="fas fa-chevron-circle-right nav-right"></i>購物車
                    <i class="fas fa-chevron-circle-right nav-right"></i>刷卡失敗
                </p>
                <div class="barleyPatternBand barleyPatternBand-withText">
                    <div class="barleyPatternBand_inner">
                        <p class="barleyPatternBand_text">刷卡失敗</p>
                    </div>
                    <!-- /.barleyPatternBand_inner -->
                </div>
                <!-- /.barleyPatternBand -->
            </div>
            <!-- /.pageHeader -->

            <div class="pageBody">
                <div class="content">
                    <div class="content_header">
                        <h1 class="headingLv1 heading heading-decoration">刷卡失敗</h1>
                    </div>
                    <!-- /.content_header -->

                    <div class="content_body">
                        <div class="l-inner">
                            <div class="productsList">
                                <div class="l-grid l-grid-center">
                                    <div class="l-grid_item l-grid_item-2-12 l-grid_item-12-12-md xs hidden-xs hidden-sm">
                                        <inc:PKind ID="PKind" runat="server" />
                                    </div>
                                    <div class="l-grid_item l-grid_item-10-12 l-grid_item-12-12-md">
                                        <div class="page-content">
                                            <div class="cart-order-content">
                                                <div class="cart-step cart-step2">
                                                    <div class="step step1"><div></div><img src="/images/cart/icon/step1A.png" /></div>
                                                    <div class="step step2"><div></div><img src="/images/cart/icon/step2A.png" /></div>
                                                    <div class="step step3"><div></div><img src="/images/cart/icon/step3.png" /></div>
                                                </div>
                                                <asp:label id="Label1" runat="server"></asp:label>

                                            </div>
                                            <div style="margin-top: 3%">
                                                <div class="box_shadow btn-hover page-btn" style="background-color:#80C26A;" id="btn_visa_retry">再試一次<img src="/images/icon/iconChk.png" style="width:30px;margin-left:4px; vertical-align:text-bottom" /></div>
                                            </div>
                                        </div>
                                        <!-- /.page-content -->
                                    </div>
                                    <!-- /.l-grid_item -->
                                </div>
                                <!-- /.l-grid -->
                            </div>
                            <!-- /.productsList -->
                        </div>
                        <!-- /.l-inner -->
                    </div>
                    <!-- /.content_body -->

                </div>
                <!-- /.content -->

            </div>
            <!-- /.pageBody -->
        </main><!-- /.g-pageMain -->--%>
    </div>
    <!-- /.g-container -->

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder2" runat="Server">
    <script type="text/javascript">
        function ReSizeLCart() {
            //--- 購物車步驟
            var step1_width = $(".cart-step .step1").width();
            $(".cart-step .step1 > div").css("width", step1_width - 74);
            var step2_width = $(".cart-step .step2").width();
            $(".cart-step .step2 > div").css("width", step2_width - 74);
            var step3_width = $(".cart-step .step3").width();
            $(".cart-step .step3 > div").css("width", step3_width - 59);
        }

        $(function () {
            ReSizeLCart();
            $(window).resize(function () {
                ReSizeLCart();
            });

            $("#btn_visa_retry").click(function () {
                location.href = "/cartVisa.aspx?OrdId=" + $("#OrdId").val();
            });
        })

    </script>
</asp:Content>


