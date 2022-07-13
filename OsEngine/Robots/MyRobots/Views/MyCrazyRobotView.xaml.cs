using OsEngine.Robots.MyRobots.Models;
using OsEngine.Robots.MyRobots.ViewModels;
using System.Windows;

namespace OsEngine.Robots.MyRobots.Views
{
    /// <summary>
    /// Interaction logic for MyCrazyRobotView.xaml
    /// </summary>
    public partial class MyCrazyRobotView : Window
    {
        private MyCrazyRobotViewModel _myCrazyRobotVM;

        public MyCrazyRobotView(MyCrazyRobot myCrazyRobot)
        {
            InitializeComponent();
            _myCrazyRobotVM = new MyCrazyRobotViewModel(myCrazyRobot);
            DataContext = _myCrazyRobotVM;
        }
    }
}
