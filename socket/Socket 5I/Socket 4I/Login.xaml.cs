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
    /// Logica di interazione per Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        Rubrica rubricaLogin;
        public Persona Utente { set; get; }
        public Login(Rubrica rubrica) //login
        {
            InitializeComponent();
            rubricaLogin = rubrica;
            Aggiorna();
        }

        private void Aggiorna() //aggiorno gli utenti selezionabili
        {
            lstRubrica.ItemsSource = null;
            lstRubrica.ItemsSource = rubricaLogin.Persone;
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(lstRubrica.SelectedItem != null)
                {
                    Utente = lstRubrica.SelectedItem as Persona; //prendo l'utente selezionato come sender
                    Close();
                }
                else
                {
                    throw new Exception("SELEZIONA UN UTENTE");
                }
                
            }catch(Exception ex)
            {
                MessageBox.Show("ERRORE " + ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) //non posso chiudere la finestra se non faccio prima il login
        {
            if (Utente == null)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(
                  "Selezionare un utente", "Warning",
                  System.Windows.MessageBoxButton.OK);
                e.Cancel = true;
            }
        }

        private void txtRegister_MouseDown(object sender, MouseButtonEventArgs e) //per il register
        {
            try
            {
                Register register = new Register();
                register.ShowDialog();

                foreach (Persona p in rubricaLogin.Persone) //controllo che non si sia registrato con una porta già in uso
                {
                    if (register.Persona.Porta == p.Porta)
                    {
                        throw new Exception("IMPOSSIBILE REGISTRARSI CON QUESTA PORTA PERCHE' GIA' ASSEGNATA");
                    }
                }

                rubricaLogin.Persone.Add(register.Persona); //aggiorno la rubrica
                Aggiorna();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}