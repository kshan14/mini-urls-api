namespace MiniUrl.Services;

public interface IBase62Encoder
{
    string Encode(long number);
}
