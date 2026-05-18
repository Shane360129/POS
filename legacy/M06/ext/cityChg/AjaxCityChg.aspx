<%@ Page Language="C#" %>

<%@ Import Namespace="System.Data" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Response.Clear();
            //城市連動鄉鎮
            string SqlComm = "SELECT * FROM WP_AddrZip";
            DataTable PKTbl = Global.getTbl.table("WP", SqlComm);

            string JSON = "";
            foreach (DataRow row in PKTbl.Rows)
            {
                JSON += $"{(JSON == "" ? "" : ",")}{{\"cityId\":\"{row["AddrCityId"]}\",\"cityName\":\"{row["AddrCity"]}\",\"zoneId\":\"{row["AddrZoneId"]}\",\"zoneName\":\"{row["AddrZone"]}\"}}";
            }
            JSON = $"[{JSON}]";
            Response.Write(JSON);

            Response.End();
        }
    }

</script>

