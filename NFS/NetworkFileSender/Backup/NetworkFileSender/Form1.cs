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
        // The TCP client will connect to the server using an IP and a port
        TcpClient tcpClient;
        // The file stream will read bytes from the local file you are sending
        FileStream fstFile;
        // The network stream will send bytes to the server application
        NetworkStream strRemote;

        public Form1()
        {
            InitializeComponent();
        }

        private void ConnectToServer(string ServerIP, int ServerPort)
        {
            // Create a new instance of a TCP client
            tcpClient = new TcpClient();
            try
            {
                // Connect the TCP client to the specified IP and port
                tcpClient.Connect(ServerIP, ServerPort);
                txtLog.Text += "Successfully connected to server\r\n";
            }
            catch (Exception exMessage)
            {
                // Display any possible error
                txtLog.Text += exMessage.Message;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Call the ConnectToServer method and pass the parameters entered by the user
            ConnectToServer(txtServer.Text, Convert.ToInt32(txtPort.Text));
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // If tclClient is not connected, try a connection
            if (tcpClient.Connected == false)
            {
                // Call the ConnectToServer method and pass the parameters entered by the user
                ConnectToServer(txtServer.Text, Convert.ToInt32(txtPort.Text));
            }

            // Prompt the user for opening a file
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                txtLog.Text += "Sending file information\r\n";
                // Get a stream connected to the server
                strRemote = tcpClient.GetStream();
                byte[] byteSend = new byte[tcpClient.ReceiveBufferSize];
                // The file stream will read bytes from the file that the user has chosen
                fstFile = new FileStream(openFile.FileName, FileMode.Open, FileAccess.Read);
                // Read the file as binary
                BinaryReader binFile = new BinaryReader(fstFile);

                // Get information about the opened file
                FileInfo fInfo = new FileInfo(openFile.FileName);

                // Get and store the file name
                string FileName = fInfo.Name;
                // Store the file name as a sequence of bytes
                byte[] ByteFileName = new byte[2048];
                ByteFileName = System.Text.Encoding.ASCII.GetBytes(FileName.ToCharArray());
                // Write the sequence of bytes (the file name) to the network stream
                strRemote.Write(ByteFileName, 0, ByteFileName.Length);

                // Get and store the file size
                long FileSize = fInfo.Length;
                // Store the file size as a sequence of bytes
                byte[] ByteFileSize = new byte[2048];
                ByteFileSize = System.Text.Encoding.ASCII.GetBytes(FileSize.ToString().ToCharArray());
                // Write the sequence of bytes (the file size) to the network stream
                strRemote.Write(ByteFileSize, 0, ByteFileSize.Length);

                txtLog.Text += "Sending the file " + FileName + " (" + FileSize + " bytes)\r\n";

                // Reset the number of read bytes
                int bytesSize = 0;
                // Define the buffer size
                byte[] downBuffer = new byte[2048];

                // Loop through the file stream of the local file
                while ((bytesSize = fstFile.Read(downBuffer, 0, downBuffer.Length)) > 0)
                {
                    // Write the data that composes the file to the network stream
                    strRemote.Write(downBuffer, 0, bytesSize);
                }

                // Update the log textbox and close the connections and streams
                txtLog.Text += "File sent. Closing streams and connections.\r\n";
                tcpClient.Close();
                strRemote.Close();
                fstFile.Close();
                txtLog.Text += "Streams and connections are now closed.\r\n";
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            // Close connections and streams and update the log textbox
            tcpClient.Close();
            strRemote.Close();
            fstFile.Close();
            txtLog.Text += "Disconnected from server.\r\n";
        }
    }
}