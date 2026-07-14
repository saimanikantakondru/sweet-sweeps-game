using VContainer;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using SweetSweeps.Security;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Bootstrap
{
    public class BootstrapController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMPro.TMP_InputField inputField;
        [SerializeField] private TMPro.TextMeshProUGUI feedbackText;
        [SerializeField] private string defaultFeedback = "Enter password...";
        [SerializeField] private string accessGranted = "Access granted";
        [SerializeField] private string accessDenied = "Incorrect password";
        [SerializeField] private Button submitButton;
        [SerializeField] private Button exitButton;

        private readonly string _expectedHash = "3260f0647a7c1c76fd794a0ce737a78927561bd3b43f57423b09ab09bc635896";

        private IPasswordValidator _validator;
        private ISceneService _sceneService;
        private ILoadingScreenService _loadingScreen;

        [Inject]
        public void Construct(ISceneService sceneService, ILoadingScreenService loadingScreen)
        {
            _sceneService = sceneService;
            _loadingScreen = loadingScreen;
        }

        private void Awake()
        {
            var hasher = new Sha256PasswordHasher();
            _validator = new PasswordValidator(hasher, _expectedHash);
            SetFeedback(defaultFeedback);
        }

        private void Start()
        {
            _loadingScreen.Hide();
        }

        private void OnEnable()
        {
            submitButton.onClick.AddListener(OnSubmit);
            exitButton.onClick.AddListener(OnExit);
        }

        private void OnDisable()
        {
            submitButton.onClick.RemoveAllListeners();
            exitButton.onClick.RemoveAllListeners();
        }

        public void OnSubmit()
        {
            string input = inputField.text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                SetFeedback(defaultFeedback);
                return;
            }

            if (!_validator.Validate(input))
            {
                SetFeedback(accessDenied);
                inputField.text = string.Empty;
                return;
            }

            SetFeedback(accessGranted);
            _sceneService.LoadMenuAsync().Forget();
        }

        private void OnExit()
        {
            Application.Quit();
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
        }
    }
}
