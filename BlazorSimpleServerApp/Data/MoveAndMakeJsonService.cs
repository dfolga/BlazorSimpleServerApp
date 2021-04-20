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
            SourcePath = @"\datagrid2_pdfs\";
            DestinationPath = @"\datagrid2_result\";
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
                string oldPath = wwwRootPath + SourcePath+ attributes.OldFileName;
                string newPath = wwwRootPath + DestinationPath+ attributes.NewFileName;
                File.Move(Path.Combine(wwwRootPath, oldPath), Path.Combine(wwwRootPath, newPath));
            }
            catch (Exception) { 
            }
            
        }

       
        public void MakeJson(Attributes attributes, string wwwRootPath)
        {
            try
            {
                
                string path = wwwRootPath + DestinationPath + attributes.NewFileName.Replace(".pdf",".json");
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
            using (var sr = new StreamReader(wwwRootPath+SourcePath+ fileName.Replace(".pdf", ".json")))
            {
                var json=sr.ReadToEnd();
                result=JsonConvert.DeserializeObject<Attributes>(json);
            }
            return result;

        }
        public bool HasJson(string fileName, string wwwRootPath)
        {
            return (File.Exists(wwwRootPath + SourcePath + fileName.Replace(".pdf", ".json")));
        }

    }
}
