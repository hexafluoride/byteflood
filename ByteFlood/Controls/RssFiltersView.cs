using System.Windows;
using System.Windows.Controls;

namespace ByteFlood.Controls
{
    public class RssFiltersView : ViewBase
    {
        #region DefaultStyleKey

        protected override object DefaultStyleKey
        {
            get
            {
                return new ComponentResourceKey(GetType(), "RssFiltersView");
            }
        }

        #endregion

        #region ItemContainerDefaultStyleKey

        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "RssFiltersViewItem"); }
        }

        #endregion
    }
}
