using Asana.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asana.Library.Services
{
    public class ToDoServiceProxy
    {
        private List<ToDo> _toDoList = new List<ToDo>();
        public List<ToDo> ToDos
        {
            get
            {
                var currentUser = UserServiceProxy.Current.CurrentUser;
                if (currentUser == null || currentUser.IsManager)
                {
                    // Manager or no user logged in - see all todos
                    return _toDoList.Take(100).ToList();
                }
                else
                {
                    // Regular user - only see their assigned todos
                    return _toDoList.Where(t => t.AssignedUserId == currentUser.Id).Take(100).ToList();
                }
            }

            private set
            {
                if (value != _toDoList)
                {
                    _toDoList = value;
                }
            }
        }

        private ToDoServiceProxy()
        {
            // _toDoList is already initialized above
        }

        private static ToDoServiceProxy? instance;

        private int nextKey
        {
            get
            {
                if (ToDos.Any())
                {
                    return ToDos.Select(t => t.Id).Max() + 1;
                }
                return 1;
            }
        }

        public static ToDoServiceProxy Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new ToDoServiceProxy();
                }

                return instance;
            }
        }
        public ToDo? AddOrUpdate(ToDo? toDo)
        {
            if (toDo == null)
                return null;

            // Ensure Project object is set if ProjectId is provided
            if (toDo.ProjectId.HasValue && toDo.ProjectId > 0 && toDo.Project == null)
            {
                toDo.Project = ProjectServiceProxy.Current.GetById(toDo.ProjectId.Value);
            }

            // Ensure User object is set if AssignedUserId is provided
            if (toDo.AssignedUserId.HasValue && toDo.AssignedUserId > 0 && toDo.AssignedUser == null)
            {
                toDo.AssignedUser = UserServiceProxy.Current.GetById(toDo.AssignedUserId.Value);
            }

            // If no user is assigned and current user is manager, assign to manager
            var currentUser = UserServiceProxy.Current.CurrentUser;
            if (toDo.AssignedUserId == null && currentUser?.IsManager == true)
            {
                toDo.AssignedUserId = currentUser.Id;
                toDo.AssignedUser = currentUser;
            }

            if (toDo.Id == 0)
            {
                toDo.Id = nextKey;
                _toDoList.Add(toDo);
            }
            else
            {
                var existing = GetById(toDo.Id);
                if (existing != null)
                {
                    // Update existing todo
                    existing.Name = toDo.Name;
                    existing.Description = toDo.Description;
                    existing.Priority = toDo.Priority;
                    existing.IsCompleted = toDo.IsCompleted;
                    existing.DueDate = toDo.DueDate;
                    existing.ProjectId = toDo.ProjectId;
                    existing.Project = toDo.Project;
                    existing.AssignedUserId = toDo.AssignedUserId;
                    existing.AssignedUser = toDo.AssignedUser;
                    
                    // Ensure Project object is set for existing todo too
                    if (existing.ProjectId.HasValue && existing.ProjectId > 0 && existing.Project == null)
                    {
                        existing.Project = ProjectServiceProxy.Current.GetById(existing.ProjectId.Value);
                    }
                    
                    // Ensure User object is set for existing todo too
                    if (existing.AssignedUserId.HasValue && existing.AssignedUserId > 0 && existing.AssignedUser == null)
                    {
                        existing.AssignedUser = UserServiceProxy.Current.GetById(existing.AssignedUserId.Value);
                    }
                }
                else
                {
                    // Add new todo with specific ID
                    _toDoList.Add(toDo);
                }
            }

            // Always update the project's completion percentage if the todo belongs to a project
            if (toDo?.ProjectId != null && toDo.ProjectId > 0)
            {
                var project = ProjectServiceProxy.Current.GetById(toDo.ProjectId.Value);
                if (project != null)
                {
                    // Ensure the todo is in the project's todo list
                    if (!project.ToDos.Contains(toDo))
                    {
                        project.ToDos.Add(toDo);
                    }
                    // This will recalculate the completion percentage
                    ProjectServiceProxy.Current.AddOrUpdate(project);
                }
            }
            else if (toDo?.Project != null)
            {
                // Fallback for when Project object is available but ProjectId might not be set
                ProjectServiceProxy.Current.AddOrUpdate(toDo.Project);
            }
            return toDo;
        }

        public void DisplayToDos(bool isShowCompleted = false)
        {
            if (isShowCompleted)
            {
                ToDos.ForEach(Console.WriteLine);
            }
            else
            {
                ToDos.Where(t => (t != null) && !(t?.IsCompleted ?? false))
                                .ToList()
                                .ForEach(Console.WriteLine);
            }
        }

        public ToDo? GetById(int id)
        {
            return _toDoList.FirstOrDefault(t => t.Id == id);
        }

        public List<ToDo> GetAllToDos()
        {
            // This method allows managers to access all todos regardless of assignment
            return _toDoList.Take(100).ToList();
        }

        public void DeleteToDo(ToDo? toDo)
        {
            if (toDo == null)
            {
                return;
            }
            if (toDo.Project != null)
            {
                toDo.Project.ToDos.Remove(toDo);
                ProjectServiceProxy.Current.AddOrUpdate(toDo.Project);
            }
            _toDoList.Remove(toDo);
        }

    }
}