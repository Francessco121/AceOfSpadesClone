using System;
using System.IO;
using System.Security.Cryptography;

/* NetEncryption.cs
 * Author: Ethan Lafrenais
 * Last Update: 2/24/15
*/

namespace Dash.Net
{
    static class NetEncryption
    {
        static byte[] key;
        static byte[] iv;

        static NetEncryption()
        {
            key = Convert.FromBase64String("iEUceFTeD4JQBHI6ucf1X0yWeo6wqQGYi1WCMLzBObQ=");
            iv = Convert.FromBase64String("cLAUFVQCYgmAQ2UfASwiVw==");
        }

        internal static void EncryptPacket(NetOutboundPacket msg)
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.Key = key;
                rm.IV = iv;

                // Set the packets data to the encrypted bytes
                msg.data = EncryptBytes(rm, msg.data);
                // Set position
                msg.position = msg.data.Length;

                msg.isEncrypted = true;
            }
        }

        internal static void DecryptPacket(NetInboundPacketBase msg)
        {
            using (RijndaelManaged rm = new RijndaelManaged())
            {
                rm.Key = key;
                rm.IV = iv;

                // Decrypt
                byte[] dbytes = msg.ReadBytes(msg.data.Length - NetOutboundPacket.PacketHeaderSize);
                dbytes = DecryptBytes(rm, dbytes);

                // Write new packet
                msg.data = dbytes;
                msg.position = 0;

                msg.HasHeader = false;
                msg.isEncrypted = false;
            }
        }

        static byte[] EncryptBytes(SymmetricAlgorithm alg, byte[] data)
        {
            if ((data == null) || (data.Length == 0))
                return data;

            using (MemoryStream stream = new MemoryStream())
            using (ICryptoTransform encryptor = alg.CreateEncryptor())
            using (CryptoStream encrypt = new CryptoStream(stream, encryptor, CryptoStreamMode.Write))
            {
                encrypt.Write(data, 0, data.Length);
                encrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }

        static byte[] DecryptBytes(SymmetricAlgorithm alg, byte[] data)
        {
            if ((data == null) || (data.Length == 0))
                return data;

            using (MemoryStream stream = new MemoryStream())
            using (ICryptoTransform decryptor = alg.CreateDecryptor())
            using (CryptoStream decrypt = new CryptoStream(stream, decryptor, CryptoStreamMode.Write))
            {
                decrypt.Write(data, 0, data.Length);
                decrypt.FlushFinalBlock();
                return stream.ToArray();
            }
        }
    }
}
