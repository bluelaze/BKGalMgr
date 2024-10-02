using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BKGalMgr.Models.Bangumi;

public class Subject
{
    public string date { get; set; }
    public string platform { get; set; }
    public Images images { get; set; }
    public string summary { get; set; }
    public string name { get; set; }
    public string name_cn { get; set; }
    public List<Tag> tags { get; set; }
    public List<InfoPaire> infobox { get; set; }
    public int id { get; set; }
    public bool nsfe { get; set; }
    public int type { get; set; }
}
