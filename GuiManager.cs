using BepInEx.Configuration;
using LethalCompanyTrollMenuMod;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalCompanyTrollMenuMod
{
    
    public class GUIManager : MonoBehaviour
    {
        private class Dimensions
        {
            public float width, height;
        }
        //The main window
        public static GUIManager Instance;
        private Dimensions windowInfo = new Dimensions()
        {
            width = 0.25f,
            height = 0.5f
        };
        private int x = 500, y = 800,width = 500,height = 800;
        private KeyboardShortcut openButton = new KeyboardShortcut(KeyCode.Insert);

        private bool isOpen = false;

        void Awake()
        {
            TrollMenu.mls.LogError("GUI Initialization ...");
            height = (int)(Screen.height * windowInfo.height);// 50% of screen height
            width = (int)(Screen.width * windowInfo.width);//25% of screen width
            x = width;
            y = height;
            
        }

        public void ToggleMenu()
        {
            isOpen = !isOpen;
            Cursor.visible = isOpen;
            if(isOpen )
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            
        }

        public void Update()
        {
            if (openButton.IsUp())
            {
                ToggleMenu();
            }
        }

        public void OnGui()
        {
            GUI.Box(new Rect(x, y, width, height), "Troll menu", TrollMenuStyle.menuStyle);
            if (GUI.Button(new Rect(10, 10, 150, 100), "Je suis un bouton"))
                print("Vous avez cliqué sur le bouton !");
            TrollMenu.mls.LogWarning("PRINTING GUI");
        }

        public void OnDestroy()
        {
            TrollMenu.mls.LogError("The GUILoader was destroyed :(");
        }

    }
}
