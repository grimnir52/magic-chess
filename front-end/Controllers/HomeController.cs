using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using front_end.Models;
using System.Net.Sockets;
using System.Net;
using System.Text;
using front_end.Udp;

namespace front_end.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
