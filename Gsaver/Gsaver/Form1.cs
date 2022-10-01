using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SearchOption = System.IO.SearchOption;

namespace Gsaver
{
    public partial class Form1 : Form
    {
        String save_folder = "";
        String ex_file= "";
        int mindex = 0;
        String Gsaver_folder = "";
        IniFile mini = new IniFile();
        public Form1()
        {
            InitializeComponent();
            
            if (!File.Exists(mini.Path)) File.WriteAllText(mini.Path, "");
            
            if (!mini.KeyExists("save_folder")) mini.Write("save_folder", "");
            else save_folder = mini.Read("save_folder");

            if (!mini.KeyExists("index")) mini.Write("index", "0");
            else mindex =  Convert.ToInt32( mini.Read("index"));


            if (save_folder=="")
            {
                string userName = Environment.UserName;
                string mss = "C:\\Users\\" + userName + "\\AppData\\LocalLow\\noio\\Kingdom\\";
                if (Directory.Exists(mss))
                {
                    MessageBox.Show("I found this Save folder (Kingdom folder)\r\n" + mss);
                    save_folder = mss;
                    mini.Write("save_folder", save_folder);
                }
            }

            

            if (save_folder != "")
            {
                Gsaver_folder = Directory.GetParent(save_folder).Parent.FullName+"\\";
                textBox1.Text = save_folder;
                ex_file = save_folder + "output_log.txt";
            }

            
            

            refresh_folder(null);
          
        }

        private void button1_Click(object sender, EventArgs e) //folder locaion
        {
            DialogResult mres = folderBrowserDialog1.ShowDialog();
            if(mres == DialogResult.OK)
            {
                save_folder = folderBrowserDialog1.SelectedPath+"\\";
                textBox1.Text = save_folder;
                mini.Write("save_folder", save_folder);
            }

        }

        private void button2_Click(object sender, EventArgs e) //create save new
        {
            String targ = "Gsave - " + mindex + " - " + DateTime.Now.ToString("dd.MM.yyyy - H;mm");
            CopyAllFolder(save_folder, Gsaver_folder + targ+"\\");
            mindex++;
            mini.Write("index",mindex.ToString());
            refresh_folder(targ);
        }






        void refresh_folder(String sleect)
        {
            listBox1.Items.Clear();
            String[] saves = Directory.GetDirectories(Gsaver_folder);
            foreach (String save in saves)
            {
                if (Path.GetFileName(save).StartsWith("Gsave"))
                {
                    String mname = Path.GetFileName(save);
                    listBox1.Items.Add(mname);
                }
            }
            if (sleect != null) listBox1.SelectedItem = sleect;
        }



        void CopyAllFolder(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if(newPath!=ex_file) File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }




        private void button3_Click(object sender, EventArgs e)  //delete
        {
            if (listBox1.SelectedItem == null) { MessageBox.Show("select save"); return; }
            String sname = listBox1.SelectedItem.ToString();
            //Directory.Move(Gsaver_folder + "sname", Gsaver_folder + "XX" + sname);
            Directory.Delete(Gsaver_folder + sname, true);
            
            string mmind = sname[8..(sname.IndexOf(" - ", 5) + 5)];
            write_comment(mmind, "");
            
                
                refresh_folder(null);
        }

        private void button5_Click(object sender, EventArgs e) //overwrite
        {
            if (listBox1.SelectedItem == null) { MessageBox.Show("select save"); return; }
            String sname = listBox1.SelectedItem.ToString();
            Directory.Delete(Gsaver_folder + sname+"\\", true);
            sname = sname.Substring(0, 12)+ DateTime.Now.ToString("dd.MM.yyyy - H;mm");
            Directory.CreateDirectory(Gsaver_folder + sname+"\\");
            CopyAllFolder(save_folder, Gsaver_folder + sname+"\\");
            refresh_folder(sname);
            MessageBox.Show("Success (^_^)");
        }

        private void button4_Click(object sender, EventArgs e)//load
        {
            if (listBox1.SelectedItem == null) { MessageBox.Show("select save"); return; }
            String sname = listBox1.SelectedItem.ToString()+"\\";
            CopyAllFolder(Gsaver_folder + sname,save_folder);
            MessageBox.Show("Success (^_^)");
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }



        string read_comment(String id)
        {
            string retval = "";
            if (!Directory.Exists(Gsaver_folder + "Comments\\")) Directory.CreateDirectory(Gsaver_folder + "Comments\\");
            try
            {
                 retval = File.ReadAllText(Gsaver_folder + "Comments\\"+ id + ".txt");
            }
            catch { }
            return retval;
        }

        void write_comment(string id,string comment)
        {
            string retval = "";
            if (!Directory.Exists(Gsaver_folder + "Comments\\")) Directory.CreateDirectory(Gsaver_folder + "Comments\\");
            try
            {
                File.WriteAllText(Gsaver_folder + "Comments\\" + id + ".txt",comment);
            }
            catch { }
        }

        private void button6_Click(object sender, EventArgs e) //save comment
        {
            if (listBox1.SelectedItem == null) { MessageBox.Show("select save"); return; }
            String sname = listBox1.SelectedItem.ToString();

            string mmind = sname[8..(sname.IndexOf(" - ",5)+5)];
            write_comment(mmind, textBox2.Text);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) {  return; }
            String sname = listBox1.SelectedItem.ToString();

            string mmind = sname[8..(sname.IndexOf(" - ", 5) + 5)];
            textBox2.Text= read_comment(mmind);

        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
           // System.Media.SystemSounds.Asterisk.Play();
        }
    }
    


}