﻿using CarNotes.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarNotes.Controllers
{
    public class RepairController : Controller
    {
        // GET: Repair
        public ActionResult Delete(int id)
        {
            new RepairHelper().Delete(id, HttpContext);
            return Redirect("~/Home/GoToRepairEvents");
        }
    }
}