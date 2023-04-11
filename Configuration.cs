using Newtonsoft.Json;

namespace Config
{
    public class JSON<T>
    {
        private string FilePath;
        public JSON(string FilePath)
        {
            this.FilePath = Path.Combine(Environment.CurrentDirectory, FilePath);
        }

        public void SaveData(T data)
        {
            string json = JsonConvert.SerializeObject(data);
            File.WriteAllText(FilePath, json);
        }

        public T LoadData()
        {
            if (!File.Exists(FilePath))
            {
                throw new FileNotFoundException($"File not found: {FilePath}");
            }

            string json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<T>(json);
        }

    }

    public class ConfigurationEmail
    {
        public List<string> Emails { get; set; }
        public PrivateEmailData PrivateEmailData { get; set; }
        public ConfigurationEmail()
        {
            Emails = new List<string>();
        }

        public void pushEmail(string email)
        {
            Emails.Add(email);
        }

    }

    public class PrivateEmailData
    {
        public string Email { get; set; }
        public string AppName { get; set; }
        public string CredentialFile { get; set; }
        public string Title { get; set; }
        public PrivateEmailData(string Email, string AppName, string CredentialFile, string Title)
        {
            this.Email = Email;
            this.AppName = AppName;
            this.CredentialFile = CredentialFile;
            this.Title = Title;
        }
    }
}
