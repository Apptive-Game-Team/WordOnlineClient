using System;
using System.Collections.Generic;
using FixMath.NET;

namespace GameScene.Simulation.Magic
{
    public sealed class ProductionMagicCatalog
    {
        private readonly Dictionary<int, MagicDefinition> byId = new Dictionary<int, MagicDefinition>();
        private readonly Dictionary<string, MagicDefinition> byName =
            new Dictionary<string, MagicDefinition>(StringComparer.Ordinal);

        public IReadOnlyCollection<MagicDefinition> Definitions => byId.Values;

        public ProductionMagicCatalog(IEnumerable<MagicDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            foreach (MagicDefinition definition in definitions)
            {
                if (definition == null || byId.ContainsKey(definition.Id) || byName.ContainsKey(definition.Name))
                    throw new InvalidOperationException("Duplicate or null magic definition");
                byId.Add(definition.Id, definition); byName.Add(definition.Name, definition);
            }
        }

        public MagicDefinition GetRequired(int id) => byId.TryGetValue(id, out MagicDefinition value)
            ? value : throw new InvalidOperationException("Missing magic configuration: " + id);

        public static ProductionMagicCatalog Create()
        {
            List<MagicDefinition> values = new List<MagicDefinition>();
            Add(values, MagicBehaviorFamily.Spawn, "1:ember_spirit_swarm,2:water_slime_swarm,3:mini_rock_swarm,4:seed_spirit_swarm,5:wind_spirit,28:aqua_archer,29:rock_golem,30:storm_rider,31:thunder_spirit,32:fire_spirit,39:magma_spirit,42:wind_slime_swarm,43:tornado_strike,46:cloud_dragon,47:thunder_bird_swarm,48:tree_golem,49:vine_spirit,51:rock_mage,55:chicken_commando,58:zap_mouse,64:fire_lord_spirit,65:dimension_toad,68:bubble_spirit");
            Add(values, MagicBehaviorFamily.Projectile, "6:water_shot,7:fire_shot,8:tide_call,9:chain_lightning,10:vine_toss,11:rock_rolling,12:wind_blade,13:nature_shot,21:will_o_wisp,33:rock_shot,45:lightning_shot");
            Add(values, MagicBehaviorFamily.Area, "19:magma_explosion,20:water_explosion,22:nature_explosion,35:lightning_drop,36:wind_explosion,37:rock_explosion,38:fire_explosion,41:sand_storm,44:meteor_shower,54:rock_drop,56:overgrowth,59:rallying_torch,60:lightning_explosion,61:razor_gale,67:shock_overload");
            Add(values, MagicBehaviorFamily.Build, "14:fire_slime_nest,15:water_slime_nest,16:life_tree,17:rock_turret,18:wind_totem,23:frenzy_totem,25:cannon,26:tower,27:mana_well,40:healing_totem,50:vine_colony,52:pve_nature_slime_nest,53:pve_vine,57:crater,62:bubble_generator,63:electric_tower,66:towerback");
            Add(values, MagicBehaviorFamily.Drop, "24:leafair");
            if (values.Count != 67) throw new InvalidOperationException("Production magic catalog must contain 67 entries");
            return new ProductionMagicCatalog(values);
        }

        private static void Add(List<MagicDefinition> output, MagicBehaviorFamily family, string encoded)
        {
            string[] entries = encoded.Split(',');
            for (int index = 0; index < entries.Length; index++)
            {
                string[] parts = entries[index].Split(':');
                int id = int.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                int count = parts[1].Contains("swarm") ? 3 : 1;
                Fix64 speed = family == MagicBehaviorFamily.Projectile ? (Fix64)8 : Fix64.Zero;
                int damage = family == MagicBehaviorFamily.Area || family == MagicBehaviorFamily.Projectile ? 10 : 0;
                output.Add(new MagicDefinition(id, parts[1], family, count, 40, damage,
                    (Fix64)6, (Fix64)1.5m, speed,
                    family == MagicBehaviorFamily.Area ? "area-hit" : null,
                    family == MagicBehaviorFamily.Area ? 20 : 0));
            }
        }
    }
}
