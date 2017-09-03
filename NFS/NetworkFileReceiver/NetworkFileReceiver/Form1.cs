using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace NetworkFileReceiver
{
    public partial class Form1 : Form
    {
        private Thread thrDownload;
        private Stream strLocal;
        private NetworkStream strRemote;
        private TcpListener tlsServer;
        private delegate void UpdateStatusCallback(string StatusMessage);
        private delegate void UpdateProgressCallback(Int64 BytesRead, Int64 TotalBytes);
        private static int PercentProgress;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            thrDownload = new Thread(StartReceiving);
            thrDownload.Start();
        }

        private void StartReceiving()
        {
          
            try
            {
                string hstServer = Dns.GetHostName();
                IPAddress ipaLocal = Dns.GetHostEntry(hstServer).AddressList[0];
                if (!string.IsNullOrEmpty(textIP.Text))
                {
                   ipaLocal = IPAddress.Parse(textIP.Text); 
                }
                if (tlsServer == null)
                {
                    tlsServer = new TcpListener(ipaLocal, Convert.ToInt32(txtPort.Text));
                }
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "Starting the server...\r\n" + " " + DateTime.Now.ToLongTimeString() });
                tlsServer.Start();
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The server has started. Please connect the client to " + ipaLocal.ToString() + "\r\n" + " " + DateTime.Now.ToLongTimeString() });
                TcpClient tclServer = tlsServer.AcceptTcpClient();
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The server has accepted the client\r\n" + " " + DateTime.Now.ToLongTimeString() });
                strRemote = tclServer.GetStream();
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The server has received the stream\r\n" + " " + DateTime.Now.ToLongTimeString() });

                int bytesSize = 0;
                
                byte[] downBuffer = new byte[2048];
                bytesSize = strRemote.Read(downBuffer, 0, 2048);
                string FileName = System.Text.Encoding.ASCII.GetString(downBuffer, 0, bytesSize);
                strLocal = new FileStream(@"C:\" + FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                downBuffer = new byte[2048];
                bytesSize = strRemote.Read(downBuffer, 0, 2048);
                long FileSize = Convert.ToInt64(System.Text.Encoding.ASCII.GetString(downBuffer, 0, bytesSize));

                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "Receiving file " + FileName + " (" + FileSize + " bytes)\r\n" + DateTime.Now });

                downBuffer = new byte[2048];

                while ((bytesSize = strRemote.Read(downBuffer, 0, downBuffer.Length)) > 0)
                {
                    strLocal.Write(downBuffer, 0, bytesSize);
                    this.Invoke(new UpdateProgressCallback(this.UpdateProgress), new object[] { strLocal.Length, FileSize });
                }
            }
            finally
            {
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The file was received. Closing streams.\r\n" + " " + DateTime.Now.ToLongTimeString() });

                strLocal.Close();
                strRemote.Close();

                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "Streams are now closed.\r\n" + " " + DateTime.Now.ToLongTimeString() });

                StartReceiving();
            }
        }

        private void UpdateStatus(string StatusMessage)
        {
            txtLog.Text += StatusMessage;
        }

        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            if (TotalBytes > 0)
            {
                PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);
                prgDownload.Value = PercentProgress;
            }
        }


        private void btnStop_Click(object sender, EventArgs e)
        {
            strLocal.Close();
            strRemote.Close();
            txtLog.Text += "Streams are now closed.\r\n" + " " + DateTime.Now.ToLongTimeString();
        }
    }
}