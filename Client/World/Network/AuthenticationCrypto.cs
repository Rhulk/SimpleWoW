﻿using System;
using Client.Crypto;
using HMACSHA1 = System.Security.Cryptography.HMACSHA1;

namespace Client.World.Network
{
    enum AuthStatus
    {
        Uninitialized,
        Pending,
        Ready
    }

    static class AuthenticationCrypto
    {
        private static readonly byte[] encryptionKey = new byte[]
        {
            0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5,
            0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE
        };
        private static readonly byte[] decryptionKey = new byte[]
        {
            0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA,
            0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57
        };

        static readonly HMACSHA1 outputHMAC = new HMACSHA1(encryptionKey);
        static readonly HMACSHA1 inputHMAC = new HMACSHA1(decryptionKey);

        static ARC4 encryptionStream;
        static ARC4 decryptionStream;

        public static AuthStatus Status
        {
            get;
            private set;
        }

        static AuthenticationCrypto()
        {
            Status = AuthStatus.Uninitialized;
        }

        [Obsolete("NYI", true)]
        public static void Pending()
        {
            Status = AuthStatus.Pending;
        }

        public static void Initialize(byte[] sessionKey)
        {
            // create RC4-drop[1024] stream
            encryptionStream = new ARC4(outputHMAC.ComputeHash(sessionKey));
            encryptionStream.Process(new byte[1024], 0, 1024);

            // create RC4-drop[1024] stream
            decryptionStream = new ARC4(inputHMAC.ComputeHash(sessionKey));
            decryptionStream.Process(new byte[1024], 0, 1024);

            Status = AuthStatus.Ready;
        }

        public static void Decrypt(byte[] data, int start, int count)
        {
            if (Status == AuthStatus.Ready)
                decryptionStream.Process(data, start, count);
        }

        public static void Encrypt(byte[] data, int start, int count)
        {
            if (Status == AuthStatus.Ready)
                encryptionStream.Process(data, start, count);
        }
    }
}
