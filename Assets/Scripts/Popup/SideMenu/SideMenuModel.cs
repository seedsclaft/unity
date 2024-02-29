namespace Ryneus
{
    public class SideMenuModel : BaseModel
    {
        public void DeletePlayerData()
        {
            SaveSystem.DeletePlayerData();
        }
    }
}
