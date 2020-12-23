using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class AttributeForm : Form
    {
        public AttributeForm(string filePath)
        {
            InitializeComponent();
            InitDisplay(filePath);
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Khởi tạo giao diện
        private void InitDisplay(string filePath)
        {
            //Nếu filePath là đường dẫn của tệp
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);

                txtFileName.Text = fileInfo.Name;
                txtFileType.Text = fileInfo.Extension;
                txtFileLocation.Text = (fileInfo.DirectoryName != null) ? fileInfo.DirectoryName : null;
                txtFileSize.Text = ShowFileSize(fileInfo.Length);
                txtFileCreateTime.Text = fileInfo.CreationTime.ToString();
                txtFileModifyTime.Text = fileInfo.LastWriteTime.ToString();
                txtFileAccessTime.Text = fileInfo.LastAccessTime.ToString();
            }
            //Nếu filePath là đường dẫn của thư mục
            else if (Directory.Exists(filePath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(filePath);

                txtFileName.Text = directoryInfo.Name;
                txtFileType.Text = "Folder";
                txtFileLocation.Text = (directoryInfo.Parent != null) ? directoryInfo.Parent.FullName : null;
                txtFileSize.Text = ShowFileSize(GetDirectoryLength(filePath));
                txtFileCreateTime.Text = directoryInfo.CreationTime.ToString();
                txtFileModifyTime.Text = directoryInfo.LastWriteTime.ToString();
                txtFileAccessTime.Text = directoryInfo.LastAccessTime.ToString();
            }
        }


        //Nhận kích thước của thư mục
        private long GetDirectoryLength(string dirPath)
        {
            long length = 0;
            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);


            //Nhận kích thước của tất cả các tệp trong thư mục
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            if (fileInfos.Length > 0)
            {
                foreach (FileInfo fileInfo in fileInfos)
                {
                    length += fileInfo.Length;
                }
            }


            //Đệ quy lấy kích thước của tất cả các thư mục trong thư mục
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
            if (directoryInfos.Length > 0)
            {
                foreach (DirectoryInfo dirInfo in directoryInfos)
                {
                    length += GetDirectoryLength(dirInfo.FullName);
                }
            }

            return length;
        }


        //Hiển thị kích thước của tệp ở một định dạng nhất định
        //Math.Round (num, 2, MidpointRounds.AwayFromZero) num giữ lại 2 chữ số thập phân
        public static string ShowFileSize(long fileSize)
        {
            string fileSizeStr = "";

            if (fileSize < 1024)
            {
                fileSizeStr = fileSize + " byte";
            }
            else if (fileSize >= 1024 && fileSize < 1024 * 1024)
            {
                fileSizeStr = Math.Round(fileSize * 1.0 / 1024, 2, MidpointRounding.AwayFromZero) + " KB(" + fileSize + "byte)";
            }
            else if (fileSize >= 1024 * 1024 && fileSize < 1024 * 1024 * 1024)
            {
                fileSizeStr = Math.Round(fileSize * 1.0 / (1024 * 1024), 2, MidpointRounding.AwayFromZero) + " MB(" + fileSize + "byte)";
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                fileSizeStr = Math.Round(fileSize * 1.0 / (1024 * 1024 * 1024), 2, MidpointRounding.AwayFromZero) + " GB(" + fileSize + "byte)";
            }

            return fileSizeStr;
        }

    }
}
