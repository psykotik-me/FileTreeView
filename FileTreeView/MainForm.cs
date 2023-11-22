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

namespace FileTreeView
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            _PopulateTreeView();
        }


        private void _PopulateTreeView()
        {
            TreeNode rootNode;

            // Отримати всі доступні диски
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Створити DirectoryInfo для кожного кореневого каталогу
            foreach (DriveInfo drive in drives)
            {
                // Визначити шлях до кореневої директорії
                string rootDirectoryPath = drive.RootDirectory.FullName;

                // Створити об'єкт DirectoryInfo
                DirectoryInfo info = new DirectoryInfo(rootDirectoryPath);

                // Отримати підкаталоги з доступом за допомогою LINQ
                var accessibleDirectories = info.GetDirectories()
                    .Where(directory => IsAccessibleDir(directory));
                List<DirectoryInfo> accessibleInfo = new List<DirectoryInfo>();
 
                // Вивести підкаталоги
                foreach (DirectoryInfo directory in accessibleDirectories)
                {
                    accessibleInfo.Add(directory);
                }

                if (info.Exists)
                {
                    rootNode = new TreeNode(info.Name);
                    rootNode.Tag = info;
                    _GetDirectories(accessibleInfo.ToArray(), rootNode);
                    treeView1.Nodes.Add(rootNode);
                }

            }

        }

        private void _GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            try
            {
                DirectoryInfo[] subSubDirs;
                foreach (DirectoryInfo subDir in subDirs)
                {
                    aNode = new TreeNode(subDir.Name, 0, 0);
                    aNode.Tag = subDir;
                    aNode.ImageKey = "folder";
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0)
                    {
                        _GetDirectories(subSubDirs, aNode);
                    }
                    nodeToAddTo.Nodes.Add(aNode);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Unauthorized Access Exception: {ex.Message}");
            }
        }

        // Метод для перевірки, чи є доступ до директорії
        static bool IsAccessibleDir(DirectoryInfo directory)
        {
            try
            {
                _ = directory.GetFiles(); // Спробувати отримати файли в директорії
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        static string ConvertSizeToReadable(long sizeInBytes)
        {
            const long kilobyte = 1024;
            const long megabyte = kilobyte * 1024;
            const long gigabyte = megabyte * 1024;

            if (sizeInBytes >= gigabyte)
            {
                return $"{sizeInBytes / (double)gigabyte:F2} GB";
            }
            else if (sizeInBytes >= megabyte)
            {
                return $"{sizeInBytes / (double)megabyte:F2} MB";
            }
            else if (sizeInBytes >= kilobyte)
            {
                return $"{sizeInBytes / (double)kilobyte:F2} KB";
            }
            else
            {
                return $"{sizeInBytes} bytes";
            }
        }

        private void _TreeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    { 
                        new ListViewItem.ListViewSubItem(item, "Directory"),
                        new ListViewItem.ListViewSubItem(item, "")
                    };
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    {
                        new ListViewItem.ListViewSubItem(item, file.Extension.ToString()),
                        new ListViewItem.ListViewSubItem(item, ConvertSizeToReadable(file.Length))
                    };

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}
