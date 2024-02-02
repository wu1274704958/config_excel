using NPOI.SS.UserModel;

namespace core
{
    public interface IGenMeta<out MD>
    {
        MD GenerateMeta(ISheet sheet);
    }
}