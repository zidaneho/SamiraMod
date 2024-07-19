using IL.EntityStates.MajorConstruct;
using SamiraMod.Survivors.Samira.SkillStates;

namespace SamiraMod.Survivors.Samira
{
    public static class SamiraStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(Flair));

            Modules.Content.AddEntityState(typeof(BladeWhirl));

            Modules.Content.AddEntityState(typeof(WildRush));

            Modules.Content.AddEntityState(typeof(InfernoTrigger));
            
            Modules.Content.AddEntityState(typeof(SamiraDeathState));
            
            Modules.Content.AddEntityState(typeof(FlairDash));
        }
    }
}
