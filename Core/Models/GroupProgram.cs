using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class GroupProgram
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public List<int> SubjectIds { get; set; }

    }
}
