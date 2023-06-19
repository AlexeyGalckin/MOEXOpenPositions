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
        void Init();
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
        public string Passport { get; set; }
        public bool Legal { get; set; }
        public string Symbol { get; set; }
        //
        static string _base = "https://iss.moex.com/iss/analyticalproducts/futoi/securities/{0}.csv";
        public long Get(DateTime time)
        {
            if(!_data.Any())
                return 0;
            //
            var e = new Entry(time, 0);
            //
            return Get(e);
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
        public void Init()
        {
            _uri = new Uri(String.Format(_base, Symbol));
            //
            var d = _uri.Host.ToString();
            //
            _cookie = new Cookie("MicexPassportCert", Passport, "/", d);
        }
        void Parse(Stream s)
        {
            //
            StreamReader r = new StreamReader(s);
            //
            var l = r.ReadToEnd();
            //
            var t = l.Split('\n', ';');
            //
            var o = Legal ? Row * 2 : Row;
            //
            for (var i = t.Length - o - End; i >= Begin; i -= Row * 2)
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
            var s = _uri.ToString();
            //
            if (_last > DateTime.MinValue)
            {
                if (DateTime.Now - _last >= _interval)
                    s += "?from=" + DateTime.Now.ToString("yyyy-MM-dd");
                else
                    return;
            }
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
            DebugAppend(d);
            return d;
        }
        public virtual void DebugAppend(Dictionary<string, string> d)
        {
            d["Count"] = _data.Count.ToString();
            d["Last"] = _last.ToString();
        }
        virtual public void Clear()
        {
            _min = long.MaxValue;
            _max = long.MinValue;
            //
            _data.Clear();
            _last = DateTime.MinValue;
            //
            Symbol = String.Empty;
        }
        //
        protected Data _data = new Data();
        //
        protected DateTime _last = DateTime.MinValue;
        //
        Uri _uri;
        Cookie _cookie;
        //
        long _min = long.MaxValue;
        long _max = long.MinValue;
        //
        const int Begin = 14;
        const int Row = 12;
        const int End = 11;
        //
        static Comparator _comp = new Comparator();
        static TimeSpan _interval = TimeSpan.FromMinutes(5);
    }
    //
}
