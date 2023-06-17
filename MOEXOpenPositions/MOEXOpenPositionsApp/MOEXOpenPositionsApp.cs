using System;

namespace TigerTrade.Chart.Indicators.Custom
{
    internal class MOEXOpenPositionsApp
    {
        static void Main(string[] args)
        {
            MOEXClient cli = new MOEXClientAsync();
            //
            cli.Init("NG");
            //
            var t = new DateTime(2023, 6, 16, 21, 5, 0);
            var p = cli.Get(t);
        }
    }
}
