using ReactiveUI;


namespace ReactiveUIAvaloniaCoreRT.Models
{
    public class MainWindowViewModel : ReactiveObject, IScreen
    {
        public RoutingState Router { get; } = new RoutingState();
        public MainWindowViewModel()
        {
            Router.Navigate.Execute(new LoginViewModel(this));
        }
    }
}
