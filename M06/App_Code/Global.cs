/// <summary>
/// Global 全域變數
/// </summary>
public class Global
{
    public static Cookies cookies = new Cookies();
    public static GetTbl getTbl = new GetTbl();
    public static ToDouble toDouble = new ToDouble();
    public static ThreeDot threeDot = new ThreeDot();
    public static StringEncrypt stringEncrypt = new StringEncrypt();


    public static string
        PageTitle = "水里鄉農會POS進銷存管理系統",
        CryptoKey = "S20Key",    // AES 加密金鑰
        StartYM = "20221201",    // 期初年月
        pvType = "2",            // 廠商1:下拉選單 / 2:autocomplete
        memType = "2",            // 商品1:下拉選單 / 2:autocomplete
        PosKey = "077402935";    // POS 交易金鑰

    public static double
        TaxPercent = 0.05;

    public static int
        pointQty = 2;
}


