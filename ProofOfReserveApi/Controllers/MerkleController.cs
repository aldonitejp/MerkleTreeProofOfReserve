using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using ProofOfReserveApi.Services;  // The new service's namespace
using MerkleTreeLib;

namespace ProofOfReserveApi.Controllers
{
    /// <summary>
/// Controller for Proof-of-Reserve Merkle tree operations.
/// </summary>
[ApiController]
[Route("[controller]")]
public class MerkleController : ControllerBase
{
    private readonly ProofOfReserveService _service;

    /// <summary>
    /// Initializes the Merkle controller with the <see cref="ProofOfReserveService"/>.
    /// </summary>
    public MerkleController(ProofOfReserveService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns the daily-cached Merkle root of all user balances,
    /// computed with "ProofOfReserve_Leaf" and "ProofOfReserve_Branch".
    /// </summary>
    [HttpGet("merkle-root")]
    public IActionResult GetRoot()
    {
        var root = _service.GetMerkleRoot();
        return Ok(new { MerkleRoot = root });
    }

    /// <summary>
    /// Returns the Merkle proof path for the specified user <paramref name="id"/>.
    /// The proof includes sibling hashes from the leaf up to the root.
    /// </summary>
    /// <param name="id">The user ID whose balance is being proven.</param>
    /// <returns>A JSON object with the user's balance and an array of sibling node hashes.</returns>
    [HttpGet("merkle-proof/{id}")]
    public IActionResult GetProof(int id)
    {
        if (!_service.GetUserData().TryGetValue(id, out var balance))
            return NotFound();

        var leaves = LeafEncoder.EncodeAndSort(_service.GetUserData());
        var target = $"({id},{balance})";

        var proof = MerkleTree.GetProof(
            target,
            leaves,
            "ProofOfReserve_Leaf",
            "ProofOfReserve_Branch"
        );

        return Ok(new
        {
            UserBalance = balance,
            Proof = proof.Path
        });
    }
}

}
