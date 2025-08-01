using Asana.Library.Models;
using Asana.Library.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asana.Maui.ViewModels
{
    public class ToDoDisplayItem
    {
        public string Name { get; set; } = string.Empty;
        public string CompletionIcon { get; set; } = string.Empty;
        public Color TextColor { get; set; } = Colors.Black;
        public TextDecorations TextDecoration { get; set; } = TextDecorations.None;
        public string Priority { get; set; } = string.Empty;
    }

    public class ProjectViewModel : INotifyPropertyChanged
    {
        private Project? _model;
        public Project? Model 
        { 
            get => _model;
            set 
            {
                if (_model != value)
                {
                    _model = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(DisplayText));
                    NotifyPropertyChanged(nameof(ToDoCount));
                    NotifyPropertyChanged(nameof(CompletionPercent));
                    NotifyPropertyChanged(nameof(ProjectToDos));
                    NotifyPropertyChanged(nameof(ToDoListHeight));
                    NotifyPropertyChanged(nameof(ToDosList));
                }
            }
        }

        public string DisplayText => $"{Model?.Id ?? -1}. {Model?.Name}";

        public int CompletionPercent => Model?.CompletionPercent ?? 0;

        public void RefreshFromService()
        {
            if (Model?.Id != null && Model.Id > 0)
            {
                var updatedProject = ProjectServiceProxy.Current.GetById(Model.Id);
                if (updatedProject != null)
                {
                    Model = updatedProject;
                }
            }
        }

        public int ToDoCount
        {
            get
            {
                if (Model?.Id == null) return 0;
                return ToDoServiceProxy.Current.ToDos.Count(t => t.ProjectId == Model.Id);
            }
        }

        public ObservableCollection<ToDoDisplayItem> ProjectToDos
        {
            get
            {
                var todos = new ObservableCollection<ToDoDisplayItem>();
                if (Model?.Id == null) return todos;

                var projectTodos = ToDoServiceProxy.Current.ToDos
                    .Where(t => t.ProjectId == Model.Id)
                    .OrderBy(t => t.IsCompleted)
                    .ThenBy(t => t.Name);

                foreach (var todo in projectTodos)
                {
                    todos.Add(new ToDoDisplayItem
                    {
                        Name = todo.Name ?? "Unnamed Todo",
                        CompletionIcon = todo.IsCompleted == true ? "✅" : "⭕",
                        TextColor = todo.IsCompleted == true ? Colors.Gray : Colors.Black,
                        TextDecoration = todo.IsCompleted == true ? TextDecorations.Strikethrough : TextDecorations.None,
                        Priority = todo.Priority?.ToString() ?? "None"
                    });
                }

                return todos;
            }
        }

        public double ToDoListHeight
        {
            get
            {
                var count = ToDoCount;
                if (count == 0) return 30; // Height for "No todos" message
                return Math.Min(count * 35, 200); // Max height of 200 with scrolling
            }
        }

        public string ToDosList
        {
            get
            {
                if (Model?.ToDos == null || !Model.ToDos.Any())
                    return "No ToDos assigned";
                
                var todoNames = Model.ToDos.Take(3).Select(t => $"• {t.Name}").ToList();
                var result = string.Join("\n", todoNames);
                
                if (Model.ToDos.Count > 3)
                    result += $"\n... and {Model.ToDos.Count - 3} more";
                
                return result;
            }
        }

        public override string ToString()
        {
            return DisplayText;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}