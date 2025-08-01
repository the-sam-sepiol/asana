using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asana.Library.Models
{
    public class ToDo
    {
        public int Id {  get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Priority { get; set; }
        public bool? IsCompleted { get; set; }

        public DateTime? DueDate { get; set; }



        public override string ToString()
        {
            string? comp = "Not Completed";
            if (IsCompleted == true)
                comp = "Completed";
            string proj = Project != null ? $" (Project: {Project.Name})" : "";
            string prio = Priority.HasValue ? $" [Priority: {Priority}]" : "";
            string due = DueDate.HasValue ? $" [Due: {DueDate:yyyy-MM-dd}]" : "";
            return $"{Name} - {Description} - {comp}{prio}{proj}{due}";
        }
    }
}
