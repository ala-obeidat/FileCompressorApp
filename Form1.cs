using iTextSharp.text.pdf;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

using ImageSharp = SixLabors.ImageSharp.Image;

namespace FileCompressorApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtDirectory.Text = folderDialog.SelectedPath;
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtDirectory.Text))
            {
                btnStart.Enabled = false;
                await Task.Run(() => CompressFilesInDirectory(txtDirectory.Text));
                MessageBox.Show("Compression completed!");
                btnStart.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please select a valid directory.");
            }
        }

        private void CompressFilesInDirectory(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                .Where(f => new FileInfo(f).Length > 100 * 1024).ToArray();

            Invoke(new Action(() =>
            {
                progressBar.Maximum = files.Length;
                progressBar.Value = 0;
            }));

            foreach (var file in files)
            {
                FileInfo fileInfo = new(file);
                string extension = fileInfo.Extension.ToLower();

                switch (extension)
                {
                    case ".png":
                        if (fileInfo.Length > 200 * 1024)
                        {
                            CompressPng(file);
                        }
                        break;
                    case ".jpg":
                    case ".jpeg":
                        CompressJpeg(file);
                        break;
                    case ".pdf":
                        CompressPdf(file);
                        break;
                }

                Invoke(new Action(() => progressBar.Value++));
            }
        }

        private void CompressPng(string filePath)
        {
            using ImageSharp image = ImageSharp.Load(filePath);
            var encoder = new SixLabors.ImageSharp.Formats.Png.PngEncoder()
            {
                CompressionLevel = SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.BestCompression
            };
            image.Save(filePath, encoder);
        }

        private void CompressJpeg(string filePath)
        {
            using ImageSharp image = ImageSharp.Load(filePath);
            var encoder = new JpegEncoder()
            {
                Quality = 50 // Set quality between 0 to 100
            };
            image.Save(filePath, encoder);
        }

        private void CompressPdf(string filePath)
        {
            string tempFile = Path.GetTempFileName();

            using (PdfReader reader = new(filePath))
            using (PdfStamper stamper = new(reader, new FileStream(tempFile, FileMode.Create)))
            {
                stamper.SetFullCompression();
                stamper.Writer.CompressionLevel = PdfStream.BEST_COMPRESSION;
            }

            File.Copy(tempFile, filePath, true);
            File.Delete(tempFile);
        }
    }
}
