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

            // Export Projects
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

            // Export ToDos
            sb.AppendLine("=== TODOS ===");
            var todos = ToDoServiceProxy.Current.ToDos;
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

                var projects = new List<Project>();
                var todos = new List<ToDo>();

                // Parse Projects
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "PROJECT_START")
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

                // Clear existing data and import new data
                ProjectServiceProxy.Current.Projects.Clear();
                ToDoServiceProxy.Current.ToDos.Clear();

                // Import projects first
                foreach (var project in projects)
                {
                    ProjectServiceProxy.Current.AddOrUpdate(project);
                }

                // Then import todos
                foreach (var todo in todos)
                {
                    ToDoServiceProxy.Current.AddOrUpdate(todo);
                }

                // Debug: Check if data was actually imported
                System.Diagnostics.Debug.WriteLine($"Imported {projects.Count} projects and {todos.Count} todos");
                System.Diagnostics.Debug.WriteLine($"Total projects in service: {ProjectServiceProxy.Current.Projects.Count}");
                System.Diagnostics.Debug.WriteLine($"Total todos in service: {ToDoServiceProxy.Current.ToDos.Count}");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
