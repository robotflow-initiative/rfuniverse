namespace RFUniverse
{
    public interface ICustomMove
    {
        void Forward(float dis, float speed);
        void Back(float dis, float speed);
        void Left(float angle, float speed);
        void Right(float angle, float speed);
    }
}
