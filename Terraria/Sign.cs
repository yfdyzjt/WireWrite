using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireWrite.Terraria
{
    public class Sign
    {
        public const int LegacyLimit = 1000;

        public Sign()
        {
            Text = string.Empty;
        }

        public Sign(int x, int y, string text)
        {
            Text = text;
            X = x;
            Y = y;
        }

        private string _name = string.Empty;

        public string Name { get; set; }
        public string Text { get; set; }

        public int Y { get; set; }

        public int X { get; set; }


        public override string ToString()
        {
            return $"[Sign: {Text.Substring(0, Math.Max(25, Text.Length))}[{Text.Length}], ({X},{Y})]";
        }


        public Sign Copy()
        {
            return new Sign(X, Y, Text);
        }
    }
}
