using OsEngine.Robots.MyRobots.Models;
using System.ComponentModel;

namespace OsEngine.Robots.MyRobots.ViewModels
{
    public class MyCrazyRobotViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MyCrazyRobot _crazyRobot;
                      
        public MyCrazyRobotViewModel(MyCrazyRobot crazyRobot)
        {
            _crazyRobot = crazyRobot;
        }

        public string Mode
        {
            get => _crazyRobot.ModeParameter.ValueString;
            set
            {
                if (value != _crazyRobot.ModeParameter.ValueString)
                {
                    _crazyRobot.ModeParameter.ValueString = value;
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }

        public int Lot
        {
            get => _crazyRobot.LotParameter.ValueInt;
            set
            {
                if (value != _crazyRobot.LotParameter.ValueInt)
                {
                    _crazyRobot.LotParameter.ValueInt = value;
                    OnPropertyChanged(nameof(Lot));
                }
            }
        }

        public int Stop
        {
            get => _crazyRobot.StopParameter.ValueInt;
            set
            {
                if (value != _crazyRobot.StopParameter.ValueInt)
                {
                    _crazyRobot.StopParameter.ValueInt = value;
                    OnPropertyChanged(nameof(Stop));
                }
            }
        }

        public int Take
        {
            get => _crazyRobot.TakeParameter.ValueInt;
            set
            {
                if (value != _crazyRobot.TakeParameter.ValueInt)
                {
                    if (value < _crazyRobot.TakeParameter.ValueIntStart)
                    {
                        value = _crazyRobot.TakeParameter.ValueIntStart;
                    }
                    else if (value > _crazyRobot.TakeParameter.ValueIntStop)
                    {
                        value = _crazyRobot.TakeParameter.ValueIntStop;
                    }

                    _crazyRobot.TakeParameter.ValueInt = value;
                    OnPropertyChanged(nameof(Take));
                }
            }
        }

        protected void OnPropertyChanged(string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
