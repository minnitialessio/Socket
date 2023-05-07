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
            rubrica.Persone.Add(new Persona("Alessio", 10000));
            rubrica.Persone.Add(new Persona("Francesco", 11000));
            rubrica.Persone.Add(new Persona("Enrico", 12000));
            rubrica.Persone.Add(new Persona("Giacomo", 13000));
            rubrica.Persone.Add(new Persona("Matteo", 14000));
            rubrica.Persone.Add(new Persona("Mario", 15000));
            rubrica.Persone.Add(new Persona("Luca", 16000));
            Login login = new Login(rubrica);
            login.ShowDialog();
            Sender = login.Utente;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            
            IPAddress local_address = IPAddress.Any;
            IPEndPoint local_endpoint = new IPEndPoint(local_address, Sender.Porta);

            socket.Bind(local_endpoint);

            //socket.Blocking = false;
            //socket.EnableBroadcast = true;

            Task.Run(RiceviMessaggio);
            //dTimer = new DispatcherTimer();

            //dTimer.Tick += new EventHandler(aggiornamento_dTimer);
            //dTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            //dTimer.Start();
            Aggiorna();
        }
        private void Aggiorna()
        {
            foreach(Persona p in rubrica.Persone)
            {
                lstRubrica.Items.Add(p);
            }

            lstRubrica.Items.Remove(Sender);
        }
        private bool IsBase64Encoded(string stringa)
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
        private Task RiceviMessaggio()
        {
            while (true)
            {
                int nBytes;
                //ricezione dei caratteri in attesa
                byte[] buffer = new byte[60000];

                EndPoint remoreEndPoint = new IPEndPoint(IPAddress.Any, 0);

                nBytes = socket.ReceiveFrom(buffer, ref remoreEndPoint);

                int from = ((IPEndPoint)remoreEndPoint).Port;

                
                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                string nome = "unknown";

                if(Receiver != null && from == Receiver.Porta)
                {

                    if (IsBase64Encoded(messaggio))
                    {

                        System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(messaggio)));
                         
                        lstMessaggi.Dispatcher.Invoke(() =>
                        {
                            var wpfImage = new System.Windows.Controls.Image();
                            wpfImage.Source = ConvertImageToBitmapImage(image);
                            wpfImage.Width = 200;
                            wpfImage.Margin = new Thickness(5);
                            ListBoxItem listBoxItem = new ListBoxItem();
                            listBoxItem.Content = nome + wpfImage;
                            listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                            lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(listBoxItem));
                            Messaggio m = new Messaggio(messaggio, Receiver);
                            Receiver.Chat.Add(m);
                        }

                        );
                    }
                    else
                    {
                        nome = Receiver.Nome;
                        lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(nome + " : " + messaggio));
                        Messaggio m = new Messaggio(messaggio, Receiver);
                        Receiver.Chat.Add(m);
                    }

                }
                else
                {
                    foreach (Persona p in rubrica.Persone)
                    {
                        if (from == p.Porta)
                        {
                            nome = p.Nome;
                            if (IsBase64Encoded(messaggio))
                            {
                                
                                System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(messaggio)));

                                lstNotifiche.Dispatcher.Invoke(() =>
                                {
                                    var wpfImage = new System.Windows.Controls.Image();
                                    wpfImage.Source = ConvertImageToBitmapImage(image);
                                    wpfImage.Width = 200;
                                    wpfImage.Margin = new Thickness(5);
                                    
                                    lstNotifiche.Dispatcher.Invoke(() => lstNotifiche.Items.Add(nome + " : " + "image"));
                                    Messaggio m = new Messaggio(messaggio, p);
                                    p.Chat.Add(m);
                                }

                                );
                            }
                            else
                            {
                                lstNotifiche.Dispatcher.Invoke(() => lstNotifiche.Items.Add(nome + " : " + messaggio));
                                Messaggio m = new Messaggio(messaggio, p);
                                p.Chat.Add(m);
                            }

                        }
                    }
                    
                }

                //lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(nome + " : " + messaggio));
                //lstMessaggi.Dispatcher.Invoke(() => AggiornaChat());

            }
            return Task.CompletedTask;
        }
        private BitmapImage ConvertImageToBitmapImage(System.Drawing.Image image)
        {
            var bitmap = new Bitmap(image);
            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Bgr24, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream();
            bitmap.Save(bitmapImage.StreamSource, ImageFormat.Bmp);
            bitmapImage.EndInit();

            return bitmapImage;
        }
        private void AggiornaNotifiche()
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

        private void btnInviaBroadcast_Click(object sender, RoutedEventArgs e)
        {
            string hostName = Dns.GetHostName();
            IPAddress myIP = Dns.GetHostByName(hostName).AddressList[0];

            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text);

            foreach (Persona p in rubrica.Persone)
            {
                if(p != Sender)
                {
                    IPEndPoint remote_endpoint = new IPEndPoint(myIP, p.Porta);
                    socket.SendTo(messaggio, remote_endpoint);
                    Messaggio m = new Messaggio(txtMessaggio.Text, Sender);
                    p.Chat.Add(m);
                    AggiornaChat();
                }
            }
        }

        private void txtTo_GotFocus(object sender, RoutedEventArgs e)
        {
            txtTo.Text = null;
        }

        private void txtMessaggio_GotFocus(object sender, RoutedEventArgs e)
        {
            txtMessaggio.Text = null;
        }

        private void AggiornaChat()
        {
            lstMessaggi.Items.Clear();
            
            if(Receiver != null)
            {
                foreach (Messaggio m in Receiver.Chat)
                {
                    if (m.Mittente.Porta != Sender.Porta)
                    {
                        if (IsBase64Encoded(m.SMS))
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(m.SMS)));

                            lstMessaggi.Dispatcher.Invoke(() =>
                            {
                                var wpfImage = new System.Windows.Controls.Image();
                                wpfImage.Source = ConvertImageToBitmapImage(image);
                                wpfImage.Width = 200;
                                wpfImage.Margin = new Thickness(5);
                                ListBoxItem listBoxItem = new ListBoxItem();
                                listBoxItem.Content = wpfImage;
                                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Left;
                                lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(listBoxItem));
                            }

                            );
                        }
                        else
                        {
                            lstMessaggi.Items.Add(m);
                        }
                    }
                    else
                    {
                        if (IsBase64Encoded(m.SMS))
                        {
                            System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Convert.FromBase64String(m.SMS)));

                            lstMessaggi.Dispatcher.Invoke(() =>
                            {
                                var wpfImage = new System.Windows.Controls.Image();
                                wpfImage.Source = ConvertImageToBitmapImage(image);
                                wpfImage.Width = 200;
                                wpfImage.Margin = new Thickness(5);
                                ListBoxItem listBoxItem = new ListBoxItem();
                                listBoxItem.Content = wpfImage;
                                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                                lstMessaggi.Dispatcher.Invoke(() => lstMessaggi.Items.Add(listBoxItem));
                            }

                            );

                        }
                        else
                        {
                            ListBoxItem listBoxItem = new ListBoxItem();
                            listBoxItem.Content = m;
                            listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                            lstMessaggi.Items.Add(listBoxItem);
                        }
                        
                    }
                }
            }
            
        }

        private void btnInvio_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Receiver = lstRubrica.SelectedItem as Persona;
                string hostName = Dns.GetHostName();
                IPAddress myIP = Dns.GetHostByName(hostName).AddressList[0];
                //IPAddress remote_address = IPAddress.Parse(txtTo.Text);

                IPEndPoint remote_endpoint = new IPEndPoint(myIP, Receiver.Porta);

                byte[] messaggio = Encoding.UTF8.GetBytes(txtTo.Text);

                socket.SendTo(messaggio, remote_endpoint);

                string from = myIP.ToString();
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = Sender.Nome + " : " + txtTo.Text;
                listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                lstMessaggi.Items.Add(listBoxItem);
                Messaggio m = new Messaggio(txtTo.Text, Sender);
                Receiver.Chat.Add(m);
                
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAllega_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog open = new OpenFileDialog();
                open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";

                System.Windows.Controls.Image simpleImage = new System.Windows.Controls.Image();
                simpleImage.Width = 200;
                simpleImage.Margin = new Thickness(5);

                BitmapImage bi = new BitmapImage();

                if (open.ShowDialog() == true)
                {
                    using(MemoryStream ms = new MemoryStream())
                    {
                        bi.BeginInit();
                        bi.UriSource = new Uri(open.FileName, UriKind.RelativeOrAbsolute);
                        bi.EndInit();
                        simpleImage.Source = bi;
                        //Bitmap pictureBox = new Bitmap(open.FileName);
                        lstMessaggi.Items.Add(simpleImage);

                        Receiver = lstRubrica.SelectedItem as Persona;
                        string hostName = Dns.GetHostName();

                        IPAddress myIP = Dns.GetHostByName(hostName).AddressList[0];
                        //IPAddress remote_address = IPAddress.Parse(txtTo.Text);

                        IPEndPoint remote_endpoint = new IPEndPoint(myIP, Receiver.Porta);
                        string base64 = Convert.ToBase64String(File.ReadAllBytes(open.FileName.ToString()));
                        byte[] messaggio = Encoding.UTF8.GetBytes(base64);

                        //byte[] messaggio = ms.ToArray();

                        socket.SendTo(messaggio, remote_endpoint);

                        ListBoxItem listBoxItem = new ListBoxItem();
                        listBoxItem.Content = simpleImage;
                        listBoxItem.HorizontalContentAlignment = HorizontalAlignment.Right;
                        lstMessaggi.Items.Add(listBoxItem);
                        Messaggio m = new Messaggio(base64, Sender);
                        Receiver.Chat.Add(m);
                        
                    }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lstRubrica_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                Register register = new Register();
                register.ShowDialog();

                foreach (Persona p in rubrica.Persone)
                {
                    if (register.Persona.Porta == p.Porta)
                    {
                        throw new Exception("IMPOSSIBILE AGGIUNGERE UN CONTATTO CON UNA PORTA GIA' IN USO");
                    }
                }

                rubrica.Persone.Add(register.Persona);
                Aggiorna();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}