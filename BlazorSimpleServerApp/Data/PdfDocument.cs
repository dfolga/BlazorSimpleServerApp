using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PdfSharp.Pdf;

namespace BlazorSimpleServerApp.Data
{
    public class PdfDocument
    {
        public string PdfPath { get; set; }

        public int Id { get; set; }

        public string FileName { get; set; }

        public DateTime Date { get; set; }
        public PdfDocument(string _PdfPath, int _Id, string _FileName, DateTime _Date)
        {
            PdfPath = _PdfPath;
            Id = _Id;
            FileName = _FileName;
            Date = _Date;
        }
    }
}
