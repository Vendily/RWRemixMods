using Menu.Remix.MixedUI;
using System.Linq;
using UnityEngine;
using System;

namespace DeathPlayerGhosts;
sealed class Options : OptionInterface
{
    public static Configurable<Plugin.PlayerGhostMode> PlayerGhostMode { get; private set; }
    public static Configurable<int> PlayerGhostCap { get; private set; }
    public static Configurable<int> PlayerGhostTimer { get; private set; }
    public static Configurable<bool> PlayerGhostOverride { get; private set; }
    
    public Options()
    {
        PlayerGhostMode = this.config.Bind("PlayerGhostMode", Plugin.PlayerGhostMode.TimerAndProximity);
        PlayerGhostCap = this.config.Bind("PlayerGhostCap", 0, new ConfigAcceptableRange<int>(0,int.MaxValue));
        PlayerGhostTimer = this.config.Bind("PlayerGhostTimer", 2800, new ConfigAcceptableRange<int>(1,int.MaxValue));
        PlayerGhostOverride = this.config.Bind("PlayerGhostOverride", false);
    }
    public override void Initialize()
    {
        this.Tabs = new OpTab[] { new OpTab(this) };
        float baseX = 270;
        float y = 360;
        Tabs[0].AddItems(new OpLabel(new Vector2(50f, y), new Vector2(150f, 20f), "Ghost Cap Override"));
        Tabs[0].AddItems(new OpCheckBox(PlayerGhostOverride, new Vector2(baseX, y - 6))
        { description = "Use the cap value instead of death count in the ending. 0 = death count" });
        y += 60f;
        Tabs[0].AddItems(new OpLabel(new Vector2(50f, y), new Vector2(150f, 20f), "Time to Max Ghosts (frames)"));
        Tabs[0].AddItems(new OpUpdown(PlayerGhostTimer, new Vector2(baseX, y - 6), 200f)
        { description = "The time in frames to reach the maximum amount of ghosts at a time. Default = 2800." });
        y += 60f;
        Tabs[0].AddItems(new OpLabel(new Vector2(50f, y), new Vector2(150f, 20f), "Ghost Cap"));
        Tabs[0].AddItems(new OpUpdown(PlayerGhostCap, new Vector2(baseX, y - 6), 200f)
        { description = "The max number of ghosts you can have at once, regardless of deaths. 0 = No Cap." });
        y += 60f;
        Tabs[0].AddItems(new OpLabel(new Vector2(50f, y),new Vector2(150f,20f), "Ghost Cap Formula"));
        Tabs[0].AddItems(new OpComboBox(PlayerGhostMode, new Vector2(baseX, y - 6), 200f, OpResourceSelector.GetEnumNames(null, typeof(Plugin.PlayerGhostMode)).ToList())
        { description = "Timer = Time Spent in Ending. Proximity = Distance to Light." });
        
    }
}
