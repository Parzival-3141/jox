using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jox
{
    public static class Utils
    {
        public static string BytesToString(this byte[] bytes)
        {
            //  char & string uses UTF-16, but byte is a u8.
            //  So unless there's some implicit data manipulation,
            //  I think it ends up being UTF-8.

            string result = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                result += (char)bytes[i];
            }

            return result;
        }
    }

    public sealed class Void
    {
        Void() => throw new InvalidOperationException("You cannot create an instance of Void.");
    }
}
