using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileTreeViewWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _PopulateTreeView();
        }

        public class Filedata
        {
            public Filedata(string name, string extension, string size)
            {
                Name = name;
                Extension = extension;
                Size = size;
            }
    
            public string Name { get; set; }
            public string Extension { get; set; }
            public string Size { get; set; }
        }

        private void _PopulateTreeView()
        {
            TreeViewItem rootNode;

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
                    rootNode = new TreeViewItem();
                    rootNode.Header = info.Name;
                    rootNode.Tag = info;
                    rootNode.GotFocus += _OnTreeViewItemGotFocus;
                    _GetDirectories(accessibleInfo.ToArray(), rootNode);
                    treeView1.Items.Add(rootNode);
                }

            }

        }

        private void _GetDirectories(DirectoryInfo[] subDirs, TreeViewItem nodeToAddTo)
        {
            TreeViewItem aNode;
            try
            {
                DirectoryInfo[] subSubDirs;
                foreach (DirectoryInfo subDir in subDirs)
                {
                    aNode = new TreeViewItem();
                    aNode.Header = subDir.Name;
                    aNode.Tag = subDir;
                    aNode.GotFocus += _OnTreeViewItemGotFocus;
                    aNode.HeaderTemplate = (DataTemplate)FindResource("folderViewItem");
                    Console.WriteLine(aNode.HeaderTemplate);
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length != 0)
                    {
                        _GetDirectories(subSubDirs, aNode);
                    }
                    nodeToAddTo.Items.Add(aNode);
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

    private void _OnTreeViewItemGotFocus(object sender, RoutedEventArgs e)
    {
            ObservableCollection<Filedata> fds = new ObservableCollection<Filedata>();

            TreeViewItem newSelected = e.OriginalSource as TreeViewItem;
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                fds.Add(new Filedata(dir.Name, "<= directory", ""));
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                fds.Add(new Filedata(file.Name, file.Extension.ToString(), ConvertSizeToReadable(file.Length)));
            }
      
            listView1.ItemsSource = fds;                            
      
        }

    }
}
