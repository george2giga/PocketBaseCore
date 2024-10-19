using System.Text.Json;
using System.Threading.Tasks;

namespace PocketBaseCore
{
    public interface IPocketBaseClient
    {
        /// <summary>
        /// Gets the authentication token.
        /// </summary>
        string AuthToken { get; }
        
        /// <summary>
        /// Gets the JSON serializer options used by the HttpClient for serializing and deserializing JSON data.
        /// </summary>
        JsonSerializerOptions JsonOptions { get; }
        
        /// <summary>
        /// Authenticate user and returns new auth token and account data by a combination of username/email and password.
        /// </summary>
        /// <param name="identity">The username or email of the record to authenticate.</param>
        /// <param name="password">The auth record password.</param>
        /// <param name="expand">Auto expand record relations. Ex: "relField1,relField2.subRelField"</param>
        /// <param name="fields">Comma separated string of the fields to return in the JSON (default returns all values). Ex: "*,record.expand.relField.name" </param>
        /// <returns>Returns new auth token and account data by a combination of username/email and password.</returns>
        Task<AuthResponse> AuthenticateAsync(string identity, string password, string expand = null, string fields = null);

        /// <summary>
        /// Authenticate user and returns new auth token and account data by a combination of username/email and password.
        /// </summary>
        /// <param name="identity">The username or email of the record to authenticate.</param>
        /// <param name="password">The auth record password.</param>
        /// <param name="expand">Auto expand record relations. Ex: "relField1,relField2.subRelField"</param>
        /// <param name="fields">Comma separated string of the fields to return in the JSON (default returns all values). Ex: "*,record.expand.relField.name"</param>
        /// <typeparam name="T">The type of the authentication response.</typeparam>
        /// <returns>Returns new auth token and account data by a combination of username/email and password.</returns>
        Task<T> AuthenticateAsync<T>(string identity, string password, string expand = null, string fields = null) where T : AuthResponse;

        /// <summary>
        /// Creates a new record in the specified collection.
        /// </summary>
        /// <param name="collection">The name of the collection where the record will be created.</param>
        /// <param name="data">The data of the record to be created.</param>
        /// <param name="expand">Auto expand record relations. Ex: "relField1,relField2.subRelField"</param>
        /// <param name="fields">Comma separated string of the fields to return in the JSON (default returns all values). Ex: "*,?fields=*,expand.relField.name"</param>
        /// <typeparam name="T">The type of the record to be created.</typeparam>
        /// <returns>The created record.</returns>
        Task<T> CreateRecordAsync<T>(string collection, object data, string expand = null, string fields = null) where T : class;

        
        /// <summary>
        /// Retrieves a specific record from the specified collection.
        /// </summary>
        /// <param name="collection">The name of the collection where the record is located.</param>
        /// <param name="id">The unique identifier of the record to retrieve.</param>
        /// <param name="expand">Auto expand record relations. Ex: "relField1,relField2.subRelField"</param>
        /// <param name="fields">Comma separated string of the fields to return in the JSON (default returns all values). Ex: "*,expand.relField.name"</param>
        /// <typeparam name="T">The type of the record to retrieve.</typeparam>
        /// <returns>The requested record.</returns>
        Task<T> GetRecordAsync<T>(string collection, string id, string expand = null, string fields = null) where T : class;
        
        
        /// <summary>
        /// Retrieves a list of records from the specified collection with pagination and optional filters.
        /// </summary>
        /// <param name="collection">The name of the collection from which to retrieve records.</param>
        /// <param name="page">The page number to retrieve (default is 1).</param>
        /// <param name="perPage">The number of records per page (default is 100).</param>
        /// <param name="skipTotal">Whether to skip the total count of records to improve performaces (default is false).</param>
        /// <param name="filter">Optional filter to apply to the records. Ex: "id='abc' && created>'2022-01-01'"</param>
        /// <param name="sort">Optional sort order for the records. Ex: "-created,id"</param>
        /// <param name="expand">Auto expand record relations. Ex: "relField1,relField2.subRelField"</param>
        /// <param name="fields">Comma separated string of the fields to return in the JSON (default returns all values). Ex: "*,expand.relField.name"</param>
        /// <typeparam name="T">The type of the records to retrieve.</typeparam>
        /// <returns>A list of records of type <typeparamref name="T"/>.</returns>
        Task<RecordList<T>> GetRecordsAsync<T>(
            string collection,
            int page = 1,
            int perPage = 100,
            bool skipTotal = true,
            string filter = null,
            string sort = null,
            string expand = null,
            string fields = null) where T : class;
        

        /// <summary>
        /// Updates a specific record in the specified collection.
        /// </summary>
        /// <param name="collection">The name of the collection where the record is located.</param>
        /// <param name="id">The unique identifier of the record to update.</param>
        /// <param name="data">The data to update the record with.</param>
        /// <param name="expand">Auto expand record relations. Ex: "relField1,relField2.subRelField"</param>
        /// <param name="fields">Comma separated string of the fields to return in the JSON (default returns all values). Ex: "*,expand.relField.name"</param>
        /// <typeparam name="T">The type of the record to update.</typeparam>
        /// <returns>The updated record.</returns>
        Task<T> UpdateRecordAsync<T>(
            string collection,
            string id,
            object data, string expand = null, string fields = null) where T : class;
        
        
        /// <summary>
        /// Deletes a specific record from the specified collection.
        /// </summary>
        /// <param name="collection">The name of the collection where the record is located.</param>
        /// <param name="id">The unique identifier of the record to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteRecordAsync(string collection, string id);
    }
}