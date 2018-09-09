using System;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;

namespace PingWPF
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int timePeriod = 7000;

        private BackgroundWorker bWorker;
        private BackgroundWorker bWorker2;

        public MainWindow()
        {
            InitializeComponent();
            ButtonWyslij.Content = "Wysyłaj zapytania ping przez " + (timePeriod/1000).ToString() + " sek.";
        }

        private void ButtonWyslij_Click(object sender, RoutedEventArgs e)
        {
            ListViewLista.Items.Clear();
            ListViewLista.Items.Add("Odpowiedzi PING:");

            TextBoxAdres.IsEnabled = false;
            ButtonWyslij.IsEnabled = false;

            bWorker = new BackgroundWorker();

            bWorker.WorkerReportsProgress = true;
            bWorker.DoWork += bWorker_DoWork;
            bWorker.ProgressChanged += bWorker_ProgressChanged;
            bWorker.RunWorkerCompleted += bWorker_RunWorkerCompleted;
            bWorker.RunWorkerAsync(timePeriod/100);

            bWorker2 = new BackgroundWorker();

            bWorker2.WorkerReportsProgress = true;
            bWorker2.DoWork += bWorker2_DoWork;
            bWorker2.ProgressChanged += bWorker2_ProgressChanged;
            bWorker2.RunWorkerCompleted += bWorker2_RunWorkerCompleted;
            bWorker2.RunWorkerAsync(TextBoxAdres.Text);

        }

        void bWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int max = (int)e.Argument;

            for (int i = 0; i < max; i++)
            {
                if(bWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                int progressPercentage = Convert.ToInt32(((double)i / (max-1)) * 100);

                (sender as BackgroundWorker).ReportProgress(progressPercentage);

                Thread.Sleep(100);
            }
        }

        void bWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarCzas.Value = e.ProgressPercentage;
        }

        void bWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bWorker2.WorkerSupportsCancellation = true;
            bWorker2.CancelAsync();
            TextBoxAdres.IsEnabled = true;
            ButtonWyslij.IsEnabled = true;
        }

        void bWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            int max = timePeriod;

            for (int i = 0; i < max; i++)
            {
                if (bWorker2.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                int progressPercentage = Convert.ToInt32(((double)i / max) * 100);

                Ping p = new Ping();
                
                try
                {
                    (sender as BackgroundWorker).ReportProgress(progressPercentage, "PING status: " + p.Send(e.Argument.ToString(), 5000).Status.ToString() + " IP: " + p.Send(e.Argument.ToString(), 5000).Address.ToString() + " time = " + p.Send(e.Argument.ToString(), 5000).RoundtripTime.ToString() + " ms");
                }
                catch (Exception)
                {
                    bWorker.WorkerSupportsCancellation = true;
                    bWorker.CancelAsync();
                    (sender as BackgroundWorker).ReportProgress(progressPercentage, "Źle wpisane dane");
                    break;
                }
                Thread.Sleep(1);
            }
        }

        void bWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                if(e.UserState.ToString() == "Źle wpisane dane")
                {
                    ListViewLista.Items.Clear();
                }
                ListViewLista.Items.Add(e.UserState);
            }
        }

        void bWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ListViewLista.Items.Add("Koniec testu");
        }
    }
}
