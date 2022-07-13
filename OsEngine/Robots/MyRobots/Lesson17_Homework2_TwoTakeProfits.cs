using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.MyRobots
{
    public class Lesson17_Homework2_TwoTakeProfits : BotPanel
    {
        private BotTabSimple _tab;

        /// <summary>
        /// Риск на сделку в %
        /// </summary>
        private StrategyParameterDecimal _risk;

        /// <summary>
        /// Во сколько раз TakeProfit превышает риск на сделку
        /// </summary>
        private StrategyParameterDecimal _profitKoef;

        /// <summary>
        /// Во сколько раз объем превышает средний
        /// </summary>
        private StrategyParameterDecimal _volumeKoef;

        /// <summary>
        /// Количество падающих свечей подряд перед объёмным разворотом
        /// </summary>
        private StrategyParameterInt _downCandlesCount;

        /// <summary>
        /// Средний объем за период _volumeCandlesCount
        /// </summary>
        private decimal _averageVolume;

        /// <summary>
        /// Количество свечей необходимых для расчета среднего объема _averageVolume
        /// </summary>
        private StrategyParameterInt _volumeCandlesCount;

        /// <summary>
        /// Размер стоп-лосса
        /// </summary>
        private decimal stopLossInCash = 0m;

        /// <summary>
        /// Low свечи на которой мы получили сигнал для входа в позицию; является стопом
        /// </summary>
        private decimal lastBeforeEntryCandleLow = 0m;


        public Lesson17_Homework2_TwoTakeProfits(string name, StartProgram startProgram) : base(name, startProgram)
        {
            this.TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            this.CreateParameter("Mode", "Edit", new[] { "Edit", "Trade" });
            _risk = CreateParameter("Risk %", 1m, 0.1m, 10m, 0.1m);
            _profitKoef = CreateParameter("ProfitLoss Koef", 2m, 0.1m, 10m, 0.1m);
            _downCandlesCount = CreateParameter("Down Candles Count", 3, 1, 7, 1);
            _volumeKoef = CreateParameter("Volume Koef", 2m, 2m, 10m, 0.5m);
            _volumeCandlesCount = CreateParameter("Volume Candles Count", 10, 5, 50, 1);

            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
            _tab.PositionOpeningSuccesEvent += _tab_PositionOpeningSuccesEvent;
            _tab.PositionClosingSuccesEvent += _tab_PositionClosingSuccesEvent;
        }

        private void _tab_PositionClosingSuccesEvent(Position pos)
        {
            SaveCSV(pos);
        }

        /// <summary>
        /// Обработка события "Позиция открыта успешно"
        /// </summary>
        /// <param name="pos"></param>
        private void _tab_PositionOpeningSuccesEvent(Position pos)
        {
            decimal takeProfitPrice = 0;
            if (_tab.PositionOpenLong.Count == 1)//Установим тейк для первой части позиции равной половине общей позиции
            {
                takeProfitPrice = pos.EntryPrice + stopLossInCash;
                _tab.CloseAtProfit(pos, takeProfitPrice, takeProfitPrice);
                _tab.CloseAtStop(pos, lastBeforeEntryCandleLow, lastBeforeEntryCandleLow * 0.9m);
            }
            else//видимо особенность платформы OsEngine - при одновременном срабатывании ордеров приходит сразу второе событие, первое теряется
            {
                _tab.CloseAllOrderToPosition(_tab.PositionOpenLong[0]);//отменяем возможные ордера на всякий случай
                takeProfitPrice = pos.EntryPrice + stopLossInCash;
                _tab.CloseAtProfit(_tab.PositionOpenLong[0], takeProfitPrice, takeProfitPrice);
                _tab.CloseAtStop(_tab.PositionOpenLong[0], lastBeforeEntryCandleLow, lastBeforeEntryCandleLow * 0.9m);

                takeProfitPrice = pos.EntryPrice + stopLossInCash * _profitKoef.ValueDecimal; //Установим тейк для второй части позиции равной половине общей позиции              
                _tab.CloseAtProfit(_tab.PositionOpenLong[1], takeProfitPrice, takeProfitPrice);
                _tab.CloseAtStop(_tab.PositionOpenLong[1], lastBeforeEntryCandleLow, lastBeforeEntryCandleLow * 0.9m);

            }
        }

        /// <summary>
        /// Обработка события "Получена новая свеча"
        /// </summary>
        /// <param name="candles"></param>
        private void _tab_CandleFinishedEvent(List<Candle> candles) //Question: Какое максимальное количество свечей может прийти?
        {
            if (_tab.PositionOpenLong.Count > 0)
            {
                return;
            }

            if (candles.Count < _downCandlesCount.ValueInt + 1 || candles.Count < _volumeCandlesCount.ValueInt + 1)
            {
                return;
            }

            Candle lastCandle = candles[candles.Count - 1];

            if (lastCandle.Close < (lastCandle.High + lastCandle.Low) / 2)
            {
                return;
            }

            _averageVolume = 0;
            for (int i = candles.Count - 2; i > candles.Count - _volumeCandlesCount.ValueInt - 2; i--)
            {
                _averageVolume += candles[i].Volume;
            }
            _averageVolume = _averageVolume / _volumeCandlesCount.ValueInt;

            if (lastCandle.Volume < _averageVolume * _volumeKoef.ValueDecimal)
            {
                return;
            }

            for (int i = candles.Count - 2; i > candles.Count - 2 - _downCandlesCount.ValueInt; i--)
            {
                if (candles[i].Close > candles[i].Open)
                {
                    return;
                }
            }

            int stopLossInPriceSteps = (int)((lastCandle.Close - lastCandle.Low) / _tab.Securiti.PriceStep); //Securiti - ошибка в наименовании?

            if (stopLossInPriceSteps < 5)//не входим в сделку при стопе менее 5 шагов цены
            {
                return;
            }

            stopLossInCash = stopLossInPriceSteps * _tab.Securiti.PriceStepCost; //PriceStepCost?!

            decimal riskForDealInCash = _tab.Portfolio.ValueBegin * _risk.ValueDecimal / 100; //Почему _tab.Portfolio.ValueBegin, а не _tab.Portfolio.ValueCurrent?
            decimal amountOfContractsForTrading = riskForDealInCash / stopLossInCash;//количество контрактов для торговли

            int marginInProcents = 20; //ГО в процентах от стоимости контракта для тестового режима
            decimal marginInCash = (lastCandle.Close / 100) * marginInProcents;//Рассчитали ГО на 1 контракт

            if (_tab.Securiti.Go > 1)//если мы находимся в боевом, а не тестовом режиме
            {
                marginInCash = _tab.Securiti.Go;
            }

            amountOfContractsForTrading = Math.Min(amountOfContractsForTrading, _tab.Portfolio.ValueBegin / marginInCash);//если пришел сигнал, то торгуем количеством меньшим из двух
            
            //ДОМАШНЕЕ ЗАДАНИЕ!
            decimal amountOfContractsForTrading1 = amountOfContractsForTrading / 2;
            decimal amountOfContractsForTrading2 = amountOfContractsForTrading - amountOfContractsForTrading1;

            lastBeforeEntryCandleLow = lastCandle.Low;

            if(amountOfContractsForTrading1 > 0)
            {
                _tab.BuyAtMarket(amountOfContractsForTrading1);
            }
            if (amountOfContractsForTrading2 > 0)
            {
                _tab.BuyAtMarket(amountOfContractsForTrading2);
            }
        }


        private void SaveCSV(Position pos)
        {
            if (!File.Exists(@"Engine\trades.csv"))
            {
                string header = ";Позиция;Символ;Лоты;Изменение/Максимум Лотов";

                using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", false))
                {
                    writer.WriteLine(header);
                }
            }

            using (StreamWriter writer = new StreamWriter(@"Engine\trades.csv", true))
            {
                string str = $";;;;;;;;{pos.TimeOpen.ToShortDateString()};{pos.TimeOpen.TimeOfDay};;;;;;;;;;;;;{pos.ProfitPortfolioPunkt};;;;;";
                writer.WriteLine(str);
            }
        }

        public override string GetNameStrategyType()
        {
            return nameof(BinBarBot_Lesson17);
        }

        public override void ShowIndividualSettingsDialog()
        {

        }
    }
}
