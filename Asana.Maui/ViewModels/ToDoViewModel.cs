using Asana.Library.Models;
using Asana.Library.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asana.Maui.ViewModels
{
    public class ToDoViewModel : INotifyPropertyChanged
    {
        public ToDo? Model { get; set; }

        public string DisplayText => Model?.ToString() ?? "Unknown ToDo";

        public bool IsManagerLoggedIn
        {
            get
            {
                var currentUser = UserServiceProxy.Current.CurrentUser;
                return currentUser?.IsManager == true;
            }
        }

        public override string ToString()
        {
            return DisplayText;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}