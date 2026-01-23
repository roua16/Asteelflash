using ITStockM.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Dapper;
using System.DirectoryServices;

namespace ITStockM.Services
{
   

    public class AuthService : IAuthService
    {
        private readonly string _connectionString;

        public AuthService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("ITStockManagmentConnection");
        }

        public async Task<AppUser> Authenticate(string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            var user = await connection.QueryFirstOrDefaultAsync<AppUser>(
                "SELECT * FROM Employee WHERE Email = @Email", new { Email = email });
           
            // ad auth to be added

            if (user == null || ! (password == user.Password))
                return null;
            
            return user;
        }

       
        public async Task<AppUser> GetUserByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<AppUser>(
                "SELECT * FROM Employee WHERE Email = @Email", new { Email = email });
        }

        public bool ADAuthenticateUser(string username, string password)

        {

            try

            {

                using (var entry = new System.DirectoryServices.DirectoryEntry("LDAP://asteelflash.europe.lan", username, password))

                {

                    if (entry.NativeObject != null)

                    {

                        using (var searcher = new DirectorySearcher(entry))

                        {

                            searcher.Filter = $"(&(ObjectClass=user)(sAMAccountName={username}))";

                            searcher.PropertiesToLoad.Add("displayName");



                            SearchResult user = searcher.FindOne();

                            if (user != null && user.Properties["displayName"].Count > 0)

                            {

                                string fullName = user.Properties["displayName"][0].ToString();

                                Console.WriteLine($"Utilisateur authentifié : {fullName}");

                                return true;

                            }

                        }

                    }

                }

            }

            catch (Exception ex)

            {

                Console.WriteLine($"Erreur lors de l'authentification : {ex.Message}");

            }



            return false;

        } 
    }
}
