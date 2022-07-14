using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.PriceChannel.Model
{
    public class PriceChannelFix : BotPanel
    {
        public PriceChannelFix(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);

            _tab = TabsSimple[0];

            PeriodUp = CreateParameter("Period Channel Up", 12, 5, 80, 2);
            PeriodDown = CreateParameter("Period Channel Down", 12, 5, 80, 2);

            Mode = CreateParameter("Mode", "Off", new[] { "Off", "On" });
            Lot = CreateParameter("Lot", 10, 5, 20, 1);
            Risk = CreateParameter("Risk", 1m, 0.2m, 3m, 0.1m);
            TradeRegime = CreateParameter("Trade regime", "Long and Short",
                new[] { "Long and Short", "Long mode", "Short mode", "Close position" });

            _pc = IndicatorsFactory.CreateIndicatorByName("PriceChannel", name + "PriceChannel", false);

            _pc.ParametersDigit[0].Value = PeriodUp.ValueInt;
            _pc.ParametersDigit[1].Value = PeriodDown.ValueInt;

            _pc = (Aindicator)_tab.CreateCandleIndicator(_pc, "Prime");
            _pc.Save();

            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
        }

        #region Fields ===================================
        private BotTabSimple _tab;

        private Aindicator _pc;

        private StrategyParameterInt PeriodUp;
        private StrategyParameterInt PeriodDown;

        private StrategyParameterString Mode;
        private StrategyParameterString TradeRegime;
        private StrategyParameterInt Lot;
        private StrategyParameterDecimal Risk;

        private decimal lastPriceChannelUp;
        private decimal lastPriceChannelDown;

        #endregion

        #region Methods =========================================
        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (Mode.ValueString == "Off")
            {
                return;
            }

            if (_pc.DataSeries[0].Values == null
                || _pc.DataSeries[1].Values == null
                || _pc.DataSeries[0].Values.Count < PeriodUp.ValueInt + 1
                || _pc.DataSeries[1].Values.Count < PeriodDown.ValueInt + 1)
            {
                return;
            }

            Candle lastCandle = candles[candles.Count - 1];
            lastPriceChannelUp = _pc.DataSeries[0].Values[_pc.DataSeries[0].Values.Count - 2];
            lastPriceChannelDown = _pc.DataSeries[1].Values[_pc.DataSeries[1].Values.Count - 2];

            List<Position> positions = _tab.PositionsOpenAll;
            if (positions.Count == 0 && TradeRegime.ValueString != "Close position")
            {
                if ((TradeRegime.ValueString == "Long and Short"
                    || TradeRegime.ValueString == "Long mode")
                    && lastCandle.Close > lastPriceChannelUp
                    && lastCandle.Open < lastPriceChannelUp)
                {
                    _tab.BuyAtMarket(GetLotForTrading());
                }
                else if ((TradeRegime.ValueString == "Long and Short"
                    || TradeRegime.ValueString == "Short mode")
                    && lastCandle.Close < lastPriceChannelDown
                    && lastCandle.Open > lastPriceChannelDown)
                {
                    _tab.SellAtMarket(GetLotForTrading());
                }
            }

            if (positions.Count > 0)
            {
                Traling(positions);
            }
        }

        private void Traling(List<Position> positions)
        {
            decimal lastDown = _pc.DataSeries[1].Values.Last();
            decimal lastUp = _pc.DataSeries[0].Values.Last();

            foreach (var pos in positions)
            {
                if (pos.State == PositionStateType.Open)
                {
                    if (pos.Direction == Side.Buy)
                    {
                        _tab.CloseAtTrailingStop(pos, lastDown, lastDown - 50 * _tab.Securiti.PriceStep);
                    }
                    else if (pos.Direction == Side.Sell)
                    {
                        _tab.CloseAtTrailingStop(pos, lastUp, lastUp + 50 * _tab.Securiti.PriceStep);
                    }
                }
            }
        }

        private decimal GetLotForTrading()
        {
            decimal riskMoney = _tab.Portfolio.ValueBegin * Risk.ValueDecimal / 100;

            decimal priceStepCost = _tab.Securiti.PriceStepCost;
            //priceStepCost = 1;

            decimal steps = Math.Abs((lastPriceChannelUp - lastPriceChannelDown) / _tab.Securiti.PriceStep);

            return (int)(riskMoney / (steps * priceStepCost));
        }
        #endregion

        public override string GetNameStrategyType()
        {
            return nameof(PriceChannelFix);
        }

        public override void ShowIndividualSettingsDialog()
        {

        }
    }
}
