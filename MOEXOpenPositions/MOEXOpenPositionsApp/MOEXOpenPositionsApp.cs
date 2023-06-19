using System;

namespace TigerTrade.Chart.Indicators.Custom
{
    internal class MOEXOpenPositionsApp
    {
        static void Main(string[] args)
        {
            MOEXClient cli = new MOEXClient();
            //
            cli.Passport = "CWYRf4a4MYR1WzwdjEHKiQUAAAAIk2vp3llqix6hlne9tgCg8dspidbL5rGZgGkTM0HGD8X5_UMjHr-3s3l1nZSWZF1TAwdu1xpIiX2P28GdXg4X5dqx0vVZPcX6D3Cjvh_gNIpFdpUbpU8kUAvNf1i-aXH0zVRctDHR14eWQ71_JRkmtMIq7slboW1KQnm8wiFj-p30Ba4W0";
            cli.Legal = false;
            cli.Symbol = "NG";
            //
            cli.Update();
            //
            cli.Update();
            //
            var t = new DateTime(2024, 6, 16, 21, 5, 0);
            var p = cli.Get(t);
        }
    }
}
