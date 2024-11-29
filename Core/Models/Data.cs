using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Data
    {
        public List<Teacher> Teachers { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Group> Groups { get; set; }
        public List<GroupProgram> GroupPrograms { get; set; }
        public List<Room> Rooms { get; set; }

        public Data()
        {
            Teachers = new List<Teacher>();
            Subjects = new List<Subject>();
            Groups = new List<Group>();
            GroupPrograms = new List<GroupProgram>();
            Rooms = new List<Room>();
        }

        public void SaveDataToJson(string path)
        {
            string folderPath = Path.Combine(path, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}");
            Directory.CreateDirectory(folderPath);

            SaveListToJson(Teachers, Path.Combine(folderPath, "Teachers-random.json"));
            SaveListToJson(Subjects, Path.Combine(folderPath, "Subjects-random.json"));
            SaveListToJson(Groups, Path.Combine(folderPath, "Groups-random.json"));
            SaveListToJson(GroupPrograms, Path.Combine(folderPath, "GroupPrograms-random.json"));
            SaveListToJson(Rooms, Path.Combine(folderPath, "Rooms-random.json"));
        }

        public void SaveListToJson<T>(List<T> list, string filePath)
        {
            string jsonString = JsonConvert.SerializeObject(list, Formatting.Indented);

            File.WriteAllText(filePath, jsonString);
        }

        public void SaveObjectToJson<T>(T objct, string filePath)
        {
            string jsonString = JsonConvert.SerializeObject(objct, Formatting.Indented);

            File.WriteAllText(filePath, jsonString);
        }


        public void LoadDataFromJson(string path, string folder)
        {
            // Load each list from a separate JSON file
            Teachers = LoadListFromJson<Teacher>(Path.Combine(path, folder, "Teachers-random.json"));
            Subjects = LoadListFromJson<Subject>(Path.Combine(path, folder, "Subjects-random.json"));
            Groups = LoadListFromJson<Group>(Path.Combine(path, folder, "Groups-random.json"));
            GroupPrograms = LoadListFromJson<GroupProgram>(Path.Combine(path, folder, "GroupPrograms-random.json"));
            Rooms = LoadListFromJson<Room>(Path.Combine(path, folder, "Rooms-random.json"));




        }

        private List<T> LoadListFromJson<T>(string filePath)
        {
            // Read the JSON string from the file
            string jsonString = File.ReadAllText(filePath);

            // Convert the JSON string to a list of objects
            return JsonConvert.DeserializeObject<List<T>>(jsonString);
        }
    }
}
