using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.Windows.Controls;

namespace ByteFlood.Controls
{
    public class FileListTemplateSelector : DataTemplateSelector
    {
        public override System.Windows.DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FileInfo) { return this.FileTemplate; }
            if (item is DirectoryKey) { return this.DirectoryTemplate; }

            return null;
        }
        public DataTemplate FileTemplate { get; set; }
        public DataTemplate DirectoryTemplate { get; set; }
    }
}
