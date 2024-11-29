using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> SubjectIds { get; set; }
        public bool CanLectures { get; set; }
        public bool CanPracticals { get; set; }


        public Teacher()
        {
            SubjectIds = new List<int>();
        }

    }
}
