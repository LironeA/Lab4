using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
    public class CSP
    {
        private Dictionary<string, List<Domain>> _domains;
        private readonly Data _data;
        private Dictionary<string, string> _assignment;
        private List<string> _variables;
        public CSP(Data data)
        {
            _data = data;
        }

        public WeekTimeTable Solve()
        {
            _domains = CreateDomains();
            _assignment = new Dictionary<string, string>();
            _variables = _domains.Keys.ToList();
            var res = Backtrack();
            return MapToWeekTimeTable(res);
        }

        WeekTimeTable MapToWeekTimeTable(Dictionary<string, Domain> asg)
        {
            var res = new WeekTimeTable(_data.Groups);

            foreach(var domains in asg.Values)
            {
                var groupTimeTable = res.GroupTimeTables.FirstOrDefault(x => x.Group.Id == domains.GroupId);
                var dayTimetable = groupTimeTable.DayTimeTables[domains.Day - 1];
                var cell = new TimeTableCell
                {
                    Subject = _data.Subjects.First(s => s.Id == domains.SubjectId),
                    Teacher = _data.Teachers.First(t => t.Id == domains.TeacherId),
                    Room = _data.Rooms.First(r => r.Id == domains.RoomId),
                    IsLecture = domains.IsLecture
                };
                dayTimetable.TimeTableCells[domains.Time - 1] = cell;
            }

            return res;

        }

        public Dictionary<string, List<Domain>> CreateDomains()
        {
            var domains = new Dictionary<string, List<Domain>>();
            int[] days = { 1, 2, 3, 4, 5 };  // Дні: Понеділок - П’ятниця
            int[] times = { 1, 2, 3, 4 };    // Пара: Перша - Четверта
            foreach(var group in _data.Groups)
            {
                foreach(var day in days)
                {
                    foreach(var time in times)
                    {
                        string variableKey = $"{group.Id}_{day}_{time}";

                        var domainList = new List<Domain>();


                        var subjects = _data.GroupPrograms.First(x => x.GroupId == group.Id).SubjectIds;
                        foreach(var subject in _data.Subjects.Where(x => subjects.Contains(x.Id)))
                        {
                            var suitableTeachers = _data.Teachers
                                .Where(t => t.SubjectIds.Contains(subject.Id))
                                .ToList();

                            if(!suitableTeachers.Any())
                                continue;

                            foreach(var teacher in suitableTeachers)
                            {
                                if(teacher.CanLectures)
                                {
                                    foreach(var room in _data.Rooms)
                                    {
                                        if(room.Capacity >= group.NumberOfStudents)
                                        {
                                            domainList.Add(new Domain
                                            {
                                                GroupId = group.Id,
                                                SubjectId = subject.Id,
                                                TeacherId = teacher.Id,
                                                RoomId = room.Id,
                                                Day = day,
                                                Time = time,
                                                IsLecture = true
                                            });
                                        }
                                    }
                                }

                                if(teacher.CanPracticals)
                                {
                                    foreach(var room in _data.Rooms)
                                    {
                                        if(room.Capacity >= group.NumberOfStudents)
                                        {
                                            domainList.Add(new Domain
                                            {
                                                GroupId = group.Id,
                                                SubjectId = subject.Id,
                                                TeacherId = teacher.Id,
                                                RoomId = room.Id,
                                                Day = day,
                                                Time = time,
                                                IsLecture = false
                                            });
                                        }
                                    }
                                }
                            }


                        }


                        if(domainList.Any())
                        {
                            domains[variableKey] = domainList;
                        }
                    }
                }
            }

            return domains;
        }

        public Dictionary<string, Domain> Backtrack()
        {
            return BacktrackRecursive(new Dictionary<string, Domain>());
        }

        private Dictionary<string, Domain> BacktrackRecursive(Dictionary<string, Domain> currentAssignment)
        {
            // Умова завершення: якщо всі змінні призначені
            if(currentAssignment.Count == _variables.Count)
                return currentAssignment;

            // Вибір наступної змінної (MRV)
            string variable = SelectUnassignedVariable(currentAssignment);

            var possibleValues = OrderDomainValues(variable, currentAssignment);

            foreach(var value in possibleValues)
            {
                if(IsConsistent(variable, value, currentAssignment))
                {
                    currentAssignment[variable] = value;

                    var result = BacktrackRecursive(currentAssignment);
                    if(result != null)
                        return result;

                    currentAssignment.Remove(variable);
                }
            }
            return null;
        }

        // Вибір змінної з найменшим доменом (евристика MRV)
        private string SelectUnassignedVariable(Dictionary<string, Domain> assignment)
        {
            var unassignedVars = _variables.Where(v => !assignment.ContainsKey(v)).ToList();

            int minDomainSize = unassignedVars.Min(v => _domains[v].Count);
            var mrvVars = unassignedVars.Where(v => _domains[v].Count == minDomainSize).ToList();

            if(mrvVars.Count == 1)
            {
                return mrvVars.First();
            }

            var maxDegreeVar = mrvVars
                .Select(v =>
                {
                    int degree = _variables.Count(otherVar => otherVar != v && IsNeighbor(v, otherVar)); // Обчислюємо степінь
                    return new { Variable = v, Degree = degree };
                })
                .OrderByDescending(x => x.Degree)
                .First();

            return maxDegreeVar.Variable;
        }

        private bool IsNeighbor(string var1, string var2)
        {
            var domain1 = _domains[var1];
            var domain2 = _domains[var2];

            foreach(var value1 in domain1)
            {
                foreach(var value2 in domain2)
                {
                    if(value1.Day == value2.Day && value1.Time == value2.Time)
                    {
                        return true;
                    }

                    if(value1.RoomId == value2.RoomId && value1.IsLecture && value2.IsLecture)
                    {
                        return true;
                    }

                    if(value1.TeacherId == value2.TeacherId)
                    {
                        return true;
                    }
                }
            }

            return false; // Якщо не знайшли жодного спільного елемента, то вони не є сусідами
        }

        private List<Domain> OrderDomainValues(string variable, Dictionary<string, Domain> assignment)
        {
            return _domains[variable]
                .OrderBy(value => CountConflicts(variable, value, assignment))
                .ToList();
        }

        // Перевірка, чи узгоджене поточне призначення
        private bool IsConsistent(string variable, Domain value, Dictionary<string, Domain> assignment)
        {
            foreach(var assignedValue in assignment.Values)
            {
                // 1. Викладач не може проводити більше одного заняття одночасно
                if(assignedValue.TeacherId == value.TeacherId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    if(assignedValue.RoomId != value.RoomId) 
                        return false;
                }

                // 2. Кімната не може бути зайнята одночасно для різних типів занять
                if(assignedValue.RoomId == value.RoomId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    if(assignedValue.SubjectId == value.SubjectId && (!assignedValue.IsLecture || !value.IsLecture))
                        return false;
                }

                if(assignedValue.RoomId == value.RoomId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    if(assignedValue.SubjectId != value.SubjectId)
                        return false;
                }

                // 3. Група не може мати більше одного заняття одночасно
                if(assignedValue.GroupId == value.GroupId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    return false;
                }


            }

            //4.Кількість годин
            var subjectCount = assignment.Values.Where(x => x.GroupId == value.GroupId && x.SubjectId == value.SubjectId && x.IsLecture == value.IsLecture).Count();
            var subject = _data.Subjects.FirstOrDefault(x => x.Id == value.SubjectId);
            if(subjectCount > (value.IsLecture ? subject.LectureHours : subject.PracticalHours))
            {
                return false;
            }


            return true;
        }

        private int CountConflicts(string variable, Domain value, Dictionary<string, Domain> assignment)
        {
            int conflicts = 0;

            foreach(var assignedValue in assignment.Values)
            {
                if(assignedValue.TeacherId == value.TeacherId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    if(assignedValue.RoomId != value.RoomId)
                        conflicts++;
                }

                if(assignedValue.RoomId == value.RoomId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    if(assignedValue.SubjectId == value.SubjectId && (!assignedValue.IsLecture || !value.IsLecture))
                        conflicts++;
                }

                if(assignedValue.GroupId == value.GroupId &&
                    assignedValue.Day == value.Day &&
                    assignedValue.Time == value.Time)
                {
                    conflicts++;
                }

            }

            return conflicts;
        }

    }

    public class Domain
    {
        public int GroupId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public int RoomId { get; set; }
        public int Day { get; set; }
        public int Time { get; set; }
        public bool IsLecture { get; set; }
    }

}
