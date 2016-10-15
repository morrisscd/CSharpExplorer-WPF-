using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Drawing;
using System.Runtime.InteropServices;

namespace fileExplorer 
{
    public partial class LoadDrivesDirs : Window
    {   
        public LoadDrivesDirs()
        {
            InitializeComponent();

            DriveInfo[] drives = DriveInfo.GetDrives();
            DriveInfo defaultDrive = drives[0];

            CreateDriveColumn(drives);
            CreateNameColumn(defaultDrive.RootDirectory);
           
        }

        public void CreateDriveColumn(DriveInfo[] drives)
        {                                   
            foreach (DriveInfo driveInfo in drives)
            {
                trvStructure.Items.Add(CreateTreeItem(driveInfo));                                   
            }
        }
           
        private TreeViewItem CreateTreeItem(object o)
        {
            TreeViewItem item = new TreeViewItem();

            item.Margin = new Thickness(0, 0, 0, 0);
            item.Header = CreateStackPanel(o, false);       
            item.Tag = o ;
            item.Items.Add("Loading...");
            //event to handle clicking the expand arrow
            item.Expanded += new RoutedEventHandler(OnTreeViewDirExpand);
            //this allows a single mouse click to expand the child tree nodes
           // item.Selected += new RoutedEventHandler(OnTreeViewDirExpand);
            return item;
        }
      
        public BitmapImage GetFolderImage()
        {
            BitmapImage folderImage = new BitmapImage();
            folderImage.BeginInit();
            folderImage.UriSource = new Uri("C:\\Programming\\CSharpExplorer (WPF)\\FolderImage2.png");
            folderImage.EndInit();

            return folderImage;
        }

        public void CreateNameColumn(DirectoryInfo dir)
        {
            dirFileList.Items.Clear();

            foreach (DirectoryInfo treeViewDirExpanded in dir.GetDirectories())
            {
                try
                {
                    dirFileList.Items.Add(new stackPanelParams()
                    {
                        ImageSource = GetFolderImage(),
                        ImageHeight = 16,
                        ImageWidth = 16,
                        FolderLabel = treeViewDirExpanded.ToString(),
                        FileCreationTime = treeViewDirExpanded.CreationTime,
                        ItemTag = treeViewDirExpanded,
                        FileType = "Folder"
                       // CheckBoxShow= Visibility.Hidden
                    });
                  
                }
                catch (UnauthorizedAccessException ex)
                {

                }
            }
        }

      //This populates the name column etc when the expand arrow in the treeview is clicked
        public void OnTreeViewDirExpand(object sender, RoutedEventArgs e)
        {
            TreeViewItem dirItem = e.Source as TreeViewItem;

            if ((dirItem.Items.Count == 1) && (dirItem.Items[0] is string))
            {
                dirItem.Items.Clear();
                dirFileList.Items.Clear();

                DirectoryInfo expandedDir = null;
                if (dirItem.Tag is DriveInfo)
                    expandedDir = (dirItem.Tag as DriveInfo).RootDirectory;
                if (dirItem.Tag is DirectoryInfo)
                    expandedDir = (dirItem.Tag as DirectoryInfo);
                try
                {
                    foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                    {
                        dirItem.Items.Add(CreateTreeItem(subDir));
                    }
                    CreateNameColumn(expandedDir);
                   // dirItem.Selected += new RoutedEventHandler(OnTreeViewDirExpand);
                }
                catch { }
            }

            ResizeGridViewColumn(gvcName);
            ResizeGridViewColumn(gvcFileType);
            
        }

        public void TreeViewItem_Enter(object sender, RoutedEventArgs e)
        {
          ListViewItem item = e.Source as ListViewItem;

          //dirFileList.Items.Add(new stackPanelParams()
          //{            
          //    CheckBoxShow = Visibility.Visible
          //});
                  

         // stackPanelParams spp = new stackPanelParams();
          
          //spp.CheckBoxShow = Visibility.Visible;
           // spp.FolderLabel = "hello";
       
        }

        public void TreeViewItem_Leave(object sender, RoutedEventArgs e)
        {
            stackPanelParams obj = new stackPanelParams();
            obj.CheckBoxShow = Visibility.Hidden;
        }

