namespace TigerTrade.Chart.Indicators.Custom
{
    internal class MOEXOpenPositionsApp
    {
        static void Main(string[] args)
        {
            MOEXClient cli = new MOEXClient();
            //
            cli.Init("NG");
            cli.Update();
        }
    }
}
