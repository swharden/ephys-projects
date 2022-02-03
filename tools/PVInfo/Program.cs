using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace PVInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderOfScans = @"X:\Data\OT-Cre\OT-Tom-F5-NE\2022-01-03-practice\2p";
            Report.CreateMultiFolderIndex(folderOfScans);
        }
    }
}