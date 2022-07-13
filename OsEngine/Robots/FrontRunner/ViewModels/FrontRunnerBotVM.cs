using OsEngine.Commands;
using OsEngine.Entity;
using OsEngine.Robots.FrontRunner.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OsEngine.Robots.FrontRunner.ViewModels
{
    public class FrontRunnerBotVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly FrontRunnerBot _bot;

        public FrontRunnerBotVM(FrontRunnerBot bot)
        {
            _bot = bot;
            _bot.PropertyChanged += _bot_PropertyChanged;
        }



        #region Properties =====================================
        public decimal BigVolume
        {
            get => _bot.BigVolume;
            set
            {
                _bot.BigVolume = value;
                OnPropertyChanged(nameof(BigVolume));
            }
        }

        public int Offset
        {
            get => _bot.Offset;
            set
            {
                _bot.Offset = value;
                OnPropertyChanged(nameof(Offset));
            }
        }

        public int Take
        {
            get => _bot.Take;
            set
            {
                _bot.Take = value;
                OnPropertyChanged(nameof(Take));
            }
        }

        public decimal Lot
        {
            get => _bot.Lot;
            set
            {
                _bot.Lot = value;
                OnPropertyChanged(nameof(Lot));
            }
        }

        public string SymbolName
        {
            get => _symbolName;
            set
            {
                _symbolName = value;
                OnPropertyChanged(nameof(SymbolName));
            }
        }
        private string _symbolName = String.Empty;

        public PositionStateType PositionState
        {
            get => _positionState;
            set
            {
                _positionState = value;
                OnPropertyChanged(nameof(PositionState));
            }
        }
        private PositionStateType _positionState = PositionStateType.None;

        public decimal AmountOfOpenLots
        {
            get => _amountOfOpenLots;
            set
            {
                _amountOfOpenLots = value;
                OnPropertyChanged(nameof(AmountOfOpenLots));
            }
        }
        private decimal _amountOfOpenLots;

        public decimal PositionOpenPrice
        {
            get => _positionOpenPrice;
            set
            {
                _positionOpenPrice = value;
                OnPropertyChanged(nameof(PositionOpenPrice));
            }
        }
        private decimal _positionOpenPrice;

        public decimal TakeProfitPrice
        {
            get => _takeProfitPrice;
            set
            {
                _takeProfitPrice = value;
                OnPropertyChanged(nameof(TakeProfitPrice));
            }
        }
        private decimal _takeProfitPrice;

        public decimal Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                OnPropertyChanged(nameof(Margin));
            }
        }
        private decimal _margin;

        public decimal PnL
        {
            get => _pnl;
            set
            {
                _pnl = value;
                OnPropertyChanged(nameof(PnL));
            }
        }
        private decimal _pnl;

        public Edit Edit
        {
            get => _bot.Edit;
            set
            {
                _bot.Edit = value;
                OnPropertyChanged(nameof(Edit));
            }
        }
        #endregion

        #region Commands =======================================
        private DelegateCommand commandStart;

        public ICommand CommandStart
        {
            get
            {
                if (commandStart == null)
                {
                    commandStart = new DelegateCommand(Start);
                }
                return commandStart;
            }
        }
        #endregion

        #region Methods ========================================
        private void Start(object obj)
        {
            if (Edit == Edit.Start)
            {
                Edit = Edit.Stop;
            }
            else
            {
                Edit = Edit.Start;
            }
        }

        private void _bot_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Margin")
            {
                this.Margin = _bot.Margin;
            }

            if (e.PropertyName == "SymbolName")
            {
                this.SymbolName = _bot.SymbolName;
            }

            if (e.PropertyName == "AmountOfOpenLots")
            {
                this.AmountOfOpenLots = _bot.AmountOfOpenLots;
            }

            if (e.PropertyName == "PnL")
            {
                this.PnL = _bot.PnL;
            }

            if(e.PropertyName == "TakeProfitPrice")
            {
                this.TakeProfitPrice = _bot.TakeProfitPrice;
            }

            if(e.PropertyName == "PositionOpenPrice")
            {
                this.PositionOpenPrice = _bot.PositionOpenPrice;
            }

            if(e.PropertyName == "CurrentPositionState")
            {
                this.PositionState = _bot.CurrentPositionState;
            }
        }
        #endregion

        protected void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    public enum Edit
    {
        Start,
        Stop
    }
}
