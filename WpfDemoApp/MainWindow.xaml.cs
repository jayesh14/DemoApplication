using System;
using System.Windows;
using WpfDemoApp.helperClass;

namespace WpfDemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUrl.Text))
                xwb.Navigate(txtUrl.Text);
            else
                MessageBox.Show("Please enter URL");
        }


        private void capture_HtmlImageCapture(object sender, Uri url)
        {
            MessageBox.Show("saved successfully.");
        }

        private void btnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUrl.Text))
            {
                if (!string.IsNullOrWhiteSpace(txtSaveLocation.Text))
                {
                    HtmlCapture capture = new HtmlCapture(txtSaveLocation.Text);
                    capture.HtmlImageCapture += new HtmlCapture.HtmlCaptureEvent(capture_HtmlImageCapture);
                    capture.Create(txtUrl.Text);
                }
                else
                {
                    MessageBox.Show("Please enter Save Location with image name");
                }
            }
            else
            {
                MessageBox.Show("Please enter URL");
            }
        }
    }
}





