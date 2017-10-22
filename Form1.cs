using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EpgData
{
    public partial class Form1 : Form
    {
        private EpgData _epgData;
       
        public Form1()
        {
            InitializeComponent();
            _epgData = new EpgData();
            _epgData.Message += _epgData_Message;
        }
        delegate void SetControlPropertyThreadSafeDelegate(Control control, string propertyName, object propertyValue);
        private static void SetControlPropertyThreadSafe(Control control, string propertyName, object propertyValue)
        {
            if(control.InvokeRequired)
            {
                control.Invoke(new SetControlPropertyThreadSafeDelegate(SetControlPropertyThreadSafe), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }
        }
        private void _epgData_Message(string message)
        {
            SetControlPropertyThreadSafe(label3, "Text", message);            
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                _epgData.LoadEpgDataInclude(textBox1.Text);
                _epgData.LoadEpgDataPrograms(textBox1.Text, (int)(numericUpDown1.Value));
                btnConvert.Enabled = true;
            }
            else
            {
                MessageBox.Show("You must enter the EpgData.com Pin!", "Problem");
            }            
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text))
            { 
                if (File.Exists(textBox2.Text))
                {
                    File.Delete(textBox2.Text);
                }
                using (var filestream = new FileStream(textBox2.Text, FileMode.CreateNew))
                {
                    var xmltv = new XmlTv();
                    xmltv.CreateXMLTV(_epgData, filestream);
                }
                SetControlPropertyThreadSafe(label3, "Text", "Done");
            }
            else { MessageBox.Show("You must enter the an Location for the output!", "Problem"); }

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = string.Format(folderBrowserDialog1.SelectedPath+"\\TvGuide.xml");
                Properties.Settings.Default.Output = string.Format(folderBrowserDialog1.SelectedPath + "\\TvGuide.xml");
                Properties.Settings.Default.Save();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnConvert.Enabled = false;
            textBox1.Text = Properties.Settings.Default.Pin;
            numericUpDown1.Value = Properties.Settings.Default.Days;
            textBox2.Text = Properties.Settings.Default.Output;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Pin = textBox1.Text;
            Properties.Settings.Default.Save();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Days = numericUpDown1.Value;
            Properties.Settings.Default.Save();
        }
    }

    public class GuideChannel
    {
        public string ChannelId { get; internal set; }
        public string DisplayName { get; internal set; }
        public string CallSign { get; internal set; }
        public string Language { get; internal set; }
    }

    public class GuideProgram
    {
        //d0
        public string BroadcastId { get; internal set; }
        //d1
        public string GuideProgramId { get; internal set; }
        //d2
        public string ChannelId { get; internal set; }        
        //d4
        public DateTime StartTime { get; internal set; }
        //d5
        public DateTime StopTime { get; internal set; }
        //d7
        public String Duration { get; internal set; }
        //d9
        public string PrimeTime { get; internal set; }
        //d10
        public string Category { get; internal set; }
        //d19
        public String Title { get; internal set; }
        //d20
        public String EpisodeTitle { get; internal set; }
        //d21
        public String Description { get; internal set; }
        //d25
        public string Genre { get; internal set; }
        //d26
        public string EpisodeNumber { get; internal set; }        
        //d32
        public String Country { get; internal set; }
        //d33
        public String Date { get; internal set; }
        //d34
        public string Presenter { get; internal set; }
        //d35
        public string Guest { get; internal set; }
        //d36
        public string Director { get; internal set; }
        //d37
        public Actor[] Actors { get; internal set; }
        //16
        public string Rating { get; internal set; }
        //30
        public string StarRating { get; internal set; }
    }

    public class Actor
    {
        public string Name { get; internal set; }
        public string Role { get; internal set; }
    }
}
