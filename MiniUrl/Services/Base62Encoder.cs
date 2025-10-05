using System.Text;

namespace MiniUrl.Services;

public class Base62Encoder : IBase62Encoder
{
    private const string Base62Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public string Encode(long number)
    {
        var strBuilder = new StringBuilder();
        while (number > 0)
        {
            var remainder = (int)(number % 62);
            strBuilder.Insert(0, Base62Chars[remainder]);
            number /= 62;
        }

        return strBuilder.ToString();
    }
}
