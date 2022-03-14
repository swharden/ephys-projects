namespace Report2P.Abf;

internal static class AbfTools
{
    public static DateTime StartDateTime(AbfSharp.ABFFIO.ABF abf)
    {
        int datecode = (int)abf.Header.uFileStartDate;

        int day = datecode % 100;
        datecode /= 100;

        int month = datecode % 100;
        datecode /= 100;

        int year = datecode;

        try
        {
            if (year < 1980 || year >= 2080)
                throw new InvalidOperationException("unexpected creation date year in header");
            return new DateTime(year, month, day).AddMilliseconds(abf.Header.uFileStartTimeMS);
        }
        catch
        {
            return new DateTime(0);
        }
    }
}
