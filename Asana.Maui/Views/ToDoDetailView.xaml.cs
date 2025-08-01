using Asana.Library.Models;
using Asana.Maui.ViewModels;

namespace Asana.Maui.Views
{
    [QueryProperty(nameof(ToDoId), "ToDoId")]
    public partial class ToDoDetailView : ContentPage
    {
        public ToDoDetailViewModel ViewModel { get; set; }

        private int _toDoId;
        public int ToDoId
        {
            get => _toDoId;
            set
            {
                _toDoId = value;
                if (value > 0)
                {
                    ViewModel = new ToDoDetailViewModel(value);
                }
                else
                {
                    ViewModel = new ToDoDetailViewModel();
                }
                BindingContext = ViewModel;
            }
        }

        public ToDoDetailView()
        {
            InitializeComponent();
            ViewModel = new ToDoDetailViewModel();
            BindingContext = ViewModel;
        }

        private async void OkClicked(object sender, EventArgs e)
        {
            try
            {
                ViewModel.AddOrUpdateToDo();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save ToDo: {ex.Message}", "OK");
            }
        }

        private async void CancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}