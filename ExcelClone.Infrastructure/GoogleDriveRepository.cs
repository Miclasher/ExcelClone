using ExcelClone.Domain.Tables;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text.Json;

namespace ExcelClone.Infrastructure;

public class GoogleDriveRepository
{
    private readonly string _clientSecretPath;
    private readonly string _folderId;
    private readonly DriveService _driveService;
    private const string UserId = "defaultUser";
    private const string TokenFolderName = "token.json";

    public GoogleDriveRepository(string clientSecretPath, string folderId)
    {
        ArgumentNullException.ThrowIfNull(clientSecretPath);
        ArgumentNullException.ThrowIfNull(folderId);

        _clientSecretPath = clientSecretPath;
        _folderId = folderId;
        _driveService = AuthenticateAsync().GetAwaiter().GetResult();
    }

    private async Task<DriveService> AuthenticateAsync()
    {
        try
        {
            UserCredential credential;
            await using (var stream = new FileStream(_clientSecretPath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                    [DriveService.ScopeConstants.DriveFile],
                    UserId,
                    CancellationToken.None,
                    new FileDataStore(TokenFolderName, true)
                );
            }
            Console.WriteLine("OAuth 2.0 authentication successful.");

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Spreadsheet Uni Lab OAuth",
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL: Failed to authenticate using OAuth 2.0. Check client secret path: {_clientSecretPath}. Error: {ex.Message}");
            throw new InvalidOperationException("Google Drive OAuth 2.0 authentication failed.", ex);
        }
    }

    public async Task<Table?> LoadAsync(string id)
    {
        string? fileId = null;
        try
        {
            fileId = await FindFileIdByNameAsync(id, _folderId);
            if (fileId == null)
            {
                Console.WriteLine($"Info: File '{id}' not found in specified folder '{_folderId}'.");
                return null;
            }

            var request = _driveService.Files.Get(fileId);

            using var memoryStream = new MemoryStream();
            var progress = await request.DownloadAsync(memoryStream);

            if (progress.Status != Google.Apis.Download.DownloadStatus.Completed)
            {
                Console.WriteLine($"Error: Failed to download file '{id}' (ID: {fileId}). Status: {progress.Status}, Exception: {progress.Exception?.Message}");
                return null;
            }

            memoryStream.Position = 0;

            var table = await JsonSerializer.DeserializeAsync<Table>(memoryStream);
            return table;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: LoadAsync failed for Table ID '{id}', Google File ID '{fileId ?? "N/A"}'. Error: {ex.Message}");
            return null;
        }
    }

    public async Task SaveAsync(Table table)
    {
        var fileName = table.Id;
        string? fileId = null;
        try
        {
            fileId = await FindFileIdByNameAsync(fileName, _folderId);

            using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, table);
            memoryStream.Position = 0;

            var fileMetadata = new Google.Apis.Drive.v3.Data.File() { Name = fileName };
            FilesResource.CreateMediaUpload createRequest;
            FilesResource.UpdateMediaUpload updateRequest;

            if (fileId != null)
            {
                updateRequest = _driveService.Files.Update(fileMetadata, fileId, memoryStream, "application/json");
                updateRequest.Fields = "id";
                var uploadProgress = await updateRequest.UploadAsync();
                if (uploadProgress.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new IOException($"Failed to update file '{fileName}' (ID: {fileId}).", uploadProgress.Exception);
                }
                Console.WriteLine($"Info: Successfully updated file '{fileName}' (ID: {fileId}).");
            }
            else
            {
                fileMetadata.Parents = new List<string> { _folderId };
                createRequest = _driveService.Files.Create(fileMetadata, memoryStream, "application/json");
                createRequest.Fields = "id";
                var uploadProgress = await createRequest.UploadAsync();
                if (uploadProgress.Status != Google.Apis.Upload.UploadStatus.Completed)
                {
                    throw new IOException($"Failed to create file '{fileName}'.", uploadProgress.Exception);
                }
                var newFile = createRequest.ResponseBody;
                Console.WriteLine($"Info: Successfully created file '{fileName}' with ID '{newFile?.Id ?? "N/A"}'.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: SaveAsync failed for Table '{fileName}', Google File ID '{fileId ?? "Not Found"}'. Error: {ex.Message}");
            throw;
        }
    }

    private async Task<string?> FindFileIdByNameAsync(string fileName, string folderId)
    {
        try
        {
            var request = _driveService.Files.List();
            request.Q = $"name = '{fileName}' and '{folderId}' in parents and trashed = false";
            request.Fields = "files(id, name)";
            request.PageSize = 1;
            request.SupportsAllDrives = true;
            request.IncludeItemsFromAllDrives = true;

            var result = await request.ExecuteAsync();
            var file = result.Files.FirstOrDefault();
            return file?.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Failed to search for file '{fileName}' in folder '{folderId}'. Error: {ex.Message}");
            return null;
        }
    }
}
