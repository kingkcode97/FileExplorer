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
    public partial class NewFileForm : Form
    {
        // Đường dẫn hiện tại
        private string curPath;

        // Tham chiếu đến biểu mẫu chính của người quản lý
        private MainForm mainForm;


        public NewFileForm(string curPath, MainForm mainForm)
        {
            InitializeComponent();
            this.curPath = curPath;
            this.mainForm = mainForm;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string newFileName = txtNewFileName.Text;

            string newFilePath = Path.Combine(curPath, newFileName);

            // Tên tệp không hợp lệ
            if (!IsValidFileName(newFileName))
            {
                MessageBox.Show("The file name cannot contain any of the following characters:\r\n" + "\t\\/:*?\"<>|", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (File.Exists(newFilePath))
            {
                MessageBox.Show("A file with the same name exists in the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                File.Create(newFilePath);

                // Cập nhật danh sách tệp
                mainForm.ShowFilesList(curPath, false);

                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        // Kiểm tra xem tên tệp có hợp pháp không, tên tệp không được chứa các ký tự \ /: *? "<> |
        private bool IsValidFileName(string fileName)
        {
            bool isValid = true;

            // Biểu tượng Invaild
            string errChar = "\\/:*?\"<>|";

            for (int i = 0; i < errChar.Length; i++)
            {
                if (fileName.Contains(errChar[i].ToString()))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }


    }
}
