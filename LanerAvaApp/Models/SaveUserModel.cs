using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanerAvaApp.Models
{
    public class UserData
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = true;
    }

    public static class SaveUserModel
    {
        private static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LanerAvaApp");
        private static string DataFile = Path.Combine(DataPath, "userdata.json");

        static SaveUserModel()
        {
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
        }

        public static void SaveData(UserData userData)
        {
            try
            {
                var json = JsonSerializer.Serialize(userData);
                File.WriteAllText(DataFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static UserData LoadData()
        {
            try
            {
                var json = File.ReadAllText(DataFile);
                return JsonSerializer.Deserialize<UserData>(json) ?? new UserData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return new UserData();
        }

        public static void ExitUser()
        {
            try
            {
                File.WriteAllText(DataFile, "");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
