using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireWrite.Terraria
{
    public partial class World
    {
        private readonly ObservableCollection<Sign> _signs = new ObservableCollection<Sign>();
        public ObservableCollection<Sign> Signs
        {
            get { return _signs; }
        }
    }
}
