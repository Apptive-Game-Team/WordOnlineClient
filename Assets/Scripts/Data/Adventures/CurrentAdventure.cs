using System.Collections.Generic;
using Data.Adventures.Domain;
using Data.Adventures.Local;
using Global;
using UnityEngine;

namespace Data.Adventures
{
    public class CurrentAdventure : BoundedSingletonObject<CurrentAdventure>
    {
        protected override List<string> BoundScenes =>
            new List<string>()
            {
                "AdventureScene",
                "AdventuresScene"
            };
        
        private Adventure adventure;

        public void SetAdventure(Adventure adventure)
        {
            this.adventure = adventure;
        }
        
        public Adventure Adventure => adventure;
    }
}