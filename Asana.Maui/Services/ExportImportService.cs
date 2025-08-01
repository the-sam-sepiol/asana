using Asana.Library.Models;
using Asana.Library.Services;
using System.Text;

namespace Asana.Maui.Services
{
    public class ExportImportService
    {
        public static async Task<string> ExportToTextAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ASANA CLI EXPORT ===");
            sb.AppendLine($"Exported on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // export Users
            sb.AppendLine("=== USERS ===");
            var users = UserServiceProxy.Current.Users;
            foreach (var user in users)
            {
                sb.AppendLine($"USER_START");
                sb.AppendLine($"ID: {user.Id}");
                sb.AppendLine($"NAME: {user.Name}");
                sb.AppendLine($"IS_MANAGER: {user.IsManager}");
                sb.AppendLine($"MANAGER_USERNAME: {user.ManagerUsername ?? ""}");
                sb.AppendLine($"MANAGER_PASSWORD: {user.ManagerPassword ?? ""}");
                sb.AppendLine($"USER_END");
                sb.AppendLine();
            }

            // export Projects
            sb.AppendLine("=== PROJECTS ===");
            var projects = ProjectServiceProxy.Current.Projects;
            foreach (var project in projects)
            {
                sb.AppendLine($"PROJECT_START");
                sb.AppendLine($"ID: {project.Id}");
                sb.AppendLine($"NAME: {project.Name}");
                sb.AppendLine($"DESCRIPTION: {project.Description ?? ""}");
                sb.AppendLine($"COMPLETION: {project.CompletionPercent:F2}");
                sb.AppendLine($"PROJECT_END");
                sb.AppendLine();
            }

            // export ToDos (get all todos for export, not filtered by user)
            sb.AppendLine("=== TODOS ===");
            var todos = ToDoServiceProxy.Current.GetAllToDos();
            foreach (var todo in todos)
            {
                sb.AppendLine($"TODO_START");
                sb.AppendLine($"ID: {todo.Id}");
                sb.AppendLine($"NAME: {todo.Name}");
                sb.AppendLine($"DESCRIPTION: {todo.Description ?? ""}");
                sb.AppendLine($"PRIORITY: {todo.Priority}");
                sb.AppendLine($"DUE_DATE: {todo.DueDate?.ToString("yyyy-MM-dd") ?? ""}");
                sb.AppendLine($"IS_COMPLETED: {todo.IsCompleted}");
                sb.AppendLine($"PROJECT_ID: {todo.ProjectId ?? -1}");
                sb.AppendLine($"ASSIGNED_USER_ID: {todo.AssignedUserId ?? -1}");
                sb.AppendLine($"TODO_END");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static async Task<bool> ImportFromTextAsync(string content)
        {
            try
            {
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(line => line.Trim())
                                  .ToArray();

                var users = new List<User>();
                var projects = new List<Project>();
                var todos = new List<ToDo>();

                // parse Users, Projects, and ToDos
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "USER_START")
                    {
                        var user = ParseUser(lines, ref i);
                        if (user != null)
                        {
                            users.Add(user);
                        }
                    }
                    else if (lines[i] == "PROJECT_START")
                    {
                        var project = ParseProject(lines, ref i);
                        if (project != null)
                        {
                            projects.Add(project);
                        }
                    }
                    else if (lines[i] == "TODO_START")
                    {
                        var todo = ParseToDo(lines, ref i);
                        if (todo != null)
                        {
                            todos.Add(todo);
                        }
                    }
                }

                // clear existing data and import new data
                var userService = UserServiceProxy.Current;
                var projectService = ProjectServiceProxy.Current;
                var todoService = ToDoServiceProxy.Current;
                
                // clear data by creating new service instances (this is a simple approach)
                // in a production app, you'd want proper Clear methods
                
                // import users first
                foreach (var user in users)
                {
                    userService.AddOrUpdate(user);
                }

                // import projects
                foreach (var project in projects)
                {
                    projectService.AddOrUpdate(project);
                }

                // then import todos
                foreach (var todo in todos)
                {
                    todoService.AddOrUpdate(todo);
                }

                // after importing todos, link them to their projects
                foreach (var todo in todoService.GetAllToDos())
                {
                    if (todo.ProjectId.HasValue && todo.ProjectId.Value > 0)
                    {
                        var project = projectService.GetById(todo.ProjectId.Value);
                        if (project != null)
                        {
                            todo.Project = project;
                        }
                    }
                }

                // debug: check if data was actually imported
                System.Diagnostics.Debug.WriteLine($"Imported {users.Count} users, {projects.Count} projects and {todos.Count} todos");
                System.Diagnostics.Debug.WriteLine($"Total users in service: {userService.Users.Count}");
                System.Diagnostics.Debug.WriteLine($"Total projects in service: {projectService.Projects.Count}");
                System.Diagnostics.Debug.WriteLine($"Total todos in service: {todoService.GetAllToDos().Count}");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static User? ParseUser(string[] lines, ref int index)
        {
            var user = new User();
            index++; // Skip USER_START

            while (index < lines.Length && lines[index] != "USER_END")
            {
                var line = lines[index];
                var parts = line.Split(':', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "ID":
                            if (int.TryParse(value, out int id))
                                user.Id = id;
                            break;
                        case "NAME":
                            user.Name = value;
                            break;
                        case "IS_MANAGER":
                            if (bool.TryParse(value, out bool isManager))
                                user.IsManager = isManager;
                            break;
                        case "MANAGER_USERNAME":
                            user.ManagerUsername = string.IsNullOrEmpty(value) ? null : value;
                            break;
                        case "MANAGER_PASSWORD":
                            user.ManagerPassword = string.IsNullOrEmpty(value) ? null : value;
                            break;
                    }
                }
                index++;
            }

            return string.IsNullOrEmpty(user.Name) ? null : user;
        }

        private static Project? ParseProject(string[] lines, ref int index)
        {
            var project = new Project();
            index++; // Skip PROJECT_START

            while (index < lines.Length && lines[index] != "PROJECT_END")
            {
                var line = lines[index];
                var parts = line.Split(':', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "ID":
                            if (int.TryParse(value, out int id))
                                project.Id = id;
                            break;
                        case "NAME":
                            project.Name = value;
                            break;
                        case "DESCRIPTION":
                            project.Description = string.IsNullOrEmpty(value) ? null : value;
                            break;
                        case "COMPLETION":
                            if (double.TryParse(value, out double completion))
                                project.CompletionPercent = (int)Math.Round(completion);
                            break;
                    }
                }
                index++;
            }

            return string.IsNullOrEmpty(project.Name) ? null : project;
        }

