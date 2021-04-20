using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSimpleServerApp.Data
{
    public class DirectorDataService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        public string WwwRootPath { get; set; }
        public string DirectorDirectory { get; set; }
        public string DestinationDirectory { get; set; }
        public DirectorDataService(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            DirectorDirectory = @"\director_pdfs_test\";
            WwwRootPath = _hostingEnvironment.WebRootPath;
            DestinationDirectory = @"\datagrid2_result_test\";
        }
        public Task<List<DirectorData>> GetDirectorDataAsync()
        {
            
            var files = Directory.GetFiles(WwwRootPath + DirectorDirectory, "*.json");
            List<DirectorData> directorDatas = new List<DirectorData>();
            foreach (var path in files)
            {
                using (StreamReader r = new StreamReader(path))
                {
                    string json = r.ReadToEnd();
                    directorDatas.Add(JsonConvert.DeserializeObject<DirectorData>(json));
                }
            }

            return Task.FromResult(directorDatas);
        }
        public void UpdateJson(string fileName,string note) {
            var sourcePath = WwwRootPath+DirectorDirectory+fileName.Replace(".pdf", ".json");
            string result;
            using (StreamReader r = new StreamReader(sourcePath))
            {
                string json = r.ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);
                obj.Note = note;
                result= JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            using (FileStream fs = File.Create(sourcePath))
            {
                using var sr = new StreamWriter(fs);
                sr.Write(result);
            }
            FileInfo jsonFileInfo = new FileInfo(sourcePath);
            string tempPath = Path.Combine(WwwRootPath+DestinationDirectory, jsonFileInfo.Name);
            File.Move(Path.Combine(WwwRootPath+DirectorDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")), Path.Combine(WwwRootPath+DestinationDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")));
            jsonFileInfo.MoveTo(tempPath);

        }
        
        
        
       
    }
}
