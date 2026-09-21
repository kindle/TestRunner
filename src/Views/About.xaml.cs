using System.Reflection;
using System.Windows;

namespace TestRunner.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();

            this.Version.Text = assembly.GetName().Version.ToString();
        }
    }
}
