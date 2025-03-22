using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerkleTreeLib
{
    /// <summary>
    /// Provides static methods to compute a BIP340-like Merkle root and generate proofs.
    /// </summary>
    public class MerkleTree
    {
        /// <summary>
        /// Computes the Merkle Root of the given string leaves, using the specified BIP340-like tags for leaf and branch hashing.
        /// 
        /// The tree is built in a Bitcoin-like fashion:
        ///   - Each leaf is first hashed with <paramref name="leafTag"/>.
        ///   - If a level has an odd number of elements, the last element is duplicated.
        ///   - Internal node pairs are concatenated (left + right) and then hashed with <paramref name="branchTag"/>.
        ///   - Once only one element remains, it is hashed again with <paramref name="branchTag"/> to form the final root.
        /// 
        /// Note: After reducing down to a single node, this implementation
        /// applies an additional tagged hash with <paramref name="branchTag"/>
        /// to form the final root. This differs slightly from the raw
        /// Bitcoin Merkle approach, where a single remaining node is directly
        /// taken as the Merkle root with no further hash. We included this step
        /// for consistency with the rest of our code and test suite.
        /// </summary>
        /// <param name="leaves">The list of string leaves to be hashed. If empty, returns <paramref name="branchTag"/> hash of an empty byte array.</param>
        /// <param name="leafTag">The BIP340-like tag used to hash leaf data.</param>
        /// <param name="branchTag">The BIP340-like tag used for internal branches (and the final step).</param>
        /// <returns>A lowercase hex-encoded string representing the final Merkle root.</returns>
        public static string ComputeRoot(IEnumerable<string> leaves, string leafTag, string branchTag)
        {
            var currentLevel = leaves.Select(x =>
                TaggedHash.Compute(leafTag, Encoding.UTF8.GetBytes(x))
            ).ToList();

            if (currentLevel.Count == 0)
            {
                return Convert.ToHexString(TaggedHash.Compute(branchTag, Array.Empty<byte>())).ToLower();
            }

            while (currentLevel.Count > 1)
            {
                if (currentLevel.Count % 2 == 1)
                {
                    currentLevel.Add(currentLevel.Last());
                }

                var nextLevel = new List<byte[]>();
                for (int i = 0; i < currentLevel.Count; i += 2)
                {
                    var left = currentLevel[i];
                    var right = currentLevel[i + 1];
                    var combined = left.Concat(right).ToArray();

                    nextLevel.Add(TaggedHash.Compute(branchTag, combined));
                }

                currentLevel = nextLevel;
            }

            var finalRoot = TaggedHash.Compute(branchTag, currentLevel[0]);
            return Convert.ToHexString(finalRoot).ToLower();
        }

        /// <summary>
        /// Generates a Merkle proof for the specified <paramref name="targetLeaf"/> in the list of <paramref name="leaves"/>.
        /// 
        /// The proof includes a path of sibling nodes (with their position) required to reconstruct the root.
        /// Each node is hashed with <paramref name="branchTag"/>, and each leaf is hashed with <paramref name="leafTag"/>.
        /// </summary>
        /// <param name="targetLeaf">The string representation of the leaf to prove membership for.</param>
        /// <param name="leaves">The collection of string leaves used to build the Merkle tree.</param>
        /// <param name="leafTag">The tag used for the leaf hash operation.</param>
        /// <param name="branchTag">The tag used for the internal branch hash operations.</param>
        /// <returns>A <see cref="MerkleProof"/> containing the proof path up to the Merkle root.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="targetLeaf"/> is not present in <paramref name="leaves"/>.</exception>
        public static MerkleProof GetProof(string targetLeaf, List<string> leaves, string leafTag, string branchTag)
        {
            var leafHashes = leaves.Select(x => TaggedHash.Compute(leafTag, Encoding.UTF8.GetBytes(x))).ToList();
            var targetHash = TaggedHash.Compute(leafTag, Encoding.UTF8.GetBytes(targetLeaf));
            var index = leafHashes.FindIndex(h => h.SequenceEqual(targetHash));

            if (index == -1)
                throw new ArgumentException("Target not found in leaf list");

            var proofNodes = new List<MerkleNode>();
            var currentLevel = leafHashes;

            while (currentLevel.Count > 1)
            {
                if (currentLevel.Count % 2 == 1)
                    currentLevel.Add(currentLevel.Last());

                int siblingIndex = (index % 2 == 0) ? index + 1 : index - 1;
                string siblingHex = Convert.ToHexString(currentLevel[siblingIndex]).ToLower();
                proofNodes.Add(new MerkleNode
                {
                    Hash = siblingHex,
                    Position = index % 2 == 0 ? 1 : 0
                });

                index /= 2;

                var prevLevel = currentLevel.ToList();
                currentLevel = Enumerable.Range(0, prevLevel.Count / 2)
                    .Select(i =>
                    {
                        var left = prevLevel[2 * i];
                        var right = prevLevel[2 * i + 1];
                        return TaggedHash.Compute(branchTag, left.Concat(right).ToArray());
                    }).ToList();
            }

            return new MerkleProof(targetLeaf, proofNodes);
        }

    }
}
