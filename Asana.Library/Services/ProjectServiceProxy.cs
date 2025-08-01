using Asana.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Asana.Library.Services
{
    public class ProjectServiceProxy
    {
        private List<Project> _projectList = new List<Project>();

        public List<Project> Projects
        {
            get
            {
                return _projectList.Take(100).ToList();
            }
            private set
            {
                if (value != _projectList)
                {
                    _projectList = value;
                }
            }
        }

        private ProjectServiceProxy()
        {
            // _projectList is already initialized above
        }

        private static ProjectServiceProxy? instance;

        private int nextKey
        {
            get
            {
                if (Projects.Any())
                {
                    return Projects.Select(p => p.Id).Max() + 1;
                }
                return 1;
            }
        }

        public static ProjectServiceProxy Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProjectServiceProxy();
                }
                return instance;
            }
        }

        public Project? AddOrUpdate(Project? project)
        {
            if (project == null)
                return null;

            project.CompletionPercent = CalculateCompletionPercent(project);

            if (project.Id == 0)
            {
                project.Id = nextKey;
                if (project.ToDos == null)
                    project.ToDos = new List<ToDo>();
                _projectList.Add(project);
            }
            else
            {
                var existing = GetById(project.Id);
                if (existing != null)
                {
                    existing.Name = project.Name;
                    existing.Description = project.Description;
                    existing.ToDos = project.ToDos ?? new List<ToDo>();
                    existing.CompletionPercent = CalculateCompletionPercent(existing);
                }
                else
                {
                    // Add new project with specific ID
                    if (project.ToDos == null)
                        project.ToDos = new List<ToDo>();
                    _projectList.Add(project);
                }
            }
            return project;
        }

        private int CalculateCompletionPercent(Project project)
        {
            if (project.ToDos == null || project.ToDos.Count == 0)
                return 0;
            int completed = project.ToDos.Count(t => t.IsCompleted == true);
            return (int)Math.Round((completed * 100.0) / project.ToDos.Count);
        }

        public void DisplayProjects()
        {
            Projects.ForEach(Console.WriteLine);
        }

        public Project? GetById(int id)
        {
            return Projects.FirstOrDefault(p => p.Id == id);
        }

        public void DeleteProject(Project? project)
        {
            if (project == null)
                return;
            if (project.ToDos != null && project.ToDos.Count > 0)
            {
                var toDoSvc = ToDoServiceProxy.Current;
                var toDosToDelete = project.ToDos.ToList();
                foreach (var todo in toDosToDelete)
                    toDoSvc.DeleteToDo(todo);
            }
            
            _projectList.Remove(project);
            
        }

        public void DisplayToDosInProject(int projectId)
        {
            var project = GetById(projectId);
            if (project == null)
            {
                Console.WriteLine("Project not found.");
                return;
            }
            if (project.ToDos == null || !project.ToDos.Any())
            {
                Console.WriteLine("No ToDos in this project.");
                return;
            }
            project.ToDos.ForEach(Console.WriteLine);
        }
    }
}