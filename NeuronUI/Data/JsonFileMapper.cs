using System.IO;
using System.Text.Json;

namespace NeuronUI.Data
{
    public static class JsonFileMapper
    {
        public static void SaveObject(string fileName, object o)
        {
            string jsonString = JsonSerializer.Serialize(o);


            using StreamWriter streamWriter = new(fileName);
            streamWriter.Write(jsonString);
        }

        public static T LoadObject<T>(string fileName)
        {
            string jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
