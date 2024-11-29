using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{

    public class WeekTimeTable
    {
        public int GroupsCount { get; set; }
        public GroupTimeTable[] GroupTimeTables { get; set; }

        public int Fittness { get; set; }

        public WeekTimeTable()
        {
            
        }

        public WeekTimeTable(List<Group> groups)
        {
            GroupsCount = groups.Count;
            GroupTimeTables = new GroupTimeTable[GroupsCount];

            for(int i = 0; i < GroupsCount; i++)
            {
                GroupTimeTables[i] = new GroupTimeTable(groups[i], 5);
                for(int j = 0; j < 5; j++)
                {
                    GroupTimeTables[i].DayTimeTables[j] = new DayTimeTable(4);
                }
            }

        }

        public void CalculateFitness()
        {
            int fitness = 0;


            // Кожна заповнена комірка розкладу додає до фітнесу
            foreach(var groupTimetable in GroupTimeTables)
            {
                foreach(var dayTimeTable in groupTimetable.DayTimeTables)
                {
                    foreach(var cell in dayTimeTable.TimeTableCells)
                    {
                        if(cell != null)
                        {
                            fitness++; // Базова оцінка за заповнену комірку

                            // обмеження по викладачам: список предметів та типів занять, що викладач може вести 
                            if(!(cell.Teacher.SubjectIds.Contains(cell.Subject.Id) && ((cell.IsLecture && cell.Teacher.CanLectures) || (!cell.IsLecture && cell.Teacher.CanPracticals))))
                            {
                                fitness -= 10;
                            }

                            // заняття не може проводитися, якщо група більша за кількість місць в аудиторії
                            if(cell.IsLecture ? cell.Room.Capacity < groupTimetable.Group.NumberOfStudents : cell.Room.Capacity < (int)(groupTimetable.Group.NumberOfStudents / 2))
                            {
                                fitness -= 10;
                            }
                        }
                    }
                }
            }

            // Перевірка конфліктів використання викладача та аудиторії
            for(int day = 0; day < 5; day++)
            {
                for(int period = 0; period < 4; period++)
                {
                    var teacherSchedule = new Dictionary<int, TimeTableCell>();
                    var roomSchedule = new Dictionary<int, Subject>();

                    foreach(var groupTimetable in GroupTimeTables)
                    {
                        var cell = groupTimetable.DayTimeTables[day].TimeTableCells[period];
                        if(cell == null) continue;

                        // один лектор повинен одночасно проводити заняття тільки в одній аудиторії;
                        if(teacherSchedule.ContainsKey(cell.Teacher.Id) && teacherSchedule[cell.Teacher.Id].Room.Id != cell.Room.Id)
                        {
                            fitness -= 100;
                        }
                        else
                        {
                            teacherSchedule[cell.Teacher.Id] = cell;
                        }

                        // одна аудиторія може використовуватися одночасно лише під одне заняття
                        if(roomSchedule.ContainsKey(cell.Room.Id) && roomSchedule[cell.Room.Id] != cell.Subject)
                        {
                            fitness -= 100;
                        }
                        else
                        {
                            roomSchedule[cell.Room.Id] = cell.Subject;
                        }
                    }
                }
            }

            // зауважте, що в одній аудиторії може навчатися декілька груп одночасно, у одного лектора, але тільки для лекцій.  
            for(int d = 0; d < GroupTimeTables[0].DayTimeTables.Length; d++)
            {
                for(int c = 0; c < GroupTimeTables[0].DayTimeTables[0].TimeTableCells.Length; c++)
                {
                    var roomOcupation = new Dictionary<int, List<TimeTableCell>>();
                    for(int g = 0; g < GroupTimeTables.Length; g++)
                    {
                        var cell = GroupTimeTables[g].DayTimeTables[d].TimeTableCells[c];
                        if(!roomOcupation.ContainsKey(cell.Room.Id))
                        {
                            roomOcupation[cell.Room.Id] = new List<TimeTableCell>();
                        }
                        roomOcupation[cell.Room.Id].Add(cell);
                    }


                    foreach(var roomOc in roomOcupation.Values.ToList())
                    {
                        if(roomOc.Count > 1)
                        {
                            var subjectsInRoom = roomOc.Select(x => x.Subject).Distinct().Count();
                            if(subjectsInRoom > 1)
                            {
                                fitness -= 100;
                            }

                            var teachersInRoom = roomOc.Select(x => x.Teacher).Distinct().Count();
                            if(teachersInRoom > 1)
                            {
                                fitness -= 100;
                            }

                            var lecturesInRoom = roomOc.Select(x => x.IsLecture);
                            if(lecturesInRoom.Count() > 1 && lecturesInRoom.Any(x => !x))
                            {
                                fitness -= 100;
                            }

                        }
                    }
                }
            }

            

            Fittness = fitness;
        }
    }

    public class GroupTimeTable
    {
        public Group Group { get; set; }
        public int WorkDayNumber { get; set; }
        public DayTimeTable[] DayTimeTables { get; set; }

        public GroupTimeTable()
        {
            
        }
        public GroupTimeTable(Group group, int workDayNumber)
        {
            this.Group = group;
            this.WorkDayNumber = workDayNumber;
            DayTimeTables = new DayTimeTable[workDayNumber];
        }

        public GroupTimeTable Copy()
        {
            // Копіюємо групу
            var groupTimeTable = new GroupTimeTable(Group, WorkDayNumber);
            for(int i = 0; i < WorkDayNumber; i++)
            {
                groupTimeTable.DayTimeTables[i] = DayTimeTables[i].Copy();
            }
            return groupTimeTable;

        }

    }

    public class DayTimeTable
    {

        public int MaxNumberOfLessons { get; set; }
        public TimeTableCell[] TimeTableCells { get; set; }

        public DayTimeTable()
        {
            
        }

        public DayTimeTable(int maxNumberOfLessons)
        {
            MaxNumberOfLessons = maxNumberOfLessons;
            TimeTableCells = new TimeTableCell[MaxNumberOfLessons];
        }

        public DayTimeTable Copy()
        {
            // Копіюємо день
            var dayTimeTable = new DayTimeTable(MaxNumberOfLessons);
            for(int i = 0; i < MaxNumberOfLessons; i++)
            {
                dayTimeTable.TimeTableCells[i] = TimeTableCells[i].Copy();
            }

            return dayTimeTable;
        }
    }


    public class TimeTableCell
    {
        public Subject Subject { get; set; }
        public Room Room { get; set; }
        public Teacher Teacher { get; set; }
        public bool IsLecture { get; set; }

        public TimeTableCell Copy()
        {
            // Копіюємо комірку
            var cell = new TimeTableCell
            {
                Subject = Subject,
                Room = Room,
                Teacher = Teacher,
                IsLecture = IsLecture
            };

            return cell;
        }
    }


}
