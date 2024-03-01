using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WireWrite.Terraria;

namespace WireWrite.Script
{
    public partial class Script
    {
        public Script(Sign sign)
        {
            string text = sign.Text.Replace("\\n", "\n").Replace("\\r", "\r");
            _title = text.Substring(11, text.IndexOf(Environment.NewLine) - 11);
            _text = text.Remove(0, text.IndexOf(Environment.NewLine));
            _x = sign.X;
            _y = sign.Y;
        }

        string _title;
        string _text;
        int _x;
        int _y;

        public string Title => _title;
        public string Text => _text;
        public int X => _x;
        public int Y => _y;

    }
}
