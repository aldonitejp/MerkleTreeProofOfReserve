using System;
using System.Collections.Generic;
using MerkleTreeLib;

namespace ProofOfReserveApi.Services
{
    /// <summary>
    /// Holds a daily snapshot of user balances, provides the Merkle root, and refreshes the data once a day.
    /// </summary>
    public class ProofOfReserveService
    {
        private readonly object _lock = new object();
        private Dictionary<int, int> _userData = new();
        private string _cachedRoot = string.Empty;
        private DateTime _lastUpdated = DateTime.MinValue;

        /// <summary>
        /// Replaces the user balance dictionary with <paramref name="newUserData"/> and
        /// recomputes the Merkle root, storing it in memory for quick retrieval.
        /// </summary>
        public void RefreshData(Dictionary<int, int> newUserData)
        {
            lock (_lock)
            {
                _userData = newUserData;
                var leaves = LeafEncoder.EncodeAndSort(_userData);

                _cachedRoot = MerkleTree.ComputeRoot(
                    leaves,
                    "ProofOfReserve_Leaf",
                    "ProofOfReserve_Branch"
                );

                _lastUpdated = DateTime.UtcNow;
                Console.WriteLine($"[ProofOfReserveService] Refreshed at {_lastUpdated}, new root: {_cachedRoot}");
            }
        }

        /// <summary>
        /// Retrieves the cached Merkle root for the current snapshot.
        /// </summary>
        public string GetMerkleRoot()
        {
            lock (_lock)
            {
                return _cachedRoot;
            }
        }

        /// <summary>
        /// Returns a copy of the current snapshot of (UserID, Balance) data.
        /// </summary>
        public Dictionary<int, int> GetUserData()
        {
            lock (_lock)
            {
                return new Dictionary<int, int>(_userData);
            }
        }
    }

}
