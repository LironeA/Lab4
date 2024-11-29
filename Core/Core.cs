using ConsoleTables;
using Core.Models;

namespace Core
{
    public class CoreRunner
    {
        public void Run()
        {
            var data = new Data();
            data.LoadDataFromJson("D:\\CSC\\4th\\Intelegense Systems\\Lab4\\Lab4\\Core", "JsonFiles");
            DisplayData(data);

            var scheuder = new CSP(data);
            var res = scheuder.Solve();


            DisplayGroupTimetabel(res);

        }

        void DisplayData(Data Data)
        {
            Console.WriteLine("Teachers:");
            var teachersTable = new ConsoleTable();
            teachersTable.Options.NumberAlignment = Alignment.Right;
            teachersTable.AddColumn(new[] { "ID", "Name", "Subjects", "CanLectures", "CanPracticals" });
            foreach(var teacher in Data.Teachers)
            {
                var subjects = "";
                foreach(var subjectID in teacher.SubjectIds)
                {
                    var subject = Data.Subjects.FirstOrDefault(x => x.Id == subjectID);
                    subjects += $"{subject.Name}, ";
                }
                teachersTable.AddRow(teacher.Id, teacher.Name, subjects, teacher.CanLectures, teacher.CanPracticals);
            }
            teachersTable.Write(Format.Alternative);

            Console.WriteLine("\nSubjects:");
            ConsoleTable
                .From(Data.Subjects)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);

            Console.WriteLine("\nGroups:");
            ConsoleTable
                .From(Data.Groups)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);



            Console.WriteLine("\nGroup Programs:");
            var groupProgramsTable = new ConsoleTable();
            groupProgramsTable.Options.NumberAlignment = Alignment.Right;
            groupProgramsTable.AddColumn(new[] { "ID", "Group", "Subjects" });
            foreach(var groupProgram in Data.GroupPrograms)
            {
                var subjects = "";
                foreach(var subjectID in groupProgram.SubjectIds)
                {
                    var subject = Data.Subjects.FirstOrDefault(x => x.Id == subjectID);
                    subjects += $"{subject.Name}, ";
                }
                groupProgramsTable.AddRow(groupProgram.Id, Data.Groups.FirstOrDefault(x => x.Id == groupProgram.GroupId).Name, subjects);
            }
            groupProgramsTable.Write(Format.Alternative);

            Console.WriteLine("\nRooms:");

            ConsoleTable
                .From(Data.Rooms)
                .Configure(o => o.NumberAlignment = Alignment.Right)
                .Write(Format.Alternative);
        }

        void DisplayGroupTimetabel(WeekTimeTable table)
        {
            var consoleTable = new ConsoleTable();
            var header = new List<string>();
            header.Add("Група/День");
            foreach(GroupTimeTable timeTable in table.GroupTimeTables)
            {
                header.Add(timeTable.Group.Name);
            }

            consoleTable.AddColumn(header);
            consoleTable.Options.EnableCount = false;

            for(int d = 0; d < 5; d++)
            {
                for(int c = 0; c < 4; c++)
                {

                    var subjects = new List<string>();
                    if(c == 0)
                    {
                        subjects.Add($"День {d}");
                    }
                    else
                    {
                        subjects.Add("");
                    }

                    subjects.AddRange(table.GroupTimeTables.Select(x => x.DayTimeTables[d].TimeTableCells[c]?.Subject.Name ?? "---"));
                    consoleTable.AddRow(subjects.ToArray());
                    var teachers = new List<string>();
                    teachers.Add("");
                    teachers.AddRange(table.GroupTimeTables.Select(x => x.DayTimeTables[d].TimeTableCells[c]?.Teacher.Name ?? "---"));
                    consoleTable.AddRow(teachers.ToArray());
                    var rooms = new List<string>();
                    rooms.Add("");
                    rooms.AddRange(table.GroupTimeTables.Select(x => x.DayTimeTables[d].TimeTableCells[c]?.Room.Name ?? "---"));
                    consoleTable.AddRow(rooms.ToArray());
                    var isLectures = new List<string>();
                    isLectures.Add("");
                    isLectures.AddRange(table.GroupTimeTables.Select(x =>
                    {
                        if(x.DayTimeTables[d].TimeTableCells[c] is null)
                        {
                            return "---";
                        }
                        return x.DayTimeTables[d].TimeTableCells[c].IsLecture ? "Лекція" : "Практика";
                    }));
                    consoleTable.AddRow(isLectures.ToArray());
                    consoleTable.AddRow(Enumerable.Repeat("", table.GroupTimeTables.Count() + 1).ToArray());



                }
            }

            consoleTable.Write(Format.Alternative);

        }
    }
}
