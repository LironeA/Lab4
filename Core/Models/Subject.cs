using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LectureHours { get; set; }
        public int PracticalHours { get; set; }
        public bool RequiresSubGroupDivision { get; set; }
    }
}
