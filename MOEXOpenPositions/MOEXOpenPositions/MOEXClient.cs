using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
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
        string Passport
        {
            get;
            set;
        }
        bool Legal
        {
            get;
            set;
        }
        string Symbol
        {
            get;
            set;
        }
        void Update();
        void Clear();
        long Get(DateTime time);
        bool GetMinMax(out double min, out double max);
        Dictionary<string, string> Debug();
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
        string _passport;
        public string Passport
        {
            get { return _passport; }
            set
            {
                if (_passport != value)
                {
                    _passport = value;
                    Clear();
                }
            }
        }

        bool _legal;
        public bool Legal
        {
            get { return _legal; }
            set
            {
                if (_legal != value)
                {
                    _legal = value;
                    Clear();
                }
            }
        }
        string _symbol;
        public string Symbol
        {
            get { return _symbol; }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    Clear();
                }
            }
        }
        public long Get(DateTime time)
        {
            if (!_data.Any())
                return 0;
            //
            var e = new Entry(time, 0);
            //
            return Get(e);
        }
        public MOEXClient()
        {
            Passport = "CWYRf4a4MYR1WzwdjEHKiQUAAAAIk2vp3llqix6hlne9tgCg8dspidbL5rGZgGkTM0HGD8X5_UMjHr-3s3l1nZSWZF1TAwdu1xpIiX2P28GdXg4X5dqx0vVZPcX6D3Cjvh_gNIpFdpUbpU8kUAvNf1i-aXH0zVRctDHR14eWQ71_JRkmtMIq7slboW1KQnm8wiFj-p30Ba4W0";
            Legal = true;
        }

        long Get(Entry e)
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
        void Parse(Stream s)
        {
            const int Begin = 14;
            const int Row = 12;
            const int End = 11;
            //
            StreamReader r = new StreamReader(s);
            //
            var l = r.ReadToEnd();
            //
            var t = l.Split('\n', ';');
            //
            for (var i = t.Length - Row - End; i >= Begin; i -= Row)
            {
                var type = t[i + 5];
                //
                if (Legal && type != "YUR")
                    continue;
                else if (!Legal && type != "FIZ")
                    continue;
                else
                {
                    var diff = Convert.ToInt32(t[i + 6]);
                    var date = Convert.ToDateTime(t[i + 2] + ' ' + t[i + 3]);
                    //
                    if (date < Last)
                        continue;
                    //
                    Add(new Entry(date, diff));
                    //
                    if (diff > _max) _max = diff;
                    if (diff < _min) _min = diff;
                }
            }
        }
        virtual public void Update()
        {
            var s = String.Format(_base, Symbol);
            //
            if (Last != DateTime.MinValue)
            {
                if (DateTime.Now - Last >= _interval)
                    s += "?from=" + DateTime.Now.ToString("yyyy-MM-dd");
                else
                    return;
            }
            //
            var uri = new Uri(String.Format(_base, Symbol));
            //
            var cookie = new Cookie("MicexPassportCert", Passport, "/", uri.Host.ToString());
            //
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(s);
            //
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookie);
            //
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //
            var webStream = response.GetResponseStream();
            Parse(webStream);
        }
        public bool GetMinMax(out double min, out double max)
        {
            if (_min == long.MaxValue || _max == long.MinValue)
            {
                min = 0;
                max = 0;
                //
                return false;
            }
            //
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
        public Dictionary<string, string> Debug()
        {
            Dictionary<string, string> d = new Dictionary<string, string>();

            d["Count"] = _data.Count.ToString();
            d["Last"] = Last.ToString();

            return d;
        }
        virtual public void Clear()
        {
            _min = long.MaxValue;
            _max = long.MinValue;
            //
            _data.Clear();
        }
        DateTime Last
        { 
            get => _data.Any() ? _data.Last().Item1 : DateTime.MinValue;
        }
        //
        Data _data = new Data();
        //
        long _min = long.MaxValue;
        long _max = long.MinValue;
        //
        static Comparator _comp = new Comparator();
        static TimeSpan _interval = TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(5);
        //
        static string _base = "https://iss.moex.com/iss/analyticalproducts/futoi/securities/{0}.csv";
    }
}
