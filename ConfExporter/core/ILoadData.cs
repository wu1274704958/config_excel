using NPOI.SS.UserModel;

namespace core
{
    public interface IDataLoader<out D,in MD>
    {
        D Load(ISheet sheet,MD meta);
    }
}