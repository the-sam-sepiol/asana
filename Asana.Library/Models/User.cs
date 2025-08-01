using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asana.Library.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsManager { get; set; }
        public string? ManagerUsername { get; set; }
        public string? ManagerPassword { get; set; }

        public override string ToString()
        {
            return $"User Id: {Id}, Name: {Name}, IsManager: {IsManager}";
        }
    }
}
