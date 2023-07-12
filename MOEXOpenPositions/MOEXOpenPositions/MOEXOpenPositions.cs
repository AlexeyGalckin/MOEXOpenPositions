using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Base.Enums;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Indicators.Enums;
using TigerTrade.Core.Utils.Time;
using TigerTrade.Dx;
using TigerTrade.Core.UI.Converters;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TigerTrade.Chart.Indicators.Custom
{
    [DataContract(Name = "MOEXOpenPositions", Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Indicators.Custom")]
    [Indicator("X_MOEXOpenPositions", "MOEXOpenPositions", false, Type = typeof(MOEXOpenPositionsIndicator))]
    internal sealed class MOEXOpenPositionsIndicator : IndicatorBase
    {
        private XBrush _lineBrush;
        private XPen _linePen;
        private XColor _lineColor;

        [DataMember(Name = "LineColor")]
        [Category("Style"), DisplayName("Line Color")]
        public XColor LineColor
        {
            get => _lineColor;
            set
            {
                if (value == _lineColor)
                {
                    return;
                }

                _lineColor = value;

                _lineBrush = new XBrush(_lineColor);
                _linePen = new XPen(_lineBrush, _lineWidth);

                OnPropertyChanged();
            }
        }

        private int _lineWidth;

        [DataMember(Name = "LineWidth")]
        [Category("Style"), DisplayName("Line Width")]
        public int LineWidth
        {
            get => _lineWidth;
            set
            {
                value = Math.Max(1, Math.Min(9, value));

                if (value == _lineWidth)
                {
                    return;
                }

                _lineWidth = value;

                _linePen = new XPen(_lineBrush, _lineWidth);
                OnPropertyChanged();
            }
        }

        // I don't know why when opening the application object types are still unitialized because ctor was not called (i have checked in debugger)
        string _passport =  null;

        [DataMember(Name = "Passport")]
        [Category("API"), DisplayName("MicexPassportCert")]
        public string Passport
        {
            get => _passport;
            set
            {
                // So I'm using this workaround
                if (_passport == null) _passport = String.Empty;
                if (_passport == value)
                {
                    return;
                }

                _passport = value;
                OnPropertyChanged();
            }
        }

        bool _legal;
        [DataMember(Name = "Legal")]
        [Category("API"), DisplayName("Legal entities positions")]
        public bool Legal
        {
            get => _legal;
            set
            {
                if(_legal == value)
                {
                    return;
                }

                _legal = value;
                OnPropertyChanged();
            }
        }

        bool _debug = true;

        [DataMember(Name = "Debug")]
        [Category("API"), DisplayName("Debug Information")]
        public bool Debug
        {
            get => _debug;
            set
            {
                if (value == _debug)
                {
                    return;
                }

                _debug = value;
                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public override IndicatorCalculation Calculation => IndicatorCalculation.OnEachTick;

        public override bool IntegerValues => true;

        private IMOEXClient _api;
        public MOEXOpenPositionsIndicator()
        {
            LineColor = Color.FromArgb(255, 0, 255, 255);
            LineWidth = 3;
            //
            if (_passport == null) _passport = String.Empty;
            //
            _api = new MOEXClient();
        }
        protected override void Execute()
        {
            var exc = DataProvider.Symbol.Exchange.ToString();
            //
            if(exc == "MOEX" && !ClearData)
            {
                _api.Passport = Passport;
                _api.Legal = Legal;
                //
                _api.Symbol = DataProvider.Symbol.ToString();
                //
                _api.Update();
            }
        }

        public override void Render(DxVisualQueue visual)
        {
            var symbol = DataProvider.Symbol;
            var points = new Point[Canvas.Count];

            for (var i = 0; i < Canvas.Count; i++)
            {
                var index = Canvas.GetIndex(i);
                //
                var x = Canvas.GetX(index);
                //
                var d = Canvas.IndexToDate(index);
                //
                var v = _api.Get(d);
                //
                var y = Canvas.GetY(v);
                //
                points[i] = new Point(x, y);
            }

            visual.DrawLines(_linePen, points);
        }

        public override void ApplyColors(IChartTheme theme)
        {
            LineColor = theme.GetNextColor();
            base.ApplyColors(theme);
        }

        public override void CopyTemplate(IndicatorBase indicator, bool style)
        {
            var i = (MOEXOpenPositionsIndicator)indicator;

            LineColor = i.LineColor;
            LineWidth = i.LineWidth;

            _api = i._api;

            //Passport = i._passport;
            Legal =    i._legal;
            Debug  = i._debug;

            base.CopyTemplate(indicator, style);
        }

        public override bool GetMinMax(out double min, out double max)
        {
            return _api.GetMinMax(out min, out max);
        }

        public override List<IndicatorValueInfo> GetValues(int cursorPos)
        {
            var info = new List<IndicatorValueInfo>();

            if (cursorPos >= 0)
            {
                var d = Canvas.IndexToDate(cursorPos);
                var v = _api.Get(d);

                var s = Canvas.FormatValue((double)DataProvider.Symbol.GetSize(v));

                info.Add(new IndicatorValueInfo(s, _lineBrush));
            }

            info.Add(new IndicatorValueInfo(_api.Legal ? "(Legal)" : "(Individual)", Canvas.Theme.ChartFontBrush));

            if (Debug)
            {
                foreach (var d in _api.Debug())
                    info.Add(new IndicatorValueInfo(d.ToString(), Canvas.Theme.ChartFontBrush));
            }

            return info;
        }
    }
}
