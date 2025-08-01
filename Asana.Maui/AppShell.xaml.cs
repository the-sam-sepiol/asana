using Asana.Maui.Views;

namespace Asana.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes for navigation
            Routing.RegisterRoute(nameof(ToDoDetailView), typeof(ToDoDetailView));
            Routing.RegisterRoute(nameof(ProjectsPage), typeof(ProjectsPage));
            Routing.RegisterRoute(nameof(ProjectDetailView), typeof(ProjectDetailView));
        }
    }
}