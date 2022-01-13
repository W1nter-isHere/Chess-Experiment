namespace Utilities
{
    public interface ISerializable<TDummyClass>
    {
        void LoadData(TDummyClass data);

        TDummyClass SaveData();
    }
}