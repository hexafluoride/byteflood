using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ByteFlood.Controls
{
    public class ClickableLabel : Label
    {
        private bool _is_clicking = false;

        public ClickableLabel()
        {
            this.MouseLeftButtonDown += (s, e) =>
            {
                this._is_clicking = true;
            };

            this.MouseLeave += (s, e) =>
            {
                this._is_clicking = false;
            };

            this.MouseLeftButtonUp += (s, e) =>
            {
                if (_is_clicking & this.ClickEvent != null)
                {
                    ClickEvent(this, null);
                    _is_clicking = false;
                }
            };

            this.Cursor = System.Windows.Input.Cursors.Hand;
        }

        public System.Windows.RoutedEventHandler ClickEvent
        {
            get { return (System.Windows.RoutedEventHandler)GetValue(ClickEventProperty); }
            set { SetValue(ClickEventProperty, value); }
        }

        public static readonly DependencyProperty ClickEventProperty =
            DependencyProperty.Register("ClickEvent", typeof(System.Windows.RoutedEventHandler), typeof(ClickableLabel), new PropertyMetadata(null));
    }
}
