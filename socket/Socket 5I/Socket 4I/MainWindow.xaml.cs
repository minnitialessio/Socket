using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Security.Principal;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using Microsoft.Win32;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Diagnostics.Eventing.Reader;

namespace Socket_4I
{
    /// <summary>
    /// creazione di una comunicazione udp con le socket
    /// </summary>
    public partial class MainWindow : Window
    {
        //METTERE COMMENTI
        
        Rubrica rubrica; 
        Socket socket = null;
        //DispatcherTimer dTimer = null;
        Persona Sender;
        Persona Receiver;
        public MainWindow()
        {
            InitializeComponent();
            rubrica = new Rubrica();
            rubrica.Persone.Add(new Persona("Alessio", 10000)); //inserisco tutti i contatti nella rubrica
            rubrica.Persone.Add(new Persona("Francesco", 11000));
            rubrica.Persone.Add(new Persona("Enrico", 12000));
            rubrica.Persone.Add(new Persona("Giacomo", 13000));
            rubrica.Persone.Add(new Persona("Matteo", 14000));
            rubrica.Persone.Add(new Persona("Mario", 15000));
            rubrica.Persone.Add(new Persona("Luca", 16000));
            Login login = new Login(rubrica); //l'utente esegue il login
            login.ShowDialog();
            Sender = login.Utente;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //creo la socket col protocollo udp

            
            IPAddress local_address = IPAddress.Any; //imposto l'ip address
            IPEndPoint local_endpoint = new IPEndPoint(local_address, Sender.Porta); //impsoto l'endpoint con l'ip address e la porta del sender

            socket.Bind(local_endpoint); //con il bind associo il local endpoint (caratterizzato dalla coppia ip e porta) alla socket

            //socket.Blocking = false;
            //socket.EnableBroadcast = true;

            Task.Run(RiceviMessaggio); //gestisco la ricezione dei messaggi con le task
            //dTimer = new DispatcherTimer();

            //dTimer.Tick += new EventHandler(aggiornamento_dTimer);
            //dTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            //dTimer.Start();
            Aggiorna(); //visualizzo la rubrica in wpf
        }
        private void Aggiorna() //aggiorno la rubrica in wpf
        {
            foreach(Persona p in rubrica.Persone)
            {
                lstRubrica.Items.Add(p);
            }

            lstRubrica.Items.Remove(Sender);
        }
        private bool IsBase64Encoded(string stringa) //controllo se il messaggio dato è una foto controllando se contiene uno "/" dato che è l'unico parametro controllabile per le stringhe base64
        {
            try
            {
                return stringa.Contains("/");
            }
            catch
            {
                return false;
            }
        }
        private Task RiceviMessaggio() //ricezione messaggio gestito con le task
        {
            while (true)
            {
                
                    int nBytes;
                    //ricezione dei caratteri in attesa
                    byte[] buffer = new byte[60000];

                    EndPoint remoreEndPoint = new IPEndPoint(IPAddress.Any, 0); //creo remote endpoint

                    nBytes = socket.ReceiveFrom(buffer, ref remoreEndPoint);//restituisco il numero di byte letti correttamente e acquisisco l'endpoint host remoto da cui sono stati inviati i dati

                    int from = ((IPEndPoint)remoreEndPoint).Port; //prendo la porta del remote endpoint


                    string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes); //trasformo i bite ricevuti dalla trasmissione in stringa

                    string nome = "unknown";

                    if (Receiver != null && from == Receiver.Porta) //controllo se la chat con il remote endpoint è già aperta
                    {
                        //entro se la chat è già aperta
                        if (IsBase64Encoded(messaggio)) //controllo se è una stringa in base64
                        { //entro se è base64 encoded

                            System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(messaggio)));  //converto la stringa in immagine

                            lstMessaggi.Dispatcher.Invoke(() => //mostro l'immagine
                            {
                                System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image(); //per mostrare l'immagine devo convertirla da System.Drawing a System.Windows.Controls
                                wpfImage.Source = ConvertImageToBitmapImage(image); //prendo il source con una conversione da image a bitmap
                                wpfImage.Width = 200;
                                wpfImage.Margin = new Thickness(5);
                                ListBoxItem listBoxItem = new ListBoxItem(); //per metterla nella listbox devo creare un listbox item
                                listBoxItem.Content = nome + wpfImage; //metto l'immagine nel listgbox item
                                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Left; //allineo a sinistra
                                lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(listBoxItem));
                                Messaggio m = new Messaggio(messaggio, Receiver); //salvo il messaggio
                                Receiver.Chat.Add(m);
                            }

                            );
                        }
                        else
                        { //se non è un'immagine salvo il messaggio normalmente
                            nome = Receiver.Nome;
                            lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(nome + " : " + messaggio));
                            Messaggio m = new Messaggio(messaggio, Receiver);
                            Receiver.Chat.Add(m);
                        }

                    }
                    else
                    { //entro se la chat con il remote endpoint non è aperta
                        foreach (Persona p in rubrica.Persone) //controllo se il messaggio viene da un contatto già salvato
                        {
                            if (from == p.Porta)
                            {
                                nome = p.Nome; //salvo la chat
                                Messaggio m = new Messaggio(messaggio, p);
                                p.Chat.Add(m);
                            }
                        }
                        if (IsBase64Encoded(messaggio)) //guardo se il messaggio è un'immagine
                        {
                            lstNotifiche.Dispatcher.Invoke(() => lstNotifiche.Items.Add(nome + " : " + "image"));
                        }
                        else
                        {
                            lstNotifiche.Dispatcher.Invoke(() => lstNotifiche.Items.Add(nome + " : " + messaggio));
                        }

                        if (nome == "unknown") //se viene da un contatto non salvato scrivo unknown
                        {
                            rubrica.Persone.Add(new Persona("unknown", from));
                            Aggiorna(); //nella rubrica ci aggiungo un contatto senza nome
                        }
                    }
                

            }
            return Task.CompletedTask;
        }
        private BitmapImage ConvertImageToBitmapImage(System.Drawing.Image image) //converto l'immagine da image a bitmap
        {
            Bitmap bitmap = new Bitmap(image);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat); //setto l'immagine bitmap

            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Bgr24, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit(); //imposto il source del'immagine in bitmap
            bitmapImage.StreamSource = new MemoryStream();
            bitmap.Save(bitmapImage.StreamSource, ImageFormat.Bmp);
            bitmapImage.EndInit();

            return bitmapImage;
        }
        private void AggiornaNotifiche() //aggiorno il listbox delle notifiche ogni volta che cambio chat
        {
            if(Receiver != null)
            {
                for (int i = 0; i < lstNotifiche.Items.Count; i++)
                {
                    
                    if (lstNotifiche.Items[i].ToString().Contains(Receiver.Nome))
                    {
                        lstNotifiche.Items.RemoveAt(i);
                    }
                }
            }
           
        }

        private void btnInviaBroadcast_Click(object sender, RoutedEventArgs e) //invio i messaggi via broadcast
        {
            string hostName = Dns.GetHostName(); //prendo l'host name
            IPAddress myIP = Dns.GetHostByName(hostName).AddressList[0]; //prendo l'ip del pc dall'host name

            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text); //trasformo il messaggio da string ad array di byte usando UTF8

            foreach (Persona p in rubrica.Persone) //mando il messaggio a tutti i contatti della rubrica
            {
                if(p != Sender) //tranne al mittente
                {
                    IPEndPoint remote_endpoint = new IPEndPoint(myIP, p.Porta); //indico il remote endpoint
                    socket.SendTo(messaggio, remote_endpoint); //mando il messaggio
                    Messaggio m = new Messaggio(txtMessaggio.Text, Sender); //salvo il messaggio nella chat
                    p.Chat.Add(m);
                    
                }
            } 
            AggiornaChat(); //aggiorno la chat
        }

        private void txtTo_GotFocus(object sender, RoutedEventArgs e)
        {
            txtTo.Text = null;
        }

        private void txtMessaggio_GotFocus(object sender, RoutedEventArgs e)
        {
            txtMessaggio.Text = null;
        }

        private void AggiornaChat() //aggiorno il listbox della chat
        {
            lstMessaggi.Items.Clear();
            
            if(Receiver != null)
            {
                foreach (Messaggio m in Receiver.Chat) //guardo chi è il receiver nuovo e metto dentro la chat tutto lo storico dei messaggi con quella persona
                {
                    if (m.Mittente.Porta != Sender.Porta) //controllo chi ha inviato il messaggio tra le due persone della chat
                    {
                        if (IsBase64Encoded(m.SMS)) //guardo se il messaggio selezionato è un'immagine o no
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(m.SMS))); //se è un'immagine converto la stringa base64 in immagine

                            lstMessaggi.Dispatcher.Invoke(() =>
                            {
                                System.Windows.Controls.Image wpfImage = new System.Windows.Controls.Image(); //converto l'immagine da System.Drawing a System.Windows.Controls per mostrarla nel listbox
                                wpfImage.Source = ConvertImageToBitmapImage(image); //prendo il source con una conversione da image a bitmap
                                wpfImage.Width = 200;
                                wpfImage.Margin = new Thickness(5);
                                ListBoxItem listBoxItem = new ListBoxItem(); //creo un listbox in cui inserire l'immagine per mostrarla nel listbox
                                listBoxItem.Content = wpfImage;
                                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Left; //allineo a sinistra
                                lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(listBoxItem)); //aggiungo il messaggio nel listbox
                            } 

                            );
                        }
                        else
                        {
                            lstMessaggi.Items.Add(m); //aggiungo il messaggio nel listbox
                        }
                    }
                    else
                    { 
                        if (IsBase64Encoded(m.SMS))//guardo se il messaggio selezionato è un'immagine o no
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(m.SMS))); //se è un'immagine converto la stringa base64 in immagine

                            lstMessaggi.Dispatcher.Invoke(() =>
                            {
                                var wpfImage = new System.Windows.Controls.Image(); //converto l'immagine da System.Drawing a System.Windows.Controls per mostrarla nel listbox
                                wpfImage.Source = ConvertImageToBitmapImage(image); //prendo il source con una conversione da image a bitmap
                                wpfImage.Width = 200;
                                wpfImage.Margin = new Thickness(5);
                                ListBoxItem listBoxItem = new ListBoxItem(); //creo un listbox in cui inserire l'immagine per mostrarla nel listbox
                                listBoxItem.Content = wpfImage;
                                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right; //allineo a destra perchè è un mio messaggio
                                lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(listBoxItem)); //aggiungo il messaggio nel listbox
                            }

                            );

                        }
                        else
                        {
                            ListBoxItem listBoxItem = new ListBoxItem(); //allineo i miei messaggi a destra nel listbox
                            listBoxItem.Content = m;
                            listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                            lstMessaggi.Items.Add(listBoxItem); //aggiungo il messaggio nel listbox
                        }
                        
                    }
                }
            }
            
        }

        private void btnInvio_Click(object sender, RoutedEventArgs e) //invio messaggi
        {
            try
            {
                if(Receiver == null) //controllo che il receiver non sia null
                {
                    throw new Exception("SELEZIONA UN UTENTE A CUI MANDARE IL MESSAGGIO");
                }

                Receiver = lstRubrica.SelectedItem as Persona; //prendo il receiver dal contatto selezionato
                string hostName = Dns.GetHostName(); //prendo l'host name
                IPAddress myIP = Dns.GetHostByName(hostName).AddressList[0]; //prendo l'ip dall'host name

                IPEndPoint remote_endpoint = new IPEndPoint(myIP, Receiver.Porta); //prendo il remote endpoint dall'ip e dalla porta del contatto selezionato

                byte[] messaggio = Encoding.UTF8.GetBytes(txtTo.Text); //trasformo la stringa in un array di byte

                socket.SendTo(messaggio, remote_endpoint); //mando il messaggio

                ListBoxItem listBoxItem = new ListBoxItem(); //allineo il mio messaggio a destra nel listbox
                listBoxItem.Content = Sender.Nome + " : " + txtTo.Text;
                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                lstMessaggi.Items.Add(listBoxItem); //aggiungo il messaggio nel listbox
                Messaggio m = new Messaggio(txtTo.Text, Sender); //salvo il messaggio nello storico
                Receiver.Chat.Add(m);
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAllega_Click(object sender, RoutedEventArgs e) //per allegare le immagini
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog(); //permetto all'utente di selezionare l'immagine (l'immagine non deve essere troppo grande)
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp"; //imposto il filtro dell'open file dialog

                System.Windows.Controls.Image simpleImage = new System.Windows.Controls.Image(); //prendo l'immagine
                simpleImage.Width = 200;
                simpleImage.Margin = new Thickness(5);

                BitmapImage bi = new BitmapImage();

                if (open.ShowDialog() == true)
                {
                    using(MemoryStream ms = new MemoryStream()) //uso un memorystream per salvare il source dell'immagine
                    {
                        bi.BeginInit();
                        bi.UriSource = new Uri(open.FileName, UriKind.RelativeOrAbsolute); //salvo il source dell'immagine
                        bi.EndInit();
                        simpleImage.Source = bi;
                        Receiver = lstRubrica.SelectedItem as Persona; //prendo il receiver se non è null
                        string hostName = Dns.GetHostName(); //prendo l'host name

                        IPAddress myIP = Dns.GetHostByName(hostName).AddressList[0]; //ricavo l'ip dall'host name


                        IPEndPoint remote_endpoint = new IPEndPoint(myIP, Receiver.Porta); //imposto l'endpoint
                        string base64 = Convert.ToBase64String(File.ReadAllBytes(open.FileName.ToString())); //converto il percorso in una stringa base64
                        byte[] messaggio = Encoding.UTF8.GetBytes(base64); //trasformo la stringa in un array di bytes

                        socket.SendTo(messaggio, remote_endpoint); //mando il messaggio

                        ListBoxItem listBoxItem = new ListBoxItem(); //creo un listbox item per mettere l'immagine ed allinearla a destra
                        listBoxItem.Content = simpleImage;
                        listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                        lstMessaggi.Items.Add(listBoxItem);
                        Messaggio m = new Messaggio(base64, Sender); //salvo il messaggio nello storico
                        Receiver.Chat.Add(m);
                        
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lstRubrica_SelectionChanged(object sender, SelectionChangedEventArgs e) //ogni volta che cambio il contatto selezionato aggiorno la chat e le notifiche
        {
            if ((Persona)lstRubrica.SelectedItem != Receiver)
            {
                Receiver = lstRubrica.SelectedItem as Persona;
                AggiornaChat();
                AggiornaNotifiche();
            }
                
        }

        private void txtRegister_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Register register = new Register(); //creo un nuovo contatto
                register.ShowDialog();

                if (register.Persona  != null)
                {
                    foreach (Persona p in rubrica.Persone) //guardo che il nuovo utente non corrisponda ad un altro contatto già salvato
                    {
                        if (register.Persona.Porta == p.Porta)
                        {
                            if (p.Nome == "unknown") //se era riferito ad utente salvato come unknown allora lo cambio
                            {
                                register.Persona.Chat = p.Chat;
                                rubrica.Persone.Remove(p);
                                break;
                            }
                            else
                            {
                                throw new Exception("IMPOSSIBILE AGGIUNGERE UN CONTATTO CON UNA PORTA GIA' IN USO");
                            }

                        }
                    }
                    

                    rubrica.Persone.Add(register.Persona); //aggiorno la rubrica
                    Aggiorna();
                }
                else
                {
                    throw new Exception("nessun utente aggiunto");
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}