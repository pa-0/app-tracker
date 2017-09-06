using AppsTracker.Domain.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Domain.Screenshots
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ScreenshotStore
    {
        private const int MAX_FILE_NAME_LENGTH = 245;

        private readonly IAppSettingsService settingsService;

        [ImportingConstructor]
        public ScreenshotStore(IAppSettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public async Task SaveToFileAsync(ScreenshotModel screenshot, Image image)
        {
            var path = new StringBuilder();
            path.Append(screenshot.AppName);
            path.Append("_");
            path.Append(screenshot.WindowTitle);
            path.Append("_");
            path.Append(image.GetHashCode());
            path.Append(".jpg");
            string folderPath;

            if (Directory.Exists(settingsService.Settings.DefaultScreenshotSavePath))
                folderPath = Path.Combine(settingsService.Settings.DefaultScreenshotSavePath, CorrectPath(path.ToString()));
            else
                folderPath = CorrectPath(path.ToString());

            folderPath = TrimPath(folderPath);
            await SaveScreenshot(image.Screensht, folderPath);
        }

        private string CorrectPath(string path)
        {
            string newTitle = path;
            char[] illegalChars = new char[] { '<', '>', ':', '"', '\\', '/', '|', '?', '*', '0' };
            if (path.IndexOfAny(illegalChars) >= 0)
            {
                foreach (var chr in illegalChars)
                {
                    if (newTitle.Contains(chr))
                    {
                        while (newTitle.Contains(chr))
                        {
                            newTitle = newTitle.Remove(newTitle.IndexOf(chr), 1);
                        }
                    }
                }
            }
            char[] charArray = newTitle.ToArray();
            foreach (var chr in charArray)
            {
                int i = chr;
                if (i >= 1 && i <= 31) newTitle = newTitle.Remove(newTitle.IndexOf(chr), 1);
            }

            return newTitle;
        }

        private string TrimPath(string path)
        {
            var extension = Path.GetExtension(path);
            var pathNoExtension = path.Remove(path.Length - extension.Length - 1, extension.Length + 1);
            if (pathNoExtension.Length >= MAX_FILE_NAME_LENGTH)
            {
                while (pathNoExtension.Length >= MAX_FILE_NAME_LENGTH)
                {
                    pathNoExtension = pathNoExtension.Remove(pathNoExtension.Length - 1, 1);
                }
            }
            return pathNoExtension + extension;
        }

        private async Task SaveScreenshot(byte[] image, string path)
        {
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

            using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
            {
                await fileStream.WriteAsync(image, 0, image.Length);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
