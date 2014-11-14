using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteFlood
{
    public class NiceDataContext<T>
    {
        public Settings Settings { get { return App.Settings; } }

        public LanguageEngine Language { get { return App.CurrentLanguage; } }

        public State AppState { get { return (App.Current.MainWindow as MainWindow).state; } }

        public T Self { get; private set; }

        public NiceDataContext(T self)
        {
            this.Self = self;
        }
    }
}
