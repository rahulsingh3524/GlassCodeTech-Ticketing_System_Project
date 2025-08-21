using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace GlassCodeTech_Ticketing_System_Project.Services
{ 
public class DatabaseHelper
{
    private readonly string _connectionString;

    // Use secure key/iv in production, ideally via environment variables or secure storage!
    private static readonly string EncryptionKey = "zQ5nD7pRf3KwL8tVeG0aY2uXiJ6vG4Nb"; // 32 chars for AES-256
    private static readonly string IVString = "bXc9vYt5rUe2tO7k"; // 16 chars for AES

    public DatabaseHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    // 1. Generic method to execute any stored procedure
    public List<Dictionary<string, object>> ExecuteStoredProcedure(string spName, SqlParameter[] parameters)
    {
        var result = new List<Dictionary<string, object>>();
        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(spName, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }
                        result.Add(row);
                    }
                }
            }
        }
        return result;
    }

        // 2. Encrypt a string (returns Base64)
        public static string Encrypt(string plainText)
        {
            byte[] key = Encoding.UTF8.GetBytes(EncryptionKey);
            byte[] iv = Encoding.UTF8.GetBytes(IVString);

            if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes for AES-256.");
            if (iv.Length != 16) throw new ArgumentException("IV must be 16 bytes.");

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    // ms.ToArray() is guaranteed to have complete data now
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }


        // 3. Decrypt a Base64 string (returns plaintext)
        public static string Decrypt(string cipherText)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.IV = Encoding.UTF8.GetBytes(IVString);

            ICryptoTransform decryptor = aes.CreateDecryptor();
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
}