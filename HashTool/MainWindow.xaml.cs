using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;

namespace HashTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker worker = new BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
            worker.DoWork += worker_DoWork;
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += worker_ProgressChanged;
        }

        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if(dialog.ShowDialog()==true)
            {
                string filename = dialog.FileName;
                //this.tbHash.Text = calHash(filename);
                if(!worker.IsBusy)
                {
                    this.tbHash.Text = "Calculating...\r\n " + filename;
                    worker.RunWorkerAsync(filename);
                }
            }

        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.tbHash.Text = e.UserState.ToString();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            string hash = calHash((string)e.Argument);
            worker.ReportProgress(100, hash);
        }

        private string calHash(string filename)
        {
            StringBuilder sb = new StringBuilder();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            sb.AppendLine(filename);
            byte[] hash;
            try
            {
                sw.Restart();
                hash = MD5Encrypt(filename);
                sw.Stop();
                sb.AppendLine("MD5:\t\t" + sw.ElapsedMilliseconds + "ms\t" + bytesToString(hash));

                sw.Restart();
                hash = SHA1Encrypt(filename);
                sw.Stop();
                sb.AppendLine("SHA1:\t\t" + sw.ElapsedMilliseconds + "ms\t" + bytesToString(hash));

                sw.Restart();
                hash =SHA256Encrypt(filename);
                sw.Stop();
                sb.AppendLine("SHA256:\t" + sw.ElapsedMilliseconds + "ms\t" + bytesToString(hash));

                sw.Restart();
                hash = SHA512Encrypt(filename);
                sw.Stop();
                sb.AppendLine("SHA512:\t" + sw.ElapsedMilliseconds + "ms\t" + bytesToString(hash));

                return sb.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private string bytesToString(byte[] bs)
        {
            string s = string.Empty;
            foreach(var b in bs)
            {
                s += b.ToString("x2");
            }
            return s;
        }

        #region Hash Algorithm
        private byte[] MD5Encrypt(string filename)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hash;
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
              hash  = md5.ComputeHash(br.BaseStream);
            }
            
            return hash;
        }

        private byte[] SHA1Encrypt(string filename)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] hash;
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                hash = sha1.ComputeHash(br.BaseStream);
            }

            return hash;
        }

        private byte[] SHA256Encrypt(string filename)
        {
            //SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();//调用系统api
            SHA256Managed shaM = new SHA256Managed();//.net托管代码
            byte[] hash;
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                hash = shaM.ComputeHash(br.BaseStream);
            }

            return hash;
        }

        private byte[] SHA512Encrypt(string filename)
        {
            SHA512Managed sha512 = new SHA512Managed();
            byte[] hash;
            using (BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open)))
            {
                hash = sha512.ComputeHash(br.BaseStream);
            }

            return hash;
        }
        #endregion

        #region DragDrop
        /*********
         *1.TextBox的AllowDrop属性设为 True
         *2.先处理PreviewDragEnter、PreviewDragOver事件，设置e.Handled = true
         *3.触发PreviewDrop、Drop事件
         *http://www.codeproject.com/Articles/42696/Textbox-Drag-Drop-in-WPF
         */
        private void tbHash_PreviewDrop(object sender, DragEventArgs e)
        {
            object o = e.Data.GetData(DataFormats.FileDrop);//获取拖放源数据
            string filename =((string[])o)[0];
            if (!worker.IsBusy)
            {
                this.tbHash.Text = "Calculating...\r\n " + filename;
                worker.RunWorkerAsync(filename);
            }
        }
        private void tbHash_PreviewDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;//定义拖放类型
            e.Handled = true;//关键，为False时不触发Drop相关事件
        }
        #endregion
    }
}
