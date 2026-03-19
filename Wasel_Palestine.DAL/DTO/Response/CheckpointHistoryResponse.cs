using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.DAL.DTO.Response
{
    public class CheckpointHistoryResponse : BaseResponse
    {
        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public string ChangedBy { get; set; }

        public DateTime ChangedAt { get; set; }
    }
}
