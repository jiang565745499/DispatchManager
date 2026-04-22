using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Schedule.Entitys
{
    public class inventoryReq
    {
        public Head? head { get; set; }

        public string? inventoryNo { get; set; }
        public string? supplierCode { get; set; }
        public string? plantCode { get; set; }
        public string? baseCode { get; set; }
        public string? whAddress { get; set; }
        public string? invOrg { get; set; }
        public string? invDate { get; set; }
        public string? isThirdParty { get; set; }
        public string? partyCode { get; set; }

        public List<inventoryList>? inventoryList { get; set; }
    }

    public class Head
    {
        public string? remoteUser { get; set; }
        public string? sn { get; set; }
        public string? signature { get; set; }
        public long timestamp { get; set; }

    }

    public class inventoryList
    {
        public string? partNo { get; set; }
        public string? partDesc { get; set; }
        public string? onhandQty { get; set; }
        public string? quarantineQty { get; set; }
        public string? inspectQty { get; set; }
        public string? defectQty { get; set; }
        public string? frozenQty { get; set; }
        public string? wipQty { get; set; }
        public string? transQty { get; set; }
        public string? unit { get; set; }

    }
}
