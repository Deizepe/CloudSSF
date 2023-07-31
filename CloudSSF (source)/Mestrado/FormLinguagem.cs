using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mestrado
{
    public partial class FormLinguagem : Form
    {
        public FormLinguagem()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (cbLinguagem.Text != "")
            {
                Properties.Settings.Default.linguagem = cbLinguagem.Text;
                Properties.Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("Select a language", "Error",  MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
    }
}
