using Asana.Maui.ViewModels;

namespace Asana.Maui.Views
{
    public partial class ProjectsPage : ContentPage
    {
        public ProjectsPageViewModel ViewModel { get; set; }

        public ProjectsPage()
        {
            InitializeComponent();
            ViewModel = new ProjectsPageViewModel();
            BindingContext = ViewModel;
        }

        private void ContentPage_NavigatedTo(object sender, NavigatedToEventArgs e)
        {
            ViewModel.RefreshProjects();
        }



        private async void AddNewClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ProjectDetailView));
        }

        private async void EditClicked(object sender, EventArgs e)
        {
            if (ViewModel.SelectedProject?.Model?.Id > 0)
            {
                await Shell.Current.GoToAsync($"{nameof(ProjectDetailView)}?ProjectId={ViewModel.SelectedProject.Model.Id}");
            }
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            if (ViewModel.SelectedProject?.Model != null)
            {
                ViewModel.DeleteProject(ViewModel.SelectedProject.Model);
            }
        }

        private void InLineDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ProjectViewModel projectViewModel)
            {
                ViewModel.DeleteProject(projectViewModel.Model);
            }
        }

        private async void BackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}