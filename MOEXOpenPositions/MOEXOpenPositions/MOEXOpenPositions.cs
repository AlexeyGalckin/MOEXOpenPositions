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

        [DataMember(Name = "Passport")]
        [Category("API"), DisplayName("MicexPassportCert")]
        public string Passport
        {
            get => _api.Passport;
            set
            {
                if (_api.Passport == value)
                {
                    return;
                }

                _api.Passport = value;
                _changed = true;

                OnPropertyChanged();
            }
        }

        [TypeConverter(typeof(EntityType))]
        [DataContract(Name = "EntityType", Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Indicators.Custom")]
        public enum EntityType
        {
            [EnumMember(Value = "Individual"), Description("Individuals directed position in contracts")]
            Individual,
            [EnumMember(Value = "Legal"), Description("Legal entities directed position in contracts")]
            Legal
        }

        bool _changed = true;

        [DataMember(Name = "Entity")]
        [Category("API"), DisplayName("Legal entities")]
        public EntityType Entity
        {
            get => _api.Legal ? EntityType.Legal : EntityType.Individual;
            set
            {
                var b = (value == EntityType.Legal);

                if (_api.Legal == b)
                {
                    return;
                }

                _api.Legal = b;
                _changed = true;

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

        private IMOEXClient _api = new MOEXClient();
        public MOEXOpenPositionsIndicator()
        {
            LineColor = Color.FromArgb(255, 0, 0, 255);
            LineWidth = 1;
            //
            Passport = "CWYRf4a4MYR1WzwdjEHKiQUAAAAIk2vp3llqix6hlne9tgCg8dspidbL5rGZgGkTM0HGD8X5_UMjHr-3s3l1nZSWZF1TAwdu1xpIiX2P28GdXg4X5dqx0vVZPcX6D3Cjvh_gNIpFdpUbpU8kUAvNf1i-aXH0zVRctDHR14eWQ71_JRkmtMIq7slboW1KQnm8wiFj-p30Ba4W0";
            Entity = EntityType.Legal;
        }
        protected override void Execute()
        {
            var exc = DataProvider.Symbol.Exchange.ToString();
            //
            if(exc == "MOEX" && !ClearData)
            {
                var sym = DataProvider.Symbol.ToString();
                if (_api.Symbol != sym || _changed)
                {
                    _api.Clear();
                    _api.Symbol = sym;
                    _api.Init();
                }
                //
                _api.Update();
                _changed = false;
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

            if (Debug)
            {
                foreach (var d in _api.Debug())
                    info.Add(new IndicatorValueInfo(d.ToString(), Canvas.Theme.ChartFontBrush));
            }

            return info;
        }
        public override string ToString()
        {
            return $"{Name} ({Entity})";
        }
    }
}
