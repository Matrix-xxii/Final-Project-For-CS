using System;
using System.Security.Cryptography;

namespace FoodOutlet.Services
{
    public class PasswordGenerated
    {
        public string Create(int length = 10)
        {
            const string chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";

            byte[] data = new byte[length];

            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }

            char[] result = new char[length];

            for (int i = 0; i < length; i++)
                result[i] = chars[data[i] % chars.Length];

            return new string(result);
        }
    }
}
