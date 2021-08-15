using DemoPhoto.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace DemoPhoto.Controllers
{
	public class HomeController : Controller
	{
		public IWebHostEnvironment host;
		public DemoPhotoCS DemoPhotos;
		public HomeController(IWebHostEnvironment _host , IConfiguration config)
		{
			host = _host;
			DemoPhotos = new DemoPhotoCS();
		}
		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Index(IFormFile file)
		{
			//demo cho Resize
			var path = Path.Combine(host.WebRootPath, "AnhResize",
				DemoPhotos.GetUniqueFileName(file.FileName));
			DemoPhotos.ResizeImageCent(file, path, 0.3);

			//demo cho Compressing
			string path01 = $"{Directory.GetCurrentDirectory()}" +
				$"{@"\wwwroot\AnhCompress\" + DemoPhotos.GetUniqueFileName(file.FileName)}";
			DemoPhotos.CompressImage(file, 20, 30, 30, path01);
			return View();
		}

	}
}
