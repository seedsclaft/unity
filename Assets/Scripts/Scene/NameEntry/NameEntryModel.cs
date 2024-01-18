public class NameEntryModel : BaseModel
{
    public void SetPlayerName(string name)
    {
        CurrentData.SetPlayerName(name);
        SavePlayerData();
    }

    public void StartOpeningStage()
    {
        InitSaveStageInfo();
        CurrentStageData.MakeStageData(1);
        CurrentStage.AddSelectActorId(1);
        PartyInfo.ChangeCurrency(DataSystem.System.InitCurrency);
        SavePlayerStageData(true);
    }
}
