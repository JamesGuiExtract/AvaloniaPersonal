using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndexConverterV2.Models.AllEnums
{
    public enum DateDayFormat {
        DD,       //01
        D,        //1
        DayShort, //Sat
        DayFull   //Saturday
    }

    public enum DateMonthFormat {
        MM,         //01
        M,          //1
        MonthShort, //Jan
        MonthFull   //January
    }

    public enum DateYearFormat {
        YY,  //00
        YYYY //2000
    }

    public enum DateSeparationFormat
    {
        Hyphens, //1-1-2000
        Slashes, //1/1/2000
        Spaces   //1 1 2000
    }
}
