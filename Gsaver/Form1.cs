using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SearchOption = System.IO.SearchOption;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

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

            //if (!mini.KeyExists("index")) mini.Write("index", "0");
            // else mindex =  Convert.ToInt32( mini.Read("index"));

            


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
                label4.Text = "Current State:\r\n" + read_credentials(save_folder);
                mindex = findmaxindex()+1;
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

            String mcre = read_credentials(save_folder);

            String targ = "Gsave - " + mindex + " - " + DateTime.Now.ToString("dd.MM.yyyy - H;mm")+mcre;

            

            CopyAllFolder(save_folder, Gsaver_folder + targ+"\\");
            mindex++;
            mini.Write("index",mindex.ToString());
            refresh_folder(targ);
        }



        private void SortFoldersByName(ref DirectoryInfo[] dis)
        {
            Array.Sort(dis, delegate (DirectoryInfo fi1, DirectoryInfo fi2) { return fi2.Name.CompareTo(fi1.Name); });
        }

        private void SortFoldersByCreationTime(ref DirectoryInfo[] dis)
        {
            Array.Sort(dis, delegate (DirectoryInfo fi1, DirectoryInfo fi2) { return fi1.CreationTime.CompareTo(fi2.CreationTime); });
        }


        void refresh_folder(String sleect)
        {
            listBox1.Items.Clear();
            String[] saves = Directory.GetDirectories(Gsaver_folder);

            DirectoryInfo digsave = new DirectoryInfo(Gsaver_folder);
            DirectoryInfo[] msaves = digsave.GetDirectories();
            SortFoldersByCreationTime(ref msaves);
            
            

            foreach (DirectoryInfo dsave in msaves)
            {
                String save = dsave.Name;
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
             DialogResult mresult =  MessageBox.Show("Are you Sure?","Deleting", MessageBoxButtons.YesNo);
            if (mresult != DialogResult.Yes) return;
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
            DialogResult mresult = MessageBox.Show("Are you Sure?", "Overwriting", MessageBoxButtons.YesNo);
            if (mresult != DialogResult.Yes) return;

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
            DialogResult mresult = MessageBox.Show("Are you Sure?", "Loading", MessageBoxButtons.YesNo);
            if (mresult != DialogResult.Yes) return;

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

             sname = listBox1.SelectedItem.ToString();

            String mcre = read_credentials(Gsaver_folder+ sname+"\\");
            label5.Text = "Selected State:\r\n"+mcre;
        }

        private void textBox2_Leave(object sender, EventArgs e)
        {
           // System.Media.SystemSounds.Asterisk.Play();
        }


        String  read_credentials(String msave_ppath)
        {
            String retval = "";
            try
            {
                String[] mfiles = Directory.GetFiles(msave_ppath);

                String sfile = Array.Find(mfiles, mfile => (mfile.Contains("storage") && mfile.EndsWith(".dat")));
                sfile = Path.GetFileName(sfile);
                if (sfile == null) return "";
                sfile = msave_ppath + sfile;

                String fcont = File.ReadAllText(sfile);


                JObject stor = JObject.Parse(fcont);

                JToken JParent = stor.SelectToken("$.objects[?(@.name=='Game')].componentData2[0].data");
                JObject JChild = JObject.Parse(JParent.ToString());
                String landValue = JChild["land"].ToString();
                int landint = Convert.ToInt32(landValue) + 1;


                JParent = stor.SelectToken("$.objects[?(@.name=='Director')].componentData2[0].data");
                JChild = JObject.Parse(JParent.ToString());
                String currentday = JChild["currentDay"].ToString();
                int dayint = Convert.ToInt32(currentday) + 1;

                retval = " [Land=" + landint + "  Day=" + dayint + "]";
            }
            catch { }
            return retval;

        }


        int findmaxindex()
        {
            int retval = 0;

            
            String[] saves = Directory.GetDirectories(Gsaver_folder);

            DirectoryInfo digsave = new DirectoryInfo(Gsaver_folder);
            DirectoryInfo[] msaves = digsave.GetDirectories();
            SortFoldersByCreationTime(ref msaves);


            List<String> mlist = new List<String>();
            foreach (DirectoryInfo dsave in msaves)
            {
                String save = dsave.Name;
                if (Path.GetFileName(save).StartsWith("Gsave"))
                {
                    String sname = Path.GetFileName(save);
                    string mmind = sname[8..(sname.IndexOf(" - ", 5) + 5)];
                    int mmindint = Convert.ToInt32(mmind);
                    if (mmindint > retval) retval = mmindint;
                }
            }
            return retval;
        }
    }
    


}