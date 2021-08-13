using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Musics_Manage
{
    class FileWriter
    {
        string fileName;
        List<string> listLines;
        bool overwrite;
        

        public FileWriter(string _fileName)
        {
            fileName = _fileName;
        }

        public void WriteFile(List<string> _listLines, bool _overwrite)
        {
            if (!File.Exists(fileName))
                CreateFile();

            listLines = _listLines;
            overwrite = _overwrite;

            WriteText();
        }

        private void WriteText()
        {
            try
            {
                using (TextWriter writer = new StreamWriter(fileName, !overwrite))
                {
                    foreach (string line in listLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch
            {
                throw new Exception("Impossible de créer le fichier : " + fileName);
            }
        }

        private void CreateFile()
        {
            var file = File.Create(fileName);
            file.Close();
        }

    }
}
