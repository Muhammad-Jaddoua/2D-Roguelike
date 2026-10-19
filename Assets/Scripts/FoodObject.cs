using UnityEngine;
using UnityEngine.Serialization;

public class FoodObject : CellObject
{
    [SerializeField, Range(1, 50), FormerlySerializedAs("AmountGranted"), Tooltip("Food added to the player when collected")]
    private int m_AmountGranted = 10;

    public override void PlayerEntered()
    {
        //increase food
        GameManager.Instance.ChangeFood(m_AmountGranted);

        SoundManager.PlayFoodPickup();
        VfxManager.Play(VfxType.FoodCollected, transform.position);

        // Back to the pool (SetActive(false)) instead of Destroy
        Despawn();
    }
}
