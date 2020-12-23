using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualBasic.Devices;
using System.Threading;
using System.Security.AccessControl;

namespace FileExplorer
{
    public partial class MainForm : Form
    {
        // Đường dẫn hiện tại
        private string curFilePath = "";

        // Nút cây hiện được chọn (nút thư mục)
        private TreeNode curSelectedNode = null;

        // Có di chuyển tệp hay không
        private bool isMove = false;

        // Đường dẫn nguồn của tệp \ thư mục sẽ được sao chép và dán
        private string[] copyFilesSourcePaths = new string[200];

        // Nút đường dẫn đầu tiên của đường dẫn truy cập lịch sử của người dùng
        private DoublyLinkedListNode firstPathNode = new DoublyLinkedListNode();

        // Nút đường dẫn hiện tại
        private DoublyLinkedListNode curPathNode = null;

        // Tên tệp để tìm kiếm
        private string fileName;

        // Có khởi chạy tvwDirectory lần đầu tiên không
        private bool isInitializeTvwDirectory = true;

        public MainForm()
        {
            InitializeComponent();
        }



        private void MainForm_Load(object sender, EventArgs e)
        {
            // Khởi tạo các tùy chọn "chế độ xem" liên quan
            InitViewChecks();

            // Khởi tạo hiển thị giao diện trình quản lý
            InitDisplay();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Khi kích thước của biểu mẫu thay đổi, độ dài của thanh địa chỉ cũng thay đổi
            tscboAddress.Width = this.Width - 290;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            // Khi kích thước của biểu mẫu thay đổi, độ dài của thanh địa chỉ cũng thay đổi
            tscboAddress.Width = this.Width - 290;
        }


        private void tsmiNewFolder_Click(object sender, EventArgs e)
        {
            // thư mục mới
            CreateFolder();
        }

        private void tsmiNewFile_Click(object sender, EventArgs e)
        {
            // tạo một tệp mới
            CreateFile();
        }

        private void tsmiProperties_Click(object sender, EventArgs e)
        {
            // Hiển thị cửa sổ thuộc tính
            ShowAttributeForm();
        }


        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            // Sao chép tệp
            CopyFiles();
        }

        private void tsmiPaste_Click(object sender, EventArgs e)
        {
            //  Dán tệp
            PasteFiles();
        }

