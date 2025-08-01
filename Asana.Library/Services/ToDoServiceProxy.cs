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
                return _toDoList.Take(100).ToList();
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
                    
                    // Ensure Project object is set for existing todo too
                    if (existing.ProjectId.HasValue && existing.ProjectId > 0 && existing.Project == null)
                    {
                        existing.Project = ProjectServiceProxy.Current.GetById(existing.ProjectId.Value);
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
            return ToDos.FirstOrDefault(t => t.Id == id);
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