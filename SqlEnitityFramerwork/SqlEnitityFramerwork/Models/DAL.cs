using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlEnitityFramerwork.Core;

namespace SqlEnitityFramerwork.Models
{
    public class Dal<T> : SqlCore where T : class, new()
    {
        public Dal()
            : base("DataContext")
        {
            Model = new SqlQueryable<T> { Include = new List<string>() };
        }
        public SqlQueryable<T> Model { get; set; }
    }
}