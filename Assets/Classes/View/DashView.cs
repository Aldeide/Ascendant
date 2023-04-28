using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ascendant.Controllers;

namespace Ascendant.Views
{
    public sealed class DashView : MonoBehaviour
    {
        public Character character;
        public int maxDashCharges;
        public int currentDashCharges;

        public GameObject dashUIPrefab;
        public GameObject[] dashUIElements;

        void Start()
        {
            // The UI is activated once the local character has been spawned.
            // As such, it is available on this script's Start function.
            character = GameManager.Instance.localPlayer.controlledCharacter;
            maxDashCharges = character.GetComponent<PlayerMovementController>().maxDashCharges;
            currentDashCharges = character.GetComponent<PlayerMovementController>().currentDashCharges;
            CreateDashUIElements();
        }

        // Update is called once per frame
        void Update()
        {
            int i = 1;
            currentDashCharges = character.GetComponent<PlayerMovementController>().currentDashCharges;
            foreach (var element in dashUIElements)
            {
                if (currentDashCharges >= i)
                {
                    element.transform.GetChild(0).GetComponent<Image>().fillAmount = 1;
                    i++;
                    continue;
                }
                if (currentDashCharges + 1 == i)
                {
                    element.transform.GetChild(0).GetComponent<Image>().fillAmount
                        = character.GetComponent<PlayerMovementController>().currentDashCooldown / character.GetComponent<PlayerMovementController>().dashCooldown;
                    i++;
                    continue;
                }
                element.transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
                i++;
            }
        }

        // Creates n dash UI elements depending on the maximum number of dash charges.
        private void CreateDashUIElements()
        {
            dashUIElements = new GameObject[maxDashCharges];
            // Create elements.
            for (int i = 0; i < maxDashCharges; i++)
            {
                dashUIElements[i] = Instantiate(dashUIPrefab,transform);
            }
            var y = dashUIElements[0].GetComponent<RectTransform>().anchoredPosition.y;
            // Place elements.
            int j = 0;

            var globalOffset = - (maxDashCharges - 1) * 0.5f;
            var offset = 35;
            foreach (var element in dashUIElements)
            {
                element.GetComponent<RectTransform>().anchoredPosition = new Vector2(globalOffset * offset + j * offset, y);
                j++;
            }

        }
    }
}


