using BlazorSimpleServerApp.Common;
using Microsoft.AspNetCore.Hosting;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSimpleServerApp.Data
{
    public class PdfDocumentService
    {
        public string DirectoryFromConfigFile { get; set; }
        private readonly IWebHostEnvironment _hostingEnvironment;
        public string WwwRootPath { get; set; }
        public PdfDocumentService(IWebHostEnvironment hostingEnvironment) {
            _hostingEnvironment = hostingEnvironment;
        }
        static PdfDocument FillPdfDocumentAttributes(FileSystemInfo fsi)
        {
            return new PdfDocument(fsi.FullName, fsi.GetHashCode(), fsi.Name, fsi.CreationTime);
        }
        public Task<List<PdfDocument>> GetPdfDocumentsAsync()
        {
            WwwRootPath = _hostingEnvironment.WebRootPath;
            var config = new Configuration().InitConfiguration();
            var dirPath = config.GetSection("DocumentsPath").Value;
            DirectoryFromConfigFile = dirPath;
                var files = Directory.GetFiles(WwwRootPath + dirPath, "*.pdf");
              List<PdfDocument> pdfDocuments = new List<PdfDocument>();
            foreach (var path in files)
            {
                if (File.Exists(path))
                {
                    var doc = FillPdfDocumentAttributes(new FileInfo(path));
                    pdfDocuments.Add(doc);
                }
            }
            return Task.FromResult(pdfDocuments);
        }
    }
}
