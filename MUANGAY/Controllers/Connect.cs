using MUANGAY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MUANGAY.Controllers
{
    public class Connect
    {
        public dbMuaNgayDataContext db;
        public static dbMuaNgayDataContext GetConnect()
        {
            string connectionString = "Data Source=LAPTOP-SD6JFUCG\\MSSQLSERVER01;Initial Catalog=DBMUANGAY;Integrated Security=True";
            dbMuaNgayDataContext dataContext = new dbMuaNgayDataContext(connectionString);
            return dataContext;
        }
    }
}