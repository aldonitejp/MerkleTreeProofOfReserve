using System;
using System.Collections.Generic;


namespace MerkleTreeLib
{
    /// <summary>
    /// Represents a Merkle proof for a specific leaf, including the path of sibling nodes up to the root.
    /// </summary>
    public class MerkleProof
    {
        /// <summary>
        /// The original leaf data (before hashing).
        /// </summary>
        public string Leaf { get; }

        /// <summary>
        /// The list of sibling node hashes (and their position: 0=left, 1=right) used to reconstruct the Merkle root.
        /// </summary>
        public List<MerkleNode> Path { get; }

        public MerkleProof(string leaf, List<MerkleNode> path)
        {
            Leaf = leaf;
            Path = path;
        }
    }


}
