using Extract;
using Microsoft.AspNetCore.Http;
using System;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for UsersController.
    /// </summary>
    public sealed class UserData: IDisposable
    {
        ApiContext _apiContext;
        FileApi _fileApi;

        /// <summary>
        /// Initializes an <see cref="UserData"/> instance.
        /// </summary>
        /// <param name="apiContext"><see cref="ApiContext"/> that defines the user's database.</param>
        public UserData(ApiContext apiContext)
        {
            try
            {
                _apiContext = apiContext;
                _fileApi = FileApiMgr.GetInterface(apiContext);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45277");
            }
        }

        /// <summary>
        /// Dispose - release the fileApi (reset in use flag)
        /// </summary>
        public void Dispose()
        {
            if (_fileApi == null)
            {
                return;
            }

            _fileApi.InUse = false;
            _fileApi = null;
        }

        /// <summary>
        /// Attempts to authenticate the <see paramref="user"/> against the current <see cref="ApiContext"/>.
        /// </summary>
        /// <param name="user">The <see cref="User"/> to authenticate.</param>
        public void LoginUser(User user)
        {
            try
            {
                var fileProcessingDB = _fileApi.FileProcessingDB;
                ExtractException.Assert("ELI45187",
                    "Database connection failure",
                    !string.IsNullOrWhiteSpace(fileProcessingDB.DatabaseID));

                HTTPError.Assert("ELI46442", StatusCodes.Status401Unauthorized,
                    !user.Username.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                    "Unknown user or password");

                try
                {
                    fileProcessingDB.LoginUser(user.Username, user.Password);
                }
                catch (Exception ex)
                {
                    throw new HTTPError("ELI45178", StatusCodes.Status401Unauthorized,
                        "Unknown user or password", ex);
                }

                // The workflow will have already been validated by FileApi constructor.
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43409");
            }
            finally
            {
                _fileApi.InUse = false;
            }
        }
    }
}
