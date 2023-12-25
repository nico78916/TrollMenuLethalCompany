using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalCompanyTrollMenuMod.helpers
{
    //To use it : Intanciate a new Select<T> with a Dictionary<string, T> as parameter
    //Then call the Draw method with a Rect as parameter, it will return the selected value or default(T) if nothing is selected
    internal class Select<T>
    {
        private Dictionary<string, T> opts;
        private bool[] old,current;
        private T defaultValue = default(T);
        private int defaultIndex = -1;
        public Select(Dictionary<string, T> opts) {
            this.opts = opts;
            old = new bool[opts.Count];
            current = new bool[opts.Count];
            for(int i = 0; i < opts.Count; i++)
            {
                old[i] = false;
                current[i] = false;
            }
        }

        public void SetDefault(T value)
        {
            this.defaultValue = value;
            defaultIndex = opts.Values.ToList().IndexOf(value);
        }

        public T Draw(Rect wr)
        {
            if (opts.Count == 0) return defaultValue;
            int i = 0;
            if(old.Length != opts.Count)
            {
                old = new bool[opts.Count];
                current = new bool[opts.Count];
                for (i = 0; i < opts.Count; i++)
                {
                    old[i] = false;
                    current[i] = false;
                }
                return defaultValue;
            }
            T selected = defaultValue;
            int y = 0;
            i = 0;
            T[] vals = opts.Values.ToArray();
            foreach (KeyValuePair<string, T> opt in opts)
            {
                current[i] = GUI.Toggle(new Rect(wr.x, wr.y + y, wr.width, 25), current[i], opt.Key);
                y += 25;
                i++;
            }
            for (i = 0; i < current.Length; i++)
            {
                if (current[i] != old[i])
                {
                    if (current[i])
                    {
                        for (int j = 0; j < current.Length; j++)
                        {
                            if (j != i)
                            {
                                current[j] = false;
                                old[j] = false;
                            }
                        }
                    }
                    old[i] = current[i];
                }
            }

            //Get the index of true value
            List<bool> currentList = current.ToList();
            int index = currentList.IndexOf(true);
            if(index != -1)
            {
                selected = vals[index];
            }

            //we check if no option is selected
            if (!current.Contains(true))
            {
                if(defaultValue != null)
                {
                    current[defaultIndex] = true;
                }
                selected = defaultValue;
            }
            return selected;
        }
    }
}
