using BlazorSimpleServerApp.Common;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BlazorSimpleServerApp.Pages.Datagrid2;

namespace BlazorSimpleServerApp.Data
{
    public class MoveAndMakeJsonService
    {
        private string DestinationPath { get; set; }
        private string  SourcePath { get; set; }
        public MoveAndMakeJsonService()
        {
            var config = new Configuration().InitConfiguration();
            SourcePath = config.GetSection("MoveMakeServiceSourcePath").Value;
            DestinationPath = config.GetSection("MoveMakeServiceDestinationPath").Value;
        }
        public void MoveAndMakeJson(Attributes attributes,string wwwRootPath)
        {
            try
            {
               
                RenameFile(attributes,wwwRootPath);
                MakeJson(attributes, wwwRootPath);

            }
            catch (Exception) { 
            
            }

        }

        private  void RenameFile(Attributes attributes, string wwwRootPath)
        {
            try
            {
                string oldPath = Path.Join(wwwRootPath, SourcePath, attributes.OldFileName);
                string newPath = Path.Join(wwwRootPath, DestinationPath,attributes.NewFileName);
                File.Move(oldPath, newPath);                
            }
            catch (Exception) { 
            }
            
        }
        public bool FileAlreadyExists(string wwwRootPath, string newFileName) {
            string newPath = Path.Join(wwwRootPath, DestinationPath, newFileName);
            return File.Exists(newPath);
        }


        public void MakeJson(Attributes attributes, string wwwRootPath)
        {
            try
            {
                
                string path = Path.Join(wwwRootPath, DestinationPath, attributes.NewFileName.Replace(".pdf", ".json"));
                string json = JsonConvert.SerializeObject(attributes, Formatting.Indented);
                using (FileStream fs = File.Create(path))
                {
                    using var sr = new StreamWriter(fs);
                    sr.Write(json);
                }

            }
            catch (Exception)
            { 
            
            }
           
            
        }
        public Attributes JsonExtract(string fileName, string wwwRootPath)
        {
            Attributes result = new Attributes();
            try {
                using (var sr = new StreamReader(Path.Join(wwwRootPath, SourcePath, fileName.Replace(".pdf", ".json"))))
                {
                    var json = sr.ReadToEnd();
                    result = JsonConvert.DeserializeObject<Attributes>(json);
                }
            }
            catch (Exception) { }
            
            return result;

        }
        public bool HasJson(string fileName, string wwwRootPath)
        {
            return (File.Exists(Path.Join(wwwRootPath, SourcePath,fileName.Replace(".pdf", ".json"))));
        }
        public void RemoveRedundantJsonFile(string fileName, string wwwRootPath) {
            try
            {
                var pathToJson = Path.Join(wwwRootPath, SourcePath, fileName);
                if (File.Exists(pathToJson))
                {
                    File.Delete(pathToJson);
                }
            }
            catch (Exception) { 
            
            }
        }
    }
}
