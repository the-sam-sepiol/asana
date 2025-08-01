using Asana.Library.Models;
using Asana.Library.Services;
using Asana.Maui.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Asana.Maui.ViewModels
{
    public enum ToDoSortOption
    {
        Name,
        Priority,
        DueDate,
        Project
    }

    public class ProjectGroupViewModel
    {
        public string ProjectName { get; set; } = string.Empty;
        public int ProjectId { get; set; }
        public ObservableCollection<ToDoViewModel> ToDos { get; set; } = new ObservableCollection<ToDoViewModel>();
        public int CompletionPercent { get; set; }
        public string TodoCountText => $"{ToDos.Count} todo(s)";
    }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private bool _isShowCompleted;
        private ToDoViewModel? _selectedToDo;
        private string _searchText = string.Empty;
        private ObservableCollection<ToDoViewModel> _allToDos = new ObservableCollection<ToDoViewModel>();
        private ToDoSortOption _selectedSortOption = ToDoSortOption.Name;
        private bool _isSortDescending = false;
        private bool _isGroupedView = false;

        public MainPageViewModel()
        {
            ClearSearchCommand = new Command(() => SearchText = string.Empty);
            SortCommand = new Command<string>(SortToDos);
            ToggleSortDirectionCommand = new Command(() => 
            {
                IsSortDescending = !IsSortDescending;
                ApplySortingAndFiltering();
            });
            ToggleGroupViewCommand = new Command(() => 
            {
                IsGroupedView = !IsGroupedView;
                ApplySortingAndFiltering();
            });
            ExportDataCommand = new Command(async () => await ExportDataAsync());
            ImportDataCommand = new Command(async () => await ImportDataAsync());
            RefreshToDos();
        }

        public ObservableCollection<ToDoViewModel> ToDos { get; set; } = new ObservableCollection<ToDoViewModel>();
        public ObservableCollection<ProjectGroupViewModel> GroupedToDos { get; set; } = new ObservableCollection<ProjectGroupViewModel>();

        public bool IsGroupedView
        {
            get => _isGroupedView;
            set
            {
                _isGroupedView = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(GroupViewButtonText));
            }
        }

        public string GroupViewButtonText => IsGroupedView ? "List View" : "Group View";

        public ICommand ToggleGroupViewCommand { get; set; }

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

        public ToDoSortOption SelectedSortOption
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

        public List<string> SortOptions => new List<string> { "Name", "Priority", "DueDate", "Project" };

        public bool IsShowCompleted
        {
            get => _isShowCompleted;
            set
            {
                _isShowCompleted = value;
                RefreshToDos();
                NotifyPropertyChanged();
            }
        }

        public ToDoViewModel? SelectedToDo
        {
            get => _selectedToDo;
            set
            {
                _selectedToDo = value;
                NotifyPropertyChanged();
            }
        }

        public void RefreshToDos()
        {
            _allToDos.Clear();
            var todos = ToDoServiceProxy.Current.ToDos
                .Where(t => IsShowCompleted || t.IsCompleted != true)
                .Select(t => new ToDoViewModel { Model = t });

            foreach (var todo in todos)
            {
                _allToDos.Add(todo);
            }
            
            ApplySortingAndFiltering();
        }

        private void SortToDos(string sortOption)
        {
            if (Enum.TryParse<ToDoSortOption>(sortOption, out var option))
            {
                SelectedSortOption = option;
            }
        }

        private void ApplySortingAndFiltering()
        {
            FilterToDos();
        }

        private void FilterToDos()
        {
            IEnumerable<ToDoViewModel> filteredToDos = _allToDos;
            
            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredToDos = filteredToDos.Where(todo =>
                    todo.Model?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    todo.Model?.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    todo.Model?.Project?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true
                );
            }

            // Apply sorting
            filteredToDos = SelectedSortOption switch
            {
                ToDoSortOption.Name => IsSortDescending 
                    ? filteredToDos.OrderByDescending(t => t.Model?.Name) 
                    : filteredToDos.OrderBy(t => t.Model?.Name),
                ToDoSortOption.Priority => IsSortDescending 
                    ? filteredToDos.OrderByDescending(t => t.Model?.Priority) 
                    : filteredToDos.OrderBy(t => t.Model?.Priority),
                ToDoSortOption.DueDate => IsSortDescending 
                    ? filteredToDos.OrderByDescending(t => t.Model?.DueDate) 
                    : filteredToDos.OrderBy(t => t.Model?.DueDate),
                ToDoSortOption.Project => IsSortDescending 
                    ? filteredToDos.OrderByDescending(t => t.Model?.Project?.Name) 
                    : filteredToDos.OrderBy(t => t.Model?.Project?.Name),
                _ => filteredToDos
            };

            if (IsGroupedView)
            {
                // Group by project
                PopulateGroupedView(filteredToDos);
            }
            else
            {
                // Regular list view
                ToDos.Clear();
                foreach (var todo in filteredToDos)
                {
                    ToDos.Add(todo);
                }
            }
        }

        private void PopulateGroupedView(IEnumerable<ToDoViewModel> filteredToDos)
        {
            GroupedToDos.Clear();
            
            var grouped = filteredToDos.GroupBy(t => 
            {
                var projectId = t.Model?.ProjectId ?? 0;
                string projectName = "No Project";
                
                if (projectId > 0)
                {
                    // Try to get project name from the Project object first
                    if (!string.IsNullOrEmpty(t.Model?.Project?.Name))
                    {
                        projectName = t.Model.Project.Name;
                    }
                    else
                    {
                        // If Project object is null, fetch from service
                        var project = ProjectServiceProxy.Current.GetById(projectId);
                        projectName = project?.Name ?? $"Project {projectId}";
                    }
                }
                
                return new { ProjectId = projectId, ProjectName = projectName };
            });

            foreach (var group in grouped.OrderBy(g => g.Key.ProjectName))
            {
                var projectGroup = new ProjectGroupViewModel
                {
                    ProjectId = group.Key.ProjectId,
                    ProjectName = group.Key.ProjectName
                };

                // Calculate completion percentage for this group
                var todoList = group.ToList();
                if (todoList.Any())
                {
                    var completedCount = todoList.Count(t => t.Model?.IsCompleted == true);
                    projectGroup.CompletionPercent = (int)Math.Round((completedCount * 100.0) / todoList.Count);
                }

                foreach (var todo in group.OrderBy(t => t.Model?.IsCompleted).ThenBy(t => t.Model?.Name))
                {
                    projectGroup.ToDos.Add(todo);
                }

                GroupedToDos.Add(projectGroup);
            }
        }

        public void DeleteToDo(ToDo? toDo)
        {
            if (toDo != null)
            {
                ToDoServiceProxy.Current.DeleteToDo(toDo);
                RefreshToDos();
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
                // For now, we'll use a simple input dialog to get file path
                // In a production app, you'd use a file picker
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
                    RefreshToDos();
                    await Application.Current?.MainPage?.DisplayAlert("Import Successful", 
                        "Data imported successfully!", "OK");
                }
                else
                {
                    await Application.Current?.MainPage?.DisplayAlert("Import Failed", 
                        "Failed to parse the import file.", "OK");
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