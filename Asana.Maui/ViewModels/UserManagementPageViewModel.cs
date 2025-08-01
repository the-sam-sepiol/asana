using Asana.Library.Services;
using Asana.Library.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Asana.Maui.ViewModels
{
    public class UserManagementPageViewModel : INotifyPropertyChanged
    {
        private string _newUserName = string.Empty;
        private string _message = string.Empty;
        private Color _messageColor = Colors.Black;

        public UserManagementPageViewModel()
        {
            AddUserCommand = new Command(() => AddUser());
            RefreshUsers();
        }

        public ObservableCollection<UserViewModel> Users { get; set; } = new ObservableCollection<UserViewModel>();

        public string NewUserName
        {
            get => _newUserName;
            set
            {
                _newUserName = value;
                NotifyPropertyChanged();
                ClearMessage();
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(HasMessage));
            }
        }

        public Color MessageColor
        {
            get => _messageColor;
            set
            {
                _messageColor = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasMessage => !string.IsNullOrEmpty(Message);

        public ICommand AddUserCommand { get; }

        private void AddUser()
        {
            if (string.IsNullOrWhiteSpace(NewUserName))
            {
                ShowErrorMessage("Please enter a user name");
                return;
            }

            // Check if user already exists
            var existingUser = UserServiceProxy.Current.GetByName(NewUserName);
            if (existingUser != null)
            {
                ShowErrorMessage("User with this name already exists");
                return;
            }

            var newUser = new User
            {
                Name = NewUserName,
                IsManager = false
            };

            UserServiceProxy.Current.AddOrUpdate(newUser);
            ShowSuccessMessage($"User '{NewUserName}' added successfully");
            NewUserName = string.Empty;
            RefreshUsers();
        }

        public void DeleteUser(User user)
        {
            if (user == null || user.IsManager)
                return;

            // Check if user has assigned tasks
            var assignedTasks = ToDoServiceProxy.Current.GetAllToDos()
                .Where(t => t.AssignedUserId == user.Id).ToList();

            if (assignedTasks.Any())
            {
                ShowErrorMessage($"Cannot delete user '{user.Name}' - they have {assignedTasks.Count} assigned task(s)");
                return;
            }

            UserServiceProxy.Current.DeleteUser(user);
            ShowSuccessMessage($"User '{user.Name}' deleted successfully");
            RefreshUsers();
        }

        private void RefreshUsers()
        {
            Users.Clear();
            var users = UserServiceProxy.Current.Users
                .Select(u => new UserViewModel { Model = u });

            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        private void ShowErrorMessage(string message)
        {
            Message = message;
            MessageColor = Colors.Red;
        }

        private void ShowSuccessMessage(string message)
        {
            Message = message;
            MessageColor = Colors.Green;
        }

        private void ClearMessage()
        {
            Message = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
