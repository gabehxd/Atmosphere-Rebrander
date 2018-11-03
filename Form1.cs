using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using LibGit2Sharp;

namespace Atmosphere_Rebrander
{
    public partial class Form1 : Form
    {
        //ReiNX Source Code Leaked! /s
        static DirectoryInfo tempPath = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "Atmosphere-Rebrander"));
        static DirectoryInfo atmosphereGit = new DirectoryInfo(Path.Combine(tempPath.FullName, "Atmosphere"));
        static DirectoryInfo boostsub = new DirectoryInfo(Path.Combine(atmosphereGit.FullName, "common", "include", "boost"));
        static FileInfo gitbootlogo = new FileInfo(Path.Combine(atmosphereGit.FullName, "fusee", "fusee-secondary", "src", "splash_screen.bmp"));
        public FileInfo newbootlogo;

        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) && newbootlogo != null)
            { 
            cte_button.Enabled = false;
            tempPath.Create();

                //work around for readonly file(s) in .git
                if (atmosphereGit.Exists)
                {
                    FileInfo[] Git = atmosphereGit.GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo file in Git)
                    {
                        File.SetAttributes(file.FullName, FileAttributes.Normal);
                    }
                    atmosphereGit.Delete(true);
                }
                 
                Repository.Clone("https://github.com/Atmosphere-NX/Atmosphere.git", atmosphereGit.FullName);
                //work around for submodule
                if (checkBox1.Checked) Repository.Clone("https://github.com/Atmosphere-NX/ext-boost.git", boostsub.FullName);

                gitbootlogo.Delete();
                File.Copy(newbootlogo.FullName, gitbootlogo.FullName);

                System.Collections.Generic.IEnumerable<string> repo = Directory.EnumerateFiles(atmosphereGit.FullName, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".cpp") || s.EndsWith(".h") || s.EndsWith(".c") || s.EndsWith(".hpp"));
                FileInfo[] makefiles = atmosphereGit.GetFiles("Makefile", SearchOption.AllDirectories);
                FileInfo readme = new FileInfo(Path.Combine(atmosphereGit.FullName, "README.md"));

                string lowerName = CultureInfo.CurrentCulture.TextInfo.ToLower(textBox1.Text);
                string upperName = CultureInfo.CurrentCulture.TextInfo.ToUpper(textBox1.Text);

                foreach (string file in repo)
                {
                    string filetext = File.ReadAllText(file);
                    if (filetext.Contains("AMS") || filetext.Contains("atmosphere") || filetext.Contains("Atmosphere") || filetext.Contains("ATMOSPHERE") || filetext.Contains("Atmosphère ") || filetext.Contains("Atmosphère "))
                    {
                        filetext = filetext.Replace("atmosphere", lowerName);
                        filetext = filetext.Replace("Atmosphere", textBox1.Text);
                        filetext = filetext.Replace("ATMOSPHERE", upperName);
                        filetext = filetext.Replace("AMS", textBox2.Text);
                        filetext = filetext.Replace("Atmosphère ", $"{textBox1.Text} ");
                        filetext += "\r\n//Modified by Atmosphere-Rebrander.";
                        File.WriteAllText(file, filetext);
                    }      
                }
                string readmetext = File.ReadAllText(readme.FullName);
                readmetext += "<br>\r\nModified by Atmosphere-Rebrander.";
                File.WriteAllText(readme.FullName, readmetext);

                foreach (FileInfo file in makefiles)
                {
                    string filetext = File.ReadAllText(file.FullName);
                    if (filetext.Contains("AMS") || filetext.Contains("atmosphere") || filetext.Contains("Atmosphere") || filetext.Contains("ATMOSPHERE"))
                    {
                        filetext = filetext.Replace("atmosphere", lowerName);
                        filetext = filetext.Replace("Atmosphere", textBox1.Text);
                        filetext = filetext.Replace("ATMOSPHERE", upperName);
                        filetext = filetext.Replace("AMS", textBox2.Text);
                        File.WriteAllText(file.FullName, filetext);
                    }
                }

                DirectoryInfo[] repoFolders = atmosphereGit.GetDirectories("atmosphere", SearchOption.AllDirectories);
                foreach (DirectoryInfo folder in repoFolders)
                {
                    string atmosfolder = folder.FullName;
                    atmosfolder = atmosfolder.Replace("atmosphere", lowerName);
                    folder.MoveTo(atmosfolder);
                }

                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    FileInfo save = new FileInfo(saveFileDialog1.FileName);
                    DirectoryInfo gitFolder = new DirectoryInfo(Path.Combine(atmosphereGit.FullName, ".git"));
                    FileInfo[] Git = gitFolder.GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo file in Git)
                    {
                      File.SetAttributes(file.FullName, FileAttributes.Normal);
                    }
                    gitFolder.Delete(true);
                    if (save.Exists) save.Delete();
                    ZipFile.CreateFromDirectory(atmosphereGit.FullName, save.FullName);
                }
                MessageBox.Show("Done!");
                cte_button.Enabled = true;
            }
            else
            {
                MessageBox.Show("Input Parameters");
            }
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                newbootlogo = new FileInfo(openFileDialog1.FileName);
                button1.Text = "Selected";
            }
        }
        
    }
}
