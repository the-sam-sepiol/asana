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
                    // managers or no user logged in - see all todos
                    return _toDoList.Take(100).ToList();
                }
                else
                {
                    // regular users only see their assigned todos
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
        public void AddOrUpdate(ToDo item)
        {
            // see if we can find a match
            var existingToDo = _toDoList.Where(x => x.Id == item.Id).FirstOrDefault();

            if (existingToDo == null)
            {
                // new todo, so add it with current highest id + 1
                if (_toDoList.Count > 0)
                {
                    item.Id = _toDoList.Max(x => x.Id) + 1;
                }
                else
                {
                    item.Id = 1;
                }
                _toDoList.Add(item);
            }
            else
            {
                // update existing todo
                var index = _toDoList.IndexOf(existingToDo);
                _toDoList[index] = item;
            }

            // make sure Project object is also updated
            if (item.Project != null)
            {
                ProjectServiceProxy.Current.AddOrUpdate(item.Project);
            }
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
            // this method allows managers to access all todos regardless of assignment
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