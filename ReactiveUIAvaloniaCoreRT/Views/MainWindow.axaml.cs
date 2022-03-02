using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUIAvaloniaCoreRT.Models;
using System.Reactive.Disposables;

namespace ReactiveUIAvaloniaCoreRT.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        private RoutedViewHost RoutedViewHost => this.FindControl<RoutedViewHost>("RoutedViewHost");
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            ViewModel = new MainWindowViewModel();
            this.WhenActivated(disposables =>
            {
                // Bind the view model router to RoutedViewHost.Router property.
                this.OneWayBind(ViewModel, x => x.Router, x => x.RoutedViewHost.Router)
                    .DisposeWith(disposables);

            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
          
        }


    }
}
