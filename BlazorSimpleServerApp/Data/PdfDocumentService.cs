using BlazorSimpleServerApp.Common;
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
        static PdfDocument FillPdfDocumentAttributes(FileSystemInfo fsi)
        {
            return new PdfDocument((string.Format("{0}", fsi.FullName)), fsi.GetHashCode(), fsi.Name, fsi.CreationTime);
        }
        public Task<PdfDocument[]> GetPdfDocumentsAsync()
        {
            var config = new Configuration().InitConfiguration();
            var dirPath = config.GetSection("DocumentsPath").Value;
            var files = Directory.GetFiles(dirPath, "*.pdf");
            List<PdfDocument> pdfDocuments=new List<PdfDocument>();
            foreach (var path in files)
            {
                var doc = FillPdfDocumentAttributes(new FileInfo(path));
                pdfDocuments.Add(doc);
            }
            return Task.FromResult(pdfDocuments.ToArray());
        }
    }
}
