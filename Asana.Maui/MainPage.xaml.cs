using Asana.Library.Services;
using Asana.Maui.ViewModels;
using Asana.Maui.Views;

namespace Asana.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPageViewModel ViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainPageViewModel();
            BindingContext = ViewModel;
        }

        private void ContentPage_NavigatedTo(object sender, NavigatedToEventArgs e)
        {
            // check if user is logged in
            var currentUser = UserServiceProxy.Current.CurrentUser;
            if (currentUser == null)
            {
                // redirect to login page if no user is logged in
                Shell.Current.GoToAsync("//LoginPage");
                return;
            }
            
            ViewModel.RefreshToDos();
        }


        private async void AddNewClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ToDoDetailView));
        }

        private async void EditClicked(object sender, EventArgs e)
        {
            if (ViewModel.SelectedToDo?.Model?.Id > 0)
            {
                await Shell.Current.GoToAsync($"{nameof(ToDoDetailView)}?ToDoId={ViewModel.SelectedToDo.Model.Id}");
            }
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            if (ViewModel.SelectedToDo?.Model != null)
            {
                ViewModel.DeleteToDo(ViewModel.SelectedToDo.Model);
            }
        }

        private void InLineDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ToDoViewModel toDoViewModel)
            {
                ViewModel.DeleteToDo(toDoViewModel.Model);
            }
        }

        private async void ProjectClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ProjectsPage));
        }
    }
}