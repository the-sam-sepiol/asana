using Asana.Library.Models;
using Asana.Maui.ViewModels;

namespace Asana.Maui.Views
{
    [QueryProperty(nameof(ProjectId), "ProjectId")]
    public partial class ProjectDetailView : ContentPage
    {
        public ProjectDetailViewModel ViewModel { get; set; }

        private int _projectId;
        public int ProjectId
        {
            get => _projectId;
            set
            {
                _projectId = value;
                if (value > 0)
                {
                    ViewModel = new ProjectDetailViewModel(value);
                }
                else
                {
                    ViewModel = new ProjectDetailViewModel();
                }
                BindingContext = ViewModel;
            }
        }

        public ProjectDetailView()
        {
            InitializeComponent();
            ViewModel = new ProjectDetailViewModel();
            BindingContext = ViewModel;
        }


        private async void OkClicked(object sender, EventArgs e)
        {
            try
            {
                ViewModel.AddOrUpdateProject();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save Project: {ex.Message}", "OK");
            }
        }

        private async void CancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}