using System;
using System.Collections.Generic;

namespace Ryneus
{
	[Serializable]
	public class SaveGameInfo
	{
		private PartyInfo _party = null;
		public PartyInfo Party => _party;

		public void Initialize()
		{
			_party = new PartyInfo();
		}
	}
}