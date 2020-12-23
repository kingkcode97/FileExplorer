using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;              //Directory  
using System.Reflection;      //BindingFlags

namespace FileExplorer
{
    // Giới thiệu về các công cụ của thư mục "Recent" trong máy tính, Win + R, đầu vào Recent có thể được gọi
    class RecentFilesUtil
    {
        //Lấy đường dẫn đích (đường dẫn thực) của phím tắt theo tên phím tắt (đường dẫn đầy đủ)
        public static string GetShortcutTargetFilePath(string shortcutFilename)
        {
            // Nhận type WScript.Shell
            var type = Type.GetTypeFromProgID("WScript.Shell");

            // Tạo một shorcut của type này
            object instance = Activator.CreateInstance(type);

            var result = type.InvokeMember("CreateShortCut", BindingFlags.InvokeMethod, null, instance, new object[] { shortcutFilename });

            //Nhận đường dẫn đích (đường dẫn thực)
            var targetFilePath = result.GetType().InvokeMember("TargetPath", BindingFlags.GetProperty, null, result, null) as string;

            return targetFilePath;
        }

        // Nhận một bộ sưu tập liệt kê các đường dẫn của các tệp được sử dụng gần đây
        public static IEnumerable<string> GetRecentFiles()
        {
            // Nhận đường dẫn Recent
            var recentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Recent);

            //Lấy tên tệp (đường dẫn đầy đủ) trong thư mục "Recent" của máy tính
            return from file in Directory.EnumerateFiles(recentFolder)

                       // Chỉ lấy các tệp loại lối tắt
                   where Path.GetExtension(file) == ".lnk"

                   // Đưa ra con đường thực tương ứng
                   select GetShortcutTargetFilePath(file);
        }
    }
}
