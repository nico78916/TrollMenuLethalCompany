using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalCompanyTrollMenuMod
{
    internal class TrollMenuStyle : MonoBehaviour
    {
        public static GUIStyle menuStyle = new GUIStyle();
        public static GUIStyle buttonStyle = new GUIStyle();
        public static GUIStyle labelStyle = new GUIStyle();
        public static GUIStyle toggleStyle = new GUIStyle();
        public static GUIStyle hScrollStyle = new GUIStyle();
        public static GUIStyle errorLabel = new GUIStyle();
        public static GUIStyle errorStyle = new GUIStyle();
        public static GUIStyle successStyle = new GUIStyle();

        private static Texture2D CreateTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static void Awake()
        {
            InitTextures();
        }

        public static void InitTextures()
        {
            menuStyle.normal.textColor = Color.white;
            menuStyle.normal.background = CreateTexture(2, 2, new Color(0.0f, 0.0f, 0.0f, .0f));
            menuStyle.fontSize = 18;
            menuStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 18;

            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 25;
            labelStyle.normal.background = CreateTexture(2, 2, new Color(0.0f, 0.0f, 0.0f, .0f));
            labelStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

            toggleStyle.normal.textColor = Color.white;
            toggleStyle.fontSize = 18;

            hScrollStyle.normal.textColor = Color.white;
            hScrollStyle.normal.background = CreateTexture(2, 2, new Color(0.0f, 0.0f, 0.0f, .0f));
            hScrollStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

            errorLabel.normal.textColor = Color.red;
            errorLabel.onNormal.textColor = Color.red;
            errorLabel.normal.background = CreateTexture(2, 2, new Color(0.0f, 0.0f, 0.0f, .0f));
            errorLabel.fontSize = 18;

            errorStyle.normal.textColor = Color.red;
            errorStyle.onNormal.textColor = Color.red;
            errorStyle.normal.background = CreateTexture(2, 2, new Color(0.0f, 0.0f, 0.0f, .0f));
            errorStyle.fontSize = 20;

            successStyle.normal.textColor = Color.green;
            successStyle.onNormal.textColor = Color.green;
            successStyle.normal.background = CreateTexture(2, 2, new Color(0.0f, 0.0f, 0.0f, .0f));
            successStyle.fontSize = 20;
        }
    }
}
