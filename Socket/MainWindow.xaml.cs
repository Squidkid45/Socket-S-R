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
using System.Windows.Navigation;
using System.Windows.Shapes;
//Librerie aggiunte
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Es_Socket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int lineCount = 0;
        public int myPort;
        public string myIP;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            //L'IP viene preso in automatico e la porta viene generata in maniera casuale per evitare conflitti quando si fa partire il programma più volte
            //Esiste la remota possibilità che il random scelga una porta già occupata e ne sono consapevole
            string connection = Dns.GetHostName();
            myIP = Dns.GetHostEntry(connection).AddressList[1].ToString();
            Random GeneratePort = new Random();
            myPort = GeneratePort.Next(49152, 65536);
            MessageBox.Show($"Il tuo IP:{myIP}\nLa tua porta:{myPort}");
            IPEndPoint sourceSocket = new IPEndPoint(IPAddress.Parse(myIP), myPort);
            btnSend.IsEnabled = true;
            txtbxMessaggio.IsEnabled = true;
            txtblkStorico.IsEnabled = true;
            txtbxIP.IsEnabled = true;
            txtbxSocket.IsEnabled = true;
            btnCreaSocket.IsEnabled = false;

            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketRecieve));
            ricezione.Start(sourceSocket);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            bool ok;
            string ipAddress = txtbxIP.Text;
            string[] checkIP = ipAddress.Split('.');
            IPAddress destination;
            ok = IPAddress.TryParse(ipAddress, out destination);
            int port;
            ok = int.TryParse(txtbxSocket.Text, out port);
            if(!ok || port<49152 || port>65535)
            {
                MessageBox.Show("Errore connessione", "I dati inseriti non sono corretti");
            }
            else
            {
                string message =$"{myIP}: {txtbxMessaggio.Text}";
                SocketSend(IPAddress.Parse(ipAddress), port, message);
            }     
        }

        public async void SocketRecieve(object socketSource)
        {
            IPEndPoint ipendp = (IPEndPoint)socketSource;
            Socket t = new Socket(ipendp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            t.Bind(ipendp);
            Byte[] bytesRicevuti = new Byte[256];
            string message;
            int charCount = 0;
            //dentro l'await non mi blocca l'interfaccia
            await Task.Run(() =>
            {
                while(true)
                {
                    if(t.Available>0)
                    {
                        message = "";
                        charCount = t.Receive(bytesRicevuti, bytesRicevuti.Length, 0);
                        message += Encoding.ASCII.GetString(bytesRicevuti, 0, charCount);
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            //Controlla se il textblock ha terminato lo spazio disponibile e se è così cancella tutto il contenuto e riparte da capo
                            if (lineCount >= 13)
                            {
                                txtblkStorico.Text = "";
                                //cambia colore del messaggio e mi mostra l'ip del mittente se è contenuto nel messaggio
                                if (message.Contains(':'))
                                {
                                    string[] messageSplit = message.Split(':');
                                    txtblkStorico.Inlines.Add(new Run($"{messageSplit[0]}: ") { Foreground = Brushes.Red });
                                    txtblkStorico.Inlines.Add(messageSplit[1] + "\n");
                                }
                                //se l'ip non è contenuto nel messaggio nomina chi ha inviato il messaggio come unknown sender
                                else
                                {
                                    txtblkStorico.Inlines.Add(new Run("Unknown sender: ") { Foreground = Brushes.Red });
                                    txtblkStorico.Inlines.Add(message + "\n");
                                }
                            }
                            else
                            {
                                //cambia colore del messaggio e mi mostra l'ip del mittente se è contenuto nel messaggio
                                if (message.Contains(':'))
                                {
                                    string[] messageSplit = message.Split(':');
                                    txtblkStorico.Inlines.Add(new Run($"{messageSplit[0]}: ") { Foreground = Brushes.Red });
                                    txtblkStorico.Inlines.Add(messageSplit[1] + "\n");
                                }
                                //se l'ip non è contenuto nel messaggio nomina chi ha inviato il messaggio come unknown sender
                                else
                                {
                                    txtblkStorico.Inlines.Add(new Run("Unknown sender: ") { Foreground = Brushes.Red });
                                    txtblkStorico.Inlines.Add(message + "\n");
                                }
                            }
                            lineCount++;

                        }));
                    }
                }
            });
        }

        //Invia il messaggio al destinatario che abbiamo inserito nelle textbox
        public void SocketSend(IPAddress dest, int destport, string message)
        {
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message);

            Socket s = new Socket(dest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint remote_endpoint = new IPEndPoint(dest, destport);
            s.SendTo(byteInviati, remote_endpoint);
            //Fa uno split del messaggio inviato per fare ordine nello storico dei messaggi
            string[] messageSplit = message.Split(':');

            //Controlla se il textblock ha terminato lo spazio disponibile e se è così cancella tutto il contenuto e riparte da capo
            if (lineCount >= 13)
            {
                txtblkStorico.Text = "";
                txtblkStorico.Inlines.Add(new Run("You: ") { Foreground = Brushes.Blue });
                txtblkStorico.Inlines.Add(messageSplit[1]+"\n");
            }
            else
            {
                txtblkStorico.Inlines.Add(new Run("You: ") { Foreground = Brushes.Blue });
                txtblkStorico.Inlines.Add(messageSplit[1] + "\n");
            }
            lineCount++;
            txtbxMessaggio.Clear();
        }

        //In caso ci si sia dimenticati che IP abbiamo o su che porta ci interfacciamo questo pulsante ci fa apparire una messagebox con le nostre informazioni
        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Il tuo IP:{myIP}\nLa tua porta:{myPort}");
        }


        //Questi due metodi mi mostrano la lable che mi dice cosa fa il pulsante info
        private void btnInfo_MouseEnter(object sender, MouseEventArgs e)
        {
            lbl_info.Visibility = Visibility.Visible;
        }

        private void btnInfo_MouseLeave(object sender, MouseEventArgs e)
        {
            lbl_info.Visibility = Visibility.Hidden;
        }
    }
}
