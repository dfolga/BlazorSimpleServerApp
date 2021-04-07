using PdfSharp.Pdf;
using PdfSharp.Pdf.Content;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSimpleServerApp.Helpers
{
    public class PdfPatternRecognitionTool
    {
            public static string GetPagesWithPattern(string path)
            {
                string pattern = "***___***";
                List<int> result=new List<int>();
                PdfDocument inputDocument = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                for (int i = 0; i < inputDocument.PageCount; i++)
                {
                    var content = ContentReader.ReadContent(inputDocument.Pages[i]);
                if (isPatternFounded(content, pattern))
                {
                    result.Add(i + 1);
                }
                }
            return string.Join<int>(",", result);

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
