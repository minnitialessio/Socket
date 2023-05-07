using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Socket_4I
{
    /// <summary>
    /// Logica di interazione per Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        public Persona Persona { set; get; }
        public Register()
        {
            InitializeComponent();
        }

        private void btnRegistrati_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Persona = new Persona(txtNome.Text, int.Parse(txtPorta.Text));
                Close();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtNome_GotFocus(object sender, RoutedEventArgs e)
        {
            txtNome.Text = "";
        }

        private void txtPorta_GotFocus(object sender, RoutedEventArgs e)
        {
            txtPorta.Text = "";
        }
    }
}
