using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBTIS.Core.Enums
{   
        public enum SubmittalStatus
        {
            Pending = 0,   
            New = 1,
            SubmitFailed = 2,
            DivisionReview = 3,
            ReturnedByDivision = 4,          
            HQReview = 5,
            Accepted = 6,
            Rejected = 7,
            Canceled = 8,
            ValidationFailed = 9,
            Deleted = 10,
            Merged = 11
        }

    public enum SubmittalType { Full, Partial }

}
