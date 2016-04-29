using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SqlEnitityFramerwork.Core
{
    public static class SqlExtension
    {
        #region 查询函数
        /// <summary>
        /// 查询所有字段的数据
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToEnumerable<T>(this SqlQueryable<T> source) where T : new()
        {
            var t = typeof(T);
            var p = t.GetProperties();
            var sql = new StringBuilder();
            sql.Append("select " + (source.PageSize > 0 ? "top " + source.PageSize : ""));
            sql.Append("* from (select ROW_NUMBER()over(order by " + (source.OrderBy ?? (p[0].Name + " desc")));
            sql.Append(") as row,* from " + t.Name);
            if (source.Include.Count > 0 && p.Any(q => source.Include.Contains(q.PropertyType.Name)))
            {
                var attr = p.Where(q => q.GetCustomAttributes(false).Any()).ToList();
                string forgeinKey = string.Empty;
                foreach (var a in attr)
                {
                    var tt = a.GetCustomAttributes(false);
                    if (tt.Select(tName => tName.GetType().Name).Any(t3 => t3 == "ForeignKeyAttribute"))
                    {
                        forgeinKey = a.Name;
                    }
                    if (!string.IsNullOrWhiteSpace(forgeinKey))
                    {
                        break;
                    }
                }
                foreach (var c in source.Include)
                {
                    var item = p.FirstOrDefault(q => q.Name == c);
                    if (item == null) continue;
                    var t2 = item.PropertyType;
                    var p1 = t2.GetProperties().Where(q => q.GetCustomAttributes(false).Any()).ToList();
                    var keyAttribute = string.Empty;
                    foreach (var pp in p1)
                    {
                        if (pp.GetCustomAttributes(false).Select(tName => tName.GetType().Name).Any(t3 => t3 == "KeyAttribute"))
                        {
                            keyAttribute = pp.Name;
                        }
                        if (!string.IsNullOrWhiteSpace(forgeinKey))
                        {
                            break;
                        }
                    }
                    sql.Append(" join " + c + " on " + forgeinKey + "=" + keyAttribute);
                }
            }
            sql.Append((source.Where ?? "") + ")as t");
            sql.Append(source.Skip > 0 ? " where row>" + source.Skip : "");
            var dt = SqlCore.SqlSelect(sql.ToString());
            return  dt.AutoMapp<T>() ;
        }
        /// <summary>
        /// 查询指定字段的数据 
        /// </summary>
        /// <typeparam name="T">字段实体</typeparam>
        /// <typeparam name="TResult">映射到的实体类</typeparam>
        /// <param name="source"></param>
        /// <param name="selector">查询字段表达式树</param>
        /// <returns></returns>
        public static IEnumerable<TResult> ToEnumerable<T, TResult>(this SqlQueryable<T> source, Expression<Func<T, TResult>> selector) where TResult : new()
        {
            var t = typeof(T);
            var p = t.GetProperties();
            var sql = new StringBuilder();
            sql.Append("select " + (source.PageSize > 0 ? "top " + source.PageSize : ""));
            sql.Append("* from (select ROW_NUMBER()over(order by " +
                       (source.OrderBy ?? (p[0].Name + " desc")) + ") as row,");
            var body = (MemberInitExpression)selector.Body;
            foreach (var item in body.Bindings)
            {
                sql.Append(item.Member.Name.Insert(0, " ") + (body.Bindings.IndexOf(item) == body.Bindings.Count - 1 ? " " : ","));
            }
            foreach (var item in source.Include)
            {
                sql.Append("," + item + ".*");
            }
            sql.Append(" from " + t.Name);
            if (source.Include.Count > 0 && p.Any(q => source.Include.Contains(q.PropertyType.Name)))
            {
                var attr = p.Where(q => q.GetCustomAttributes(false).Any()).ToList();
                string forgeinKey = string.Empty;
                foreach (var a in attr)
                {
                    var tt = a.GetCustomAttributes(false);
                    if (tt.Select(tName => tName.GetType().Name).Any(t3 => t3 == "ForeignKeyAttribute"))
                    {
                        forgeinKey = a.Name;
                    }
                    if (!string.IsNullOrWhiteSpace(forgeinKey))
                    {
                        break;
                    }
                }
                foreach (var c in source.Include)
                {
                    var item = p.FirstOrDefault(q => q.Name == c);
                    if (item == null) continue;
                    var t2 = item.PropertyType;
                    var p1 = t2.GetProperties().Where(q => q.GetCustomAttributes(false).Any()).ToList();
                    var keyAttribute = string.Empty;
                    foreach (var pp in p1)
                    {
                        if (pp.GetCustomAttributes(false).Select(tName => tName.GetType().Name).Any(t3 => t3 == "KeyAttribute"))
                        {
                            keyAttribute = pp.Name;
                        }
                        if (!string.IsNullOrWhiteSpace(forgeinKey))
                        {
                            break;
                        }
                    }
                    sql.Append(" join " + c + " on " + forgeinKey + "=" + keyAttribute);
                }
            }
            sql.Append((source.Where ?? "") + ")as t");
            sql.Append(source.Skip > 0 ? " where row>" + source.Skip : "");
            var dt = SqlCore.SqlSelect(sql.ToString());
            return dt != null ? dt.AutoMapp<TResult>() : null;
        }
         /// <summary>
        /// 查询总的数据条数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int Count<T>(this SqlQueryable<T> source)
        {
            var t = typeof(T);
            var sql = new StringBuilder();
            sql.Append("select count(0) from " + t.Name);
            sql.Append(source.Where ?? "");
            return (int)SqlCore.ExecuteScalar(sql.ToString());

        }

        /// <summary>
        /// 返回集合数据的第一条数据，没有数据返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this SqlQueryable<T> source) where T : new()
        {
            source.PageSize = 1;
            var items = source.ToEnumerable().ToArray();
            return  items.FirstOrDefault();
        }
        /// <summary>
        /// 返回符合条件的第一条数据，没有数据返回null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T, TResult>(this SqlQueryable<T> source, Expression<Func<T, TResult>> selector) where T : new()
        {
            source.PageSize = 1;
            source.Where = " where " + ExpressionRouter(selector.Body);
            var items = source.ToEnumerable().ToArray();
            return items.Any() ? items.FirstOrDefault() : default(T);
        }
        /// <summary>
        /// 生产SQL顺序排序语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="soure"></param>
        /// <param name="selector">排序字段表达式树</param>
        /// <returns></returns>
        public static SqlQueryable<T> OrderBy<T, TResult>(this SqlQueryable<T> soure, Expression<Func<T, TResult>> selector)
        {
            var body = (MemberInitExpression)selector.Body;
            var order = new StringBuilder();
            foreach (var item in body.Bindings)
            {
                order.Append(item.Member.Name.Insert(0, " ") + (body.Bindings.IndexOf(item) == body.Bindings.Count - 1 ? "" : ","));
            }
            order.Append(" asc");
            soure.OrderBy = order.ToString();
            return soure;
        }
        /// <summary>
        /// 生成SQL倒序排序语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="soure"></param>
        /// <param name="selector">排序字段表达式树</param>
        /// <returns></returns>
        public static SqlQueryable<T> OrderByDesc<T, TResult>(this SqlQueryable<T> soure, Expression<Func<T, TResult>> selector)
        {
            var body = (MemberInitExpression)selector.Body;
            var order = new StringBuilder();
            foreach (var item in body.Bindings)
            {
                order.Append(item.Member.Name.Insert(0, " ") + (body.Bindings.IndexOf(item) == body.Bindings.Count - 1 ? "" : ","));
            }
            order.Append(" desc");
            soure.OrderBy = order.ToString();
            return soure;
        }
        /// <summary>
        /// 生成SQL条件语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector">条件表达式树</param>
        /// <returns></returns>
        public static SqlQueryable<T> Where<T, TResult>(this SqlQueryable<T> source, Expression<Func<T, TResult>> selector)
        {
            var where = new StringBuilder();
            var sql = ExpressionRouter(selector.Body);
            where.Append(!string.IsNullOrWhiteSpace(sql) ? " where " + sql : "");
            source.Where = where.ToString();
            return source;
        }
        /// <summary>
        /// 联表查询
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="soure"></param>
        /// <param name="tableName">联表的表名</param>
        /// <returns></returns>
        public static SqlQueryable<T> Include<T>(this SqlQueryable<T> soure, string tableName)
        {
            if (soure.Include == null)
            {
                soure.Include = new List<string>();
            }
            soure.Include.Add(tableName);
            return soure;
        }


        #endregion

        #region 增加函数
        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="core">数据库类</param>
        /// <param name="model">数据实体</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool Create<T>(this SqlCore core, T model)
        {
            var t = model.GetType();
            var key = t.GetProperties().FirstOrDefault(q => q.GetCustomAttributes(false).Any(a => a.GetType().Name == "KeyAttribute"));
            var properties = t.GetProperties().Where(q => q.PropertyType.FullName.Contains("System") && q != key).ToList();
            var filed = new StringBuilder();
            var value = new StringBuilder();
            var sql = new StringBuilder();
            sql.Append("insert into " + t.Name);
            filed.Append("(");
            value.Append(" values(");
            foreach (var item in properties)
            {
                var fullName = item.PropertyType.FullName;
                var values = fullName.Contains("String") || fullName.Contains("DateTime") || fullName.Contains("Boolean")
                   ? "'" + item.GetValue(model, null) + "'"
                   : item.GetValue(model, null);
                filed.Append(item.Name + (properties.IndexOf(item) == properties.Count - 1 ? "" : ","));
                value.Append(values + (properties.IndexOf(item) == properties.Count - 1 ? "" : ","));
            }
            filed.Append(")");
            value.Append(")");
            sql.Append(filed);
            sql.Append(value);
            object primary = null;
            if (key != null)
            {
                sql.Append("select  top 1 " + key.Name + " from " + t.Name + " order by " + key.Name + " desc");
                primary = SqlCore.ExecuteScalar(sql.ToString());
                key.SetValue(model, primary, null);
            }
            return primary != null;
        }

        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="core">数据操作类</param>
        /// <param name="model">数据实体集合</param>
        /// <returns>返回执行成功的数量</returns>
        public static void BulkCreate<T>(this SqlCore core, List<T> model)
        {
            var t = typeof(T);
            var properties = t.GetProperties().Where(q => q.PropertyType.FullName.Contains("System") && q.GetCustomAttributes(false).All(a => a.GetType().Name != "KeyAttribute")).ToList();
            var dt = new DataTable();
            foreach (var column in properties.Select(pro => new DataColumn { ColumnName = pro.Name }))
            {
                dt.Columns.Add(column);
            }
            foreach (var m in model)
            {
                var row = dt.NewRow();
                foreach (var pro in properties)
                {
                    row[pro.Name] = pro.GetValue(m, null);
                }
                dt.Rows.Add(row);

            }
            SqlCore.BulkCreate(dt, t.Name);
        }

        #endregion

        #region 更新函数
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="core">数据库类实例对象</param>
        /// <param name="model">数据实体</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool Update<T>(this SqlCore core, T model)
        {
            var t = model.GetType();
            var key = t.GetProperties().FirstOrDefault(q => q.GetCustomAttributes(false).Any(a => a.GetType().Name == "KeyAttribute"));
            var properties = t.GetProperties().Where(q => q.PropertyType.FullName.Contains("System") && q != key).ToList();
            if (key == null)
            {
                return false;
            }
            var sql = new StringBuilder();
            sql.Append("Update " + t.Name + " set ");
            foreach (var item in properties)
            {
                var fullName = item.PropertyType.FullName;
                var value = fullName.Contains("String") || fullName.Contains("DateTime") || fullName.Contains("Boolean")
                    ? "'" + item.GetValue(model, null) + "'"
                    : item.GetValue(model, null);
                sql.Append(item.Name + "=" + value + (properties.IndexOf(item) == properties.Count - 1 ? "" : ","));
            }
            sql.Append(" where " + key.Name + "=" + (key.PropertyType.FullName.Contains("String") ? "'" + key.GetValue(model, null) + "'" : key.GetValue(model, null)));
            return SqlCore.ExecuteNonQuery(sql.ToString()) > 0;
        }
        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="source">数据实体操作类</param>
        /// <param name="model">新数据实体</param>
        /// <returns>返回受影响的行数</returns>
        public static int BulkUpdate<T>(this SqlQueryable<T> source, T model)
        {
            if (string.IsNullOrWhiteSpace(source.Where))
            {
                return 0;
            }
            var t = typeof(T);
            var key = t.GetProperties().FirstOrDefault(q => q.GetCustomAttributes(false).Any(a => a.GetType().Name == "KeyAttribute"));
            var properties = t.GetProperties().Where(q => q.PropertyType.FullName.Contains("System") && q != key).ToList();
            if (key == null)
            {
                return 0;
            }
            var sql = new StringBuilder();
            sql.Append("Update " + t.Name + " set ");
            foreach (var item in properties)
            {
                var fullName = item.PropertyType.FullName;
                var value = fullName.Contains("String") || fullName.Contains("DateTime") || fullName.Contains("Boolean")
                    ? "'" + item.GetValue(model, null) + "'"
                    : item.GetValue(model, null);
                sql.Append(item.Name + "=" + value + (properties.IndexOf(item) == properties.Count - 1 ? "" : ","));
            }
            sql.Append(source.Where);
            return SqlCore.ExecuteNonQuery(sql.ToString());
        }


        #endregion

        #region 删除函数

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="queryable">实体对象操作类</param>
        /// <param name="key">数据主键值，如果没传该值并且实体操作类的Where属性为空，表示全部删除该表的数据</param>
        /// <returns>成功返回true，失败返回false</returns>
        public static bool Delete<T>(this SqlQueryable<T> queryable, object key = null)
        {
            var t = typeof(T);
            var primary = t.GetProperties().FirstOrDefault(q => q.GetCustomAttributes(false).Any(a => a.GetType().Name == "KeyAttribute"));
            if (primary == null)
            {
                return false;
            }
            var fullName = primary.PropertyType.FullName;
            var sql = "delete from " + t.Name + (queryable.Where ?? (key == null ? "" : " where " + primary.Name + "=" + (fullName.Contains("String") ? "'" + key + "'" : key)));
            return SqlCore.ExecuteNonQuery(sql) > 0;
        }

        #endregion

        #region 公共方法
        /// <summary>
        /// 映射数据到实体对象
        /// </summary>
        /// <typeparam name="T">实体对象类</typeparam>
        /// <param name="dt">数据DataTable</param>
        /// <returns>映射后的实体集合</returns>
        public static IEnumerable<T> AutoMapp<T>(this DataTable dt) where T : new()
        {
            var type = typeof(T);
            if (dt == null || dt.Rows.Count < 0)
            {
                yield break;
            }
            var columns = (from DataColumn c in dt.Columns select c.ColumnName).ToList();
            var properties = type.GetProperties().Where(q => columns.Contains(q.Name)).ToArray();
            var childrens = type.GetProperties().Where(q => !q.PropertyType.FullName.Contains("System")).ToArray();
            foreach (DataRow row in dt.Rows)
            {
                var t = new T();
                foreach (var item in properties)
                {
                    if (row[item.Name] != DBNull.Value)
                    {
                        item.SetValue(t, row[item.Name], null);
                    }
                }
                foreach (var children in childrens)
                {
                    var childrenObj = children.GetValue(t, null);
                    if (childrenObj != null)
                    {
                        foreach (var childrenPro in children.PropertyType.GetProperties().Where(q => columns.Contains(q.Name)))
                        {
                            if (row[childrenPro.Name] != DBNull.Value)
                            {
                                childrenPro.SetValue(childrenObj, row[childrenPro.Name], null);
                            }
                        }
                    }
                    children.SetValue(t, childrenObj, null);
                }
                yield return t;
            }
        }

        /// <summary>
        /// 绑定表达式树，并根据类型生成相应的SQL语句
        /// </summary>
        /// <param name="exp">表达式树</param>
        /// <returns></returns>
        static string ExpressionRouter(Expression exp)
        {
            var expression = exp as BinaryExpression;
            if (expression != null)
            {
                var be = expression;
                return BinarExpressionProvider(be.Left, be.Right, be.NodeType);
            }
            var memberExpression = exp as MemberExpression;
            if (memberExpression != null)
            {
                var me = memberExpression;
                return me.Member.Name;
            }
            var arrayExpression = exp as NewArrayExpression;
            if (arrayExpression != null)
            {
                var ae = arrayExpression;
                var tmpstr = new StringBuilder();
                foreach (Expression ex in ae.Expressions)
                {
                    tmpstr.Append(ExpressionRouter(ex));
                    tmpstr.Append(",");
                }
                return tmpstr.ToString(0, tmpstr.Length - 1);
            }
            var callExpression = exp as MethodCallExpression;
            if (callExpression != null)
            {
                var mce = callExpression;
                if (mce.Method.Name == "Like")
                    return string.Format("({0} like {1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                if (mce.Method.Name == "NotLike")
                    return string.Format("({0} Not like {1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                if (mce.Method.Name == "In")
                    return string.Format("{0} In ({1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                if (mce.Method.Name == "NotIn")
                    return string.Format("{0} Not In ({1})", ExpressionRouter(mce.Arguments[0]), ExpressionRouter(mce.Arguments[1]));
                //if (mce.Method.Name == "Any")
                //    return string.Format("{0}",ExpressionRouter(mce.Arguments[1]));
            }
            else
            {
                var constantExpression = exp as ConstantExpression;
                if (constantExpression != null)
                {
                    var ce = constantExpression;
                    if (ce.Value == null)
                        return "null";
                    if (ce.Value is ValueType)
                        return ce.Value.ToString();
                    if (ce.Value is string || ce.Value is DateTime || ce.Value is char)
                        return string.Format("'{0}'", ce.Value);
                }
                else
                {
                    var unaryExpression = exp as UnaryExpression;
                    if (unaryExpression != null)
                    {
                        var ue = unaryExpression;
                        return ExpressionRouter(ue.Operand);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 条件表达式树，成Where条件
        /// </summary>
        /// <param name="left">条件左边</param>
        /// <param name="right">条件右边</param>
        /// <param name="type">表达式树条件枚举</param>
        /// <returns></returns>
        static string BinarExpressionProvider(Expression left, Expression right, ExpressionType type)
        {
            string sb = string.Empty;
            //先处理左边
            sb += ExpressionRouter(left);
            sb += ExpressionTypeCast(type);
            //再处理右边
            string tmpStr = ExpressionRouter(right);
            if (tmpStr == "null")
            {
                if (sb.EndsWith(" ="))
                    sb = sb.Substring(0, sb.Length - 2) + " is null";
                else if (sb.EndsWith("<>"))
                    sb = sb.Substring(0, sb.Length - 2) + " is not null";
            }
            else
                sb += tmpStr;
            return sb;
        }
        /// <summary>
        /// 转换条件表达式枚举的类型为对应的SQL表达式
        /// </summary>
        /// <param name="type">条件表达式枚举</param>
        /// <returns></returns>
        static string ExpressionTypeCast(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Equal:
                    return " =";
                case ExpressionType.GreaterThan:
                    return " >";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " Or ";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    return null;
            }
        }
        #endregion

    }
}
