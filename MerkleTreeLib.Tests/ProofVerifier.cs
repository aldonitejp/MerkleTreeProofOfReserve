using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerkleTreeLib.Tests
{
    public static class ProofVerifier
    {
        public static void VerifyProof(
            string leafData,
            string expectedRoot,
            List<(string Hash, int Position)> proofPath,
            string leafTag = "ProofOfReserve_Leaf",
            string branchTag = "ProofOfReserve_Branch"
        )
        {
            Console.WriteLine("===== Verifying Merkle Proof =====");
            Console.WriteLine($"Leaf Data:      {leafData}");
            Console.WriteLine($"Expected Root:  {expectedRoot}");
            Console.WriteLine();

            var current = TaggedHash.Compute(leafTag, Encoding.UTF8.GetBytes(leafData));
            Console.WriteLine($"Leaf Hash:      {Convert.ToHexString(current).ToLower()}");

            for (int i = 0; i < proofPath.Count; i++)
            {
                var (siblingHex, position) = proofPath[i];
                var sibling = Convert.FromHexString(siblingHex);

                byte[] concat = position == 0
                    ? sibling.Concat(current).ToArray() // sibling is on the left
                    : current.Concat(sibling).ToArray(); // sibling is on the right

                Console.WriteLine($"\nStep {i + 1}:");
                Console.WriteLine($"  Sibling:      {siblingHex}");
                Console.WriteLine($"  Position:     {(position == 0 ? "Left of current" : "Right of current")}");
                Console.WriteLine($"  Concatenated: {Convert.ToHexString(concat).ToLower()}");

                current = TaggedHash.Compute(branchTag, concat);
                Console.WriteLine($"  Hash:         {Convert.ToHexString(current).ToLower()}");
            }

            var finalHash = Convert.ToHexString(current).ToLower();
            Console.WriteLine("\n===== Final Result =====");
            Console.WriteLine($"Computed Root:  {finalHash}");

            if (finalHash == expectedRoot)
                Console.WriteLine("Proof is VALID");
            else
                Console.WriteLine("Proof is INVALID");
        }
    }
}
