using System.Linq;
using System.Windows.Controls;
using MapleStory_HealthKeeper.ViewModels;

namespace MapleStory_HealthKeeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SharedFunctions.InitJsonSerializerOptions();
            SharedFunctions.LoadJsonFile("MapleStory-HealthKeeper.json", out MainWindowViewModel? mainWindowViewModel);
            if (mainWindowViewModel is not null)
            {
                mainWindowViewModel.Slogan = (double)new Random().NextDouble() switch
                {
                    double a when a < 0.2 => "有病就要看醫生，不要亂吃藥",
                    double a when a < 0.4 => "不用再吃藥了，已經沒藥醫了",
                    double a when a < 0.6 => "每天都攝取白開水2000cc哦",
                    double a when a < 0.8 => "浪費醫療資源是可恥的，住手吧！",
                    _ => "多喝水沒事，沒事多喝水",
                };
                DataContext = mainWindowViewModel;
            }
            if (DataContext is MainWindowViewModel viewModel)
            {
                MainWindowViewModel = viewModel;
                HealthKeeperService healthKeeperService = new(viewModel);
                healthKeeperService.Start();
            }
            else
                throw new Exception("DataContext not set");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) => SharedFunctions.TextBox_PreviewKeyDown((TextBox)sender, ref e);

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e) => SharedFunctions.NumberValidationTextBox(sender, ref e);

        private MainWindowViewModel MainWindowViewModel;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SharedFunctions.SaveJsonFile(DataContext, "MapleStory-HealthKeeper.json");
        }
    }
}