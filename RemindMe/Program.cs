using System;
using System.Media;
using System.Threading;
using System.Windows.Forms;

namespace RemindApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new RemindApplicationContext());
        }
    }

    public class RemindApplicationContext : ApplicationContext
    {
        private int timerMinutes;
        private string message;
        private NotifyIcon notifyIcon;
        private System.Threading.Timer timer;
        private System.Media.SoundPlayer player;

        public RemindApplicationContext()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length < 4 || args[1] != "-timer" || args[3] != "-message")
            {
                MessageBox.Show("Usage: Remind.exe -timer <minutes> -message \"<message>\"", "Remind", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitThread();
                return;
            }

            if (!int.TryParse(args[2], out timerMinutes))
            {
                MessageBox.Show("Invalid timer value. Please provide an integer value for minutes.", "Remind", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitThread();
                return;
            }

            message = args[4];

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RemindMe.ico"));
            notifyIcon.Visible = true;
            notifyIcon.Text = "Remind";
            notifyIcon.MouseClick += NotifyIcon_MouseClick;

            timer = new System.Threading.Timer(TimerCallback, null, 0, timerMinutes * 60000); // Convert minutes to milliseconds

            player = new System.Media.SoundPlayer();
            player.SoundLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HarryAlert.wav");
            player.Load();
        }

        private void TimerCallback(object state)
        {
            ShowAlert();
        }

        private void ShowAlert()
        {
            player.Play();

            Form alertForm = new Form();
            alertForm.StartPosition = FormStartPosition.CenterScreen;
            alertForm.Text = "Remind";
            alertForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            alertForm.ControlBox = false;

            Label messageLabel = new Label();
            messageLabel.Text = message;
            messageLabel.Dock = DockStyle.Fill;
            messageLabel.TextAlign = ContentAlignment.MiddleCenter;

            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Dock = DockStyle.Bottom;
            closeButton.Click += (sender, e) =>
            {
                player.Stop();
                alertForm.Close();
            };

            alertForm.Controls.Add(messageLabel);
            alertForm.Controls.Add(closeButton);

            alertForm.ShowDialog();
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notifyIcon.Dispose();
                timer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
