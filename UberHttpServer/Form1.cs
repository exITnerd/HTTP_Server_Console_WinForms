using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace SimpleHttpServerApp
{
    public class Form1 : Form
    {
        private SimpleHttpServer httpServer;
        private TextBox portTextBox;
        private TextBox directoryTextBox;
        private Button startButton;
        private Button stopButton;
        private BackgroundWorker serverWorker;
        private Label portLabel;
        private Label directoryLabel;

        public Form1()
        {
            InitializeComponents();
            InitializeServerWorker();
        }

        private void InitializeComponents()
        {
            Text = "Simple HTTP Server";
            Width = 250;
            Height = 200;

            portLabel = new Label
            {
                Location = new System.Drawing.Point(20, 0),
                Text = "Port:"
            };

            portTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(200, 20),
                Text = "8015"
            };
            portTextBox.Enter += TextBox_Enter;
            portTextBox.Leave += TextBox_Leave;

            directoryLabel = new Label
            {
                Location = new System.Drawing.Point(20, 40),
                Text = "Directory:"
            };

            directoryTextBox = new TextBox
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(200, 20),
                Text = "C:/Users/exitn/Desktop/HttpServerSites/HTML"
            };
            directoryTextBox.Enter += TextBox_Enter;
            directoryTextBox.Leave += TextBox_Leave;

            startButton = new Button
            {
                Location = new System.Drawing.Point(20, 100),
                Size = new System.Drawing.Size(90, 30),
                Text = "Start"
            };
            startButton.Click += StartButton_Click;

            stopButton = new Button
            {
                Location = new System.Drawing.Point(120, 100),
                Size = new System.Drawing.Size(90, 30),
                Text = "Stop",
                Enabled = false
            };
            stopButton.Click += StopButton_Click;

            Controls.Add(portLabel);
            Controls.Add(portTextBox);
            Controls.Add(directoryLabel);
            Controls.Add(directoryTextBox);
            Controls.Add(startButton);
            Controls.Add(stopButton);
        }

        private void InitializeServerWorker()
        {
            serverWorker = new BackgroundWorker();
            serverWorker.DoWork += ServerWorker_DoWork;
        }

        private void ServerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                int port;
                if (!int.TryParse(portTextBox.Text, out port))
                {
                    MessageBox.Show("Invalid port number. Please enter a valid integer value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                httpServer = new SimpleHttpServer();
                httpServer.StartServer(portTextBox.Text, directoryTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while starting the server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == textBox.AccessibleDescription)
            {
                textBox.Text = "";
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.AccessibleDescription;
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (httpServer != null && httpServer.IsRunning)
            {
                MessageBox.Show("The server is already running.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(portTextBox.Text) || string.IsNullOrWhiteSpace(directoryTextBox.Text))
            {
                MessageBox.Show("Please enter the port number and directory path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            serverWorker.RunWorkerAsync();

            startButton.Enabled = false;
            stopButton.Enabled = true;
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (httpServer != null && httpServer.IsRunning)
            {
                httpServer.StopServer();
                stopButton.Enabled = false;
                startButton.Enabled = true;
            }
        }
    }
}
