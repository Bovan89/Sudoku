using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sudoku.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            /*SudokuGrid sg = new SudokuGrid();

            sg.Reset();
            bool b = sg.Check();

            int buf = sg.Grid[5, 7];
            sg.Grid[5, 7] = sg.Grid[6, 7];
            sg.Grid[6, 7] = buf;

            b = sg.Check();*/

            //sg.Mix();
            //sg.Transpose();
            //sg.SwapColumns();
            //sg.SwapRowsArea();
            //sg.SwapColumnsArea();

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}