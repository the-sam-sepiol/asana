using Asana.Library.Models;
using Asana.Library.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asana.Maui.ViewModels
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private User _model = new User();

        public User Model
        {
            get => _model;
            set
            {
                _model = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Name));
                NotifyPropertyChanged(nameof(UserType));
                NotifyPropertyChanged(nameof(CanDelete));
                NotifyPropertyChanged(nameof(AssignedTasksCount));
            }
        }

        public string? Name => Model.Name;
        public string UserType => Model.IsManager ? "Manager" : "User";
        public bool CanDelete => !Model.IsManager;

        public int AssignedTasksCount
        {
            get
            {
                if (Model.IsManager)
                {
                    return ToDoServiceProxy.Current.GetAllToDos()
                        .Count(t => t.AssignedUserId == Model.Id || t.AssignedUserId == null);
                }
                else
                {
                    return ToDoServiceProxy.Current.GetAllToDos()
                        .Count(t => t.AssignedUserId == Model.Id);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
