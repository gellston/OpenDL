using DevExpress.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenDL.Service
{
    public class ModelExporterService
    {
        private readonly ConfigureService configService;

        public ModelExporterService(ConfigureService _configureServiice)
        {
            this.configService = _configureServiice;
        }


        public void DeleteFreezeUnzipFiles()
        {
            string[] files = Directory.GetFiles(configService.FreezeUnzipPath);
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                FileInfo fileInfo = new FileInfo(file);
                fileInfo.IsReadOnly = false;
                File.Delete(file);
            }
                
        }

        public void DeletePackageZipFiles()
        {
            string[] files = Directory.GetFiles(configService.PackageZipPath);
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                FileInfo fileInfo = new FileInfo(file);
                fileInfo.IsReadOnly = false;
                File.Delete(file);
            }

        }


        public bool UnZipSegmentDeepModel(string targetfilePath, string password)
        {

            // 모델 압축 해제 
            try
            {
                using (ZipArchive archive = ZipArchive.Read(targetfilePath))
                {
                    archive.Password = password;

                    foreach (ZipItem item in archive)
                    {
                        item.Extract(this.configService.FreezeUnzipPath);
                    }
                }

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool PackageSegmentDeepModel(string [] packageFiles, string outputFilePath, string password)
        {

            try
            {
 
                EncryptionType encryptionType = EncryptionType.PkZip;
                using (ZipArchive archive = new ZipArchive())
                {
                    archive.Password = password;
                    foreach (string file in packageFiles)
                    {
                        archive.AddFile(file, "/");
                    }
                    archive.Save(outputFilePath);
                }
            }
            catch (Exception e)
            {
                return false;
            }



            return true;
        }

        public bool PackageZipFreezeModel(string outputFilePath)
        {
            try
            {
                string[] sourceFiles = Directory.GetFiles(this.configService.PackageZipPath);
                using (ZipArchive archive = new ZipArchive())
                {
                    foreach (string file in sourceFiles)
                    {
                        archive.AddFile(file, "/");
                    }
                    archive.Save(outputFilePath);
                    archive.Dispose();
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
