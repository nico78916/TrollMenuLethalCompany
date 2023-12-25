using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalCompanyTrollMenuMod.tabs
{
    internal class AliveEnemies
    {
        public static void Draw(Rect r)
        {
            int y = 50;
            GUI.Label(new Rect(r.x, r.y + y, r.width, 25), "Alive Enemies");
            y += 25;
            GUI.Label(new Rect(r.x, r.y + y, r.width, 25), "Default mode : Kill");
            y += 25;
            if (TrollMenu.roundManager.SpawnedEnemies.Count == 0)
            {
                GUI.Label(new Rect(r.x + 10f, r.y + y, 200f, 30f), "No enemies alive", TrollMenuStyle.errorLabel);
                return;
            }
            foreach (EnemyAI enemy in TrollMenu.roundManager.SpawnedEnemies)
            {
                if(GUI.Button(new Rect(r.x + 10f, r.y + y, 200f, 25f), enemy.name))
                {
                    Kill(enemy);
                }
                y += 25;
            }
        }
        private static void Kill(EnemyAI enemy)
        {
            enemy.KillEnemy(true);
            TrollConsole.DisplayMessage("Killed " + enemy.name, MessageType.SUCCESS);
        }
    }
}
