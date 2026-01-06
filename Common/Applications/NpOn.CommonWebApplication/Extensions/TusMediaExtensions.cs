using Common.Extensions.NpOn.CommonEnums;
using Common.Extensions.NpOn.CommonMode;
using Microsoft.AspNetCore.StaticFiles;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Stores;

namespace Common.Extensions.NpOn.CommonWebApplication.Extensions;

public static class TusMediaExtensions
{
    /// <summary>
    /// Configures TUS Protocol for large file uploads and automatically moves them to a public directory.
    /// </summary>
    public static void UseTusMediaUpload(this WebApplication app)
    {
        // Get application content root path for safe default paths
        string contentRoot = app.Environment.ContentRootPath;
        string defaultTempPath = Path.Combine(contentRoot, "App_Data", "Tus_Temp");
        string defaultPublicPath = Path.Combine(contentRoot, "App_Data", "Public_Media");

        // --- GET CONFIGURATION ---

        // Upload Endpoint
        string uploadEndpoint = EApplicationConfiguration.MediaUploadEndpoint.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrEmpty(uploadEndpoint)) uploadEndpoint = "/api/Media/Upload";

        // Temp Path
        string tempStoragePath = EApplicationConfiguration.MediaTempStoragePath.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrEmpty(tempStoragePath)) tempStoragePath = defaultTempPath;

        // Public Path
        string publicStoragePath =
            EApplicationConfiguration.MediaPublicStoragePath.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrEmpty(publicStoragePath)) publicStoragePath = defaultPublicPath;

        // Download Prefix
        string downloadUrlPrefix =
            EApplicationConfiguration.MediaDownloadUrlPrefix.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrEmpty(downloadUrlPrefix)) downloadUrlPrefix = "/api/Media/Download";

        // CDN URL
        string mediaCdnUrl = EApplicationConfiguration.MediaCdnUrl.GetAppSettingConfig().AsDefaultString();

        // Delete Temp
        bool deleteTempOnSuccess =
            EApplicationConfiguration.MediaDeleteTempOnSuccess.GetAppSettingConfig().AsDefaultBool();

        // Allowed Extensions (Default * if not configured)
        string allowedExtensionsConfig =
            EApplicationConfiguration.MediaAllowedExtensions.GetAppSettingConfig().AsDefaultString();
        if (string.IsNullOrEmpty(allowedExtensionsConfig)) allowedExtensionsConfig = "*";

        // Convert to lowercase set for case-insensitive comparison
        var allowedExtensions = allowedExtensionsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLower())
            .ToHashSet();

        // Max File Size (Default 0 = Unlimited)
        long maxFileSize = 0;
        string maxFileSizeStr = EApplicationConfiguration.MediaMaxFileSize.GetAppSettingConfig().AsDefaultString();
        if (!string.IsNullOrEmpty(maxFileSizeStr) && long.TryParse(maxFileSizeStr, out long size))
        {
            maxFileSize = size;
        }

        // Is Delete Enabled
        bool isDeleteEnabled = EApplicationConfiguration.IsMediaDeleteEnabled.GetAppSettingConfig().AsDefaultBool();

        // --- CREATE DIRECTORIES ---
        try
        {
            if (!Directory.Exists(tempStoragePath))
            {
                Directory.CreateDirectory(tempStoragePath);
                Console.WriteLine($"[TUS] Created temp directory: {tempStoragePath}");
            }

            if (!Directory.Exists(publicStoragePath))
            {
                Directory.CreateDirectory(publicStoragePath);
                Console.WriteLine($"[TUS] Created public directory: {publicStoragePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TUS] CRITICAL ERROR: Cannot create directories. {ex.Message}");
        }

        // --- CONFIGURE TUS MIDDLEWARE ---
        app.MapTus(uploadEndpoint, /*httpContext*/_ => Task.FromResult(new DefaultTusConfiguration
        {
            Store = new TusDiskStore(tempStoragePath),
            // UrlPath is not set when using Endpoint Routing
            Events = new Events
            {
                // VALIDATE BEFORE CREATING FILE
                OnBeforeCreateAsync = ctx =>
                {
                    // Check Max Size
                    if (maxFileSize > 0 && ctx.UploadLength > maxFileSize)
                    {
                        ctx.FailRequest($"File size exceeds the limit of {maxFileSize} bytes.");
                        return Task.CompletedTask;
                    }

                    // Check Extension
                    if (!allowedExtensions.Contains("*"))
                    {
                        // Metadata 'filename' or 'name' is required
                        string filename = null;
                        if (ctx.Metadata.ContainsKey("filename"))
                        {
                            filename = ctx.Metadata["filename"].GetString(System.Text.Encoding.UTF8);
                        }
                        else if (ctx.Metadata.ContainsKey("name"))
                        {
                            filename = ctx.Metadata["name"].GetString(System.Text.Encoding.UTF8);
                        }

                        if (string.IsNullOrEmpty(filename))
                        {
                            ctx.FailRequest("Metadata 'filename' or 'name' is required for validation.");
                            return Task.CompletedTask;
                        }

                        string ext = Path.GetExtension(filename).ToLower(); // includes dot (.jpg)

                        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                        {
                            ctx.FailRequest($"File type '{ext}' is not allowed. Allowed: {allowedExtensionsConfig}");
                            return Task.CompletedTask;
                        }
                    }

                    return Task.CompletedTask;
                },

                // PROCESS AFTER UPLOAD COMPLETE
                OnFileCompleteAsync = async eventContext =>
                {
                    ITusFile file = await eventContext.GetFileAsync();
                    // Dictionary<string, tusdotnet.Models.Metadata> metadata =
                    Dictionary<string, Metadata> metadata =
                        await file.GetMetadataAsync(eventContext.CancellationToken);

                    string originalFileName = null;
                    if (metadata.ContainsKey("filename"))
                    {
                        originalFileName = metadata["filename"].GetString(System.Text.Encoding.UTF8);
                    }
                    else if (metadata.ContainsKey("name"))
                    {
                        originalFileName = metadata["name"].GetString(System.Text.Encoding.UTF8);
                    }
                    
                    if (string.IsNullOrEmpty(originalFileName))
                    {
                        originalFileName = $"{eventContext.FileId}.mp4";
                    }

                    // [MODIFIED] Always generate Unique Name: Name_GUID.ext
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);
                    string ext = Path.GetExtension(originalFileName);
                    string uniqueFileName = $"{nameWithoutExt}_{Guid.NewGuid()}{ext}";

                    string sourceFilePath = Path.Combine(tempStoragePath, eventContext.FileId);
                    string destFilePath = Path.Combine(publicStoragePath, uniqueFileName);

                    // Move file to Public directory
                    try
                    {
                        if (deleteTempOnSuccess)
                        {
                            File.Move(sourceFilePath, destFilePath);
                        }
                        else
                        {
                            File.Copy(sourceFilePath, destFilePath);
                        }

                        Console.WriteLine($"[TUS] Upload success: {destFilePath}");

                        // [NEW] Return Download URL in Header
                        // Construct the download URL using uniqueFileName
                        string downloadUrl;
                        if (!string.IsNullOrEmpty(mediaCdnUrl))
                        {
                            downloadUrl = $"{mediaCdnUrl.TrimEnd('/')}/{uniqueFileName}";
                        }
                        else
                        {
                            downloadUrl = $"{downloadUrlPrefix.TrimEnd('/')}/{uniqueFileName}";
                        }

                        // Add custom header to response
                        eventContext.HttpContext.Response.Headers.Append("X-Media-Download-Url", downloadUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TUS] Error moving/copying file: {ex.Message}");
                    }

                    // Clean up TUS metadata
                    try
                    {
                        if (deleteTempOnSuccess)
                        {
                            var terminationStore = (ITusTerminationStore)eventContext.Store;
                            await terminationStore.DeleteFileAsync(eventContext.FileId, eventContext.CancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TUS] Error cleaning metadata: {ex.Message}");
                    }
                }
            }
        }));

        // DOWNLOAD ENDPOINT 
        if (!string.IsNullOrEmpty(downloadUrlPrefix))
        {
            string routePattern = downloadUrlPrefix.TrimEnd('/') + "/{fileName}";

            app.MapGet(routePattern, (string fileName, HttpContext _) =>
            {
                fileName = Path.GetFileName(fileName);
                string filePath = Path.Combine(publicStoragePath, fileName);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[Download] File not found: {filePath}");
                    return Task.FromResult(Results.NotFound());
                }

                // Determine Content Type
                var provider = new FileExtensionContentTypeProvider();

                // Try to get content type from provider, fallback to octet-stream for unknown types
                if (!provider.TryGetContentType(filePath, out string? contentType))
                {
                    contentType = "application/octet-stream";
                }

                Console.WriteLine($"[Download] Serving file: {filePath} ({contentType})");

                // Return file with enableRangeProcessing for video seeking/resume download
                return Task.FromResult(Results.File(filePath, contentType, enableRangeProcessing: true));
            });
            
            // DELETE ENDPOINT
            if (isDeleteEnabled)
            {
                app.MapDelete(routePattern, (string fileName, HttpContext _) =>
                {
                    fileName = Path.GetFileName(fileName);
                    string filePath = Path.Combine(publicStoragePath, fileName);

                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"[Delete] File not found: {filePath}");
                        return Task.FromResult(Results.NotFound());
                    }

                    try
                    {
                        File.Delete(filePath);
                        Console.WriteLine($"[Delete] File deleted: {filePath}");
                        return Task.FromResult(Results.Ok());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Delete] Error deleting file: {ex.Message}");
                        return Task.FromResult(Results.Problem($"Error deleting file: {ex.Message}"));
                    }
                });
            }
        }
    }
}