using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BERecepcion.Core.Utils
{
    public class SearchText
    {
        public static string GetWhereClause(string search, List<string> fields)
        {
            string sql = "WHERE ";
            List<string> searchList = search.Split(";").ToList();
            foreach (var _search in searchList)
            {
                var searchItem = _search.Split(",").ToList();
                sql += "(";
                foreach (var src in searchItem)
                {
                    foreach (var field in fields)
                    {
                        if (!String.IsNullOrEmpty(src.Trim()))
                        {
                            sql += string.Concat(field, " LIKE '%", src.Trim(), "%' OR ");
                        }
                    }
                }
                sql = sql.Remove(sql.Length - 3, 3);
                sql += ")";
                sql += "AND ";
            }
            sql = sql.Remove(sql.Length - 4, 4);
            return sql;
        }
    }
}
