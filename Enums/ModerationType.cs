using System.ComponentModel;

namespace TheBlogProject.Enums
{
    public enum ModerationType
    {
        [Description("Political propaganda")]
        Political,
        [Description("offencive language")]
        Language,
        [Description("Drug References")]
        Drugs,
        [Description("Threatening Speech")]
        Threatening,
        [Description("Hate Speech")]
        HateSpeech
    }
}
