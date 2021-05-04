using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSimpleServerApp.Data
{
    public class DirectorData
    {
        public string NewFileName { get; set; }
        public string OldFileName { get; set; }
        public string Account { get; set; }
        public DateTime Date { get; set; }
        public string DocumentType { get; set; }
        public string PagesToRemove { get; set; }

        public string Note { get; set; }
    }
}
