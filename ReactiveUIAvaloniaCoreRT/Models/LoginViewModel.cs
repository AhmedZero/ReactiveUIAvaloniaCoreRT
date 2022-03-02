using Avalonia.Styling;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ReactiveUIAvaloniaCoreRT.Models
{
    public class LoginViewModel : ReactiveObject, IRoutableViewModel
    {
        public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);

        public string? Key { get => key; set => key = this.RaiseAndSetIfChanged(ref key, value); }

        public bool SaveKey { get; set; } = false;

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> BuyCommand { get; }

        public IScreen HostScreen { get; }
        private string _error = string.Empty;
        private string? key;

        public LoginViewModel(IScreen screen)
        {

            HostScreen = screen;
            this.LoginCommand = ReactiveCommand.Create(this.login);
            this.BuyCommand = ReactiveCommand.Create(
    () =>
    {
        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
      .GetMessageBoxStandardWindow(new MessageBoxStandardParams
      {
          ButtonDefinitions = ButtonEnum.Ok,
          ContentTitle = "VIP",
          ContentMessage = "Test Test",
          Icon = Icon.Plus,
      });
        messageBoxStandardWindow.Show();
    }
);
        
        }
        public string ErrorMessage
        {
            get => _error;
            set => this.RaiseAndSetIfChanged(ref _error, value);
        }
        private async void login()
        {
           
        }
    }
}
