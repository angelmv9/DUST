﻿using DUST.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DUST.Services
{
    public class FilesService : IFilesService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData);
                return string.Format($"data:{extension};base64,{imageBase64Data}");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();

                memoryStream.Close();
                memoryStream.Dispose();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal fileSize = bytes;

            while (Math.Round(fileSize / 1024) >= 1)
            {
                fileSize /= bytes;
                counter++;
            }

            return string.Format("{0:n1}{1}",fileSize,suffixes[counter]);
        }

        public string GetFileIcon(string fileName)
        {
            string fileIconPath = "/img/contenttype/default.png";

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                string fileExtension = Path.GetExtension(fileName).Replace(".", "");
                fileIconPath = $"/img/contenttype/{fileExtension}.png";
                return fileIconPath;
            }
            return fileIconPath;
        }
    }
}
