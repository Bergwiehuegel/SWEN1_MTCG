using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Text.Json;

namespace MTCG.Models
{
    internal class CardCollection
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<Card>? Collection { get; set; }


        public static void PrintCollection(List<List<string>> collection)
        {

        }

    }
}
