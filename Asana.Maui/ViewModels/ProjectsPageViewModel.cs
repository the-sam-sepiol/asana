using Asana.Library.Models;
using Asana.Library.Services;
using Asana.Maui.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Asana.Maui.ViewModels
{
    public enum ProjectSortOption
    {
        Name,
        TodoCount,
        CompletionPercent
    }

    public class ProjectsPageViewModel : INotifyPropertyChanged
    {
        private ProjectViewModel? _selectedProject;
        private string _searchText = string.Empty;
        private ObservableCollection<ProjectViewModel> _allProjects = new ObservableCollection<ProjectViewModel>();
        private ProjectSortOption _selectedSortOption = ProjectSortOption.Name;
        private bool _isSortDescending = false;

        public ProjectsPageViewModel()
        {
            ClearSearchCommand = new Command(() => SearchText = string.Empty);
            SortCommand = new Command<string>(SortProjects);
            ToggleSortDirectionCommand = new Command(() => 
            {
                IsSortDescending = !IsSortDescending;
                ApplySortingAndFiltering();
            });
            ExportDataCommand = new Command(async () => await ExportDataAsync());
            ImportDataCommand = new Command(async () => await ImportDataAsync());
            RefreshProjects();
        }

        public ObservableCollection<ProjectViewModel> Projects { get; set; } = new ObservableCollection<ProjectViewModel>();

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                NotifyPropertyChanged();
                ApplySortingAndFiltering();
            }
        }

        public ICommand ClearSearchCommand { get; set; }
        public ICommand SortCommand { get; set; }
        public ICommand ToggleSortDirectionCommand { get; set; }
        public ICommand ExportDataCommand { get; set; }
        public ICommand ImportDataCommand { get; set; }

        public ProjectSortOption SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                NotifyPropertyChanged();
                ApplySortingAndFiltering();
            }
        }

        public bool IsSortDescending
        {
            get => _isSortDescending;
            set
            {
                _isSortDescending = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(SortDirectionText));
            }
        }

        public string SortDirectionText => IsSortDescending ? "DESC" : "ASC";

        public List<string> SortOptions => new List<string> { "Name", "TodoCount", "CompletionPercent" };

        public ProjectViewModel? SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
                NotifyPropertyChanged();
            }
        }

        public void RefreshProjects()
        {
            _allProjects.Clear();
            var projects = ProjectServiceProxy.Current.Projects
                .Select(p => new ProjectViewModel { Model = p });

            foreach (var project in projects)
            {
                _allProjects.Add(project);
            }
            
            ApplySortingAndFiltering();
            
            // Notify that project todos may have changed
            foreach (var project in _allProjects)
            {
                project.NotifyPropertyChanged(nameof(project.ProjectToDos));
                project.NotifyPropertyChanged(nameof(project.ToDoCount));
                project.NotifyPropertyChanged(nameof(project.ToDoListHeight));
                project.NotifyPropertyChanged(nameof(project.CompletionPercent));
            }
        }

        private void SortProjects(string sortOption)
        {
            if (Enum.TryParse<ProjectSortOption>(sortOption, out var option))
            {
                SelectedSortOption = option;
            }
        }

        private void ApplySortingAndFiltering()
        {
            FilterProjects();
        }

        private void FilterProjects()
        {
            IEnumerable<ProjectViewModel> filteredProjects = _allProjects;
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredProjects = filteredProjects.Where(project =>
                    project.Model?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    project.Model?.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true
                );
            }

            // Apply sorting
            filteredProjects = SelectedSortOption switch
            {
                ProjectSortOption.Name => IsSortDescending 
                    ? filteredProjects.OrderByDescending(p => p.Model?.Name) 
                    : filteredProjects.OrderBy(p => p.Model?.Name),
                ProjectSortOption.TodoCount => IsSortDescending 
                    ? filteredProjects.OrderByDescending(p => GetTodoCount(p.Model)) 
                    : filteredProjects.OrderBy(p => GetTodoCount(p.Model)),
                ProjectSortOption.CompletionPercent => IsSortDescending 
                    ? filteredProjects.OrderByDescending(p => p.Model?.CompletionPercent) 
                    : filteredProjects.OrderBy(p => p.Model?.CompletionPercent),
                _ => filteredProjects
            };

            Projects.Clear();
            foreach (var project in filteredProjects)
            {
                Projects.Add(project);
            }
        }

        private int GetTodoCount(Project? project)
        {
            if (project?.Id == null) return 0;
            return ToDoServiceProxy.Current.ToDos.Count(t => t.ProjectId == project.Id);
        }

        public void DeleteProject(Project? project)
        {
            if (project != null)
            {
                ProjectServiceProxy.Current.DeleteProject(project);
                RefreshProjects();
            }
        }

        private async Task ExportDataAsync()
        {
            try
            {
                var exportContent = await ExportImportService.ExportToTextAsync();
                var filePath = await ExportImportService.GetExportFilePathAsync();
                await ExportImportService.SaveToFileAsync(exportContent, filePath);
                
                await Application.Current?.MainPage?.DisplayAlert("Export Successful", 
                    $"Data exported to:\n{filePath}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Export Failed", 
                    $"Error: {ex.Message}", "OK");
            }
        }

        private async Task ImportDataAsync()
        {
            try
            {
                var filePath = await Application.Current?.MainPage?.DisplayPromptAsync("Import File", 
                    "Enter the full path to the export file:");

                if (string.IsNullOrWhiteSpace(filePath)) return;

                if (!File.Exists(filePath))
                {
                    await Application.Current?.MainPage?.DisplayAlert("Import Failed", 
                        "File not found.", "OK");
                    return;
                }

                var content = await ExportImportService.LoadFromFileAsync(filePath);
                var success = await ExportImportService.ImportFromTextAsync(content);

                if (success)
                {
                    RefreshProjects();
                    await Application.Current?.MainPage?.DisplayAlert("Import Successful", 
                        "Data imported!", "OK");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Import Failed", 
                        "Failed to parse file.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current?.MainPage?.DisplayAlert("Import Failed", 
                    $"Error: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}