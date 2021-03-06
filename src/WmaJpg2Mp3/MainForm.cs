﻿using WmaJpg2Mp3.Operations;
using System;
using System.Windows.Forms;

namespace WmaJpg2Mp3
{
    public partial class MainForm : Form
    {
        #region Private static members: Methods

        private static void SelectPath(string description, TextBox tbDestination)
        {
            using (var fileDialog = new FolderBrowserDialog
            {
                Description = description
            })
            {
                switch (fileDialog.ShowDialog())
                {
                    case DialogResult.OK:
                        tbDestination.Text = fileDialog.SelectedPath;
                        break;
                }
            }
        }

        #endregion

        #region Private members: Fields

        private readonly OperationManager _operationManager;

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();
            _operationManager = new OperationManager();
            _operationManager.OnStarted += (sender, e) => SetControlState(isRunning: true);
            _operationManager.OnCompleted += (sender, e) =>
            {
                SetControlState(isRunning: false);

                if (e.Exception is OperationCanceledException)
                {
                    ShowBox(
                        "Operation cancelled",
                        "Operation was cancelled.",
                         MessageBoxIcon.Information);
                }
                else if (e.Exception != null)
                {
                    ShowBox(
                        "Có lỗi xảy ra",
                        "Có lỗi xảy ra với những tập tin dưới đây"
                        + Environment.NewLine + Environment.NewLine
                        + e.Exception.ToString(),
                         MessageBoxIcon.Error);
                }
                else
                {
                    ShowBox(
                        "Chuyển đổi thành công", 
                        $"{e.ItemCount} đã chuyển đổi thành công.", 
                        MessageBoxIcon.Information
                    );
                }
            };
            _operationManager.OnMessage += (sender, e) =>
            {
                tbOutput.BeginInvoke(new MethodInvoker(() => tbOutput.Text += e.Message + Environment.NewLine));
            };
            _operationManager.OnProgress += (sender, e) =>
            {
                progressBar1.BeginInvoke(new MethodInvoker(() =>
                {
                    progressBar1.Value = e.ProcessedItemCount;
                    progressBar1.Maximum = e.TotalItemCount;
                }));
            };
            tbSourceFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        }

        #endregion

        #region Private members: Event handlers

        private void ShowBox(
            string caption,
            string text,
            MessageBoxIcon icon)
        {
            tbDestinationFolder.BeginInvoke(new MethodInvoker(() =>
            {
                MessageBox.Show(
                    text: text,
                    caption: caption,
                    buttons: MessageBoxButtons.OK,
                    icon: icon
                );
            }));
        }

        private void SetControlState(bool isRunning)
        {
            tbDestinationFolder.BeginInvoke(new MethodInvoker(() =>
            {
                tbDestinationFolder.Enabled =
                    btnDestinationFolder.Enabled =
                    tbSourceFolder.Enabled =
                    btnSourceFolder.Enabled =
                    btnGo.Enabled = !isRunning;
                btnCancel.Enabled = isRunning;
            }));
        }

        private void btnSourceFolder_Click(object sender, EventArgs e) => SelectPath("Chọn thư mục chưa tập tin wma", tbSourceFolder);
        private void btnDestinationFolder_Click(object sender, EventArgs e) => SelectPath("Chọn thư mục chứa tập tin mp3", tbDestinationFolder);
        private void btnGo_Click(object sender, EventArgs e) => _operationManager.Start(tbSourceFolder.Text, tbDestinationFolder.Text);
        private void btnCancel_Click(object sender, EventArgs e) => _operationManager.CancelCurrent();
        private void Form1_Load(object sender, EventArgs e) => SetControlState(false);

        #endregion
    }
}
