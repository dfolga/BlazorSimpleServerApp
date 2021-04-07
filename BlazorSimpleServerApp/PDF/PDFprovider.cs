using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSimpleServerApp.PDF
{
    public class PDFprovider
    {
        public static void Pdf()
        {

            string pattern = "***___***";
            PdfDocument inputDocument = PdfReader.Open("Test.pdf", PdfDocumentOpenMode.Import);
            for (int i = 0; i < inputDocument.PageCount; i++)
            {
                var content = ContentReader.ReadContent(inputDocument.Pages[i]);
                if (isPatternFounded(content, pattern))
                {
                    Console.WriteLine("Pattern you are looking for: " + pattern + " founded on page: " + (i + 1));
                }

            }

        }
        public static bool isPatternFounded(CSequence contents, string searchText)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i] is COperator)
                {
                    var cOp = contents[i] as COperator;
                    for (int j = 0; j < cOp.Operands.Count; j++)
                    {
                        if (cOp.OpCode.Name == OpCodeName.Tj.ToString() ||
                            cOp.OpCode.Name == OpCodeName.TJ.ToString())
                        {
                            if (cOp.Operands[j] is CString)
                            {
                                var cString = cOp.Operands[j] as CString;
                                if (cString.Value.Contains(searchText))
                                {
                                    return true;
                                }

                            }
                        }
                    }
                }

            }
            return false;
        }
    }
}
