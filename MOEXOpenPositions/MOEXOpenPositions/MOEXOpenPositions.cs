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

        [Browsable(false)]
        public override IndicatorCalculation Calculation => IndicatorCalculation.OnEachTick;

        public override bool IntegerValues => true;

        private IMOEXClient _api = new MOEXClient();
        public MOEXOpenPositionsIndicator()
        {
            LineColor = Color.FromArgb(255, 0, 0, 255);
            LineWidth = 1;
            //
        }

        protected override void Execute()
        {
            _api.Init("NG");
            _api.Update();
        }

        public override void Render(DxVisualQueue visual)
        {
            var symbol = DataProvider.Symbol;
            var points = new Point[Canvas.Count];

             
            for (var i = 0; i < Canvas.Count; i++)
            {
                var index = Canvas.GetIndex(i);


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

        public override string ToString()
        {
            return $"{Name} (Penis)";
        }
    }
}
