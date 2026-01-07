public static class MaskHelper
{
    public static string MaskPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "****";

        phone = phone.Trim();
        if (phone.Length < 7)
            return "****" + phone.Substring(Math.Max(0, phone.Length - 3));

        // Lấy 3 số đầu + **** + 3 số cuối
        return string.Concat(phone.AsSpan(0, 3), "****", phone.AsSpan(phone.Length - 3));
    }

    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return "****";

        var parts = email.Split('@');
        var name = parts[0];
        var domain = parts[1];

        if (name.Length <= 2)
            return "**@" + domain;

        // Lấy 2 ký tự đầu của tên + **** + domain
        return name.Substring(0, 2) + "****@" + domain;
    }


    public static string MaskGeneric(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "********";
        if (value.Length < 4)
            return "****";

        return "****" + value.Substring(value.Length - 3);
    }
}
