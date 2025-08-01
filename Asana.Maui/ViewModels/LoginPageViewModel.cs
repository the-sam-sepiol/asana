using Asana.Library.Services;
using Asana.Library.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Asana.Maui.ViewModels
{
    public class LoginPageViewModel : INotifyPropertyChanged
    {
        private string _userName = string.Empty;
        private string _managerUsername = string.Empty;
        private string _managerPassword = string.Empty;
        private string _newManagerUsername = string.Empty;
        private string _newManagerPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isManagerLoginVisible = false;
        private bool _isManagerSignupVisible = false;

        public LoginPageViewModel()
        {
            LoginUserCommand = new Command(async () => await LoginUser());
            ShowManagerLoginCommand = new Command(() => ShowManagerLogin());
            LoginManagerCommand = new Command(async () => await LoginManager());
            CancelManagerLoginCommand = new Command(() => CancelManagerLogin());
            CreateManagerCommand = new Command(async () => await CreateManager());

            // check if there's already a manager account
            var existingManager = UserServiceProxy.Current.Users.FirstOrDefault(u => u.IsManager);
            if (existingManager == null)
            {
                // no manager exists, show signup form
                IsManagerSignupVisible = true;
            }
            else
            {
                // manager exists, hide signup form
                IsManagerSignupVisible = false;
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                NotifyPropertyChanged();
                ClearError();
            }
        }

        public string ManagerUsername
        {
            get => _managerUsername;
            set
            {
                _managerUsername = value;
                NotifyPropertyChanged();
                ClearError();
            }
        }

        public string ManagerPassword
        {
            get => _managerPassword;
            set
            {
                _managerPassword = value;
                NotifyPropertyChanged();
                ClearError();
            }
        }

        public string NewManagerUsername
        {
            get => _newManagerUsername;
            set
            {
                _newManagerUsername = value;
                NotifyPropertyChanged();
                ClearError();
            }
        }

        public string NewManagerPassword
        {
            get => _newManagerPassword;
            set
            {
                _newManagerPassword = value;
                NotifyPropertyChanged();
                ClearError();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsManagerLoginVisible
        {
            get => _isManagerLoginVisible;
            set
            {
                _isManagerLoginVisible = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsManagerSignupVisible
        {
            get => _isManagerSignupVisible;
            set
            {
                _isManagerSignupVisible = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ManagerButtonText));
            }
        }

        public string ManagerButtonText
        {
            get
            {
                var existingManager = UserServiceProxy.Current.Users.FirstOrDefault(u => u.IsManager);
                return existingManager == null ? "Create Manager Account" : "Manager Login";
            }
        }

        public ICommand LoginUserCommand { get; }
        public ICommand ShowManagerLoginCommand { get; }
        public ICommand LoginManagerCommand { get; }
        public ICommand CancelManagerLoginCommand { get; }
        public ICommand CreateManagerCommand { get; }

        private async Task LoginUser()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                ErrorMessage = "Please enter your name";
                return;
            }

            var success = UserServiceProxy.Current.LoginUser(UserName);
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "User not found. Please check with your manager.";
            }
        }

        private void ShowManagerLogin()
        {
            // check if manager exists
            var existingManager = UserServiceProxy.Current.Users.FirstOrDefault(u => u.IsManager);
            if (existingManager == null)
            {
                // no manager exists, show signup instead
                IsManagerSignupVisible = true;
                IsManagerLoginVisible = false;
            }
            else
            {
                // manager exists, show login
                IsManagerLoginVisible = true;
                IsManagerSignupVisible = false;
            }
            ClearError();
        }

        private async Task LoginManager()
        {
            if (string.IsNullOrWhiteSpace(ManagerUsername) || string.IsNullOrWhiteSpace(ManagerPassword))
            {
                ErrorMessage = "Please enter username and password";
                return;
            }

            var success = UserServiceProxy.Current.LoginManager(ManagerUsername, ManagerPassword);
            if (success)
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorMessage = "Invalid manager credentials";
            }
        }

        private void CancelManagerLogin()
        {
            IsManagerLoginVisible = false;
            ManagerUsername = string.Empty;
            ManagerPassword = string.Empty;
            ClearError();
        }

        private async Task CreateManager()
        {
            if (string.IsNullOrWhiteSpace(NewManagerUsername) || string.IsNullOrWhiteSpace(NewManagerPassword))
            {
                ErrorMessage = "Please enter username and password";
                return;
            }

            var newManager = new User
            {
                Name = "Manager",
                IsManager = true,
                ManagerUsername = NewManagerUsername,
                ManagerPassword = NewManagerPassword
            };

            UserServiceProxy.Current.AddOrUpdate(newManager);
            UserServiceProxy.Current.CurrentUser = newManager;

            // clear the form
            NewManagerUsername = string.Empty;
            NewManagerPassword = string.Empty;
            IsManagerSignupVisible = false;

            await Shell.Current.GoToAsync("//MainPage");
        }

        private void ClearError()
        {
            ErrorMessage = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