        private void tsmiCut_Click(object sender, EventArgs e)
        {
            // Cắt tệp
            CutFiles();
        }

        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            // Xóa các tập tin
            DeleteFiles();
        }




        private void tsmiToolbar_Click(object sender, EventArgs e)
        {
            // Đặt xem thanh công cụ có hiển thị hay không
            tsMain.Visible = !tsMain.Visible;

            tsmiToolbar.Checked = !tsmiToolbar.Checked;
        }

        private void tsmiStatusbar_Click(object sender, EventArgs e)
        {
            // Đặt xem thanh trạng thái có hiển thị hay không
            ssFooter.Visible = !ssFooter.Visible;

            tsmiStatusbar.Checked = !tsmiStatusbar.Checked;
        }

        private void tsmiSmallIcon_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiSmallIcon.Checked = true;
            tsmiSmallIcon1.Checked = true;
            lvwFiles.View = View.SmallIcon;
        }

        private void tsmiList_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiList.Checked = true;
            tsmiList1.Checked = true;
            lvwFiles.View = View.List;
        }

        private void tsmiDetailedInfo_Click(object sender, EventArgs e)
        {
            ResetViewChecks();
            tsmiDetailedInfo.Checked = true;
            tsmiDetailedInfo1.Checked = true;
            lvwFiles.View = View.Details;
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            ShowFilesList(curFilePath, false);
        }


        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.Show();
        }




        // Lưu ý: Trong logic phía sau và phía trước, không có nút đường dẫn mới nào được tạo,
        // nhưng dựa trên các nút đường dẫn hiện có (tham chiếu)

        // Trở lại
        private void tsbtnBack_Click(object sender, EventArgs e)
        {
            if (curPathNode != firstPathNode)
            {
                curPathNode = curPathNode.PreNode;
                string prePath = curPathNode.Path;

                ShowFilesList(prePath, false);

                // Có nút chuyển tiếp
                tsbtnAdvance.Enabled = true;
            }
            else
            {
                // Nút quay lại không khả dụng
                tsbtnBack.Enabled = false;
            }
        }

        // tiến lên
        private void tsbtnAdvance_Click(object sender, EventArgs e)
        {
            if (curPathNode.NextNode != null)
            {
                curPathNode = curPathNode.NextNode;
                string nextPath = curPathNode.Path;

                ShowFilesList(nextPath, false);

                // Có nút quay lại
                tsbtnBack.Enabled = true;
            }
            else
            {
                // Nút chuyển tiếp không khả dụng
                tsbtnAdvance.Enabled = false;
            }
        }

        // Lên một thư mục cấp
        private void tsbtnUpArrow_Click(object sender, EventArgs e)
        {
            if (curFilePath == "")
            {
                return;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(curFilePath);

            // Thư mục gốc như: C: \, D: \, E: \, v.v.
            // Nếu bạn chưa vào thư mục gốc
            if (directoryInfo.Parent != null)
            {
                ShowFilesList(directoryInfo.Parent.FullName, true);
            }
            //Đã đến thư mục gốc, dừng lại
            else
            {
                return;
            }
        }


        private void tscboAddress_KeyDown(object sender, KeyEventArgs e)
        {
            // Nhập địa chỉ mới
            if (e.KeyCode == Keys.Enter)
            {
                string newPath = tscboAddress.Text;

                if (newPath == "")
                {
                    return;
                }
                else if (!Directory.Exists(newPath))
                {
                    return;
                }

                ShowFilesList(newPath, true);
            }
        }




        private void tscboSearch_Enter(object sender, EventArgs e)
        {
            tscboSearch.Text = "";
        }

        private void tscboSearch_Leave(object sender, EventArgs e)
        {
            tscboSearch.Text = "Search";
        }

        private void tscboSearch_KeyDown(object sender, KeyEventArgs e)
        {
            // Nhập tên tệp
            if (e.KeyCode == Keys.Enter)
            {
                string fileName = tscboSearch.Text;

                if (string.IsNullOrEmpty(fileName))
                {
                    return;
                }

                //Tìm kiếm tệp / thư mục bằng nhiều chuỗi
                SearchWithMultiThread(curFilePath, fileName);
            }
        }


        // Khi menu ngữ cảnh được mở
        private void cmsMain_Opening(object sender, CancelEventArgs e)
        {
            // Chuyển đổi tọa độ hiện tại thu được của chuột (chuyển đổi tọa độ màn hình thành tọa độ không gian làm việc)
            Point curPoint = lvwFiles.PointToClient(Cursor.Position);

            //Nhận ListViewItem tại tọa độ
            ListViewItem item = lvwFiles.GetItemAt(curPoint.X, curPoint.Y);

            // Vị trí hiện tại có ListViewItem
            if (item != null)
            {
                tsmiOpen.Visible = true;
                tsmiView1.Visible = false;
                tsmiRefresh1.Visible = false;
                tsmiCopy1.Visible = true;
                tsmiPaste1.Visible = false;
                tsmiCut1.Visible = true;
                tsmiDelete1.Visible = true;
                tsmiRename.Visible = true;
                tsmiNewFolder1.Visible = false;
                tsmiNewFile1.Visible = false;
                tssLine4.Visible = false;
            }
            // Không có ListViewItem ở vị trí hiện tại
            else
            {
                tsmiOpen.Visible = false;
                tsmiView1.Visible = true;
                tsmiRefresh1.Visible = true;
                tsmiCopy1.Visible = false;
                tsmiPaste1.Visible = true;
                tsmiCut1.Visible = false;
                tsmiDelete1.Visible = false;
                tsmiRename.Visible = false;
                tsmiNewFolder1.Visible = true;
                tsmiNewFile1.Visible = true;
                tssLine4.Visible = true;
            }
        }

        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            Open();
        }

        private void tsmiRename_Click(object sender, EventArgs e)
        {
            //Đổi tên tệp
            RenameFile();
        }

        private void lvwFiles_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            string newName = e.Label;

            //Chọn mục
            ListViewItem selectedItem = lvwFiles.SelectedItems[0];

            //Nếu tên trống
            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Tên tệp không được để trống！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Khi được hiển thị, hãy khôi phục nhãn gốc
                e.CancelEdit = true;
            }
            //Nhãn không thay đổi
            else if (newName == null)
            {
                return;
            }
            //Nhãn đã thay đổi, nhưng cuối cùng nó vẫn như cũ
            else if (newName == selectedItem.Text)
            {
                return;
            }
            //Tên tệp không hợp lệ
            else if (!IsValidFileName(newName))
            {
                MessageBox.Show("The file name cannot contain any of the following characters:\r\n" + "\t\\/:*?\"<>|", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                //Khi được hiển thị, hãy khôi phục nhãn gốc
                e.CancelEdit = true;
            }
            else
            {
                Computer myComputer = new Computer();

                //Nếu nó là một tập tin
                if (File.Exists(selectedItem.Tag.ToString()))
                {
                    //Nếu có một tệp có cùng tên trong đường dẫn hiện tại
                    if (File.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("There is a file with the same name in the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //Khi được hiển thị, hãy khôi phục nhãn gốc
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameFile(selectedItem.Tag.ToString(), newName);

                        FileInfo fileInfo = new FileInfo(selectedItem.Tag.ToString());
                        string parentPath = Path.GetDirectoryName(fileInfo.FullName);
                        string newPath = Path.Combine(parentPath, newName);

                        //Cập nhật tag item đã chọn 
                        selectedItem.Tag = newPath;

                        //Làm mới cây thư mục bên trái
                        LoadChildNodes(curSelectedNode);
                    }
                }
                //Nếu nó là một thư mục
                else if (Directory.Exists(selectedItem.Tag.ToString()))
                {
                    //Nếu có một thư mục có cùng tên trong đường dẫn hiện tại
                    if (Directory.Exists(Path.Combine(curFilePath, newName)))
                    {
                        MessageBox.Show("There is a folder with the same name in the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        //Khi được hiển thị, hãy khôi phục nhãn gốc
                        e.CancelEdit = true;
                    }
                    else
                    {
                        myComputer.FileSystem.RenameDirectory(selectedItem.Tag.ToString(), newName);

                        DirectoryInfo directoryInfo = new DirectoryInfo(selectedItem.Tag.ToString());
                        string parentPath = directoryInfo.Parent.FullName;
                        string newPath = Path.Combine(parentPath, newName);

                        //Cập nhật tag item đã chọn 
                        selectedItem.Tag = newPath;

                        //Làm mới cây thư mục bên trái
                        LoadChildNodes(curSelectedNode);
                    }
                }
            }
        }




        //Kích hoạt sự kiện (hành động kích hoạt mặc định là "nhấp đúp")
        private void lvwFiles_ItemActivate(object sender, EventArgs e)
        {
            Open();
        }


        //TreeView có một quy trình lấy tiêu điểm theo mặc định, nút trên cùng được chọn theo mặc định,
        // tức là chỉ số bằng 0, là nút "được truy cập gần đây" sẽ gọi
        //Chức năng này làm cho chế độ xem danh sách tệp ở bên phải là dạng xem danh sách tệp "được truy cập gần đây".
        private void tvwDirectory_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //Khởi tạo tvwDirectory lần đầu tiên
            if (isInitializeTvwDirectory)
            {
                curFilePath = @"Recently visited";
                tscboAddress.Text = curFilePath;

                //Đường dẫn đầu tiên để lưu đường dẫn lịch sử của người dùng
                firstPathNode.Path = curFilePath;
                curPathNode = firstPathNode;

                curSelectedNode = e.Node;

                //Danh sách các tệp "được truy cập gần đây" được hiển thị ở dạng bên phải
                ShowFilesList(curFilePath, true);

                isInitializeTvwDirectory = false;
            }
            else
            {
                curSelectedNode = e.Node;
                ShowFilesList(e.Node.Tag.ToString(), true);
            }

        }

        private void tvwDirectory_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            //Trước khi nút đã chọn được mở rộng, hãy tải các nút con của nút đã chọn
            LoadChildNodes(e.Node);
        }

        private void tvwDirectory_AfterExpand(object sender, TreeViewEventArgs e)
        {
            //Sau khi nút đã chọn được mở rộng, hãy mở rộng nút
            e.Node.Expand();
        }

        //Lưu chỉ mục của biểu tượng trong khu vực thư mục ở bên trái cửa sổ trong ImageList (cụ thể là ilstDirectoryIcons)
        private class IconsIndexes
        {
            public const int FixedDrive = 0; //Đĩa cố định
            public const int CDRom = 1; //CD
            public const int RemovableDisk = 2; //ổ đĩa di động
            public const int Folder = 3; //Biểu tượng thư mục
            public const int RecentFiles = 4; // gần đây
        }


        //Lớp nút danh sách được liên kết kép (được sử dụng để lưu trữ đường dẫn truy cập lịch sử của người dùng)
        class DoublyLinkedListNode
        {
            //Đường dẫn đã lưu
            public string Path { set; get; }
            public DoublyLinkedListNode PreNode { set; get; }
            public DoublyLinkedListNode NextNode { set; get; }

        }



        //Hiển thị giao diện trình quản lý khởi tạo 
        // (khởi tạo dạng xem cây ổ đĩa của biểu mẫu bên trái và chế độ xem danh sách tệp của biểu mẫu bên phải)
        private void InitDisplay()
        {
            tvwDirectory.Nodes.Clear();

            TreeNode recentFilesNode = tvwDirectory.Nodes.Add("Recently Visited");
            recentFilesNode.Tag = "Recently Visited";
            recentFilesNode.ImageIndex = IconsIndexes.RecentFiles;
            recentFilesNode.SelectedImageIndex = IconsIndexes.RecentFiles;

            DriveInfo[] driveInfos = DriveInfo.GetDrives();

            foreach (DriveInfo info in driveInfos)
            {
                TreeNode driveNode = null;

                switch (info.DriveType)
                {

                    //Đĩa cố định
                    case DriveType.Fixed:

                        //Tên hiển thị
                        driveNode = tvwDirectory.Nodes.Add("Local Disk(" + info.Name.Split('\\')[0] + ")");

                        //đường dẫn thực sự
                        driveNode.Tag = info.Name;

                        driveNode.ImageIndex = IconsIndexes.FixedDrive;
                        driveNode.SelectedImageIndex = IconsIndexes.FixedDrive;

                        break;

                    //ổ đĩa di động
                    case DriveType.Removable:

                        //Tên hiển thị
                        driveNode = tvwDirectory.Nodes.Add("Removable Disk(" + info.Name.Split('\\')[0] + ")");

                        //Real path
                        driveNode.Tag = info.Name;

                        driveNode.ImageIndex = IconsIndexes.RemovableDisk;
                        driveNode.SelectedImageIndex = IconsIndexes.RemovableDisk;

                        break;
                }
            }

            //Tải các thư mục con dưới mỗi đĩa
            foreach (TreeNode node in tvwDirectory.Nodes)
            {
                LoadChildNodes(node);
            }


            //Trong số đó, hiển thị dạng xem danh sách tệp của biểu mẫu bên phải thực sự được gọi theo mặc định khi tvwDirectory được khởi tạo.
            // chức năng tvwDirectory_AfterSelect, không cần phải viết thêm mã ở đây

        }



        //Tải nút con (tải thư mục con trong thư mục hiện tại)
        private void LoadChildNodes(TreeNode node)
        {
            try
            {
                //Xóa các nút trống trước khi tải các nút con
                node.Nodes.Clear();

                if (node.Tag.ToString() == "Recently Visited")
                {
                    return;
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(node.Tag.ToString());
                    DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

                    foreach (DirectoryInfo info in directoryInfos)
                    {
                        //Tên hiển thị
                        TreeNode childNode = node.Nodes.Add(info.Name);

                        //Real path
                        childNode.Tag = info.FullName;

                        childNode.ImageIndex = IconsIndexes.Folder;
                        childNode.SelectedImageIndex = IconsIndexes.Folder;

                        //Tải các nút trống để nhận ra dấu "+"
                        childNode.Nodes.Add("");
                    }
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //Hiển thị tất cả các tệp / thư mục theo đường dẫn được chỉ định ở dạng phù hợp
        public void ShowFilesList(string path, bool isRecord)
        {
            //Có nút quay lại
            tsbtnBack.Enabled = true;

            //Cần lưu hồ sơ, bạn cần tạo một nút đường dẫn mới
            if (isRecord)
            {
                //Lưu đường dẫn truy cập lịch sử của người dùng
                DoublyLinkedListNode newNode = new DoublyLinkedListNode();
                newNode.Path = path;
                curPathNode.NextNode = newNode;
                newNode.PreNode = curPathNode;

                curPathNode = newNode;
            }


            //Bắt đầu cập nhật dữ liệu
            lvwFiles.BeginUpdate();

            //Xóa lvwFiles
            lvwFiles.Items.Clear();

            if (path == "Recently Visited")
            {
                //Nhận một bộ sưu tập liệt kê các đường dẫn của các tệp được sử dụng gần đây
                var recentFiles = RecentFilesUtil.GetRecentFiles();

                foreach (string file in recentFiles)
                {
                    if (File.Exists(file))
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                        //Tệp Exe hoặc không có phần mở rộng
                        if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                        {
                            //Lấy biểu tượng tương ứng của tệp thông qua hệ thống hiện tại
                            Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                            //Bởi vì các tệp exe khác nhau thường có các biểu tượng khác nhau, bạn không thể truy cập các biểu tượng theo phần mở rộng, 
                            //nên truy cập các biểu tượng theo tên tệp
                            ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                            item.ImageKey = fileInfo.Name;
                        }
                        //Những tập tin khác
                        else
                        {
                            if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                            {
                                Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                                //Vì các tệp cùng loại (ngoại trừ exe) có các biểu tượng giống nhau, 
                                // các biểu tượng có thể được truy cập bằng tiện ích mở rộng
                                ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                            }

                            item.ImageKey = fileInfo.Extension;
                        }

                        item.Tag = fileInfo.FullName;
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.SubItems.Add(fileInfo.Extension + "File");
                        item.SubItems.Add(AttributeForm.ShowFileSize(fileInfo.Length).Split('(')[0]);

                    }
                    else if (Directory.Exists(file))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(file);

                        ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                        item.Tag = dirInfo.FullName;
                        item.SubItems.Add(dirInfo.LastWriteTime.ToString());
                        item.SubItems.Add("Folder");
                        item.SubItems.Add("");
                    }
                }
            }
            else
            {
                try
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();
                    FileInfo[] fileInfos = directoryInfo.GetFiles();

                    //Xóa biểu tượng của tệp exe trong ilstIcons (ImageList) để giải phóng không gian của ilstIcons
                    foreach (ListViewItem item in lvwFiles.Items)
                    {
                        if (item.Text.EndsWith(".exe"))
                        {
                            ilstIcons.Images.RemoveByKey(item.Text);
                        }
                    }



                    //Liệt kê tất cả các thư mục
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                        item.Tag = dirInfo.FullName;
                        item.SubItems.Add(dirInfo.LastWriteTime.ToString());
                        item.SubItems.Add("Folder");
                        item.SubItems.Add("");
                    }

                    //Liệt kê tất cả các tệp
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                        //Tệp Exe hoặc không có phần mở rộng
                        if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                        {
                            //Lấy biểu tượng tương ứng của tệp thông qua hệ thống hiện tại
                            Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                            //Bởi vì các tệp exe khác nhau thường có các biểu tượng khác nhau, không thể truy cập các biểu tượng theo phần mở rộng, 
                            // nên truy cập các biểu tượng theo tên tệp
                            ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                            item.ImageKey = fileInfo.Name;
                        }
                        //Những tập tin khác
                        else
                        {
                            if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                            {
                                Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                                //Vì các tệp cùng loại (ngoại trừ exe) có các biểu tượng giống nhau, các biểu tượng có thể được truy cập bằng tiện ích mở rộng
                                ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                            }

                            item.ImageKey = fileInfo.Extension;
                        }

                        item.Tag = fileInfo.FullName;
                        item.SubItems.Add(fileInfo.LastWriteTime.ToString());
                        item.SubItems.Add(fileInfo.Extension + "File");
                        item.SubItems.Add(AttributeForm.ShowFileSize(fileInfo.Length).Split('(')[0]);
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }


            //Cập nhật đường dẫn hiện tại
            curFilePath = path;

            //Cập nhật thanh địa chỉ
            tscboAddress.Text = curFilePath;

            //Cập nhật thanh trạng thái
            tsslblFilesNum.Text = lvwFiles.Items.Count + " items";

            //Kết thúc cập nhật dữ liệu
            lvwFiles.EndUpdate();
        }



        //Kiểm tra xem tên tệp có hợp pháp không, tên tệp không được chứa các ký tự \ /: *? "<> |
        private bool IsValidFileName(string fileName)
        {
            bool isValid = true;

            //Biểu tượng Invaild
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



        //Hiển thị cửa sổ thuộc tính
        private void ShowAttributeForm()
        {
            //Không có tệp / thư mục nào được chọn ở dạng phù hợp
            if (lvwFiles.SelectedItems.Count == 0)
            {

                if (curFilePath == "Recently Visited")
                {
                    MessageBox.Show("Can't view the properties of the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AttributeForm attributeForm = new AttributeForm(curFilePath);

                //Hiển thị các thuộc tính của thư mục hiện tại
                attributeForm.Show();
            }
            //Có các tệp / thư mục được chọn ở dạng phù hợp
            else
            {
                //Hiển thị các thuộc tính của tệp / thư mục đầu tiên được chọn
                AttributeForm attributeForm = new AttributeForm(lvwFiles.SelectedItems[0].Tag.ToString());

                attributeForm.Show();
            }
        }


        //Hiển thị cửa sổ quản lý quyền

        //thư mục mới
        private void CreateFolder()
        {
            if (curFilePath == "Recently Visited")
            {
                MessageBox.Show("Cannot create a new folder in the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                int num = 1;
                string path = Path.Combine(curFilePath, "New Folder");
                string newFolderPath = path;

                while (Directory.Exists(newFolderPath))
                {
                    newFolderPath = path + "(" + num + ")";
                    num++;
                }

                Directory.CreateDirectory(newFolderPath);

                ListViewItem item = lvwFiles.Items.Add("New Folder" + (num == 1 ? "" : "(" + (num - 1) + ")"), IconsIndexes.Folder);

                //Real path
                item.Tag = newFolderPath;

                //Làm mới cây thư mục bên trái
                LoadChildNodes(curSelectedNode);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        //tạo một tệp mới
        private void CreateFile()
        {
            if (curFilePath == "Recently Visited")
            {
                MessageBox.Show("Can't create a new file in the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            NewFileForm newFileForm = new NewFileForm(curFilePath, this);
            newFileForm.Show();
        }



        //Mở tệp / thư mục
        private void Open()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                string path = lvwFiles.SelectedItems[0].Tag.ToString();

                try
                {
                    //Nếu thư mục đã chọn
                    if (Directory.Exists(path))
                    {
                        //Mở thư mục
                        ShowFilesList(path, true);
                    }
                    //Nếu tệp được chọn là
                    else
                    {
                        //mở tệp tin
                        Process.Start(path);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        //Sao chép tệp
        private void CopyFiles()
        {
            //Lấy đường dẫn nguồn của tệp được sao chép
            SetCopyFilesSourcePaths();
        }



        //Dán tệp
        private void PasteFiles()
        {
            //Không có tệp nào để dán
            if (copyFilesSourcePaths[0] == null)
            {
                return;
            }

            //Đường dẫn hiện tại không hợp lệ
            if (!Directory.Exists(curFilePath))
            {
                return;
            }

            if (curFilePath == "Recently Visited")
            {
                MessageBox.Show("Can't paste in the current path！", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; copyFilesSourcePaths[i] != null; i++)
            {
                //Nếu nó là một tập tin
                if (File.Exists(copyFilesSourcePaths[i]))
                {
                    //"Chuyển tới" hoặc "Sao chép vào" của tệp thực thi
                    MoveToOrCopyToFileBySourcePath(copyFilesSourcePaths[i]);
                }
                //Nếu nó là một thư mục
                else if (Directory.Exists(copyFilesSourcePaths[i]))
                {
                    //Thực hiện thư mục "chuyển đến" hoặc "sao chép vào"
                    MoveToOrCopyToDirectoryBySourcePath(copyFilesSourcePaths[i]);
                }

            }

            //Danh sách tệp được hiển thị trong cửa sổ bên phải
            ShowFilesList(curFilePath, false);

            //Làm mới cây thư mục bên trái
            LoadChildNodes(curSelectedNode);

            //Blanking
            copyFilesSourcePaths = new string[200];
        }



        //di chuyển tệp
        private void CutFiles()
        {
            //Lấy đường dẫn nguồn của tệp được sao chép
            SetCopyFilesSourcePaths();

            //Sẵn sàng di chuyển
            isMove = true;
        }



        //Xóa các tập tin
        private void DeleteFiles()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure to delete it？", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.No)
                {
                    return;
                }
                else
                {
                    try
                    {
                        foreach (ListViewItem item in lvwFiles.SelectedItems)
                        {
                            string path = item.Tag.ToString();

                            //Nếu nó là một tập tin
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            //Nếu nó là một thư mục
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, true);
                            }

                            lvwFiles.Items.Remove(item);
                        }

                        //Làm mới cây thư mục bên trái
                        LoadChildNodes(curSelectedNode);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }



        //Lấy đường dẫn nguồn của tệp được sao chép
        private void SetCopyFilesSourcePaths()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                int i = 0;

                foreach (ListViewItem item in lvwFiles.SelectedItems)
                {
                    copyFilesSourcePaths[i++] = item.Tag.ToString();
                }

                isMove = false;
            }
        }



        //"Chuyển tới" hoặc "Sao chép vào" của tệp thực thi
        private void MoveToOrCopyToFileBySourcePath(string sourcePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(sourcePath);

                //Nhận đường dẫn đích
                string destPath = Path.Combine(curFilePath, fileInfo.Name);

                //Nếu đường dẫn đích và đường dẫn nguồn giống nhau, không làm gì cả
                if (destPath == sourcePath)
                {
                    return;
                }

                //Di chuyển tệp đến đường dẫn đích (hiện đang thực hiện thao tác "cắt + dán")
                if (isMove)
                {
                    fileInfo.MoveTo(destPath);
                }
                //Dán tệp vào đường dẫn đích (hiện đang thực hiện thao tác "sao chép + dán")
                else
                {
                    fileInfo.CopyTo(destPath);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        //Đệ quy, sao chép và dán thư mục (bao gồm tất cả các tệp trong thư mục)
        //Không có phương thức DirectoryInfo.CopyTo (đường dẫn chuỗi), bạn cần tự triển khai
        private void CopyAndPasteDirectory(DirectoryInfo sourceDirInfo, DirectoryInfo destDirInfo)
        {
            //Xác định xem thư mục đích có phải là thư mục con của thư mục nguồn hay không, 
            // nếu có, hãy đưa ra lời nhắc lỗi và không làm gì cả
            for (DirectoryInfo dirInfo = destDirInfo.Parent; dirInfo != null; dirInfo = dirInfo.Parent)
            {
                if (dirInfo.FullName == sourceDirInfo.FullName)
                {
                    MessageBox.Show("Could not copy! The target folder is a subdirectory of the source folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            //Tạo thư mục đích
            if (!Directory.Exists(destDirInfo.FullName))
            {
                Directory.CreateDirectory(destDirInfo.FullName);
            }

            //Sao chép tệp và dán tệp vào thư mục đích
            foreach (FileInfo fileInfo in sourceDirInfo.GetFiles())
            {
                fileInfo.CopyTo(Path.Combine(destDirInfo.FullName, fileInfo.Name));
            }

            //Sao chép và dán đệ quy các thư mục con trong thư mục đích
            foreach (DirectoryInfo sourceSubDirInfo in sourceDirInfo.GetDirectories())
            {
                DirectoryInfo destSubDirInfo = destDirInfo.CreateSubdirectory(sourceSubDirInfo.Name);
                CopyAndPasteDirectory(sourceSubDirInfo, destSubDirInfo);
            }

        }



        //Thực hiện thư mục "chuyển đến" hoặc "sao chép vào"
        private void MoveToOrCopyToDirectoryBySourcePath(string sourcePath)
        {
            try
            {
                DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(sourcePath);

                //  Nhận đường dẫn đích
                string destPath = Path.Combine(curFilePath, sourceDirectoryInfo.Name);

                //Nếu đường dẫn đích và đường dẫn nguồn giống nhau, không làm gì cả
                if (destPath == sourcePath)
                {
                    return;
                }

                //Di chuyển thư mục đến đường dẫn đích (hiện đang thực hiện thao tác "cắt + dán")
                if (isMove)
                {
                    //Nếu sourceDirectoryInfo.MoveTo (destPath) được sử dụng, nó không hỗ trợ di chuyển các thư mục trên các đĩa

                    //Đệ quy, sao chép và dán thư mục (bao gồm tất cả các tệp trong thư mục)
                    CopyAndPasteDirectory(sourceDirectoryInfo, new DirectoryInfo(destPath));

                    //Xóa thư mục nguồn
                    Directory.Delete(sourcePath, true);

                }
                //Dán thư mục vào đường dẫn đích (hiện đang thực hiện thao tác "sao chép + dán")
                else
                {
                    //Đệ quy, sao chép và dán thư mục (bao gồm tất cả các tệp trong thư mục)
                    CopyAndPasteDirectory(sourceDirectoryInfo, new DirectoryInfo(destPath));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        //Đổi tên tập tin
        private void RenameFile()
        {
            if (lvwFiles.SelectedItems.Count > 0)
            {
                //Mô phỏng chỉnh sửa nhãn, về cơ bản để kích hoạt sự kiện LabelEdit thông qua mã
                lvwFiles.SelectedItems[0].BeginEdit();
            }
        }



        //Khởi tạo các tùy chọn "chế độ xem" liên quan
        private void InitViewChecks()
        {
            //Cửa sổ mặc định ở bên phải hiển thị chế độ xem chi tiết
            tsmiDetailedInfo.Checked = true;
            tsmiDetailedInfo1.Checked = true;
        }



        //Đặt lại các tùy chọn "Chế độ xem" liên quan
        private void ResetViewChecks()
        {
            tsmiSmallIcon.Checked = false;
            tsmiList.Checked = false;
            tsmiDetailedInfo.Checked = false;

            tsmiSmallIcon1.Checked = false;
            tsmiList1.Checked = false;
            tsmiDetailedInfo1.Checked = false;
        }




        //Tìm kiếm tệp / thư mục bằng nhiều chuỗi
        private void SearchWithMultiThread(string path, string fileName)
        {
            //Xóa lvwFiles
            lvwFiles.Items.Clear();

            //Cập nhật thanh trạng thái
            tsslblFilesNum.Text = 0 + " items";

            this.fileName = fileName;

            ThreadPool.SetMaxThreads(1000, 1000);

            //Bắt đầu một chuỗi để thực hiện thao tác tìm kiếm
            ThreadPool.QueueUserWorkItem(new WaitCallback(Search), path);

        }



        //Tìm kiếm đa luồng cho các tệp / thư mục
        public void Search(Object obj)
        {
            string path = obj.ToString();

            DirectorySecurity directorySecurity = new DirectorySecurity(path, AccessControlSections.Access);

            //Thư mục có thể được truy cập
            if (!directorySecurity.AreAccessRulesProtected)
            {

                //Đường dẫn được tìm kiếm
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                //Các tệp trong đường dẫn được tìm kiếm
                FileInfo[] fileInfos = directoryInfo.GetFiles();

                //Tìm kiếm tệp
                if (fileInfos.Length > 0)
                {
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        try
                        {
                            if (fileInfo.Name.Split('.')[0].Contains(fileName))
                            {
                                AddSearchResultItemIntoList(fileInfo.FullName, true);

                                //Cập nhật thanh trạng thái
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " items";
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }

                }


                //Các thư mục con dưới đường dẫn được tìm kiếm
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

                //Tìm kiếm thư mục
                if (directoryInfos.Length > 0)
                {
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        try
                        {
                            if (dirInfo.Name.Contains(fileName))
                            {
                                AddSearchResultItemIntoList(dirInfo.FullName, false);

                                //Cập nhật thanh trạng thái
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " items";
                            }
                            else
                            {
                                // Chiến lược đa luồng: Bắt đầu từ thư mục cần tìm kiếm, một luồng được mở để tìm kiếm đệ quy mỗi khi gặp thư mục trong quá trình đệ quy, tổng số luồng lớn, nhưng
                                // Sử dụng nhóm luồng, nó sẽ được quản lý tự động, để luồng có thể được sử dụng nhiều lần, sau khi hoàn thành tác vụ tìm kiếm một luồng, nó có thể tiếp tục được sử dụng
                                // Thực thi một tác vụ tìm kiếm khác trong hàng đợi tác vụ.
                                // Ưu điểm: Nó có thể thích ứng với các tình huống thông thường, và tốc độ tìm kiếm nói chung là rất nhanh!
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Search), dirInfo.FullName);
                            }
                        }
                        catch (Exception e)
                        {

                        }

                    }
                }

            }
        }

        //Sử dụng một chuỗi đơn để tìm kiếm một thư mục con
        public void SearchWithOneThread(object obj)
        {
            string path = obj.ToString();

            DirectorySecurity directorySecurity = new DirectorySecurity(path, AccessControlSections.Access);

            //Thư mục có thể được truy cập
            if (!directorySecurity.AreAccessRulesProtected)
            {

                //thư mục con
                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                //Tệp trong thư mục con
                FileInfo[] fileInfos = directoryInfo.GetFiles();

                //Thư mục dưới thư mục con
                DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();


                //Tìm kiếm tệp
                if (fileInfos.Length > 0)
                {
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        try
                        {
                            if (fileInfo.Name.Split('.')[0].Contains(fileName))
                            {
                                AddSearchResultItemIntoList(fileInfo.FullName, true);

                                //Cập nhật thanh trạng thái
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " items";
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }

                }


                //Tìm kiếm thư mục
                if (directoryInfos.Length > 0)
                {
                    foreach (DirectoryInfo dirInfo in directoryInfos)
                    {
                        try
                        {
                            if (dirInfo.Name.Contains(fileName))
                            {
                                AddSearchResultItemIntoList(dirInfo.FullName, false);

                                //Cập nhật thanh trạng thái
                                tsslblFilesNum.Text = lvwFiles.Items.Count + " items";
                            }
                            else
                            {
                                SearchWithOneThread(dirInfo.FullName);
                            }
                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                    }
                }
            }
        }



        //Hiển thị kết quả tìm kiếm trên danh sách tệp
        private void AddSearchResultItemIntoList(string fullPath, bool isFile)
        {
            //Là một tập tin
            if (isFile)
            {
                FileInfo fileInfo = new FileInfo(fullPath);

                ListViewItem item = lvwFiles.Items.Add(fileInfo.Name);

                //Tệp Exe hoặc không có phần mở rộng
                if (fileInfo.Extension == ".exe" || fileInfo.Extension == "")
                {
                    //Lấy biểu tượng tương ứng của tệp thông qua hệ thống hiện tại
                    Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                    //Bởi vì các tệp exe khác nhau thường có các biểu tượng khác nhau, bạn không thể truy cập các biểu tượng theo phần mở rộng, 
                    // bạn nên truy cập các biểu tượng theo tên tệp
                    ilstIcons.Images.Add(fileInfo.Name, fileIcon);

                    item.ImageKey = fileInfo.Name;
                }
                //Những tập tin khác
                else
                {
                    if (!ilstIcons.Images.ContainsKey(fileInfo.Extension))
                    {
                        Icon fileIcon = GetSystemIcon.GetIconByFileName(fileInfo.FullName);

                        //Vì các tệp cùng loại (ngoại trừ exe) có các biểu tượng giống nhau, 
                        // các biểu tượng có thể được truy cập bằng tiện ích mở rộng
                        ilstIcons.Images.Add(fileInfo.Extension, fileIcon);
                    }

                    item.ImageKey = fileInfo.Extension;
                }

                item.Tag = fileInfo.FullName;

                item.SubItems.Add(fileInfo.LastWriteTimeUtc.ToString());
                item.SubItems.Add(fileInfo.Extension + "File");
                item.SubItems.Add(AttributeForm.ShowFileSize(fileInfo.Length).Split('(')[0]);
            }
            //Là một thư mục
            else
            {
                DirectoryInfo dirInfo = new DirectoryInfo(fullPath);

                ListViewItem item = lvwFiles.Items.Add(dirInfo.Name, IconsIndexes.Folder);
                item.Tag = dirInfo.FullName;
                item.SubItems.Add(dirInfo.LastWriteTimeUtc.ToString());
                item.SubItems.Add("Folder");
                item.SubItems.Add("");
            }
        }

    }

}
