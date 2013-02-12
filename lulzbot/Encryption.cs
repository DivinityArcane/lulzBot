using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace lulzbot
{
    public class Encryption
    {
        private static readonly byte[] key = new byte[128] {
            162, 151, 178, 252, 156, 003, 198, 164, 035, 
            234, 070, 132, 236, 030, 100, 185, 212, 164, 
            133, 090, 035, 063, 129, 132, 241, 186, 089, 
            005, 045, 244, 042, 110, 002, 237, 120, 069, 
            211, 073, 164, 030, 184, 097, 032, 043, 133, 
            239, 090, 182, 146, 092, 228, 168, 142, 094, 
            212, 046, 243, 001, 021, 145, 200, 148, 040, 
            144, 051, 073, 043, 235, 250, 145, 128, 176, 
            169, 087, 201, 014, 027, 232, 221, 008, 202, 
            159, 254, 177, 177, 130, 196, 165, 058, 016, 
            202, 196, 083, 159, 194, 124, 159, 015, 078, 
            180, 100, 029, 225, 202, 122, 126, 111, 150, 
            019, 166, 086, 133, 097, 205, 161, 070, 128, 
            103, 195, 249, 105, 166, 123, 036, 174, 197, 
            119, 244};

        private static readonly byte[] iv = new byte[64] {
            160, 249, 064, 204, 058, 100, 046, 162, 094, 
            104, 110, 078, 034, 216, 104, 034, 243, 113, 
            025, 249, 048, 156, 237, 066, 056, 231, 039, 
            087, 215, 193, 145, 055, 123, 030, 214, 105, 
            019, 236, 132, 224, 112, 086, 248, 030, 054, 
            169, 092, 220, 082, 183, 070, 226, 102, 152, 
            001, 139, 094, 014, 058, 055, 076, 216, 142, 
            107};

        public static String Encrypt(String data)
        {
            RijndaelManaged crypt = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7
            };

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, crypt.CreateEncryptor(key, iv), CryptoStreamMode.Write);

            byte[] encrypted_data = Encoding.ASCII.GetBytes(data);
            cs.Write(encrypted_data, 0, encrypted_data.Length);
            cs.FlushFinalBlock();
            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static String Decrypt(String data)
        {
            byte[] data_bytes = Convert.FromBase64String(data);

            RijndaelManaged crypt = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7
            };

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, crypt.CreateDecryptor(key, iv), CryptoStreamMode.Write);

            cs.Write(data_bytes, 0, data_bytes.Length);
            cs.FlushFinalBlock();
            cs.Close();

            return Encoding.ASCII.GetString(ms.ToArray());
        }
    }
}
