namespace IO
{
    public interface ISerializable<T>
    {
        void LoadData(T data);

        T SaveData();
    }
}