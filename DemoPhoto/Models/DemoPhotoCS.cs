using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace DemoPhoto.Models
{
	public class DemoPhotoCS
	{
		public DemoPhotoCS() { }
		public class ImageActive
		{
			public string PNG { get; set; }
			public string JPG { get; set; }
			public string JPEG { get; set; }
			public string BMP { get; set; }
			public string GIF { get; set; }
			public string TIFF { get; set; }
		}
		/// <summary>
		/// Check 1 file có phải là file ảnh hay không
		/// </summary>
		/// <param name="postedFile"> file</param>
		/// <returns>true, false</returns>
		/// 
		public bool IsImage(IFormFile postedFile)
		{
			var config = new ConfigurationBuilder()
			   .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			   .AddJsonFile("appsettings.json").Build();
			var IamgeActive = config.GetSection(nameof(ImageActive)).Get<ImageActive>();
			var path = Path.GetExtension(postedFile.FileName).ToLower();
			if (path != IamgeActive.JPG && path != IamgeActive.PNG && path != IamgeActive.GIF &&
			path != IamgeActive.JPEG && path != IamgeActive.BMP && path != IamgeActive.TIFF)
			{
				return false;
			}
			return true;
		}
		/// <summary>
		/// Random tên ảnh cho không trùng
		/// </summary>
		/// <param name="fileName">tên ảnh</param>
		/// <returns></returns>
		public string GetUniqueFileName(string fileName)
		{
			fileName = Path.GetFileName(fileName);
			return Path.GetFileNameWithoutExtension(fileName)
				+ "_Dino_" + Guid.NewGuid().ToString().Substring(0, 4)
				+ Path.GetExtension(fileName);
		}
		/// <summary>
		/// resize anh theo %
		/// </summary>
		/// <param name="file"></param>
		/// <param name="path"></param>
		/// <param name="percentage">ti le % toi da 100% cua anh</param>
		public void ResizeImageCent(IFormFile file, string path, double percentage)
		{
			int width = 0;
			int height = 0;
			var checkFileImage = IsImage(file);
			if (checkFileImage == true)
			{
				Image image = Image.FromStream(file.OpenReadStream(), true, true);
				if (percentage > 1)
				{
					Console.WriteLine("% cua anh khong phu hop");
				}
				//tính kích thước cho ảnh mới theo tỷ lệ đưa vào vd: 0.4f = 40% cua anh goc
				width = (int)(image.Width * percentage);
				height = (int)(image.Height * percentage);
				// Chuyển đổi các định dạng khác (bao gồm CMYK) sang RGB.
				var newImage = new Bitmap(width, height);

				using (var graphics = Graphics.FromImage(newImage))
				{
					// Vẽ hình ảnh ở kích thước được chỉ định với chế độ chất lượng được đặt thành HighQuality
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.CompositingMode = CompositingMode.SourceCopy;
					//vẽ lại ảnh ban đầu lên bmp theo kích thước mới
					graphics.DrawImage(image, 0, 0, width, height);
					// save duong dan
					newImage.Save(path);
					//giải phóng tài nguyên mà graphic đang giữ
					graphics.Dispose();
				}
			}
			else
			{
				Console.WriteLine("đây không phải file ảnh");
			}
		}
		/// <summary>
		/// Resize Images
		/// </summary>
		/// <param name="file">Tên file</param>
		/// <param name="path">Đường dẫn</param>
		/// <param name="maxWidth"></param>
		/// <param name="maxHeight"></param>
		public void ResizeImage(IFormFile file, string path, int maxWidth, int maxHeight)
		{
			int width = 0;
			int height = 0;
			var checkFileImage = IsImage(file);
			if (checkFileImage == true)
			{
				Image image = Image.FromStream(file.OpenReadStream(), true, true);
				if (maxHeight == 0)
				{
					width = Convert.ToInt32(image.Width * maxWidth / (double)image.Height);
					height = maxWidth;
				}
				if (maxWidth == 0)
				{
					height = Convert.ToInt32(image.Height * maxHeight / (double)image.Width);
					width = maxHeight;
				}
				if (maxWidth == 0 && maxHeight == 0)
				{
					width = image.Width / 2; height = image.Height / 2;
				}
				if (maxWidth > maxHeight)
				{
					width = maxWidth;
					height = Convert.ToInt32(image.Height * maxWidth / (double)image.Width);
				}
				else
				{
					height = maxHeight;
					width = Convert.ToInt32(image.Width * maxHeight / (double)image.Height);
				}
				// Chuyển đổi các định dạng khác (bao gồm CMYK) sang RGB.
				var newImage = new Bitmap(width, height);

				using (var graphics = Graphics.FromImage(newImage))
				{
					// Vẽ hình ảnh ở kích thước được chỉ định với chế độ chất lượng được đặt thành HighQuality
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.CompositingMode = CompositingMode.SourceCopy;
					graphics.DrawImage(image, 0, 0, width, height);
					newImage.Save(path);
					//giải phóng tài nguyên mà graphic đang giữ
					graphics.Dispose();
				}
			}
			else
			{
				Console.WriteLine("đây không phải file ảnh");
			}
		}
		/// <summary>
		///  mã hóa cho định dạng hình ảnh nhất định
		/// </summary>
		/// <param name="format">Image format</param>
		/// <returns>Mã Code Hình</returns>
		private ImageCodecInfo GetEncoderInfo(ImageFormat format)
		{
			return ImageCodecInfo.GetImageDecoders()
				.SingleOrDefault(c => c.FormatID == format.Guid);
		}
		public float RatioImage(float x, float y)
		{
			return Math.Min(x, y);
		}
		/// <summary>
		/// Hàm Nén ảnh
		/// </summary>
		/// <param name="file">tên file</param>
		/// <param name="maxWidth">Chiều rộng</param>
		/// <param name="maxHeight">chiều cao</param>
		/// <param name="quality">Độ nén</param>
		/// <param name="filePath">Đường dẫn ảnh cần chọn</param>
		public void CompressImage(IFormFile file, int maxWidth, int maxHeight, int quality, string filePath)
		{
			int newWidth = 0;
			int newHeight = 0;
			float ratioX, ratioY, ratio;
			var checkFileImage = IsImage(file);
			if (checkFileImage == true)
			{
				Image image = Image.FromStream(file.OpenReadStream(), true, true);
				// Lấy chiều rộng và chiều cao ban đầu của hình ảnh
				int originalWidth = image.Width;
				int originalHeight = image.Height;

				// Để bảo toàn tỷ lệ khung hình
				ratioX = (float)maxWidth / (float)originalWidth;
				ratioY = (float)maxHeight / (float)originalHeight;
				ratio = RatioImage(ratioX, ratioY);

				// Chiều rộng và chiều cao mới dựa trên tỷ lệ khung hình
				newWidth = (int)(originalWidth * ratio);
				newHeight = (int)(originalHeight * ratio);

				// Chuyển đổi các định dạng khác (bao gồm CMYK) sang RGB.
				Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

				// Vẽ hình ảnh ở kích thước được chỉ định với chế độ chất lượng được đặt thành HighQuality
				using (Graphics graphics = Graphics.FromImage(newImage))
				{
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.DrawImage(image, 0, 0, newWidth, newHeight);
				}

				// Lấy một đối tượng ImageCodecInfo đại diện cho codec JPEG.
				ImageCodecInfo imageCodecInfo = this.GetEncoderInfo(ImageFormat.Jpeg);

				// Tạo đối tượng Bộ mã hóa cho thông số Chất lượng.
				Encoder encoder = Encoder.Quality;

				// Tạo một đối tượng EncoderParameters.
				EncoderParameters encoderParameters = new EncoderParameters(1);

				//Lưu hình ảnh dưới dạng tệp JPEG với mức chất lượng.
				EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
				encoderParameters.Param[0] = encoderParameter;
				newImage.Save(filePath, imageCodecInfo, encoderParameters);
			}
			else
			{
				Console.WriteLine("Đây không phải file Ảnh");
			}
		}
	}
}