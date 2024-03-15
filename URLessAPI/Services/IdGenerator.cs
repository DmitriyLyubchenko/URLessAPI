﻿using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace URLess.Services
{
    public class IdGenerator
    {
        private readonly byte[] _inputBytes;

        public IdGenerator(string url) 
        {
            var encoded = HttpUtility.UrlEncode(url);
            _inputBytes = Encoding.UTF8.GetBytes(encoded);
        }

        public string Generate() 
        {
            var result = GenerateFromBytes(_inputBytes);
            return result;
        }

        public string Regenerate() 
        {
            var changedInput = new byte[_inputBytes.Length];
            _inputBytes.CopyTo(changedInput, 0);

            var rnd = new Random();
            var indexToChange = rnd.Next(changedInput.Length - 1);

            var buffer = new byte[1];
            rnd.NextBytes(buffer);
            _inputBytes[indexToChange] = buffer[0];

            var result = GenerateFromBytes(changedInput);

            return result;
        }

        private string GenerateFromBytes(byte[] input) 
        {
            using var sha1 = SHA1.Create();

            var hashBytes = sha1.ComputeHash(input);
            var base64 = Convert.ToBase64String(hashBytes);
            var result = base64.Substring(0, 6);

            return result;
        }
    }
}
