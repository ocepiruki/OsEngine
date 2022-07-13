using OsEngine.Robots.FrontRunner.Models;
using OsEngine.Robots.FrontRunner.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OsEngine.Robots.FrontRunner.Views
{
    /// <summary>
    /// Interaction logic for FrontRunnerBotUI.xaml
    /// </summary>
    public partial class FrontRunnerBotUI : Window
    {
        private FrontRunnerBotVM _frontRunnerBotVM;
        public FrontRunnerBotUI(FrontRunnerBot frontRunnerBot)
        {
            InitializeComponent();
            _frontRunnerBotVM = new FrontRunnerBotVM(frontRunnerBot);
            DataContext = _frontRunnerBotVM;
        }
    }
}
