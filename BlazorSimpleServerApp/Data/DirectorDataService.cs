using BlazorSimpleServerApp.Common;
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
            var config = new Configuration().InitConfiguration();
            DirectorDirectory = config.GetSection("DirectorSourcePath").Value;
            WwwRootPath = _hostingEnvironment.WebRootPath;
            DestinationDirectory = config.GetSection("DirectorDestinationPath").Value;
        }
        public Task<List<DirectorData>> GetDirectorDataAsync()
        {
            var files = Directory.GetFiles(Path.Join(WwwRootPath, DirectorDirectory), "*.json");
            List<DirectorData> directorDatas = new List<DirectorData>();
            try
            {
                foreach (var path in files)
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        string json = r.ReadToEnd();
                        directorDatas.Add(JsonConvert.DeserializeObject<DirectorData>(json));
                    }
                }
            }
            catch (Exception) { }
            

            return Task.FromResult(directorDatas);
        }
        public void UpdateJson(string fileName,string note) {
                var sourcePath =Path.Join(WwwRootPath,DirectorDirectory, (fileName.Replace(".pdf", ".json")));
                string result;
            try
            {
                using (StreamReader r = new StreamReader(sourcePath))
                {
                    string json = r.ReadToEnd();
                    dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json);
                    obj.Note = note;
                    result = JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                if (result != null) {
                    using (FileStream fs = File.Create(sourcePath))
                    {
                        using var sr = new StreamWriter(fs);
                        sr.Write(result);
                    }
                }  
            }
            catch (Exception) 
            {
            
            }
            try
            {
                FileInfo jsonFileInfo = new FileInfo(sourcePath);
                string tempPath = Path.Join(WwwRootPath, DestinationDirectory, jsonFileInfo.Name);
                File.Move(Path.Join(WwwRootPath, DirectorDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")), Path.Join(WwwRootPath, DestinationDirectory, jsonFileInfo.Name.Replace(".json", ".pdf")));
                jsonFileInfo.MoveTo(tempPath);
            }
            catch (Exception) { 
            }
                
            

        }
        
        
        
       
    }
}
