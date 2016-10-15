using System;
//using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Input;
using CSharpExplorerWPF;

namespace fileExplorer
{
    public partial class LoadDrivesDirs : Window
    {
        public StackPanel CreateStackPanel(object o, bool checkBoxes)
        {
            StackPanel sp1 = new StackPanel();
            sp1.Orientation = Orientation.Horizontal;

            Image folderImage = new Image();
            folderImage.Source = new BitmapImage
                (new Uri("C:\\Programming\\CSharpExplorer (WPF)\\FolderImage2.png"));
            folderImage.Width = 16;
            folderImage.Height = 16;

            //using textblock as the Label class seems to add some padding
            TextBlock folderLabel = new TextBlock();
           
            folderLabel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            folderLabel.Text = o.ToString();

            //CheckBox itemCheckBox = new CheckBox();
            //if (checkBoxes == true)
            //    sp1.Children.Add(itemCheckBox);

            sp1.Children.Add(folderImage);
            sp1.Children.Add(folderLabel);
            sp1.Tag = "w";
            return sp1;
        }
    }

       
    //public partial class SingleClickExpand : TreeViewItem, LoadDrivesDirs
    //{
    //    //public UserControl1()
    //    //{
    //    //    InitializeComponent();
    //    //}

    //    public static readonly DependencyProperty ExpandNodeProperty =
    //       DependencyProperty.Register("ExpandNode", typeof(string), typeof(SingleClickExpand), new
    //          PropertyMetadata("", new PropertyChangedCallback(OnExpandNodeChanged)));

    //    public string ExpandNode
    //    {
    //        get { return (string)GetValue(ExpandNodeProperty); }
    //        set { SetValue(ExpandNodeProperty, value); }
    //    }

    //    private static void OnExpandNodeChanged(DependencyObject d,
    //       DependencyPropertyChangedEventArgs e)
    //    {
    //        SingleClickExpand obj = d as SingleClickExpand;
    //        obj.OnExpandNodeChanged(e);
    //    }

    //    private void OnExpandNodeChanged(DependencyPropertyChangedEventArgs e)
    //    {
    //        //int a = 10;
    //    }
    //}

    //An attached behaviour
    //public static class TextBoxBehaviour : LoadDrivesDirs
    //{
    //    public static bool GetSelectAll(TreeViewItem target)
    //    {
    //        return (bool)target.GetValue(SelectAllAttachedProperty);
    //    }

    //    public static void SetSelectAll(TreeViewItem target, bool value)
    //    {
    //        target.SetValue(SelectAllAttachedProperty, value);
    //    }

    //    public static readonly DependencyProperty 
    //        SelectAllAttachedProperty = DependencyProperty.RegisterAttached("SelectAll", 
    //        typeof(bool), 
    //        typeof(TextBoxBehaviour), 
    //        new UIPropertyMetadata(false, OnSelectAllAttachedPropertyChanged));

    //    static void OnSelectAllAttachedPropertyChanged(DependencyObject o, 
    //        DependencyPropertyChangedEventArgs e)
    //    {
    //        //((TreeViewItem)o).SelectAll();
    //        //OnTreeViewDirExpand(o, e);
    //        int a = 10;
    //    }

       
    //}


    public static class MyTreeViewHelper
    {
        //
        // The TreeViewItem that the mouse is currently directly over (or null).
        //
        private static TreeViewItem _currentItem = null;

        //
        // IsMouseDirectlyOverItem:  A DependencyProperty that will be true only on the 
        // TreeViewItem that the mouse is directly over.  I.e., this won't be set on that 
        // parent item.
        //
        // This is the only public member, and is read-only.
        //

        // The property key (since this is a read-only DP)
        private static readonly DependencyPropertyKey IsMouseDirectlyOverItemKey =
            DependencyProperty.RegisterAttachedReadOnly("IsMouseDirectlyOverItem",
                                                typeof(bool),
                                                typeof(MyTreeViewHelper),
                                                new FrameworkPropertyMetadata(null, 
                                                new CoerceValueCallback(CalculateIsMouseDirectlyOverItem)));

        // The DP itself
        public static readonly DependencyProperty IsMouseDirectlyOverItemProperty =
            IsMouseDirectlyOverItemKey.DependencyProperty;

