using System.Collections.Generic;
using Phoenix.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Phoenix.UI
{
    public class GameStartButton : MonoBehaviour
    {

        private readonly List<Behaviour> _visualComponents = new List<Behaviour>();
        
        public void Awake()
        {
            _visualComponents.Add(GetComponent<Image>());
            _visualComponents.Add(GetComponent<Button>());
            _visualComponents.Add(GetComponentInChildren<TextMeshProUGUI>());
            
            _visualComponents.ForEach(c => c.enabled = true);
        }

        private void OnEnable()
        {
            LevelManager.GameStarting += LevelManagerOnGameStarting;
        }

        private void OnDisable()
        {
            LevelManager.GameStarting -= LevelManagerOnGameStarting;
        }

        private void LevelManagerOnGameStarting(object sender, System.EventArgs e)
        {
            _visualComponents.ForEach(c => c.enabled = false);
        }

        public void OnClick()
        {
            LevelManager.StartGame();
        }
    }
}
