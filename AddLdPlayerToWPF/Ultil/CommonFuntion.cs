using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddLdPlayerToWPF.Ultil
{
    public static class CommonFuntion
    {
        public static List<string> GetName(int thread)
        {
            List<string> list = new List<string>();
            for (int i = 1; i < thread + 1; i++)
            {
                list.Add("LDPlayer-" + i.ToString());
            }
            return list;
        }
    }
}
