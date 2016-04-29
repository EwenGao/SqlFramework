using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SqlEnitityFramerwork.Models
{
    public class tblItem
    {
        [Key]
        public int FlowID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string ItemSPic { get; set; }
        public string ItemBPic { get; set; }
        public string ItemUnit { get; set; }
        public decimal BuyMoney { get; set; }
        public decimal SaleMoney { get; set; }
        public string ItemPercent { get; set; }
        public int ItemStatus { get; set; }
        public DateTime? UpdatesTime { get; set; }
        public DateTime? OffTime { get; set; }
        public DateTime? Created { get; set; }
    }
}