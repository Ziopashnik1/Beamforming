using System.Windows.Input;
using Microsoft.Win32;
using OxyPlot.Wpf;

namespace BeamForming
{

    public partial class MainWindow
    {
        public MainWindow() { InitializeComponent(); }

        private void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }

        private void OnPlotMouseUp(object Sender, MouseButtonEventArgs E)
        {
            var plot = (Plot) Sender;
            var dialog = new SaveFileDialog
            {
                Title = plot.Title,
                Filter = "Файлы PNG (*.png)|*.png|Все файлы (*.*)|*.*"
            };
            if(dialog.ShowDialog() != true) return;
            plot.SaveBitmap(dialog.FileName);
        }
    }
}
