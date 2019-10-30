using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using technology.tabula;

namespace Extract.AttributeFinder
{
    [CLSCompliant(false)]
    public interface ITabulaTableFinder
    {
        IEnumerable<Table> GetTablesFromPageArea(Page pageArea);
    }
}
