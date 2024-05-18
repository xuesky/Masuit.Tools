using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Masuit.Tools.Models;

namespace Masuit.Tools.DateTimeExt
{
    /// <summary>
    /// 日期操作工具类
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 转换为日期的起始时刻。
        /// </summary>
        /// <param name="time">当前的日期。</param>
        /// <returns>日期在0点0分0秒的时刻。</returns>
        public static DateTime StartOfDay(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }

        /// <summary>
        ///转换为日期的终止时刻。
        /// </summary>
        /// <param name="time">当前的日期。</param>
        /// <returns>日期在23点59分59秒的时刻。</returns>
        public static DateTime EndOfDay(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
        }

        /// <summary>
        /// 获取当前日期中本月的第一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本月的第一天。</returns>
        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second);
        }

        /// <summary>
        /// 获取当前日期中本月的最后一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本月的最后一天。</returns>
        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.AddMonths(1).StartOfMonth().AddDays(-1);
        }

        /// <summary>
        /// 获取当前日期中本周的第一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本周的第一天。</returns>
        public static DateTime StartOfWeek(this DateTime date)
        {
            var startDayOfWeek = DayOfWeek.Monday;
            if (date.DayOfWeek != startDayOfWeek)
            {
                var d = startDayOfWeek - date.DayOfWeek;
                return startDayOfWeek <= date.DayOfWeek ? date.AddDays(d) :
                    date.AddDays(-7 + d);
            }

            return date;
        }
        /// <summary>
        /// 获取当前日期中本周的最后一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本周的最后一天。</returns>
        public static DateTime EndOfWeek(this DateTime date)
        {
            var startDayOfWeek = DayOfWeek.Monday;
            var endDayOfWeek = startDayOfWeek - 1;
            if (endDayOfWeek < 0)
            {
                endDayOfWeek = DayOfWeek.Saturday;
            }

            if (date.DayOfWeek != endDayOfWeek)
            {
                if (endDayOfWeek == date.DayOfWeek)
                {
                    return date.AddDays(6);
                }

                if (endDayOfWeek < date.DayOfWeek)
                {
                    return date.AddDays(7 - (date.DayOfWeek - endDayOfWeek));
                }

                return date.AddDays(endDayOfWeek - date.DayOfWeek);
            }

            return date;
        }
        /// <summary>
        /// 将日期转换为 <see cref="DateTimeOffset"/>。
        /// </summary>
        /// <param name="localDateTime"></param>
        /// <param name="localTimeZone"></param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime localDateTime, TimeZoneInfo? localTimeZone = null)
        {
            localTimeZone = localTimeZone ?? TimeZoneInfo.Local;

            if (localDateTime.Kind != DateTimeKind.Unspecified)
            {
                localDateTime = new DateTime(localDateTime.Ticks, DateTimeKind.Unspecified);
            }

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);
        }
        /// <summary>
        /// 获取某一年有多少周
        /// </summary>
        /// <param name="now"></param>
        /// <returns>该年周数</returns>
        public static int GetWeekAmount(this in DateTime now)
        {
            var end = new DateTime(now.Year, 12, 31); //该年最后一天
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(end, CalendarWeekRule.FirstDay, DayOfWeek.Monday); //该年星期数
        }

        /// <summary>
        /// 返回年度第几个星期   默认星期日是第一天
        /// </summary>
        /// <param name="date">时间</param>
        /// <returns>第几周</returns>
        public static int WeekOfYear(this in DateTime date)
        {
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        /// <summary>
        /// 返回年度第几个星期
        /// </summary>
        /// <param name="date">时间</param>
        /// <param name="week">一周的开始日期</param>
        /// <returns>第几周</returns>
        public static int WeekOfYear(this in DateTime date, DayOfWeek week)
        {
            var gc = new GregorianCalendar();
            return gc.GetWeekOfYear(date, CalendarWeekRule.FirstDay, week);
        }

        /// <summary>
        /// 得到一年中的某周的起始日和截止日
        /// 周数 nNumWeek
        /// </summary>
        /// <param name="now"></param>
        /// <param name="nNumWeek">第几周</param>
        public static DateTimeRange GetWeekTime(this DateTime now, int nNumWeek)
        {
            var dt = new DateTime(now.Year, 1, 1);
            dt += new TimeSpan((nNumWeek - 1) * 7, 0, 0, 0);
            return new DateTimeRange(dt.AddDays(-(int)dt.DayOfWeek + (int)DayOfWeek.Monday), dt.AddDays((int)DayOfWeek.Saturday - (int)dt.DayOfWeek + 1));
        }

        #region P/Invoke 设置本地时间

        [DllImport("kernel32.dll")]
        private static extern bool SetLocalTime(ref SystemTime time);

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            public short year;
            public short month;
            public short dayOfWeek;
            public short day;
            public short hour;
            public short minute;
            public short second;
            public short milliseconds;
        }

        /// <summary>
        /// 设置本地计算机系统时间，仅支持Windows系统
        /// </summary>
        /// <param name="dt">DateTime对象</param>
        public static void SetLocalTime(this in DateTime dt)
        {
            SystemTime st;
            st.year = (short)dt.Year;
            st.month = (short)dt.Month;
            st.dayOfWeek = (short)dt.DayOfWeek;
            st.day = (short)dt.Day;
            st.hour = (short)dt.Hour;
            st.minute = (short)dt.Minute;
            st.second = (short)dt.Second;
            st.milliseconds = (short)dt.Millisecond;
            SetLocalTime(ref st);
        }

        #endregion P/Invoke 设置本地时间

        /// <summary>
        /// 返回相对于当前时间的相对天数
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="relativeday">相对天数</param>
        public static string GetDateTime(this in DateTime dt, int relativeday)
        {
            return dt.AddDays(relativeday).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalSeconds(this in DateTime dt) => new DateTimeOffset(dt).UtcDateTime.Ticks / 10_000_000L - 62135596800L;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的毫秒数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalMilliseconds(this in DateTime dt) => new DateTimeOffset(dt).UtcDateTime.Ticks / 10000L - 62135596800000L;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的微秒时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalMicroseconds(this in DateTime dt) => (new DateTimeOffset(dt).UtcTicks - 621355968000000000) / 10;

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的纳秒时间戳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetTotalNanoseconds(this in DateTime dt)
        {
            var ticks = (new DateTimeOffset(dt).UtcTicks - 621355968000000000) * 100;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                QueryPerformanceCounter(out var timestamp);
                return ticks + timestamp % 100;
            }

            return ticks + Stopwatch.GetTimestamp() % 100;
        }

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的分钟数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetTotalMinutes(this in DateTime dt) => new DateTimeOffset(dt).Offset.TotalMinutes;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的小时数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetTotalHours(this in DateTime dt) => new DateTimeOffset(dt).Offset.TotalHours;

        /// <summary>
        /// 获取该时间相对于1970-01-01T00:00:00Z的天数
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static double GetTotalDays(this in DateTime dt) => new DateTimeOffset(dt).Offset.TotalDays;

        /// <summary>本年有多少天</summary>
        /// <param name="dt">日期</param>
        /// <returns>本天在当年的天数</returns>
        public static int GetDaysOfYear(this in DateTime dt)
        {
            //取得传入参数的年份部分，用来判断是否是闰年
            int n = dt.Year;
            return DateTime.IsLeapYear(n) ? 366 : 365;
        }

        /// <summary>本月有多少天</summary>
        /// <param name="now"></param>
        /// <returns>天数</returns>
        public static int GetDaysOfMonth(this DateTime now)
        {
            return now.Month switch
            {
                1 => 31,
                2 => DateTime.IsLeapYear(now.Year) ? 29 : 28,
                3 => 31,
                4 => 30,
                5 => 31,
                6 => 30,
                7 => 31,
                8 => 31,
                9 => 30,
                10 => 31,
                11 => 30,
                12 => 31,
                _ => 0
            };
        }

        /// <summary>返回当前日期的星期名称</summary>
        /// <param name="now">日期</param>
        /// <returns>星期名称</returns>
        public static string GetWeekNameOfDay(this in DateTime now)
        {
            return now.DayOfWeek switch
            {
                DayOfWeek.Monday => "星期一",
                DayOfWeek.Tuesday => "星期二",
                DayOfWeek.Wednesday => "星期三",
                DayOfWeek.Thursday => "星期四",
                DayOfWeek.Friday => "星期五",
                DayOfWeek.Saturday => "星期六",
                DayOfWeek.Sunday => "星期日",
                _ => ""
            };
        }

        /// <summary>
        /// 判断时间是否在区间内
        /// </summary>
        /// <param name="this"></param>
        /// <param name="start">开始</param>
        /// <param name="end">结束</param>
        /// <param name="mode">模式</param>
        /// <returns></returns>
        public static bool In(this in DateTime @this, DateTime start, DateTime end, RangeMode mode = RangeMode.Close)
        {
            return mode switch
            {
                RangeMode.Open => start < @this && end > @this,
                RangeMode.Close => start <= @this && end >= @this,
                RangeMode.OpenClose => start < @this && end >= @this,
                RangeMode.CloseOpen => start <= @this && end > @this,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        /// <summary>
        /// 返回某年某月最后一天
        /// </summary>
        /// <param name="now"></param>
        /// <returns>日</returns>
        public static int GetMonthLastDate(this DateTime now)
        {
            DateTime lastDay = new DateTime(now.Year, now.Month, new GregorianCalendar().GetDaysInMonth(now.Year, now.Month));
            return lastDay.Day;
        }

        /// <summary>
        /// 获得一段时间内有多少小时
        /// </summary>
        /// <param name="start">起始时间</param>
        /// <param name="end">终止时间</param>
        /// <returns>小时差</returns>
        public static string GetTimeDelay(this in DateTime start, DateTime end)
        {
            return (end - start).ToString("c");
        }

        /// <summary>
        /// 返回时间差
        /// </summary>
        /// <param name="dateTime1">时间1</param>
        /// <param name="dateTime2">时间2</param>
        /// <returns>时间差</returns>
        public static string DateDiff(this in DateTime dateTime1, in DateTime dateTime2)
        {
            string dateDiff;
            var ts = dateTime2 - dateTime1;
            if (ts.TotalDays >= 1)
            {
                dateDiff = ts.TotalDays >= 30 ? (ts.TotalDays / 30) + "个月前" : ts.TotalDays + "天前";
            }
            else
            {
                dateDiff = ts.Hours > 1 ? ts.Hours + "小时前" : ts.Minutes + "分钟前";
            }

            return dateDiff;
        }

        /// <summary>
        /// 计算2个时间差
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>时间差</returns>
        public static string GetDiffTime(this in DateTime beginTime, in DateTime endTime)
        {
            string strResout = string.Empty;

            //获得2时间的时间间隔秒计算
            TimeSpan span = endTime.Subtract(beginTime);
            if (span.Days >= 365)
            {
                strResout += span.Days / 365 + "年";
            }
            if (span.Days >= 30)
            {
                strResout += span.Days % 365 / 30 + "个月";
            }
            if (span.Days >= 1)
            {
                strResout += (int)(span.TotalDays % 30.42) + "天";
            }
            if (span.Hours >= 1)
            {
                strResout += span.Hours + "小时";
            }
            if (span.Minutes >= 1)
            {
                strResout += span.Minutes + "分钟";
            }
            if (span.Seconds >= 1)
            {
                strResout += span.Seconds + "秒";
            }
            return strResout;
        }

        /// <summary>
        /// 根据某个时间段查找在某批时间段中的最大并集
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="sources"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ICollection<T> GetUnionSet<T>(this T destination, List<T> sources) where T : ITimePeriod, new()
        {
            var result = true;
            ICollection<T> frames = new List<T>();

            var timeFrames = sources.Where(frame =>
                !(destination.Start > frame.End || destination.End < frame.Start)).ToList();
            if (timeFrames.Any())
                foreach (var frame in timeFrames)
                {
                    frames.Add(frame);
                    sources.Remove(frame);
                }

            if (!frames.Any()) return frames;
            var timePeriod = new T()
            {
                End = frames.OrderBy(frame => frame.End).Max(frame => frame.End),
                Start = frames.OrderBy(frame => frame.Start).Min(frame => frame.Start)
            };

            while (result)
            {
                var maxTimeFrame = GetUnionSet<T>(timePeriod, sources);
                if (!maxTimeFrame.Any())
                    result = false;
                else
                    foreach (var frame in maxTimeFrame)
                        frames.Add(frame);
            }

            return frames;
        }

        /// <summary>
        /// 获取一批时间段内存在相互重叠的最大时间段
        /// </summary>
        /// <param name="destination">基础时间段</param>
        /// <param name="sources">一批时间段</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>源数据sources 会受到影响</remarks>
        public static T GetMaxTimePeriod<T>(this T destination, List<T> sources) where T : ITimePeriod, new()
        {
            var list = sources.Select(period => new T()
            {
                End = period.End,
                Start = period.Start,
            }).ToList();

            var timePeriods = GetUnionSet(destination, list);
            return new T()
            {
                End = timePeriods.OrderBy(period => period.End).Max(period => period.End),
                Start = timePeriods.OrderBy(period => period.Start).Min(period => period.Start)
            };
        }
    }

    public interface ITimePeriod
    {
        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// 终止时间
        /// </summary>
        public DateTime End { get; set; }
    }

    /// <summary>
    /// 区间模式
    /// </summary>
    public enum RangeMode
    {
        /// <summary>
        /// 开区间
        /// </summary>
        Open,

        /// <summary>
        /// 闭区间
        /// </summary>
        Close,

        /// <summary>
        /// 左开右闭区间
        /// </summary>
        OpenClose,

        /// <summary>
        /// 左闭右开区间
        /// </summary>
        CloseOpen
    }
}