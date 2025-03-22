using System;
using System.Collections.Generic;
using System.Linq;

namespace MerkleTreeLib
{
    public static class LeafEncoder
    {
        public static List<string> EncodeAndSort(IDictionary<int, int> users)
        {
            return users
                .Select(u => $"({u.Key},{u.Value})")
                .OrderBy(s => s)
                .ToList();
        }

        public static List<string> EncodeAndSort(List<(int Id, int Balance)> users)
        {
            return users
                .Select(u => $"({u.Id},{u.Balance})")
                .OrderBy(s => s)
                .ToList();
        }
    }
}
