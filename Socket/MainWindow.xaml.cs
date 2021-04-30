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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            //Cambiare ip e forse la porta selezionato quando cambio PC (cerca restituzione automatica)
            IPEndPoint sourceSocket = new IPEndPoint(IPAddress.Parse("10.73.0.19"), 56000);
            string ipAddress = txtbxIP.Text;
            string port = txtbxSocket.Text;
            txtbxIP.Text = "";
            txtbxSocket.Text = "";
            //Aggiungi il controllo sulle textbox
            btnSend.IsEnabled = true;
            txtbxMessaggio.IsEnabled = true;
            txtblkStorico.IsEnabled = true;

            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketRecieve));
            ricezione.Start(sourceSocket);
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = txtbxIP.Text;
            int port = int.Parse(txtbxSocket.Text); //fai il tryparse
            //txtbxIP.Text = "";
            //txtbxSocket.Text = "";
            //Aggiungi il controllo sulle textbox
            SocketSend(IPAddress.Parse(ipAddress), port, txtbxMessaggio.Text);
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
                            //cambia colore del messaggio e aggiungi ip mittente
                            txtblkStorico.Text += "\n"+message;
                        }));
                    }
                }
            });
        }

        public void SocketSend(IPAddress dest, int destport, string message)
        {
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message);

            Socket s = new Socket(dest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint remote_endpoint = new IPEndPoint(dest, destport);
            s.SendTo(byteInviati, remote_endpoint);
            txtblkStorico.Text += "\n You:" + message;
            txtbxMessaggio.Text = "";
        }
    }
}