        // A strongly-typed getter for the property.
        public static bool GetIsMouseDirectlyOverItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMouseDirectlyOverItemProperty);
        }

        // A coercion method for the property
        private static object CalculateIsMouseDirectlyOverItem(DependencyObject item, object value)
        {
            // This method is called when the IsMouseDirectlyOver property is being calculated
            // for a TreeViewItem.  

            if (item == _currentItem)
                return true;
            else
                return false;
        }

        //
        // UpdateOverItem:  A private RoutedEvent used to find the nearest encapsulating
        // TreeViewItem to the mouse's current position.
        //

        private static readonly RoutedEvent UpdateOverItemEvent = EventManager.RegisterRoutedEvent(
            "UpdateOverItem", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MyTreeViewHelper));

        //
        // Class constructor
        //

        static MyTreeViewHelper()
        {
            // Get all Mouse enter/leave events for TreeViewItem.
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseEnterEvent, new MouseEventHandler(OnMouseTransition), true);
            EventManager.RegisterClassHandler(typeof(TreeViewItem), TreeViewItem.MouseLeaveEvent, new MouseEventHandler(OnMouseTransition), true);

            // Listen for the UpdateOverItemEvent on all TreeViewItem's.
            EventManager.RegisterClassHandler(typeof(TreeViewItem), UpdateOverItemEvent, new RoutedEventHandler(OnUpdateOverItem));
        }


        //
        // OnUpdateOverItem:  This method is a listener for the UpdateOverItemEvent.  When it is received,
        // it means that the sender is the closest TreeViewItem to the mouse (closest in the sense of the tree,
        // not geographically).

        static void OnUpdateOverItem(object sender, RoutedEventArgs args)
        {
            // Mark this object as the tree view item over which the mouse
            // is currently positioned.
            _currentItem = sender as TreeViewItem;

            // Tell that item to re-calculate the IsMouseDirectlyOverItem property
            _currentItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);

            // Prevent this event from notifying other tree view items higher in the tree.
            args.Handled = true;
        }

        //
        // OnMouseTransition:  This method is a listener for both the MouseEnter event and
        // the MouseLeave event on TreeViewItems.  It updates the _currentItem, and updates
        // the IsMouseDirectlyOverItem property on the previous TreeViewItem and the new
        // TreeViewItem.

        static void OnMouseTransition(object sender, MouseEventArgs args)
        {
            lock (IsMouseDirectlyOverItemProperty)
            {
                if (_currentItem != null)
                {
                    // Tell the item that previously had the mouse that it no longer does.
                    DependencyObject oldItem = _currentItem;
                    _currentItem = null;
                    oldItem.InvalidateProperty(IsMouseDirectlyOverItemProperty);
                }

                // Get the element that is currently under the mouse.
                IInputElement currentPosition = Mouse.DirectlyOver;

                // See if the mouse is still over something (any element, not just a tree view item).
                if (currentPosition != null)
                {
                    // Yes, the mouse is over something.
                    // Raise an event from that point.  If a TreeViewItem is anywhere above this point
                    // in the tree, it will receive this event and update _currentItem.

                    RoutedEventArgs newItemArgs = new RoutedEventArgs(UpdateOverItemEvent);
                    currentPosition.RaiseEvent(newItemArgs);

                }
            }
        }


    }

    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }
            return 0;
        }

        private static TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as TreeViewItem;
        }
    }

    public class LeftMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
                return new Thickness(0);

            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }


   // [ValueConversion(typeof(bool), typeof(Visibility))]
    //public class VisibilityConverter : IValueConverter
    //{
    //    public const string Invert = "Invert";

    //    #region IValueConverter Members

    //    public object Convert(object value, Type targetType, object parameter,
    //        System.Globalization.CultureInfo culture)
    //    {
    //        if (targetType != typeof(Visibility))
    //            throw new InvalidOperationException("The target must be a Visibility.");

    //        bool? bValue = (bool?)value;

    //        if (parameter != null && parameter as string == Invert)
    //            bValue = !bValue;

    //        return bValue.HasValue && bValue.Value ? Visibility.Visible : Visibility.Collapsed;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter,
    //        System.Globalization.CultureInfo culture)
    //    {
    //        throw new NotSupportedException();
    //    }
    //    #endregion
    //}
      

//Namespace
}


