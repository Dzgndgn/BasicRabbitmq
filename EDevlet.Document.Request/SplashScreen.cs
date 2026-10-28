using System.Drawing;
using System.Windows.Forms;

namespace EDevlet.Document.Request
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(460, 240);
            BackColor = Color.FromArgb(24, 32, 48);
            ShowInTaskbar = false;

            var title = new Label
            {
                Text = "Belge Talep Sistemi",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 130
            };

            var status = new Label
            {
                Text = "Yükleniyor...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 140),
                Size = new Size(460, 30)
            };

            var progress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Location = new Point(60, 190),
                Size = new Size(340, 8)
            };

            Controls.Add(title);
            Controls.Add(status);
            Controls.Add(progress);
        }
        private void SplashScreen_Load(object sender, System.EventArgs e)
        {
        }
    }
}