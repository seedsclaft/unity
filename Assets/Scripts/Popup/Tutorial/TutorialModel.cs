using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TutorialModel : BaseModel
    {
        public TutorialModel()
        {
        }

        public string HelpText()
        {
            return DataSystem.GetText(18010);
        }

    }
}