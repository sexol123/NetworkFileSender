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

namespace NetworkFileSender
{
    public partial class Form1 : Form
    {
        TcpClient tcpClient;
        FileStream fstFile;
        NetworkStream strRemote;

        public Form1()
        {
            InitializeComponent();
        }

        private void ConnectToServer(string ServerIP, int ServerPort)
        {
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(ServerIP, ServerPort);
                txtLog.Text += "\nSuccessfully connected to server\r\n"+ " " + DateTime.Now.ToLongTimeString();
            }
            catch (Exception exMessage)
            {
                txtLog.Text += exMessage.Message +" "+ DateTime.Now.ToLongTimeString()+"\n";
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectToServer(txtServer.Text, Convert.ToInt32(txtPort.Text));
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (tcpClient.Connected == false)
            {
                ConnectToServer(txtServer.Text, Convert.ToInt32(txtPort.Text));
            }
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtLog.Text += "\nSending file information\r\n" + " " + DateTime.Now.ToLongTimeString();
                strRemote = tcpClient.GetStream();
                byte[] byteSend = new byte[tcpClient.ReceiveBufferSize];
                fstFile = new FileStream(openFile.FileName, FileMode.Open, FileAccess.Read);
                BinaryReader binFile = new BinaryReader(fstFile);
                FileInfo fInfo = new FileInfo(openFile.FileName);

                string FileName = fInfo.Name;
                byte[] ByteFileName = new byte[2048];
                ByteFileName = System.Text.Encoding.ASCII.GetBytes(FileName.ToCharArray());
                strRemote.Write(ByteFileName, 0, ByteFileName.Length);

                long FileSize = fInfo.Length;
                byte[] ByteFileSize = new byte[2048];
                ByteFileSize = System.Text.Encoding.ASCII.GetBytes(FileSize.ToString().ToCharArray());
                strRemote.Write(ByteFileSize, 0, ByteFileSize.Length);

                txtLog.Text += "Sending the file " + FileName + " (" + FileSize + " bytes)\r\n";

                int bytesSize = 0;
                byte[] downBuffer = new byte[2048];

                while ((bytesSize = fstFile.Read(downBuffer, 0, downBuffer.Length)) > 0)
                {
                    strRemote.Write(downBuffer, 0, bytesSize);
                }

                txtLog.Text += "\nFile sent. Closing streams and connections.\r\n"+" " + DateTime.Now.ToLongTimeString();
                strRemote.Close();
                fstFile.Close();
                txtLog.Text += "\nStreams and connections are now closed.\r\n" + " " + DateTime.Now.ToLongTimeString();
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            tcpClient.Close();
            strRemote.Close();
            fstFile.Close();
            txtLog.Text += "\nDisconnected from server.\r\n" + " " + DateTime.Now.ToLongTimeString();
        }
    }
}