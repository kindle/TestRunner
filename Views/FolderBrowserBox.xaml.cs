
namespace TestViewer.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    /// <summary>
    /// 1. Dependency property FolderName supports data binding
    /// 2. Routed event FolderNameChanged supports triggers
    /// 3. Content property equals FolderName property
    /// </summary>
    [ContentProperty("FolderName")]
    public partial class FolderBrowserBox : UserControl
    {
        public static DependencyProperty FolderNameProperty =
            DependencyProperty.Register("FolderName", typeof(string), typeof(FolderBrowserBox));

        public static RoutedEvent FolderNameChangedEvent =
            EventManager.RegisterRoutedEvent("FolderNameChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FolderBrowserBox));

        public string FolderName
        {
            get { return (string)GetValue(FolderNameProperty); }
            set { SetValue(FolderNameProperty, value); }
        }

        public event RoutedEventHandler FolderNameChanged
        {
            add { AddHandler(FolderNameChangedEvent, value); }
            remove { RemoveHandler(FolderNameChangedEvent, value); }
        }

        public FolderBrowserBox()
        {
            this.InitializeComponent();
            this.theTextBox.AddHandler(TextBox.TextChangedEvent, new RoutedEventHandler(theTextBoxTextChanged), true);
        }

        void theTextBoxTextChanged(object sender, RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(FolderNameChangedEvent));
        }

        private void theButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Interop.HwndSource source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            System.Windows.Forms.IWin32Window win = new OldWindow(source.Handle);
            System.Windows.Forms.DialogResult result = dlg.ShowDialog(win);

            FolderName = dlg.SelectedPath;
        }
    }
}

