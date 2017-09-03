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
        // The thread in which the file will be received
        private Thread thrDownload;
        // The stream for writing the file to the hard-drive
        private Stream strLocal;
        // The network stream that receives the file
        private NetworkStream strRemote;
        // The TCP listener that will listen for connections
        private TcpListener tlsServer;
        // Delegate for updating the logging textbox
        private delegate void UpdateStatusCallback(string StatusMessage);
        // Delegate for updating the progressbar
        private delegate void UpdateProgressCallback(Int64 BytesRead, Int64 TotalBytes);
        // For storing the progress in percentages
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
            // There are many lines in here that can cause an exception
            try
            {
                // Get the hostname of the current computer (the server)
                string hstServer = Dns.GetHostName();
                // Get the IP of the first network device, however this can prove unreliable on certain configurations
                IPAddress ipaLocal = Dns.GetHostEntry(hstServer).AddressList[0];
                // If the TCP listener object was not created before, create it
                if (tlsServer == null)
                {
                    // Create the TCP listener object using the IP of the server and the specified port
                    tlsServer = new TcpListener(ipaLocal, Convert.ToInt32(txtPort.Text));
                }
                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "Starting the server...\r\n" });
                // Start the TCP listener and listen for connections
                tlsServer.Start();
                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The server has started. Please connect the client to " + ipaLocal.ToString() + "\r\n" });
                // Accept a pending connection
                TcpClient tclServer = tlsServer.AcceptTcpClient();
                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The server has accepted the client\r\n" });
                // Receive the stream and store it in a NetworkStream object
                strRemote = tclServer.GetStream();
                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The server has received the stream\r\n" });

                // For holding the number of bytes we are reading at one time from the stream
                int bytesSize = 0;
                
                // The buffer that holds the data received from the client
                byte[] downBuffer = new byte[2048];
                // Read the first buffer (2048 bytes) from the stream - which represents the file name
                bytesSize = strRemote.Read(downBuffer, 0, 2048);
                // Convert the stream to string and store the file name
                string FileName = System.Text.Encoding.ASCII.GetString(downBuffer, 0, bytesSize);
                // Set the file stream to the path C:\ plus the name of the file that was on the sender's computer
                strLocal = new FileStream(@"C:\" + FileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                // The buffer that holds the data received from the client
                downBuffer = new byte[2048];
                // Read the next buffer (2048 bytes) from the stream - which represents the file size
                bytesSize = strRemote.Read(downBuffer, 0, 2048);
                // Convert the file size from bytes to string and then to long (Int64)
                long FileSize = Convert.ToInt64(System.Text.Encoding.ASCII.GetString(downBuffer, 0, bytesSize));

                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "Receiving file " + FileName + " (" + FileSize + " bytes)\r\n" });

                // The buffer size for receiving the file
                downBuffer = new byte[2048];

                // From now on we read everything that's in the stream's buffer because the file content has started
                while ((bytesSize = strRemote.Read(downBuffer, 0, downBuffer.Length)) > 0)
                {
                    // Write the data to the local file stream
                    strLocal.Write(downBuffer, 0, bytesSize);
                    // Update the progressbar by passing the file size and how much we downloaded so far to UpdateProgress()
                    this.Invoke(new UpdateProgressCallback(this.UpdateProgress), new object[] { strLocal.Length, FileSize });
                }
                // When this point is reached, the file has been received and stored successfuly
            }
            finally
            {
                // This part of the method will fire no matter wether an error occured in the above code or not

                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "The file was received. Closing streams.\r\n" });

                // Close the streams
                strLocal.Close();
                strRemote.Close();

                // Write the status to the log textbox on the form (txtLog)
                this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { "Streams are now closed.\r\n" });

                // Start the server (TCP listener) all over again
                StartReceiving();
            }
        }

        private void UpdateStatus(string StatusMessage)
        {
            // Append the status to the log textbox text 
            txtLog.Text += StatusMessage;
        }

        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            if (TotalBytes > 0)
            {
                // Calculate the download progress in percentages
                PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);
                // Make progress on the progress bar
                prgDownload.Value = PercentProgress;
            }
        }


        private void btnStop_Click(object sender, EventArgs e)
        {
            strLocal.Close();
            strRemote.Close();
            txtLog.Text += "Streams are now closed.\r\n";
        }
    }
}