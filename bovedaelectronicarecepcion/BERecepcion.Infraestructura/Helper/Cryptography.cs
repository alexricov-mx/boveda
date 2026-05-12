using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;

namespace BERecepcion.Infraestructura.Helper
{
    public class Cryptography
    {
        public static string GenerateSHA256String(string inputString)
        {
            using (FileStream stream = File.OpenRead(inputString))
            {
                var sha256 = new SHA256Managed();
                byte[] checksum = sha256.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public static string GenerateSHA256File(IFormFile File)
        {
            var sha256 = new SHA256Managed();
            byte[] checksum = sha256.ComputeHash(File.OpenReadStream());
            return Convert.ToBase64String(checksum);
        }

        public static string GenerateSHA512String(string inputString)
        {
            using (FileStream stream = File.OpenRead(inputString))
            {
                var sha512 = new SHA512Managed();
                byte[] checksum = sha512.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        public static string GenerateSHA512File(IFormFile File)
        {
            var sha512 = new SHA512Managed();
            byte[] checksum = sha512.ComputeHash(File.OpenReadStream());
            //return BitConverter.ToString(checksum).Replace("-", String.Empty);
            return Convert.ToBase64String(checksum);
        }

        public static string GenerateBase64String(string inputString)
        {
            var fileBytes = File.ReadAllBytes(inputString);

            string encodedFile = Convert.ToBase64String(fileBytes);
            return encodedFile;
        }

        public static string GenerateBase64File(IFormFile File)
        {
            using (var ms = new MemoryStream())
            {
                File.CopyTo(ms);
                var fileBytes = ms.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
        }
        /// <summary>
        /// Verificar cadena resultado del algoritmo criptográfico.
        /// </summary>
        /// <param name="cadena"></param>
        /// <returns></returns>
        public static byte[] Sha256Digest(string cadena)
        {
            byte[] data = Encoding.ASCII.GetBytes(cadena);
            Sha256Digest sha256 = new Sha256Digest();
            sha256.BlockUpdate(data, 0, data.Length);
            byte[] hash = new byte[sha256.GetDigestSize()];
            sha256.DoFinal(hash, 0);
            return hash;
        }
        public static byte[] HashDerEncoded(byte[] Sha256Digest_)
        {
            var DerObjectIdentifier_ = new DerObjectIdentifier(NistObjectIdentifiers.IdSha256.Id);
            var AlgorithmIdentifier_ = new AlgorithmIdentifier(DerObjectIdentifier_, DerNull.Instance);
            var DigestInfo_ = new DigestInfo(AlgorithmIdentifier_, Sha256Digest_);
            return DigestInfo_.GetDerEncoded();
        }
        public static byte[] Sha512Digest(string cadena)
        {
            byte[] data = Encoding.ASCII.GetBytes(cadena);
            Sha512Digest sha512 = new Sha512Digest();
            sha512.BlockUpdate(data, 0, data.Length);
            byte[] hash = new byte[sha512.GetDigestSize()];
            sha512.DoFinal(hash, 0);
            return hash;
        }
        public static byte[] HashDerEncoded512(byte[] Sha512Digest_)
        {
            var DerObjectIdentifier_ = new DerObjectIdentifier(NistObjectIdentifiers.IdSha512.Id);
            var AlgorithmIdentifier_ = new AlgorithmIdentifier(DerObjectIdentifier_, DerNull.Instance);
            var DigestInfo_ = new DigestInfo(AlgorithmIdentifier_, Sha512Digest_);
            return DigestInfo_.GetDerEncoded();
        }
        public static RsaKeyParameters PublicKeyParameters(IFormFile File)
        {
            byte[] CertificadoByteArray_ = StreamToByteArray(File.OpenReadStream());
            var X509CertificateParser_ = new X509CertificateParser();
            X509Certificate X509Certificado_ = X509CertificateParser_.ReadCertificate(CertificadoByteArray_);
            return (RsaKeyParameters)X509Certificado_.GetPublicKey();
        }
        public static Pkcs1Encoding DescodificadorRsa(RsaKeyParameters PublicKeyParameters)
        {
            var Pkcs1Encoding_ = new Pkcs1Encoding(new RsaBlindedEngine());
            Pkcs1Encoding_.Init(false, PublicKeyParameters);
            return Pkcs1Encoding_;
        }
        public static byte[] Base64ToHashDerEncode(Pkcs1Encoding DescodificadorRsa_, string CadenaSelladaB64_)
        {
            byte[] CadenaSelladaEncryptedHDE_ = Convert.FromBase64String(CadenaSelladaB64_);
            return DescodificadorRsa_.ProcessBlock(CadenaSelladaEncryptedHDE_, 0, CadenaSelladaEncryptedHDE_.Length);
        }
        public static byte[] StreamToByteArray(Stream inputStream)
        {
            byte[] bytes = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = inputStream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    memoryStream.Write(bytes, 0, count);
                }
                return memoryStream.ToArray();
            }
        }
        public static bool SafeEquals(byte[] strA, byte[] strB)
        {
            int length = strA.Length;
            if (length != strB.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (strA[i] != strB[i]) return false;
            }
            return true;
        }
    }

}
