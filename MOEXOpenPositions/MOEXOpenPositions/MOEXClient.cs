using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace TigerTrade.Chart.Indicators.Custom
{
    using Entry = Tuple<DateTime, int>;
    using Data = List<Tuple<DateTime, int>>;
    interface IMOEXClient
    {
        void Init(string symbol);
        long Get(DateTime time);
        bool GetMinMax(out double min, out double max);
    }
    class Comparator : IComparer<Entry>
    {
        public int Compare(Entry x, Entry y)
        {
            return x.Item1.CompareTo(y.Item1);
        }
    }
    public class MOEXClient : IMOEXClient
    {
        private const string _base = "https://iss.moex.com/iss/analyticalproducts/futoi/securities/{0}.csv";
        private const string _domain = ".moex.com";
        public long Get(DateTime time)
        {
            var e = new Entry(time, 0);
            //
            return Get(e);
        }
        public virtual long Get(Entry e)
        {
            var i = _data.BinarySearch(e, _comp);
            //
            if (i < 0)
            {
                i = ~i;
                if (i >= _data.Count)
                    --i;
            }
            //
            return _data[i].Item2;
        }
        public void Init(string symbol)
        {
            _url = String.Format(_base, symbol);
            //
            _cookie = new Cookie("MicexPassportCert", _passport, "/", _domain);
            //
            Update();
        }
        void Parse(Stream s)
        {
            const int Start = 14;
            const int Row = 12;
            const int End = 11;
            //
            StreamReader r = new StreamReader(s);
            //
            var l = r.ReadToEnd();
            //
            var t = l.Split('\n', ';');
            //
            for(var i = t.Length - Row*2 - End; i >= Start; i -= Row * 2)
            {
                var diff = Convert.ToInt32(t[i + 6]);
                var date = Convert.ToDateTime(t[i + 11]);
                //
                if(_last > DateTime.MinValue && _data.Last().Item1 >= date)
                    continue;
                //
                Add(new Entry(date, diff));
                //
                if(diff > _max) _max = diff;
                if(diff < _min) _min = diff;
            }
            //
            if(_data.Any())
                _last = _data.Last().Item1;
        }
        virtual public void Update()
        {
            var s = _url;
            //
            if (_last > DateTime.MinValue)
                s += "?from=" + DateTime.Now.ToString("yyyy-MM-dd");
            //
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(s);
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
        public virtual void Add(Entry entry)
        {
            _data.Add(entry);
        }
        //
        Data _data = new Data();
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
        //
        static Comparator _comp = new Comparator();
    }
    //
    public class MOEXClientAsync : MOEXClient
    {
        public override void Update() 
        {
            base.Update();
            //
            _timer = new Timer(o => { base.Update(); }, null, TimeSpan.Zero, _interval);
        }
        public override void Add(Entry entry)
        {
            lock (_lock)
            {
                base.Add(entry);
            }
        }
        public override long Get(Entry e)
        {
            lock (_lock)
            {
                return base.Get(e);
            }
        }
        //
        Timer _timer;
        private readonly object _lock = new object();
        //
        static TimeSpan _interval = TimeSpan.FromMinutes(1);
    }
}
