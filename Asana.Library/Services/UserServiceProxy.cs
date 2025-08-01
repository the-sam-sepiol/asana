using Asana.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asana.Library.Services
{
    public class UserServiceProxy
    {
        private List<User> _userList = new List<User>();
        private User? _currentUser = null;

        public List<User> Users
        {
            get
            {
                return _userList.ToList();
            }
            private set
            {
                if (value != _userList)
                {
                    _userList = value;
                }
            }
        }

        public User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        private UserServiceProxy()
        {
            // start with an empty user list - the UI will handle creating the first manager
        }

        private static UserServiceProxy? instance;

        private int nextKey
        {
            get
            {
                if (Users.Any())
                {
                    return Users.Select(u => u.Id).Max() + 1;
                }
                return 1;
            }
        }

        public static UserServiceProxy Current
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserServiceProxy();
                }
                return instance;
            }
        }

        public User? AddOrUpdate(User? user)
        {
            if (user == null)
                return null;

            if (user.Id == 0)
            {
                user.Id = nextKey;
                _userList.Add(user);
            }
            else
            {
                var existing = GetById(user.Id);
                if (existing != null)
                {
                    existing.Name = user.Name;
                    existing.IsManager = user.IsManager;
                    existing.ManagerUsername = user.ManagerUsername;
                    existing.ManagerPassword = user.ManagerPassword;
                }
                else
                {
                    _userList.Add(user);
                }
            }
            return user;
        }

        public User? GetById(int id)
        {
            return Users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByName(string name)
        {
            return Users.FirstOrDefault(u => u.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        }

        public User? AuthenticateManager(string username, string password)
        {
            return Users.FirstOrDefault(u => u.IsManager && 
                                           u.ManagerUsername?.Equals(username, StringComparison.OrdinalIgnoreCase) == true &&
                                           u.ManagerPassword == password);
        }

        public bool LoginUser(string name)
        {
            var user = GetByName(name);
            if (user != null)
            {
                CurrentUser = user;
                return true;
            }
            return false;
        }

        public bool LoginManager(string username, string password)
        {
            var manager = AuthenticateManager(username, password);
            if (manager != null)
            {
                CurrentUser = manager;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public void DeleteUser(User? user)
        {
            if (user == null || user.IsManager)
                return;

            _userList.Remove(user);
        }

        public List<User> GetRegularUsers()
        {
            return Users.Where(u => !u.IsManager).ToList();
        }
    }
}
