using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Models.Bangumi;

public class Actor
{
    public Images images { get; set; }
    public string name { get; set; }
    public string short_summary { get; set; }
    public List<string> career { get; set; }
    public int id { get; set; }
    public int type { get; set; }
}
