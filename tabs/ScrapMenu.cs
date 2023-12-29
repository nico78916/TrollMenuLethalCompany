using System;
using UnityEngine;

namespace LethalCompanyTrollMenuMod.tabs
{
    public class ScrapMenu
    {
        public static bool randomValue = false;
        public static bool randomAmount = false;
        public static int minScrap = 0;
        public static int maxScrap = 100;
        public static int minScrapValue = 0;
        public static int maxScrapValue = 1000;
        private static string minScrapStr = minScrap.ToString();
        private static string maxScrapStr = maxScrap.ToString();
        private static string minScrapValueStr = minScrapValue.ToString();
        private static string maxScrapValueStr = maxScrapValue.ToString();
        public ScrapMenu() { }
        public static void Draw(Rect wr)
        {
            int y = 50;
            int x = 100;
            GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Scrap Menu");
            y += 25;
            GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Every change will be applied on next day",TrollMenuStyle.errorLabel);
            y += 25;
            randomValue = GUI.Toggle(new Rect(wr.x, wr.y + y, wr.width, 25), randomValue, "Activate Random");
            y += 25;
            if (randomValue)
            {
                GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Min Scrap");
                minScrapStr = GUI.TextField(new Rect(wr.x + x, wr.y + y, wr.width - (x + 5), 25), minScrapStr);
                y += 25;
                GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Max Scrap");
                maxScrapStr = GUI.TextField(new Rect(wr.x + x, wr.y + y, wr.width - (x + 5), 25), maxScrapStr);
                try
                {
                    if (minScrapStr != "")
                    {
                        minScrap = int.Parse(minScrapStr);
                    }
                    if (maxScrapStr != "")
                    {
                        maxScrap = int.Parse(maxScrapStr);
                    }
                    if (maxScrap < 0)
                    {
                        maxScrap = 0;
                    }
                    if (minScrap < 0)
                    {
                        minScrap = 0;
                    }
                    if (minScrap > maxScrap)
                    {
                        minScrap = maxScrap;
                    }
                    if (maxScrap < minScrap)
                    {
                        maxScrap = minScrap;
                    }
                }
                catch(Exception e)
                {
                    TrollMenu.mls.LogError("Error while parsing custom values");
                    TrollMenu.mls.LogError(e.Message);
                }
                y += 25;
            }
            x = 0;
            randomAmount = GUI.Toggle(new Rect(wr.x + x, wr.y + y, wr.width, 25), randomAmount, "Activate Custom Value");
            y += 25;
            x = 100;
            if(randomAmount)
            {
                GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Min Scrap Value");
                minScrapValueStr = GUI.TextField(new Rect(wr.x + x, wr.y + y, wr.width - (x + 5), 25), minScrapValueStr.ToString());
                y += 25;
                GUI.Label(new Rect(wr.x, wr.y + y, wr.width, 25), "Max Scrap Value");
                maxScrapValueStr = GUI.TextField(new Rect(wr.x + x, wr.y + y, wr.width - (x + 5), 25), maxScrapValueStr.ToString());
                try
                {
                    if (minScrapValueStr != "")
                    {
                        minScrapValue = int.Parse(minScrapValueStr);
                    }
                    if (maxScrapValueStr != "")
                    {
                        maxScrapValue = int.Parse(maxScrapValueStr);
                    }
                    if (maxScrapValue < 0)
                    {
                        maxScrapValue = 0;
                    }
                    if (minScrapValue < 0)
                    {
                        minScrapValue = 0;
                    }
                    if (minScrapValue > maxScrapValue)
                    {
                        minScrapValue = maxScrapValue;
                    }
                    if (maxScrapValue < minScrapValue)
                    {
                        maxScrapValue = minScrapValue;
                    }
                }
                catch(Exception e)
                {
                    TrollMenu.mls.LogError("Error while parsing custom values");
                    TrollMenu.mls.LogError(e.Message);
                }
            }
        }
    }
}
