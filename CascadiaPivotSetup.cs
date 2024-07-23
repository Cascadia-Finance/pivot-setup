#region Using declarations
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class CascadiaPivotSetUp : Indicator
	{
		private string soundFilePath;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= @"Cascadia Pivot Setup";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				LookBack					= 3;
				DirectionalWick = true;
				LargeWick = true;
				PlayAlert = true;

				AddPlot(new Stroke(Brushes.Green, 5), PlotStyle.TriangleRight, "Entry");
				AddPlot(new Stroke(Brushes.Red, 5), PlotStyle.TriangleLeft, "Stop");
				AddPlot(Brushes.Transparent, "Direction");
				
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				soundFilePath = Path.Combine(NinjaTrader.Core.Globals.InstallDir, "sounds", Instrument.MasterInstrument.Name + "_PivotAlert.wav");
				Print($"Sound File: {soundFilePath}");
			}
		}

		protected override void OnBarUpdate()
		{
			//if (!Bars.BarsType.IsIntraday) return;
			if (CurrentBar <= LookBack) return;
			
			//Add your custom indicator logic here.
			bool shortTrend = true;
			for(int i = LookBack; i > 0 && shortTrend; i--) 
			{
				shortTrend = shortTrend && (High[i + 1] < High[i]);
			}
			shortTrend = shortTrend && (High[1] > High[0]);
			
			bool longTrend = true;
			for(int i = LookBack; i > 0 && longTrend; i--) 
			{
				longTrend = longTrend && (Low[i + 1] > Low[i]);
			}
			longTrend = longTrend && (Low[1] < Low[0]);
		
			double upperWickLength = High[1] - Math.Max(Open[1], Close[1]);
			double lowerWickLength = Math.Min(Open[1], Close[1]) - Low[1];
			double totalWickLength = upperWickLength + lowerWickLength;
			double bodyLength = Math.Max(Open[1], Close[1]) - Math.Min(Open[1], Close[1]);

			if(DirectionalWick)
			{
				shortTrend = shortTrend && (upperWickLength > lowerWickLength);
				longTrend = longTrend && (lowerWickLength > upperWickLength);
			}
			
			if(LargeWick)
			{
				shortTrend = shortTrend && (totalWickLength > bodyLength);
				longTrend = longTrend && (totalWickLength > bodyLength);
			}
						
			if( longTrend && shortTrend)
			{
				Print($"Found Long AND Short at {Time[0]}");
				Direction[0] = 0;
				Entry[0] = double.NaN;
				Stop[0] = double.NaN;
			} 
			else if(longTrend)
			{
				if(Direction[0] != 1)
				{
					Print($"Found Long at {Time[0]}");

					Entry[0] = High[1];
					Stop[0] = Low[1];
					Direction[0] = 1;
	
					if(PlayAlert)
						PlaySound(soundFilePath);
				}
			} 
			else if(shortTrend)
			{
				if(Direction[0] != -1)
				{
					Print($"Found Short at {Time[0]}");

					Entry[0] = Low[1];
					Stop[0] = High[1];
					Direction[0] = -1;
					
					if(PlayAlert)
						PlaySound(soundFilePath);
				}
			}
			else
			{
				if(Direction[0] != 0)
				{
					Print($"Disqualify at {Time[0]}");
				}
				Direction[0] = 0;
				Entry[0] = double.NaN;
				Stop[0] = double.NaN;
			}
		}

		#region Properties
				
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Look Back", Description="Number of Bars to Look Back for setup", Order=1, GroupName="Parameters")]
		public int LookBack
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Directional Wick", Description="Wick must indicate trade direction (Long: Down wick longer than Up)", Order=1, GroupName="Parameters")]
		public bool DirectionalWick
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Larger Wick than Body", Description="Total wick size must be larger than candle body", Order=1, GroupName="Parameters")]
		public bool LargeWick
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Play Alert Sound, e.g. NQ_PivotAlert.wav", Description="Play Pivot alert sound", Order=1, GroupName="Parameters")]
		public bool PlayAlert
		{ get; set; }

		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Entry
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Stop
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Direction
		{
			get { return Values[2]; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CascadiaPivotSetUp[] cacheCascadiaPivotSetUp;
		public CascadiaPivotSetUp CascadiaPivotSetUp(int lookBack, bool directionalWick, bool largeWick, bool playAlert)
		{
			return CascadiaPivotSetUp(Input, lookBack, directionalWick, largeWick, playAlert);
		}

		public CascadiaPivotSetUp CascadiaPivotSetUp(ISeries<double> input, int lookBack, bool directionalWick, bool largeWick, bool playAlert)
		{
			if (cacheCascadiaPivotSetUp != null)
				for (int idx = 0; idx < cacheCascadiaPivotSetUp.Length; idx++)
					if (cacheCascadiaPivotSetUp[idx] != null && cacheCascadiaPivotSetUp[idx].LookBack == lookBack && cacheCascadiaPivotSetUp[idx].DirectionalWick == directionalWick && cacheCascadiaPivotSetUp[idx].LargeWick == largeWick && cacheCascadiaPivotSetUp[idx].PlayAlert == playAlert && cacheCascadiaPivotSetUp[idx].EqualsInput(input))
						return cacheCascadiaPivotSetUp[idx];
			return CacheIndicator<CascadiaPivotSetUp>(new CascadiaPivotSetUp(){ LookBack = lookBack, DirectionalWick = directionalWick, LargeWick = largeWick, PlayAlert = playAlert }, input, ref cacheCascadiaPivotSetUp);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CascadiaPivotSetUp CascadiaPivotSetUp(int lookBack, bool directionalWick, bool largeWick, bool playAlert)
		{
			return indicator.CascadiaPivotSetUp(Input, lookBack, directionalWick, largeWick, playAlert);
		}

		public Indicators.CascadiaPivotSetUp CascadiaPivotSetUp(ISeries<double> input , int lookBack, bool directionalWick, bool largeWick, bool playAlert)
		{
			return indicator.CascadiaPivotSetUp(input, lookBack, directionalWick, largeWick, playAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CascadiaPivotSetUp CascadiaPivotSetUp(int lookBack, bool directionalWick, bool largeWick, bool playAlert)
		{
			return indicator.CascadiaPivotSetUp(Input, lookBack, directionalWick, largeWick, playAlert);
		}

		public Indicators.CascadiaPivotSetUp CascadiaPivotSetUp(ISeries<double> input , int lookBack, bool directionalWick, bool largeWick, bool playAlert)
		{
			return indicator.CascadiaPivotSetUp(input, lookBack, directionalWick, largeWick, playAlert);
		}
	}
}

#endregion
