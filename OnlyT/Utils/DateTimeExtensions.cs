using System;

namespace OnlyT.Utils
{
   public static class DateTimeExtensions
   {
      ///<summary>Gets the first week day following a date.</summary>
      ///<param name="date">The date.</param>
      ///<param name="dayOfWeek">The day of week to return.</param>
      ///<returns>The first dayOfWeek day following date, or date if it is on dayOfWeek.</returns>
      public static DateTime Next(this DateTime date, DayOfWeek dayOfWeek)
      {
         return date.AddDays((dayOfWeek < date.DayOfWeek ? 7 : 0) + dayOfWeek - date.DayOfWeek);
      }

      ///<summary>Gets the first week day before a date.</summary>
      ///<param name="date">The date.</param>
      ///<param name="dayOfWeek">The day of week to return.</param>
      ///<returns>The first dayOfWeek day before date, or date if it is on dayOfWeek.</returns>
      public static DateTime Prev(this DateTime date, DayOfWeek dayOfWeek)
      {
         DateTime result = date;
         while (result.DayOfWeek != dayOfWeek)
         {
            result = result.AddDays(-1);
         }

         return result;
      }

   }
}