        private static ToDo? ParseToDo(string[] lines, ref int index)
        {
            var todo = new ToDo();
            index++; // Skip TODO_START

            while (index < lines.Length && lines[index] != "TODO_END")
            {
                var line = lines[index];
                var parts = line.Split(':', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "ID":
                            if (int.TryParse(value, out int id))
                                todo.Id = id;
                            break;
                        case "NAME":
                            todo.Name = value;
                            break;
                        case "DESCRIPTION":
                            todo.Description = string.IsNullOrEmpty(value) ? null : value;
                            break;
                        case "PRIORITY":
                            if (int.TryParse(value, out int priority))
                                todo.Priority = priority;
                            break;
                        case "DUE_DATE":
                            if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out DateTime dueDate))
                                todo.DueDate = dueDate;
                            break;
                        case "IS_COMPLETED":
                            if (bool.TryParse(value, out bool isCompleted))
                                todo.IsCompleted = isCompleted;
                            break;
                        case "PROJECT_ID":
                            if (int.TryParse(value, out int projectId) && projectId != -1)
                                todo.ProjectId = projectId;
                            break;
                        case "ASSIGNED_USER_ID":
                            if (int.TryParse(value, out int assignedUserId) && assignedUserId != -1)
                                todo.AssignedUserId = assignedUserId;
                            break;
                    }
                }
                index++;
            }

            return string.IsNullOrEmpty(todo.Name) ? null : todo;
        }

        public static async Task<string> GetExportFilePathAsync()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = $"AsanaCLI_Export_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            return Path.Combine(documentsPath, fileName);
        }

        public static async Task SaveToFileAsync(string content, string filePath)
        {
            await File.WriteAllTextAsync(filePath, content);
        }

        public static async Task<string> LoadFromFileAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath);
        }
    }
}
