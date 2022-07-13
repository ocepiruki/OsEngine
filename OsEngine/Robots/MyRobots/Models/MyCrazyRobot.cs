using OsEngine.Entity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Robots.MyRobots.Views;

namespace OsEngine.Robots.MyRobots.Models
{
    public class MyCrazyRobot : BotPanel
    {
        public StrategyParameterString ModeParameter { get; set; }
        public StrategyParameterInt LotParameter { get; set; }
        public StrategyParameterInt StopParameter { get; set; }
        public StrategyParameterInt TakeParameter { get; set; }

        public MyCrazyRobot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple); //Создаем вкладку. Можно создать несколько вкладок. Каждая вкладка предназначена для отдельной связки Инструмент-Таймфрейм.
            //TabCreate(BotTabType.Simple); //Например, мы можем торговать нашу стратегию на таком же инструменте как и на первой вкладке, но на другом таймфрейме.

            _tab1 = TabsSimple[0];//получаем первую вкладку в коллекции созданных нами вкладок
            //_tab2 = TabsSimple[1];

            ModeParameter = CreateParameter("Mode", "Edit", new[] {"Edit", "Trade"});//существует несколько перегрузок параметров
            LotParameter = CreateParameter("Lot", 1, 1, 100, 1);
            StopParameter = CreateParameter("Stop", 3, 1, 100, 1);
            TakeParameter = CreateParameter("Take", 3, 1, 100, 1);
        }

        #region Fields =================================
        private BotTabSimple _tab1;
        //private BotTabSimple _tab2;
        #endregion


        public override string GetNameStrategyType()
        {
            return nameof(MyCrazyRobot);
        }

        public override void ShowIndividualSettingsDialog() //настройки торговли бота
        {
            MyCrazyRobotView view = new MyCrazyRobotView(this);
            view.ShowDialog();
        }
    }
}
