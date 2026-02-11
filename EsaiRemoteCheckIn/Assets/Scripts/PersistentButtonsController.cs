using UnityEngine;
using UnityEngine.UI;

public class PersistentButtonsController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button sayAgainButton;
    [SerializeField] private Button backButton;   
    [SerializeField] private Button endButton;

    [Header("References")]
    [SerializeField] private WheelMenuController wheelController;

    private void Awake()
    {
        if (wheelController == null)
        {
            Debug.LogError("PersistentButtonsController: WheelMenuController reference is missing.");
            return;
        }

        if (sayAgainButton != null)
            sayAgainButton.onClick.AddListener(wheelController.SayAgain);

        if (backButton != null)
            backButton.onClick.AddListener(wheelController.BackOne);

        if (endButton != null)
            endButton.onClick.AddListener(wheelController.EndSession);
    }
}
