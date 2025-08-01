using Asana.Library.Models;
using Asana.Library.Services;
using System;

namespace Asana
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var toDoSvc = ToDoServiceProxy.Current;
            var projectSvc = ProjectServiceProxy.Current;
            int choiceInt;
            do
            {
                Console.WriteLine("\nChoose a menu option:");
                Console.WriteLine("1. Create a ToDo");
                Console.WriteLine("2. Delete a ToDo");
                Console.WriteLine("3. Update a ToDo");
                Console.WriteLine("4. List all ToDos");
                Console.WriteLine("5. Create a Project");
                Console.WriteLine("6. Delete a Project");
                Console.WriteLine("7. Update a Project");
                Console.WriteLine("8. List all Projects");
                Console.WriteLine("9. List all ToDos in a Project");
                Console.WriteLine("0. Exit");
                Console.Write("Choice: ");

                var choice = Console.ReadLine() ?? "0";

                if (int.TryParse(choice, out choiceInt))
                {
                    switch (choiceInt)
                    {
                        case 1: 
                            Console.Write("Name: ");
                            var name = Console.ReadLine();
                            Console.Write("Description: ");
                            var description = Console.ReadLine();
                            Console.Write("Priority (integer): ");
                            int priority = int.TryParse(Console.ReadLine(), out var p) ? p : 0;
                            projectSvc.DisplayProjects();
                            Console.Write("Project Id (or 0 for none): ");
                            int projectId = int.TryParse(Console.ReadLine(), out var pid) ? pid : 0;
                            Project? project = projectId > 0 ? projectSvc.GetById(projectId) : null;
                            var newToDo = new ToDo
                            {
                                Name = name,
                                Description = description,
                                Priority = priority,
                                IsCompleted = false,
                                Id = 0,
                                ProjectId = project?.Id,
                                Project = project
                            };
                            toDoSvc.AddOrUpdate(newToDo);
                            project?.ToDos.Add(newToDo);
                            break;
                        case 2: // Delete ToDo
                            toDoSvc.DisplayToDos(true);
                            Console.Write("ToDo to Delete: ");
                            var toDoChoiceDel = int.Parse(Console.ReadLine() ?? "0");
                            var toDoToDelete = toDoSvc.GetById(toDoChoiceDel);
                            toDoSvc.DeleteToDo(toDoToDelete);
                            break;
                        case 3: // Update ToDo
                            toDoSvc.DisplayToDos(true);
                            Console.Write("ToDo to Update: ");
                            var toDoChoiceUpd = int.Parse(Console.ReadLine() ?? "0");
                            var toDoToUpdate = toDoSvc.GetById(toDoChoiceUpd);
                            if (toDoToUpdate != null)
                            {
                                Console.Write("Name: ");
                                toDoToUpdate.Name = Console.ReadLine();
                                Console.Write("Description: ");
                                toDoToUpdate.Description = Console.ReadLine();
                                Console.Write("Priority (integer): ");
                                toDoToUpdate.Priority = int.TryParse(Console.ReadLine(), out var p2) ? p2 : toDoToUpdate.Priority;
                                projectSvc.DisplayProjects();
                                Console.Write("Project ID (or 0 for none): ");
                                int projectIdUpdate = int.TryParse(Console.ReadLine(), out var pid2) ? pid2 : 0;
                                Project? newProject = projectIdUpdate > 0 ? projectSvc.GetById(projectIdUpdate) : null;

                                Console.Write("Is Complete? (y/n): ");
                                string? temp = Console.ReadLine();
                                if (temp != null && temp.ToLower() == "y")
                                    toDoToUpdate.IsCompleted = true;
                                else
                                    toDoToUpdate.IsCompleted = false;

                                if (toDoToUpdate.Project != null && toDoToUpdate.Project.Id != newProject?.Id)
                                {
                                    toDoToUpdate.Project.ToDos.Remove(toDoToUpdate);
                                }
                                toDoToUpdate.ProjectId = newProject?.Id;
                                toDoToUpdate.Project = newProject;
                                if (newProject != null && !newProject.ToDos.Contains(toDoToUpdate))
                                {
                                    newProject.ToDos.Add(toDoToUpdate);
                                }
                                toDoSvc.AddOrUpdate(toDoToUpdate);

                            }
                            break;
                        case 4: 
                            toDoSvc.DisplayToDos(true);
                            break;
                        case 5: 
                            Console.Write("Project Name: ");
                            var projName = Console.ReadLine();
                            Console.Write("Project Description: ");
                            var projDesc = Console.ReadLine();
                            projectSvc.AddOrUpdate(new Project
                            {
                                Name = projName,
                                Description = projDesc,
                                Id = 0
                            });
                            break;
                        case 6: 
                            projectSvc.DisplayProjects();
                            Console.Write("Project to Delete: ");
                            var projDel = int.Parse(Console.ReadLine() ?? "0");
                            var projectToDelete = projectSvc.GetById(projDel);
                            projectSvc.DeleteProject(projectToDelete);
                            break;
                        case 7: 
                            projectSvc.DisplayProjects();
                            Console.Write("Project to Update: ");
                            var projUpd = int.Parse(Console.ReadLine() ?? "0");
                            var projectToUpdate = projectSvc.GetById(projUpd);
                            if (projectToUpdate != null)
                            {
                                Console.Write("Name: ");
                                projectToUpdate.Name = Console.ReadLine();
                                Console.Write("Description: ");
                                projectToUpdate.Description = Console.ReadLine();
                                projectSvc.AddOrUpdate(projectToUpdate);
                            }
                            break;
                        case 8: 
                            projectSvc.DisplayProjects();
                            break;
                        case 9: 
                            projectSvc.DisplayProjects();
                            Console.Write("Project Id: ");
                            var projId = int.Parse(Console.ReadLine() ?? "0");
                            var pr = projectSvc.GetById(projId);
                            if (pr != null)
                            {
                                foreach (var todo in pr.ToDos)
                                {
                                    Console.WriteLine(todo);
                                }
                            }
                            break;
                        case 0:
                            break;
                        default:
                            Console.WriteLine("ERROR: Unknown menu selection");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"ERROR: {choice} is not a valid menu selection");
                }

            } while (choiceInt != 0);
        }
    }
}