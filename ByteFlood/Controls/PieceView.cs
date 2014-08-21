using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ByteFlood.Controls
{
    public class PieceView : ViewBase
    {
        #region DefaultStyleKey

        protected override object DefaultStyleKey
        {
            get
            {
                return new ComponentResourceKey(GetType(), "PieceView");
            }
        }

        #endregion

        #region ItemContainerDefaultStyleKey

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "PieceViewItem"); }
        }

        #endregion
    }
}
