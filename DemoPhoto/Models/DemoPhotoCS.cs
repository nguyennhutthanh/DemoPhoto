using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DemoPhoto.Models
{
	public class DemoPhotoCS
	{
		private readonly int ImageMinimumBytes = 512;
		public DemoPhotoCS() { }
		/// <summary>
		/// Check 1 file có phải là file ảnh hay không
		/// </summary>
		/// <param name="postedFile"> file</param>
		/// <returns>true, false</returns>
		public bool IsImage(IFormFile postedFile)
		{
			if (postedFile.ContentType.ToLower() != "image/jpg" &&
				postedFile.ContentType.ToLower() != "image/jpeg" &&
				postedFile.ContentType.ToLower() != "image/pjpeg" &&
				postedFile.ContentType.ToLower() != "image/gif" &&
				postedFile.ContentType.ToLower() != "image/x-png" &&
				postedFile.ContentType.ToLower() != "image/png" &&
				postedFile.ContentType.ToLower() != "image/bmp" &&
				postedFile.ContentType.ToLower() != "image/tiff")
			{
				return false;
			}
			if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg" &&
				Path.GetExtension(postedFile.FileName).ToLower() != ".png" &&
				Path.GetExtension(postedFile.FileName).ToLower() != ".gif" &&
				Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg" &&
				Path.GetExtension(postedFile.FileName).ToLower() != ".bmp" &&
				Path.GetExtension(postedFile.FileName).ToLower() != ".tiff")
			{
				return false;
			}
			try
			{
				if (!postedFile.OpenReadStream().CanRead)
				{
					return false;
				}
				if (postedFile.Length < ImageMinimumBytes)
				{
					return false;
				}
				byte[] buffer = new byte[ImageMinimumBytes];
				postedFile.OpenReadStream().Read(buffer, 0, ImageMinimumBytes);
				string content = System.Text.Encoding.UTF8.GetString(buffer);
				if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
					RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
				{
					return false;
				}
			}
			catch (Exception)
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

