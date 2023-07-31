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
    public partial class FormCredenciais : Form
    {
        public FormCredenciais()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtSecretKey.Text != "" && txtAccessKeu.Text!="")
            {
                Properties.Settings.Default.secretKeyIAM = txtSecretKey.Text;
                Properties.Settings.Default.accessKeyIAM = txtAccessKeu.Text;
                Properties.Settings.Default.usuarioIAM = txtUser.Text;
                Properties.Settings.Default.Save();
                this.Close();
            }
            else
            {
                MessageBox.Show("Inform User and Key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
