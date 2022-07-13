using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.FrontRunner.ViewModels;
using OsEngine.Robots.FrontRunner.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.Robots.FrontRunner.Models
{
    public class FrontRunnerBot : BotPanel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FrontRunnerBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];
            _tab.MarketDepthUpdateEvent += _tab_MarketDepthUpdateEvent;
            _tab.PositionOpeningFailEvent += _tab_PositionOpeningFailEvent;
        }

        #region Fields and Properties =================
        public decimal BigVolume { get; set; } = 7000m;
        public int Offset { get; set; } = 1;
        public int Take { get; set; } = 5;
        public decimal Lot { get; set; } = 2m;

        public string SymbolName
        {
            get => _symbolName;
            set
            {
                if (value != _symbolName)
                {
                    _symbolName = value;
                    OnBotPropertyChanged(nameof(Margin));
                }
            }

        }
        private string _symbolName = String.Empty;

        public PositionStateType CurrentPositionState { get; set; } = PositionStateType.None;

        public decimal AmountOfOpenLots
        { 
            get => _amountOfOpenLots; 
            set
            {
                if (value != _amountOfOpenLots)
                {
                    _amountOfOpenLots = value;
                    OnBotPropertyChanged(nameof(AmountOfOpenLots));
                }
            }
        }
        private decimal _amountOfOpenLots;

        public decimal PositionOpenPrice
        {
            get => _positionOpenPrice;
            set
            {
                if (value != _positionOpenPrice)
                {
                    _positionOpenPrice = value;
                    OnBotPropertyChanged(nameof(PositionOpenPrice));
                }
            }
        }
        private decimal _positionOpenPrice;

        public decimal TakeProfitPrice
        {
            get => _takeProfit;
            set
            {
                if (value != _takeProfit)
                {
                    _takeProfit = value;
                    OnBotPropertyChanged(nameof(TakeProfitPrice));
                }
            }
        }
        private decimal _takeProfit;

        public decimal Margin
        {
            get => _margin;
            set
            {
                if (value != _margin)
                {
                    _margin = value;
                    OnBotPropertyChanged(nameof(Margin));
                }
            }
        }
        private decimal _margin;

        public decimal PnL
        {
            get => _pnl;
            set
            {
                if (value != _pnl)
                {
                    _pnl = value;
                    OnBotPropertyChanged(nameof(PnL));
                }
            }
        }
        private decimal _pnl;

        public Edit Edit
        {
            get => _edit;
            set
            {
                _edit = value;

                if (_edit == Edit.Stop && Position != null
                    && Position.State == PositionStateType.Opening)
                {
                    _tab.CloseAllOrderInSystem();
                }
            }
        }
        private Edit _edit = Edit.Stop;

        public Position Position { get; set; } = null;
        private BotTabSimple _tab;
        #endregion

        #region Methods ===============================
        private void _tab_PositionOpeningFailEvent(Position pos)
        {
            Position = null;
        }

        private void _tab_MarketDepthUpdateEvent(MarketDepth marketDepth)
        {
            if (Edit == Edit.Stop)
            {
                return;
            }

            if (marketDepth.SecurityNameCode != _tab.Securiti.Name)
            {
                return;
            }

            if(SymbolName != _tab.Securiti.Name)
            {
                SymbolName = _tab.Securiti.Name;
            }

            List<Position> positions = _tab.PositionsOpenAll;


            if (positions != null && positions.Any())
            {
                foreach (Position pos in positions)
                {
                    if (pos.State == PositionStateType.ClosingFail)
                    {
                        pos.State = PositionStateType.Open;
                    }

                    CurrentPositionState = pos.State;
                    AmountOfOpenLots = pos.OpenVolume;
                    PositionOpenPrice = pos.EntryPrice;
                    TakeProfitPrice = pos.ProfitOrderPrice;
                    Margin = pos.OpenVolume * _tab.Securiti.Go;
                    PnL = pos.ProfitPortfolioPunkt;

                    if (pos.State == PositionStateType.Open)
                    {
                        Position = pos;
                        if (pos.Direction == Side.Sell && !pos.CloseActiv)
                        {
                            decimal takePrice = Position.EntryPrice - Take * _tab.Securiti.PriceStep;
                            _tab.CloseAtProfit(Position, takePrice, takePrice);
                        }
                        else if (pos.Direction == Side.Buy && !pos.CloseActiv)
                        {
                            decimal takePrice = Position.EntryPrice + Take * _tab.Securiti.PriceStep;
                            _tab.CloseAtProfit(Position, takePrice, takePrice);
                        }
                    }
                }
            }

            for (int i = 0; i < marketDepth.Asks.Count; i++)
            {
                if (marketDepth.Asks[i].Ask >= BigVolume && Position == null)
                {
                    decimal price = marketDepth.Asks[i].Price - Offset * _tab.Securiti.PriceStep;

                    //TODO: Необходимо проверить какое состояние позиции присваивается
                    Position = _tab.SellAtLimit(Lot, price);

                    if (Position != null && Position.State != PositionStateType.Open
                        && Position.State != PositionStateType.Opening)
                    {
                        Position = null;
                    }
                }

                if (Position != null
                    && marketDepth.Asks[i].Price == Position.EntryPrice + Offset * _tab.Securiti.PriceStep
                    && marketDepth.Asks[i].Ask < BigVolume / 2)
                {
                    if (Position.State == PositionStateType.Open)
                    {
                        _tab.CloseAtMarket(Position, Position.OpenVolume);
                    }
                    else if (Position.State == PositionStateType.Opening)
                    {
                        _tab.CloseAllOrderInSystem();
                    }
                    Position = null;
                }
                else if (Position != null
                    && Position.State == PositionStateType.Opening
                    && marketDepth.Asks[i].Ask > BigVolume
                    && marketDepth.Asks[i].Price < Position.EntryPrice + Offset * _tab.Securiti.PriceStep)
                {
                    _tab.CloseAllOrderInSystem();
                    Position = null;
                    break;
                }
            }

            for (int i = 0; i < marketDepth.Bids.Count; i++)
            {
                if (marketDepth.Bids[i].Bid >= BigVolume && Position == null)
                {
                    decimal price = marketDepth.Bids[i].Price + Offset * _tab.Securiti.PriceStep;

                    Position = _tab.BuyAtLimit(Lot, price);

                    if (Position != null && Position.State != PositionStateType.Open
                        && Position.State != PositionStateType.Opening)
                    {
                        Position = null;
                    }
                }
                //Откуда мы знаем состояние позиции?
                if (Position != null
                    && marketDepth.Bids[i].Price == Position.EntryPrice - Offset * _tab.Securiti.PriceStep
                    && marketDepth.Bids[i].Bid < BigVolume / 2)
                {
                    if (Position.State == PositionStateType.Open)
                    {
                        _tab.CloseAtMarket(Position, Position.OpenVolume);
                    }
                    else if (Position.State == PositionStateType.Opening)
                    {
                        _tab.CloseAllOrderInSystem();
                    }
                    Position = null;
                }
                else if (Position != null
                    && Position.State == PositionStateType.Opening
                    && marketDepth.Bids[i].Bid > BigVolume
                    && marketDepth.Bids[i].Price > Position.EntryPrice - Offset * _tab.Securiti.PriceStep)
                {
                    _tab.CloseAllOrderInSystem();
                    Position = null;
                    break;
                }
            }
        }
        public override string GetNameStrategyType()
        {
            return nameof(FrontRunnerBot);
        }

        public override void ShowIndividualSettingsDialog()
        {
            FrontRunnerBotUI view = new FrontRunnerBotUI(this);
            view.Show();
        }

        protected void OnBotPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
