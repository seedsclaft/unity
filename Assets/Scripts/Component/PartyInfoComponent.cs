using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class PartyInfoComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currency;

        public void UpdateInfo(PartyInfo partyInfo)
        {
            if (currency != null)
            {
                currency.SetText(partyInfo.Currency.ToString());
            }
        }
    }
}
