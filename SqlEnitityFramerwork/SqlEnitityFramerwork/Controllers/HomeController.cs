using SqlEnitityFramerwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlEnitityFramerwork.Core;

namespace SqlEnitityFramerwork.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            var dal = new Dal<tblItem>();
            //var demo1 = dal.Model.ToEnumerable();//简单的全查询

            //var demo2 = dal.Model.FirstOrDefault();//查询第一条数据

            //var demo3 = dal.Model.FirstOrDefault(q => q.ItemName == "香蕉");//根据条件查询第一条

            //var demo4 = dal.Model.Where(q => q.ItemName == "香蕉").ToEnumerable();//根据条件查询

            //var demo5 = dal.Model.OrderBy(q => q.ItemName).ToEnumerable();//顺序排序

            //var demo6 = dal.Model.OrderByDesc(q => q.ItemName).ToEnumerable();//倒序排序

            
            //var demo7=new tblItem();
            //dal.Create(demo7);//添加数据

            //var demo8 = new List<tblItem>();
            //dal.BulkCreate(demo8);//批量添加数据

            //dal.Model.Where(q => q.ItemName == "香蕉").Delete();//根据条件删除数据,包括批量删除

            //dal.Model.Delete(5);//根据主键值去删除数据

            ////分页查询数据
            //dal.Model.Skip = 0;
            //dal.Model.PageSize = 10;
            //dal.Model.ToEnumerable();


            return View();
        }

    }
}
