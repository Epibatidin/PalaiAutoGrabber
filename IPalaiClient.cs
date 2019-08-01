namespace PalaiAutoGrabber
{
    public interface IPalaiClient
    {
        IAuthentificatedPalaiClient Login(Account account);
    }

    public interface IAuthentificatedPalaiClient
    {
        int GrabTheCash();
    }
}