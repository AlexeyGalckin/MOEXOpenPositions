using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TigerTrade.Chart.Indicators.Custom
{
    interface IMOEXClient
    {
        void Update();
        void Init(string symbol);
        bool Get(DateTime time, out long value);
        bool GetMinMax(out double min, out double max);
    }
    public class MOEXClient : IMOEXClient
    {
        private const string _base = "https://iss.moex.com/iss/analyticalproducts/futoi/securities/{0}.csv";
        public bool Get(DateTime time, out long value)
        {
            throw new NotImplementedException();
        }
        public void Init(string symbol)
        {
            _url = String.Format(_base, symbol);
            //
            _cookie = new Cookie("MicexPassportCert", _passport, "/", ".moex.com");
        }
        void Parse(Stream s)
        {
            const int Start = 14;
            const int Row = 12;
            //
            StreamReader r = new StreamReader(s);
            //
            var l = r.ReadToEnd();
            //
            var t = l.Split('\n', ';');
            //
            for(var i = Start; i < t.Length; i += Row)
            {
                var diff = t[i + 6];
                var date = t[i + 11];
            }
        }
        public void Update()
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_url);
            //
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(_cookie);
            //
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //
            var webStream = response.GetResponseStream();
            Parse(webStream);
        }
        public bool GetMinMax(out double min, out double max)
        {
            min = _min;
            max = _max;
            //
            return true;
        }
        //
        DateTime _last = DateTime.MinValue;
        //
        string _url;
        Cookie _cookie;
        //
        long _min = long.MaxValue;
        long _max = long.MinValue;
        //
        string _passport = "CWYRf4a4MYR1WzwdjEHKiQUAAAAIk2vp3llqix6hlne9tgCg8dspidbL5rGZgGkTM0HGD8X5_UMjHr-3s3l1nZSWZF1TAwdu1xpIiX2P28GdXg4X5dqx0vVZPcX6D3Cjvh_gNIpFdpUbpU8kUAvNf1i-aXH0zVRctDHR14eWQ71_JRkmtMIq7slboW1KQnm8wiFj-p30Ba4W0";
    }
}