        //This captures single clicks on the drive tree and double clicks on the name column
        public void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            ListViewItem selectedItem = e.Source as ListViewItem;            
            TreeViewItem treeItem = e.Source as TreeViewItem;
          
            DirectoryInfo expandedDir = null;

            if (treeItem != null)
            {
                if (treeItem.Tag is DriveInfo)
                    expandedDir = (treeItem.Tag as DriveInfo).RootDirectory;
                if (treeItem.Tag is DirectoryInfo)
                    expandedDir = (treeItem.Tag as DirectoryInfo);
            }
            else
            {
                DirectoryInfo listItem = selectedItem.Tag as DirectoryInfo;
                expandedDir = listItem;
            }
                
            dirFileList.Items.Clear();

            NativeMethods.SHFILEINFO info = new NativeMethods.SHFILEINFO();
   
            try
            {               
                //Create folders in list view
                foreach (DirectoryInfo subDir in expandedDir.GetDirectories())
                {
                    dirFileList.Items.Add(new stackPanelParams()
                    {
                        ImageSource = GetFolderImage(),
                        ImageHeight = 16,
                        ImageWidth = 16,
                        FolderLabel = subDir.ToString(),
                        FileCreationTime = subDir.CreationTime,
                        ItemTag = subDir,
                        FileType = "Folder"
                    });
                }
                //Create files in list view
                foreach (FileInfo fileName in expandedDir.GetFiles())
                {
                    //FileInfo[] test = dirItems.GetFiles();
                    ////Get file extension type
                    uint dwFileAttributes = NativeMethods.FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
                    uint uFlags = (uint)(NativeMethods.SHGFI.SHGFI_TYPENAME | NativeMethods.SHGFI.SHGFI_USEFILEATTRIBUTES);

                    NativeMethods.SHGetFileInfo(fileName.ToString(), dwFileAttributes, ref info, (uint)Marshal.SizeOf(info), uFlags);
                    ////

                    ////Get file icon
                    var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(fileName.FullName);

                    var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                sysicon.Handle,
                                System.Windows.Int32Rect.Empty,
                                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    sysicon.Dispose();
                    ////
               
                    dirFileList.Items.Add(new stackPanelParams()
                    {
                        ImageSource = bmpSrc,
                        ImageHeight = 16,
                        ImageWidth = 16,
                        FolderLabel = fileName.ToString(),
                        FileCreationTime = fileName.CreationTime,
                        FileType = info.szTypeName,
                        ItemTag = fileName
                    });

                    ResizeGridViewColumn(gvcName);
                    ResizeGridViewColumn(gvcFileType);                    
                }                                          
            }
            catch { }
        }

        static class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct SHFILEINFO
            {
                public IntPtr hIcon;
                public int iIcon;
                public uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
                public string szTypeName;
            };

            public static class FILE_ATTRIBUTE
            {
                public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
            }

            public static class SHGFI
            {
                public const uint SHGFI_TYPENAME = 0x000000400;
                public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
            }

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        }

        //The file type gridview column needs resizing to file the type name in 
        //This needs to be done each time the gridcolumm is populated
        private void ResizeGridViewColumn(GridViewColumn column)
        {
            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }
            column.Width = double.NaN;
        }
    }


    public class stackPanelParams : INotifyPropertyChanged
    {
        DateTime _fileCreationTime;
        private Visibility visibility;
        string filename;
       
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public BitmapSource ImageSource { get; set; }              
        public string DriveLabel { get; set; }
        public string FolderLabel { get; set; }
        public object DriveInfoObj { get; set; }
        public object ItemTag { get; set; }   
        public object files { get; set; }
      //  public Visibility CheckBoxShow { get; set; }

        public Visibility CheckBoxShow
        {
            get
            {
                return visibility;
            }
            set
            {
                visibility = value;

              OnPropertyChanged("CheckBoxShow");
            }
        }

        public string FileType 
        {
            get { return filename; }
            set
            {
             if (filename != value)
                 {
                     filename = value;
                     OnPropertyChanged("FileType");               
                 }
            }
        }
      
        public DateTime FileCreationTime
        {
            get
            {
                return _fileCreationTime;
            }
            set
            {
                _fileCreationTime = value;
            }
        }

        public bool IsString(object value)
        {
            return value is string;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

    
}
