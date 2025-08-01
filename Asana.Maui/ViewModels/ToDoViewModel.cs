using Asana.Library.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Asana.Maui.ViewModels
{
    public class ToDoViewModel : INotifyPropertyChanged
    {
        public ToDo? Model { get; set; }

        public string DisplayText => Model?.ToString() ?? "Unknown ToDo";

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