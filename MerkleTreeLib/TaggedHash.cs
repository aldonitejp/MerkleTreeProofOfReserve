using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;


namespace MerkleTreeLib
{

    /// <summary>
    /// Implements BIP340-like "tagged hashing" using SHA-256.
    /// 
    /// Hash_tag(M) = SHA256( SHA256(tag) || SHA256(tag) || M ).
    /// </summary>
    public static class TaggedHash
    {
        /// <summary>
        /// Computes the tagged hash for the given <paramref name="tag"/> and raw <paramref name="message"/> bytes.
        /// </summary>
        /// <param name="tag">The BIP340-like tag (e.g. "Bitcoin_Transaction", "ProofOfReserve_Leaf").</param>
        /// <param name="message">The message bytes to be hashed.</param>
        /// <returns>The resulting 32-byte hash.</returns>
        public static byte[] Compute(string tag, byte[] message)
        {
            using var sha = SHA256.Create();
            var tagHash = sha.ComputeHash(Encoding.UTF8.GetBytes(tag));
            var input = tagHash.Concat(tagHash).Concat(message).ToArray();
            return sha.ComputeHash(input);
        }

        /// <summary>
        /// Computes the tagged hash for the given <paramref name="tag"/> and text <paramref name="message"/>,
        /// and returns it as a lowercase hex string.
        /// </summary>
        public static string ComputeHex(string tag, string message)
        {
            var hash = Compute(tag, Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLower();
        }
    }

}
