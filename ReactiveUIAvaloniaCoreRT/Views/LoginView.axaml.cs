using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ReactiveUIAvaloniaCoreRT.Models;

namespace ReactiveUIAvaloniaCoreRT.Views
{
    public partial class LoginView : ReactiveUserControl<LoginViewModel>
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
