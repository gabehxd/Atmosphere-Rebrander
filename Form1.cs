using System;
using System.Collections.Generic;
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
        //create variables that can be used anywhere
        static DirectoryInfo TempPath => new DirectoryInfo(Path.Combine(Path.GetTempPath(), "Atmosphere-Rebrander"));
        static DirectoryInfo Baregit => new DirectoryInfo(Path.Combine(TempPath.FullName, "bare"));
        static DirectoryInfo AtmosphereGit => new DirectoryInfo(Path.Combine(TempPath.FullName, "Atmosphere"));
        static DirectoryInfo Boostsub => new DirectoryInfo(Path.Combine(AtmosphereGit.FullName, "common", "include", "boost"));
        static FileInfo Gitbootlogo => new FileInfo(Path.Combine(AtmosphereGit.FullName, "fusee", "fusee-secondary", "src", "splash_screen.bmp"));
        static FileInfo Readme => new FileInfo(Path.Combine(AtmosphereGit.FullName, "README.md"));
        static FileInfo[] Makefiles => AtmosphereGit.GetFiles("Makefile", SearchOption.AllDirectories);
        public FileInfo newbootlogo;

        public Form1()
        {
            InitializeComponent();

            TempPath.Create();

            //remove .git file if they exist
            if (Baregit.Exists)
            {
                FileInfo[] files = Baregit.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (FileInfo file in files)
                {
                    File.SetAttributes(file.FullName, FileAttributes.Normal);
                }
                Baregit.Delete(true);
            }

            //only grab .git to get all branches
            CloneOptions init = new CloneOptions
            {
                IsBare = true
            };
            Repository.Clone("https://github.com/Atmosphere-NX/Atmosphere.git", Baregit.FullName, init);
            Repository repo = new Repository(Baregit.FullName);
            //grab all branches for .git folder
            foreach (Branch b in repo.Branches.Where(b => b.IsRemote))
            {
                //remove unnessary part of string
                string item = b.ToString().Replace("refs/remotes/origin/", "");
                comboBox1.Items.Add(item);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //make sure all params are filled in
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) && newbootlogo != null && comboBox1.SelectedItem != null)
            { 
                //disable create box while working
                cte_button.Enabled = false;

                //work around for readonly file(s) in .git
                //check if full Atmosphere git exists
                if (AtmosphereGit.Exists)
                {
                    //make an array of all files within it
                    FileInfo[] Git = AtmosphereGit.GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo file in Git)
                    {
                        //make them modifiable 
                        File.SetAttributes(file.FullName, FileAttributes.Normal);
                    }
                    AtmosphereGit.Delete(true);
                }

                //take the branch to clone from combobox
                CloneOptions options = new CloneOptions
                {
                    BranchName = comboBox1.SelectedItem.ToString()
                };
                //check if the user want submodules
                if (checkBox1.Checked) options.RecurseSubmodules = true;
                Repository.Clone("https://github.com/Atmosphere-NX/Atmosphere.git", AtmosphereGit.FullName, options);

                //boot logo replacement
                Gitbootlogo.Delete();
                File.Copy(newbootlogo.FullName, Gitbootlogo.FullName);

                //grab files in the repo
                IEnumerable<string> repo = Directory.EnumerateFiles(AtmosphereGit.FullName, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".cpp") || s.EndsWith(".h") || s.EndsWith(".c") || s.EndsWith(".hpp"));
                FileInfo[] ini = AtmosphereGit.GetFiles("*.ini", SearchOption.AllDirectories);

                //remove lz4 as it contains "STREAMS"
                List<string> strings = repo.ToList();
                foreach (string str in strings.ToList())
                {
                    if (str.Contains("lz4")) strings.Remove(str);
                }

                //change strings for certain replacements
                string lowerName = CultureInfo.CurrentCulture.TextInfo.ToLower(textBox1.Text);
                string upperName = CultureInfo.CurrentCulture.TextInfo.ToUpper(textBox1.Text);

                //actually replace strings
                foreach (string file in strings)
                {
                    string filetext = File.ReadAllText(file);
                    if (filetext.Contains("AMS") || filetext.Contains("atmosphere") || filetext.Contains("Atmosphere") || filetext.Contains("ATMOSPHERE") || filetext.Contains("Atmosphère ") || filetext.Contains("Atmosphère ") || filetext.Contains("Atmosph\\xe8re"))
                    {
                        filetext = filetext.Replace("atmosphere", lowerName);
                        filetext = filetext.Replace("Atmosphere", textBox1.Text);
                        filetext = filetext.Replace("ATMOSPHERE", upperName);
                        filetext = filetext.Replace("AMS", textBox2.Text);
                        filetext = filetext.Replace("Atmosphère ", $"{textBox1.Text} ");
                        filetext = filetext.Replace("Atmosph\\xe8re", textBox1.Text);
                        filetext += "\r\n//Modified by Atmosphere-Rebrander.";
                        File.WriteAllText(file, filetext);
                    }      
                }
                //readme replace strings
                string readmetext = File.ReadAllText(Readme.FullName);
                readmetext += "\r\nGit modified by [Atmosphere-Rebrander](https://github.com/SunTheCourier/Atmosphere-Rebrander).";
                File.WriteAllText(Readme.FullName, readmetext);

                //makefile replace strings
                foreach (FileInfo file in Makefiles)
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

                //rename folders
                DirectoryInfo[] repoFolders = AtmosphereGit.GetDirectories("atmosphere", SearchOption.AllDirectories);
                foreach (DirectoryInfo folder in repoFolders)
                {
                    string atmosfolder = folder.FullName;
                    atmosfolder = atmosfolder.Replace("atmosphere", lowerName);
                    folder.MoveTo(atmosfolder);
                }

                //remove .git folder prompt for save location and then zip
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    FileInfo save = new FileInfo(saveFileDialog1.FileName);
                    DirectoryInfo gitFolder = new DirectoryInfo(Path.Combine(AtmosphereGit.FullName, ".git"));
                    FileInfo[] Git = gitFolder.GetFiles("*.*", SearchOption.AllDirectories);
                    foreach (FileInfo file in Git)
                    {
                      File.SetAttributes(file.FullName, FileAttributes.Normal);
                    }
                    gitFolder.Delete(true);
                    if (save.Exists) save.Delete();
                    ZipFile.CreateFromDirectory(AtmosphereGit.FullName, save.FullName);
                    saveFileDialog1.FileName = "";
                    MessageBox.Show("Done!");
                }
                else
                {
                    MessageBox.Show("Canceled!");
                }
                //reenable create button (we are done)
                cte_button.Enabled = true;
            }
            else
            {
                MessageBox.Show("Input Parameters");
            }
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            //prompt user to select BMP file
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //put the boot logo selected into newbootlogo variable
                newbootlogo = new FileInfo(openFileDialog1.FileName);
                button1.Text = "Selected";
            }
        }
    }
}
