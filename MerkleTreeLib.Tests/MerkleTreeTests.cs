using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MerkleTreeLib;
using Xunit;


namespace MerkleTreeLib.Tests
{
    public class MerkleTreeTests
    {

        [Fact]
        public void MerkleRoot_SingleLeaf_UsesLeafThenBranchHash()
        {
            var input = new List<string> { "only" };

            var leaf = TaggedHash.Compute("ProofOfReserve_Leaf", Encoding.UTF8.GetBytes("only"));
            var root = TaggedHash.Compute("ProofOfReserve_Branch", leaf);
            var expected = Convert.ToHexString(root).ToLower();

            var actual = MerkleTree.ComputeRoot(input, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");


            Assert.Equal(expected, actual);
        }



        [Fact]
        public void MerkleRoot_EmptyList_ReturnsTaggedHashOfEmpty()
        {
            var expected = Convert.ToHexString(
                TaggedHash.Compute("ProofOfReserve_Branch", Array.Empty<byte>())
            ).ToLower();

            var actual = MerkleTree.ComputeRoot(new List<string>(), "ProofOfReserve_Leaf", "ProofOfReserve_Branch");

            Assert.Equal(expected, actual);
        }


        [Fact]
        public void MerkleRoot_OddNumberOfLeaves_DuplicatesLastLeaf()
        {
            var input = new List<string> { "a", "b", "c" };
            var root = MerkleTree.ComputeRoot(input, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");


            // To confirm behavior, we compare it to an explicitly padded version
            var padded = new List<string> { "a", "b", "c", "c" };
            var expected = MerkleTree.ComputeRoot(padded, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");


            Assert.Equal(expected, root);
        }

        [Fact]
        public void MerkleProof_CorrectlyGeneratesPath()
        {
            var users = new List<string> { "(1,1111)", "(2,2222)", "(3,3333)", "(4,4444)" };
            var target = "(2,2222)";
            var proof = MerkleTree.GetProof(target, users, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");

            Assert.NotNull(proof);
            Assert.Equal(target, proof.Leaf);
            Assert.NotEmpty(proof.Path);
        }

        [Fact]
        public void MerkleProof_NonexistentLeaf_Throws()
        {
            var users = new List<string> { "(1,1111)", "(2,2222)" };
            Assert.Throws<ArgumentException>(() =>
                MerkleTree.GetProof("(3,3333)", users, "ProofOfReserve_Leaf", "ProofOfReserve_Branch"));
        }

        [Fact]
        public void MerkleProof_VerifiesAgainstComputedRoot()
        {
            var rawUsers = new List<(int, int)>
            {
                (1,1111), (2,2222), (3,3333), (4,4444),
                (5,5555), (6,6666), (7,7777), (8,8888)
            };

            var leaves = LeafEncoder.EncodeAndSort(rawUsers);
            var target = "(3,3333)";

            // This now returns a MerkleProof with a List<MerkleNode> 
            var proof = MerkleTree.GetProof(
                target,
                leaves,
                "ProofOfReserve_Leaf",
                "ProofOfReserve_Branch"
            );
            var expectedRoot = MerkleTree.ComputeRoot(leaves, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");

            var current = TaggedHash.Compute("ProofOfReserve_Leaf", Encoding.UTF8.GetBytes(target));

            // Instead of tuple destructuring, use the MerkleNode properties:
            foreach (var node in proof.Path)
            {
                var siblingHex = node.Hash;
                var siblingIsLeft = node.Position;

                var sibling = Convert.FromHexString(siblingHex);

                // 0 means left, 1 means right
                var concat = siblingIsLeft == 0
                    ? sibling.Concat(current).ToArray()
                    : current.Concat(sibling).ToArray();

                current = TaggedHash.Compute("ProofOfReserve_Branch", concat);
            }

            var finalHash = TaggedHash.Compute("ProofOfReserve_Branch", current);
            var computedRoot = Convert.ToHexString(finalHash).ToLower();

            Console.WriteLine("===== Verifying Merkle Proof =====");
            Console.WriteLine($"Leaf Data:      {target}");
            Console.WriteLine($"Expected Root:  {expectedRoot}");
            Console.WriteLine($"Computed Root:  {computedRoot}");

            Assert.Equal(expectedRoot, computedRoot);
        }


        [Fact]
        public void MerkleRoot_BitcoinTransactionTag_ProducesDeterministicHash()
        {
            var input = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };
            var root1 = MerkleTree.ComputeRoot(input, "Bitcoin_Transaction", "Bitcoin_Transaction");
            var root2 = MerkleTree.ComputeRoot(input, "Bitcoin_Transaction", "Bitcoin_Transaction");
            Assert.Equal(root1, root2);


            Console.WriteLine($"Merkle root for [aaa, bbb, ccc, ddd, eee] under tag 'Bitcoin_Transaction' is: {root1}");
        }

        [Fact]
        public void MerkleRoot_BitcoinTransactionTag_ProducesExpectedRoot()
        {
            var input = new List<string> { "aaa", "bbb", "ccc", "ddd", "eee" };

            var root = MerkleTree.ComputeRoot(
                input,
                "Bitcoin_Transaction",   // leafTag
                "Bitcoin_Transaction"    // branchTag
            );

            Console.WriteLine($"Merkle root for [aaa, bbb, ccc, ddd, eee] with tag 'Bitcoin_Transaction': {root}");


            Assert.Equal("92a939c49bd157c2eeb4cdb8eb2987d4ab63cbb5c86120ba2ed45a35bec6159f", root);
        }



        /*
                [Fact]
                public void MerkleProof_DebugPrintVerification()
                {
                    var users = new List<string> { "(1,1111)", "(2,2222)", "(3,3333)", "(4,4444)" };
                    var target = "(3,3333)";
                    var proof = MerkleTree.GetProof(target, users, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");
                    var expectedRoot = MerkleTree.ComputeRoot(users, "ProofOfReserve_Leaf", "ProofOfReserve_Branch");


                    ProofVerifier.VerifyProof(target, expectedRoot, proof.Path);
                }
                */


    }
}
