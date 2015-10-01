namespace CoursesAPI.Services.Utilities
{
	public class DateTimeUtils
	{
		public static bool IsLeapYear(int year)
		{
		    var leapYear = year%4 == 0 || (year%100 != 0 && year%400 == 0);
            if (year%100 == 0) leapYear = false;
		    if (year%400 == 0) leapYear = true;

            return leapYear;
		}
	}
}
