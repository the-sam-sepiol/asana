using Asana.Maui.ViewModels;

namespace Asana.Maui.Views;

public partial class UserManagementPage : ContentPage
{
    public UserManagementPage()
    {
        InitializeComponent();
        BindingContext = new UserManagementPageViewModel();
    }

    private void DeleteUserClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is UserViewModel userViewModel)
        {
            if (BindingContext is UserManagementPageViewModel viewModel)
            {
                viewModel.DeleteUser(userViewModel.Model);
            }
        }
    }

    private async void BackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
