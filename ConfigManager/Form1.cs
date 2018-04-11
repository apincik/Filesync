using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigManager
{
    public partial class Form1 : Form
    {
        string xmlFilePath = "";

        public Form1()
        {
            InitializeComponent();
        }

        /**
         * Save config 
         */
        private void button1_Click(object sender, EventArgs e)
        {
            string[] array = new string[4];

            array[0] = textBox1.Text;       //ftp root path
            array[1] = textBox2.Text;       //local path
            array[2] = textBox3.Text;       //username
            array[3] = textBox4.Text;       //password

            bool result = Config.WriteConfig(xmlFilePath, array);

            if (result == false)
            {
                MessageBox.Show("Error occured, config has not been written.");
            }
            else
            {
                MessageBox.Show("Config has been written successfuly.");
            }
        
        }

        /**
         *Load config file. 
         */
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = openFileDialog1.ShowDialog();
            if(dialogResult == DialogResult.OK)
            {
                //load config
                xmlFilePath = openFileDialog1.FileName;
                Config.ReadConfig(xmlFilePath);

                textBox1.Text = Config.FTPRootPath;
                textBox2.Text = Config.LocalRootPath;
                textBox3.Text = Config.Username;
                textBox4.Text = Config.Pasword;

                loadedFileLabel.Text = String.Format("Loaded file: {0}", xmlFilePath);
            }

        }
    }
}
