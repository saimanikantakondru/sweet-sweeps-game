using UnityEngine;
 
namespace SweetSweeps.Core.Contracts
{
    public interface IKnockbackService
    {
        void Apply(Vector2 sourcePosition);
    }
}