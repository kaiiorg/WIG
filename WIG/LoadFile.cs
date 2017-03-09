using System;
using System.Collections.Generic;
using kaiiorg.wallplate;
using System.IO;
using System.Windows.Forms;

namespace WIG
{
    /// <summary>
    /// Class to load data from various file types
    /// </summary>
    static class LoadFile
    {
        //Load data from a Comma Seperated Value (CSV) file
        static public List<Plate> fromCSV(ref int plateCount)
        {
            List<Plate> teams = new List<Plate>();
            plateCount = 0;

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = ".CSV Files (*.csv) | *.csv";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                StreamReader file = new StreamReader(ofd.OpenFile());
                List<string> lines = new List<string>();


                //Read each line and add to the lines list
                while (!file.EndOfStream)
                    lines.Add(file.ReadLine());

                //Split each line, put values in Team object, put Team object in list
                foreach (string line in lines)
                {
                    string[] s = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    

                }
                file.Close();
            }
            else
            {
                throw new Exception("No file selected.");
            }

            return teams;
        }

        /// <summary>
        /// Return a string with the template of the .CSV needed to load data from.
        /// Save the string directly to a file with .CSV extention
        /// </summary>
        /// <returns></returns>
        static public string getTemplate()
        {
            throw new NotImplementedException("getTemplate() isn't impletmented yet.");
        }
    }
}
