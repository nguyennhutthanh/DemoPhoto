using Microsoft.AspNetCore.Http;
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
			int width, height;
			Image image = Image.FromStream(file.OpenReadStream(), true, true);
			if (image.Width > image.Height)
			{
				width = maxWidth;
				height = Convert.ToInt32(image.Height * maxHeight / (double)image.Width);
			}
			else
			{
				width = Convert.ToInt32(image.Width * maxWidth / (double)image.Height);
				height = maxHeight;
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
			Image image = Image.FromStream(file.OpenReadStream(), true, true);
			// Lấy chiều rộng và chiều cao ban đầu của hình ảnh
			int originalWidth = image.Width;
			int originalHeight = image.Height;

			// Để bảo toàn tỷ lệ khung hình
			float ratioX = (float)maxWidth / (float)originalWidth;
			float ratioY = (float)maxHeight / (float)originalHeight;
			float ratio = Math.Min(ratioX, ratioY);

			// Chiều rộng và chiều cao mới dựa trên tỷ lệ khung hình
			int newWidth = (int)(originalWidth * ratio);
			int newHeight = (int)(originalHeight * ratio);

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
	}
}

