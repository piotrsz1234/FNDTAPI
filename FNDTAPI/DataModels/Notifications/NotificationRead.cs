using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FDNTAPI.DataModels.Notifications {

    public class NotificationRead {

        public Guid ID { get; set; }
        public Guid NotificationID { get; set; }
        public string User { get; set; }

    }

}